using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ecomm_project_1145_2.Model.ViewModels
{
    public class OrderManageVM
    {
        public OrderDetails OrderDetails { get; set; }
        public OrderHeader OrderHeader { get; set; }
        //public IEnumerable<OrderHeader> OrderHeaderList { get; set; }
        public IEnumerable<OrderDetails> OrderDetailsList { get; set; }
        
    }
}
