using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using Beysik_OrderService.Models;
using Beysik_Common;
using SQLite;


namespace Beysik_OrderService.Services
{
    public class OrderService
    {
        private readonly ISQLiteConnection _db;
        private readonly IConfiguration Configuration;
        private readonly RabbitMqHelper _orderqueue;

        public OrderService(IConfiguration configuration, ISQLiteConnection sqliteConnection)
        {
            Configuration = configuration;
            _db = sqliteConnection;
            _db.CreateTable<Order>();

            RabbitMqHelper orderqueue = new RabbitMqHelper($"{Configuration["ConnectionStrings:RabbitMQConnection"]}", "order");
            _orderqueue = orderqueue;
        }


        public void Add(Order order)
        {
            order.Status = (int)Status.Pending;
            _db.Insert(order);
            _orderqueue.PublishMessage(order.OrderID.ToString()+ $" {order.Quantity} " + " order.created ").Wait();
            // Wait for the message to be consumed
            _orderqueue.ConsumeMessages($"{order.OrderID.ToString() + " order.success"} ");

            if (_orderqueue._message == $"{order.OrderID.ToString() + " order.success"} ")
            {
                order.Status = (int)Status.Completed;
                _db.InsertOrReplace(order);
            }
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

