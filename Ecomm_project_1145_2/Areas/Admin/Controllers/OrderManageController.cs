using Ecomm_project_1145_2.DataAccess.Repository.IRepository;
using Ecomm_project_1145_2.Model;
using Ecomm_project_1145_2.Model.ViewModels;
using Ecomm_project_1145_2.Utiltity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Ecomm_project_1145_2.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class OrderManageController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<IdentityUser> _userManager;

        public OrderManageController(IUnitOfWork unitOfWork,UserManager<IdentityUser> userManager)
        {
            _unitOfWork = unitOfWork;
            _userManager = userManager;
        }
        [BindProperty]
        public OrderManageVM OrderManageVM{ get; set; }
        public IActionResult Index()
        {
            return View();
        }
        #region APIs
        [HttpGet]
        public IActionResult GetAll()
        {
            var orderList = _unitOfWork.OrderHeader.GetAll();
            return Json(new { data = orderList });
        }
        #endregion  
        public IActionResult ViewData(int id)
        {
            OrderManageVM orderManageVM = new OrderManageVM()
            {
                OrderHeader = new OrderHeader(),
                OrderDetailsList = _unitOfWork.OrderDetails.GetAll(ol => ol.OrderHeaderId == id, includeProperties:"Product,OrderHeader")
            };

            //orderManageVM.OrderHeader.OrderTotal = 0;
            foreach (var list in orderManageVM.OrderDetailsList)
            {
                list.Product = _unitOfWork.Product.FirstOfDefault(p => p.Id == list.ProductId);
            }
            return View(orderManageVM);
        }
        public IActionResult OrderDate(DateTime startDate, DateTime endDate, string name)
        {
            if (name == "AllOrders")
            {
                var data = _unitOfWork.OrderHeader.GetAll().Where(d => d.OrderDate >= startDate && d.OrderDate <= endDate);
                return View(data);
            }
            if (name == "Pending")
            {
                var data = _unitOfWork.OrderHeader.GetAll().Where(d => d.OrderDate >= startDate && d.OrderDate <= endDate && d.OrderStatus == "Pending");
                return View(data);
            }
            if (name == "Approved")
            {
                var data = _unitOfWork.OrderHeader.GetAll().Where(d => d.OrderDate >= startDate && d.OrderDate <= endDate && d.OrderStatus == "Approved");
                return View(data);
            }
            else
                return View(_unitOfWork.OrderHeader.GetAll().Where(d => d.OrderDate <= startDate && d.OrderDate >= endDate && d.OrderStatus == SD.OrderStatusApproved));
        }
    }
}
