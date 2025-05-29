using SQLite;

namespace Beysik_OrderService.Models
{
    public enum Status
    {
        Pending = 0,
        Completed = 1,
        Cancelled = 2
    }
    public class Order
    {
        [PrimaryKey, AutoIncrement]
        public int OrderID { get; set; }
        public string OrderDate { get; set; }
        public string UserID { get; set; }
        public List<string> ProductID { get; set; }
        public List<int> Quantity { get; set; }
        public int Status { get; set; } // 0: Pending, 1: Completed, 2: Cancelled
    }
}
