using Ecomm_project_1145_2.Data;
using Ecomm_project_1145_2.DataAccess.Repository.IRepository;
using Ecomm_project_1145_2.Model;
using Ecomm_project_1145_2.Utiltity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Ecomm_project_1145_2.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles =SD.Role_Admin)]
    public class UserController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ApplicationDbContext _context;

        public UserController(IUnitOfWork unitOfWork, ApplicationDbContext context)
        {
            _unitOfWork = unitOfWork;
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }
        #region APIs
        [HttpGet]
        public IActionResult GetAll()
        {
            var userList = _context.ApplicationUsers.ToList(); //aspnet users
            var roles = _context.Roles.ToList(); //aspnet roles
            var userRoles = _context.UserRoles.ToList(); //aspnet user roles
            foreach (var user in userList)
            {
                var roleId = userRoles.FirstOrDefault(u => u.UserId == user.Id).RoleId;
                user.Role = roles.FirstOrDefault(r => r.Id == roleId).Name;
                if (user.CompanyId == null)
                {
                    user.Company = new Company()
                    {
                        Name = ""
                    };
                }
                if (user.CompanyId != null)
                {
                    user.Company = new Company()
                    {
                        Name=_unitOfWork.Company.Get(Convert.ToInt32(user.CompanyId)).Name
                    };
                }
            }
            //Remove Adimn Role in List
            var adminUser = userList.FirstOrDefault(u => u.Role == SD.Role_Admin);
            userList.Remove(adminUser);
            return Json(new { data = userList });
        }
        [HttpPost]
        public IActionResult LockUnLock([FromBody]string id)
        {
            bool isLocked = false;
            var userInDb=_context.ApplicationUsers.FirstOrDefault(u=>u.Id == id);
            if (userInDb == null)
                return Json(new { sucess = false, message = "Something Want Wrong While Lock Or UnLock user" });
            if(userInDb != null && userInDb.LockoutEnd>DateTime.Now) 
            {
                userInDb.LockoutEnd= DateTime.Now;
                isLocked = false;
            }
            else
            {
                userInDb.LockoutEnd = DateTime.Now.AddYears(100);
                isLocked= true;
            }
            _context.SaveChanges();
            return Json(new { success = true, message = isLocked == true ? "User Lock Successfully" : "User UnLock Successfully" });
        }
        #endregion
    }
}
