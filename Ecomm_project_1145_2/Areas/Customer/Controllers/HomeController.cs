using Ecomm_project_1145_2.DataAccess.Repository;
using Ecomm_project_1145_2.DataAccess.Repository.IRepository;
using Ecomm_project_1145_2.Model;
using Ecomm_project_1145_2.Models;
using Ecomm_project_1145_2.Utiltity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Security.Claims;

namespace Ecomm_project_1145_2.Areas.Customer.Controllers
{
    [Area("Customer")]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IUnitOfWork _unitofwork;

        public HomeController(ILogger<HomeController> logger, IUnitOfWork unitofwork)
        {
            _logger = logger;
            _unitofwork = unitofwork;
        }

        public IActionResult Index(string searchTitle, string searchAuthorName)
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claims = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
            if (claims != null)
            {
                var count = _unitofwork.ShoppingCart.GetAll(sc => sc.ApplicationUserId == claims.Value).ToList().Count;
                HttpContext.Session.SetInt32(SD.Ss_CartSessionCount,count);
            }
             var productList =_unitofwork.Product.GetAll(includeProperties:"Category,CoverType");
             foreach (var product in productList)
             {
                 var totalCount = 0;
                 var orderList=_unitofwork.OrderDetails.GetAll(p=>p.Product.Id==product.Id);
                 foreach (var order in orderList)
                 {
                     totalCount += order.Count;
                 }
                 product.TotalOrderSell=totalCount;
             }
             _unitofwork.Save();
             productList=productList.OrderByDescending(item=>item.TotalOrderSell);

            /*  var productList = _unitofwork.Product.GetAll(includeProperties: "Category,CoverType").OrderByDescending(product =>
            _unitofwork.OrderDetails.GetAll(order => order.Product.Id == product.Id).Sum(order => order.Count)).ToList();*/

            if (!String.IsNullOrEmpty(searchTitle))
            {
                productList = productList.Where(s => s.Title!.Contains(searchTitle));
            }
            if (!String.IsNullOrEmpty(searchAuthorName))
            {
                productList = productList.Where(s => s.Author!.Contains(searchAuthorName));
            }
            return View(productList);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
        public IActionResult Details(int id)
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claims = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
            if (claims != null)
            {
                var count = _unitofwork.ShoppingCart.GetAll(sc => sc.ApplicationUserId == claims.Value).ToList().Count;
                HttpContext.Session.SetInt32(SD.Ss_CartSessionCount, count);
            }
            var productInDb = _unitofwork.Product.FirstOfDefault(p => p.Id == id, includeProperties: "Category,CoverType");
            if (productInDb == null) return NotFound();
            var shoppingCart = new ShoppingCart()
            {
                Product=productInDb,
                ProductId=productInDb.Id,
            };
            return View(shoppingCart);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public IActionResult Details(ShoppingCart shoppingCart)
        {
            shoppingCart.Id = 0;
            if (ModelState.IsValid)
            {
                var claimsIdentity = (ClaimsIdentity)User.Identity;
                var claims = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
                if (claims == null) return NotFound();
                shoppingCart.ApplicationUserId = claims.Value;
                var shoppingCartInDb = _unitofwork.ShoppingCart.FirstOfDefault(sc => sc.ApplicationUserId == claims.Value && sc.ProductId == shoppingCart.ProductId);
                if (shoppingCartInDb == null)
                    _unitofwork.ShoppingCart.Add(shoppingCart);
                else
                    shoppingCartInDb.Count += shoppingCart.Count;
                _unitofwork.Save();
                return RedirectToAction("Index");
            }
            else
            {
                var productInDb = _unitofwork.Product.FirstOfDefault(p => p.Id == shoppingCart.Id, includeProperties: "Category.CoverType");
                if (productInDb == null) return NotFound();
                var shoppingCartEdit = new ShoppingCart()
                {
                    Product = productInDb,
                    ProductId = productInDb.Id
                };
                return View(shoppingCartEdit);
            }
        }
    }
}