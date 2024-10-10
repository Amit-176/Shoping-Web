using Ecomm_project_1145_2.DataAccess.Repository.IRepository;
using Ecomm_project_1145_2.Model;
using Ecomm_project_1145_2.Model.ViewModels;
using Ecomm_project_1145_2.Utiltity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Stripe;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Text;
using Ecomm_project_1145_2.DataAccess.Repository;

namespace Ecomm_project_1145_2.Areas.Customer.Controllers
{
    [Area("Customer")]
    public class CartController : Controller
    {
        private readonly IUnitOfWork _unitofwork;
        private static bool isEmailConfirm=false;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IEmailSender _emailSender;

        public CartController(IUnitOfWork unitofwork, UserManager<IdentityUser> userManager, IEmailSender emailSender)
        {
            _unitofwork = unitofwork;
            _userManager = userManager;
            _emailSender = emailSender;
        }

        [BindProperty]
        public ShoppingCartVM ShoppingCartVM { get; set; }
        public IActionResult Index()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claims = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
            if (claims == null)
            {
                ShoppingCartVM = new ShoppingCartVM()
                {
                    ListCart=new List<ShoppingCart>()
                };
                return View(ShoppingCartVM);
            }
            //*** price ke liye
            ShoppingCartVM = new ShoppingCartVM()
            {
                ListCart = _unitofwork.ShoppingCart.GetAll(sc => sc.ApplicationUserId == claims.Value, includeProperties: "Product"),
                OrderHeader = new OrderHeader()
            };
            ShoppingCartVM.OrderHeader.OrderTotal = 0;
            ShoppingCartVM.OrderHeader.ApplicationUser = _unitofwork.ApplicationUser.FirstOfDefault(au => au.Id == claims.Value);
            foreach (var list in ShoppingCartVM.ListCart)
            {
                list.Price = SD.GetPriceBasedOnQuantity(list.Count, list.Product.Price, list.Product.Price50, list.Product.Price100);
                ShoppingCartVM.OrderHeader.OrderTotal += (list.Count * list.Price);
                if (list.Product.Description.Length > 100)
                {
                    list.Product.Description = list.Product.Description.Substring(0, 99) + "....";
                }
            }
            if (isEmailConfirm)
            {
                ViewBag.EmailMessage = "Email has been send kindly verify your email !";
                ViewBag.EmailCSS = "text-success";
                isEmailConfirm = false;
            }
            else
            {
                ViewBag.EmailMessage = "Email Must be confirm for authorize customer";
                ViewBag.EmailCSS = "text-danger";
                isEmailConfirm = true;
            }
            return View(ShoppingCartVM);
        }       
        [HttpPost]
        [ValidateAntiForgeryToken]
        [ActionName("Index")]
        public async Task<IActionResult> IndexPost()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claims = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
            var user = _unitofwork.ApplicationUser.FirstOfDefault(au => au.Id == claims.Value);
            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "Email is Empty");
            }
            else
            {
                var userId = await _userManager.GetUserIdAsync(user);
                var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                var callbackUrl = Url.Page(
                    "/Account/ConfirmEmail",
                    pageHandler: null,
                    values: new { area = "Identity", userId = userId, code = code },
                    protocol: Request.Scheme);

                await _emailSender.SendEmailAsync(user.Email, "Confirm your email",
                    $"Please confirm your account by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.");
            }
            return RedirectToAction(nameof(Index));
        }
        public IActionResult plus(int id)
        {
            var cart = _unitofwork.ShoppingCart.FirstOfDefault(sc => sc.Id == id);
            cart.Count += 1;
            _unitofwork.Save();
            return RedirectToAction(nameof(Index));
        }
        public IActionResult minus(int id)
        {
            var cart = _unitofwork.ShoppingCart.FirstOfDefault(sc => sc.Id == id);
            if (cart.Count == 1)
                cart.Count = 1;
            else
                cart.Count -= 1;
            _unitofwork.Save();
            return RedirectToAction(nameof(Index)); 
        }
        public IActionResult delete(int id)
        {
            var cart = _unitofwork.ShoppingCart.FirstOfDefault(sc => sc.Id == id);
            _unitofwork.ShoppingCart.Remove(cart);
            _unitofwork.Save();
            //session
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claims = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
            var count = _unitofwork.ShoppingCart.GetAll(sc => sc.ApplicationUserId == claims.Value).ToList().Count;
            HttpContext.Session.SetInt32(SD.Ss_CartSessionCount, count);
            return RedirectToAction(nameof(Index));
        }

        /*public IActionResult Summary()
        {

            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claims = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
            ShoppingCartVM = new ShoppingCartVM()
            {
                ListCart = _unitofwork.ShoppingCart.GetAll(sc => sc.ApplicationUserId == claims.Value, includeProperties: "Product"),
                OrderHeader = new OrderHeader()
            };
            ShoppingCartVM.OrderHeader.ApplicationUser = _unitofwork.ApplicationUser.FirstOfDefault(au => au.Id == claims.Value);
            foreach (var list in ShoppingCartVM.ListCart)
            {
                list.Price = SD.GetPriceBasedOnQuantity(list.Count, list.Product.Price, list.Product.Price50, list.Product.Price100);
                ShoppingCartVM.OrderHeader.OrderTotal += (list.Count * list.Price);
                if (list.Product.Description.Length > 100)
                {
                    list.Product.Description = list.Product.Description.Substring(0, 99) + ".....";
                }
            }
            ShoppingCartVM.OrderHeader.Name = ShoppingCartVM.OrderHeader.ApplicationUser.Name;
            ShoppingCartVM.OrderHeader.StreetAddress = ShoppingCartVM.OrderHeader.ApplicationUser.StreetAddress;
            ShoppingCartVM.OrderHeader.City = ShoppingCartVM.OrderHeader.ApplicationUser.City;
            ShoppingCartVM.OrderHeader.State = ShoppingCartVM.OrderHeader.ApplicationUser.State;
            ShoppingCartVM.OrderHeader.PostalCode = ShoppingCartVM.OrderHeader.ApplicationUser.PostalCode;
            ShoppingCartVM.OrderHeader.PhoneNumber = ShoppingCartVM.OrderHeader.ApplicationUser.PhoneNumber;
            return View(ShoppingCartVM);
        }*/
        [HttpGet]
        public IActionResult Summary(List<int> selectedItems)
        {
            if (selectedItems.Count == 0) return RedirectToAction(nameof(Index));
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claims = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
            ShoppingCartVM = new ShoppingCartVM()
            {
                //ListCart = _unitofwork.ShoppingCart.GetAll(sc => sc.ApplicationUserId == claims.Value, includeProperties: "Product"),
                ListCart = _unitofwork.ShoppingCart.GetAll(sc => sc.ApplicationUserId == claims.Value && selectedItems.Contains(sc.Id), includeProperties: "Product"),
                OrderHeader = new OrderHeader()
            };
            ShoppingCartVM.OrderHeader.ApplicationUser = _unitofwork.ApplicationUser.FirstOfDefault(au => au.Id == claims.Value);            
            foreach (var list in ShoppingCartVM.ListCart)
            {
                list.Price = SD.GetPriceBasedOnQuantity(list.Count, list.Product.Price, list.Product.Price50, list.Product.Price100);
                ShoppingCartVM.OrderHeader.OrderTotal += (list.Count * list.Price);
                if (selectedItems != null && selectedItems.Contains(list.Id))
                {
                    list.IsSelected = true;
                }
                else
                {
                    list.IsSelected = false;
                }
                if (list.Product.Description.Length > 100)
                {
                    list.Product.Description = list.Product.Description.Substring(0, 99) + ".....";
                }
            }
            ShoppingCartVM.OrderHeader.Name = ShoppingCartVM.OrderHeader.ApplicationUser.Name;
            ShoppingCartVM.OrderHeader.StreetAddress = ShoppingCartVM.OrderHeader.ApplicationUser.StreetAddress;
            ShoppingCartVM.OrderHeader.City = ShoppingCartVM.OrderHeader.ApplicationUser.City;
            ShoppingCartVM.OrderHeader.State = ShoppingCartVM.OrderHeader.ApplicationUser.State;
            ShoppingCartVM.OrderHeader.PostalCode = ShoppingCartVM.OrderHeader.ApplicationUser.PostalCode;
            ShoppingCartVM.OrderHeader.PhoneNumber = ShoppingCartVM.OrderHeader.ApplicationUser.PhoneNumber;
            return View(ShoppingCartVM);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ActionName("Summary")]
        public async Task<IActionResult> SummaryPost(string stripeToken, List<int> selectedItems)
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claims = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
            ShoppingCartVM.OrderHeader.ApplicationUser = _unitofwork.ApplicationUser.FirstOfDefault(au => au.Id == claims.Value);
            //ShoppingCartVM.ListCart = _unitofwork.ShoppingCart.GetAll(sc => sc.ApplicationUserId == claims.Value, includeProperties: "Product");
            ShoppingCartVM.ListCart = _unitofwork.ShoppingCart.GetAll(sc => sc.ApplicationUserId == claims.Value && selectedItems.Contains(sc.Id), includeProperties: "Product");
            ShoppingCartVM.OrderHeader.OrderStatus = SD.OrderStatusPending;
            ShoppingCartVM.OrderHeader.PaymentStatus = SD.PaymentStatusPending;
            ShoppingCartVM.OrderHeader.OrderDate = DateTime.Now;
            ShoppingCartVM.OrderHeader.ApplicationUserId = claims.Value;
            _unitofwork.OrderHeader.Add(ShoppingCartVM.OrderHeader);
            _unitofwork.Save();
            foreach (var list in ShoppingCartVM.ListCart)
            {
                list.Price = SD.GetPriceBasedOnQuantity(list.Count, list.Product.Price, list.Product.Price50, list.Product.Price100);
                OrderDetails orderDetails = new OrderDetails()
                {
                    ProductId = list.ProductId,
                    OrderHeaderId = ShoppingCartVM.OrderHeader.Id,
                    Price = list.Price,
                    Count = list.Count
                };
                ShoppingCartVM.OrderHeader.OrderTotal += (list.Count * list.Price);
                _unitofwork.OrderDetails.Add(orderDetails);
                _unitofwork.Save();
            }
            // cart se remove kar denge
            _unitofwork.ShoppingCart.RemoveRange(ShoppingCartVM.ListCart);
            _unitofwork.Save();
            // session
            //HttpContext.Session.SetInt32(SD.Ss_CartSessionCount,0);

            var count = _unitofwork.ShoppingCart.GetAll(u => u.ApplicationUserId == claims.Value).ToList().Count;
            if (count >= 1)
                HttpContext.Session.SetInt32(SD.Ss_CartSessionCount, count);
            else
                HttpContext.Session.SetInt32(SD.Ss_CartSessionCount, 0);
            #region stripe
            if (stripeToken == null)
            {
                ShoppingCartVM.OrderHeader.PaymentDueDate = DateTime.Now.AddDays(30);
                ShoppingCartVM.OrderHeader.PaymentStatus = SD.PaymentStatusPending;
                ShoppingCartVM.OrderHeader.OrderStatus = SD.OrderStatusApproved;
            }
            else
            {
                var options = new ChargeCreateOptions()
                {
                    Amount = Convert.ToInt32(ShoppingCartVM.OrderHeader.OrderTotal),
                    Currency = "usd",
                    Description = "Order Id:" + ShoppingCartVM.OrderHeader.Id.ToString(),
                    Source = stripeToken
                };
                var service = new ChargeService();
                Charge charge = service.Create(options);
                if (charge.BalanceTransactionId == null)
                    ShoppingCartVM.OrderHeader.PaymentStatus = SD.PaymentStatusRejected;
                else
                    ShoppingCartVM.OrderHeader.TransactionId = charge.BalanceTransactionId;
                if (charge.Status.ToLower() == "succeeded")
                {
                    ShoppingCartVM.OrderHeader.OrderStatus = SD.OrderStatusApproved;
                    ShoppingCartVM.OrderHeader.PaymentStatus = SD.PaymentStatusApproved;
                    ShoppingCartVM.OrderHeader.OrderDate = DateTime.Now;
                }
                _unitofwork.Save();
            }
            #endregion
            var twilioService = new TwilioService("AC01b6b61ffe48c688d64665193c5707fe", "3418394d96cb1a8c2d84f1be121b9439", "+18172647827");
            bool result = twilioService.SendSms("+91" + ShoppingCartVM.OrderHeader.ApplicationUser.PhoneNumber,
            $"Order has been confirmed.....Thanks for shopping with us... you will receive your package shortly........."+
            $"Your Order No. {ShoppingCartVM.OrderHeader.Id} "+
            $"Your Order Total : {ShoppingCartVM.OrderHeader.OrderTotal} "+
            $"Order Date :{ShoppingCartVM.OrderHeader.OrderDate}"
            );
            if (result)
            {
                Console.WriteLine("SMS sent successfully!");
            }
            else
            {
                Console.WriteLine("Failed to send SMS.");
            }

            //Email Confirmation Order Message 
            var user = _unitofwork.ApplicationUser.FirstOfDefault(au => au.Id == claims.Value);

            // Check if the user is null
            if (user == null)
            {
                // Add a model error if the user is null
                ModelState.AddModelError(string.Empty, "Email is Empty");
            }

            // Get the user's ID
            var userId = await _userManager.GetUserIdAsync(user);

            // Generate an email confirmation token for the user
            var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);

            // Encode the confirmation token to a URL-safe string
            code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));

            // Construct the URL for the email confirmation link
            var callbackUrl = Url.Page(
                "/Account/ConfirmEmail",
                pageHandler: null,
                values: new { area = "Identity", userId = userId, code = code },
                protocol: Request.Scheme);

            // Send an email to the user
            await _emailSender.SendEmailAsync(
                user.Email,
                "Order Confirmation",
                $"Order has been confirmed.....Thanks for shopping with us... you will receive your package shortly........." +
                $"Your Order No. {ShoppingCartVM.OrderHeader.Id} " +
                $"Your Order Total : {ShoppingCartVM.OrderHeader.OrderTotal} " +
                $"Order Date :{ShoppingCartVM.OrderHeader.OrderDate}"
            );


            return RedirectToAction("OrderConfirmation", "Cart", new { id = ShoppingCartVM.OrderHeader.Id });
        }
        
        
        /*[HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult SummaryPost(string stripeToken)
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claims = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
            ShoppingCartVM.OrderHeader.ApplicationUser = _unitofwork.ApplicationUser.FirstOfDefault(au => au.Id == claims.Value);
            ShoppingCartVM.ListCart = _unitofwork.ShoppingCart.GetAll(sc => sc.ApplicationUserId == claims.Value, includeProperties: "Product");
            ShoppingCartVM.OrderHeader.OrderStatus = SD.OrderStatusPending;
            ShoppingCartVM.OrderHeader.PaymentStatus = SD.PaymentStatusPending;
            ShoppingCartVM.OrderHeader.OrderDate = DateTime.Now;
            ShoppingCartVM.OrderHeader.ApplicationUserId = claims.Value;
            _unitofwork.OrderHeader.Add(ShoppingCartVM.OrderHeader);
            _unitofwork.Save();
            foreach (var list in ShoppingCartVM.ListCart)
            {
                list.Price = SD.GetPriceBasedOnQuantity(list.Count, list.Product.Price, list.Product.Price50, list.Product.Price100);
                OrderDetails orderDetails = new OrderDetails()
                {
                    ProductId = list.ProductId,
                    OrderHeaderId = ShoppingCartVM.OrderHeader.Id,
                    Price = list.Price,
                    Count = list.Count
                };
                ShoppingCartVM.OrderHeader.OrderTotal = (list.Count * list.Price);
                _unitofwork.OrderDetails.Add(orderDetails);
                _unitofwork.Save();
            }
            // cart se remove kar denge
            _unitofwork.ShoppingCart.RemoveRange(ShoppingCartVM.ListCart);
            _unitofwork.Save();
            // session
            HttpContext.Session.SetInt32(SD.Ss_CartSessionCount, 0);
            #region stripe
            if (stripeToken == null)
            {
                ShoppingCartVM.OrderHeader.PaymentDueDate = DateTime.Now.AddDays(30);
                ShoppingCartVM.OrderHeader.PaymentStatus = SD.PaymentStatusPending;
                ShoppingCartVM.OrderHeader.OrderStatus = SD.OrderStatusApproved;
            }
            else
            {
                var options = new ChargeCreateOptions()
                {
                    Amount = Convert.ToInt32(ShoppingCartVM.OrderHeader.OrderTotal),
                    Currency = "usd",
                    Description = "Order Id:" + ShoppingCartVM.OrderHeader.Id.ToString(),
                    Source = stripeToken
                };
                var service = new ChargeService();
                Charge charge = service.Create(options);
                if (charge.BalanceTransactionId == null)
                    ShoppingCartVM.OrderHeader.PaymentStatus = SD.PaymentStatusRejected;
                else
                    ShoppingCartVM.OrderHeader.TransactionId = charge.BalanceTransactionId;
                if (charge.Status.ToLower() == "succeeded")
                {
                    ShoppingCartVM.OrderHeader.OrderStatus = SD.OrderStatusApproved;
                    ShoppingCartVM.OrderHeader.PaymentStatus = SD.PaymentStatusApproved;
                    ShoppingCartVM.OrderHeader.OrderDate = DateTime.Now;
                }
                _unitofwork.Save();
            }
            #endregion
            var twilioService = new TwilioService("AC01b6b61ffe48c688d64665193c5707fe", "3418394d96cb1a8c2d84f1be121b9439", "+18172647827");
            bool result = twilioService.SendSms("+91" + ShoppingCartVM.OrderHeader.ApplicationUser.PhoneNumber,
            $"Your Order No. {ShoppingCartVM.OrderHeader.Id} for has been placed successfully!!");

            if (result)
            {
                Console.WriteLine("SMS sent successfully!");
            }
            else
            {
                Console.WriteLine("Failed to send SMS.");
            }
            return RedirectToAction("OrderConfirmation", "Cart", new { id = ShoppingCartVM.OrderHeader.Id });
        }*/
        public IActionResult OrderConfirmation(int id)
        {
            return View(id);
        }
    }
}
