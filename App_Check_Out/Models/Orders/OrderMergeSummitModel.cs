using Entities.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace APP_CHECKOUT.Models.Orders
{
    public class OrderMergeSummitModel
    {
        public OrderMerge order_merge { get; set; }
        public List<OrderMergeSummitOrder> detail { get; set; }
        public OrderDetailMongoDbModelExtend data_mongo { get; set; }
    }
    public class OrderMergeSummitOrder
    {
        public Order order { get; set; }
        public List<OrderDetail> order_detail { get; set; }
    }
}
