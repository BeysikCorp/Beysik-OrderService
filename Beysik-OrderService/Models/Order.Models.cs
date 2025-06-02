using SQLite;

namespace Beysik_OrderService.Models
{
    public enum Status
    {
        Pending = 0,
        Completed = 1,
        Cancelled = 2,
        Fullfilled = 3
    }
    public class OrderRequest
    {
        public string OrderDate { get; set; }
        public string UserID { get; set; }
        public List<string> ProductIDs { get; set; }
        public List<int> Quantities { get; set; }
    }
    public class Order
    {
        [PrimaryKey, AutoIncrement]
        public int OrderID { get; set; }
        public string OrderDate { get; set; }
        public string UserID { get; set; }
        public string ProductID { get; set; }
        public int Quantity { get; set; }
        public int Status { get; set; } // 0: Pending, 1: Completed, 2: Cancelled
    }
}
