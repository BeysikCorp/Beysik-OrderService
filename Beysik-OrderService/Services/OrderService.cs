using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using RabbitMQ.Client;
using Beysik_OrderService.Models;
using Beysik_Common;
using SQLite;
using static Beysik_Common.RabbitMqConsumerService;


namespace Beysik_OrderService.Services
{
    public class OrderService
    {
        private readonly ISQLiteConnection _db;
        //private readonly IConfiguration Configuration;
        private readonly RabbitMqHelper _rabbitMq;


        //public OrderService(IConfiguration configuration, ISQLiteConnection sqliteConnection, RabbitMqHelper rabbitMq)
        public OrderService(RabbitMqEventAggregator eventAggregator, ISQLiteConnection sqliteConnection, RabbitMqHelper rabbitMq)
        {
            //Configuration = configuration;
            eventAggregator.MessageReceived += OnMessageReceived;
            _db = sqliteConnection;
            _db.CreateTable<Order>();

            //RabbitMqHelper orderqueue = new RabbitMqHelper($"{Configuration["ConnectionStrings:RabbitMQConnection"]}");
            //_orderqueue = orderqueue;
            _rabbitMq = rabbitMq;
        }

        private void OnMessageReceived(object? sender, MessageReceivedEventArgs e)
        {
            if (e == null || string.IsNullOrEmpty(e.Message))
            {
                return;
            }
            List<string>? message = e.Message.Split('.').ToList();
            if (e.Message.Contains("order.allocated"))
            {
                int orderId = int.Parse(message[0]);
                int quantity = int.Parse(message[1]);
                Order order = Get(orderId);
                if (order != null)
                {
                    order.Status = (int)Status.Completed;
                    Update(order);
                    _rabbitMq.PublishMessage($"{order.OrderID.ToString()}.order.success", "order.tocart", "order.api.fromorder", ExchangeType.Topic).Wait();
                    _rabbitMq.PublishMessage($"{order.OrderID.ToString()}.order.success", "order.topc", "order.api.fromorder", ExchangeType.Topic).Wait();
                }
            }

            if (e.Message.Contains("order.failed"))
            {
                int orderId = int.Parse(message[0]);
                Order order = Get(orderId);
                if (order != null)
                {
                    order.Status = (int)Status.Cancelled;
                    Update(order);
                    _rabbitMq.PublishMessage($"{order.ProductID.ToString()}.{order.Quantity}.order.failed", "order.tocart", "order.api.fromorder", ExchangeType.Topic).Wait();
                }
            }

            if (e.Message.Contains("order.cancelled"))
            {
                int orderId = int.Parse(message[0]);
                Order order = Get(orderId);
                if (order != null)
                {
                    order.Status = (int)Status.Cancelled;
                    Update(order);
                    _rabbitMq.PublishMessage($"{order.ProductID.ToString()}.{order.Quantity}.order.cancelled", "order.topc", "order.api.fromorder", ExchangeType.Topic).Wait();
                }
            }
        }

        public async Task AddAsync(OrderRequest orderReq)
        {
            for (int i = 0; i < orderReq.ProductIDs.Count; i++)
            {
                var order = new Order
                {
                    OrderDate = orderReq.OrderDate,
                    UserID = orderReq.UserID,
                    ProductID = orderReq.ProductIDs[i],
                    Quantity = orderReq.Quantities[i],
                    Status = (int)Status.Pending
                };
                _db.Insert(order); // or your DB insert logic
                _rabbitMq.PublishMessage($"{order.ProductID.ToString()}.{order.Quantity}.order.created", "order.topc","order.api.fromorder", ExchangeType.Topic).Wait();
                await Task.Delay(500); // Simulate some delay for processing
            }
            _rabbitMq.PublishMessage($"{orderReq.UserID}.order.created", "order.toui", "order.ui.fromorder", ExchangeType.Topic).Wait();
            await Task.CompletedTask;
        }

        public Order Get(int id)
        {
            TableQuery<Order> result =
                _db.Table<Order>().Where(q => q.OrderID.Equals(id));
            return result.FirstOrDefault();
        }

        public void Update(Order order)
        {
            _db.Update(order);
        }

        public IEnumerable<Order> Get()
        {
            return _db.Table<Order>();
        }
    }

}

