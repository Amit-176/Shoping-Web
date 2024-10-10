using Ecomm_project_1145_2.Data;
using Ecomm_project_1145_2.DataAccess.Repository.IRepository;
using Ecomm_project_1145_2.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ecomm_project_1145_2.DataAccess.Repository
{
    public class OrderDetailsRepository:Repository<OrderDetails>,IOrderDetailsRepository
    {
        private readonly ApplicationDbContext _context;

        public OrderDetailsRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }
    }
}
