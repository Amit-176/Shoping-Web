using Ecomm_project_1145_2.Data;
using Ecomm_project_1145_2.DataAccess.Repository;
using Ecomm_project_1145_2.DataAccess.Repository.IRepository;
using Ecomm_project_1145_2.Model;
using Ecomm_project_1145_2.Utiltity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.Differencing;

namespace Ecomm_project_1145_2.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles =SD.Role_Admin + "," + SD.Role_Employee)]
    public class CategoryController : Controller
    {
        private readonly IUnitOfWork _unitofwork;

        public CategoryController(IUnitOfWork unitofwork)
        {
            _unitofwork = unitofwork;
        }

        public IActionResult Index()
        {
            return View();
        }
         #region APIs
        [HttpGet]
        public IActionResult GetAll() 
        {
            var categoryList=_unitofwork.Category.GetAll();
            return Json(new {data=categoryList});   
        }
        [HttpDelete]
        public IActionResult Delete(int id)
        {
            var categoryInDb = _unitofwork.Category.Get(id);
            if (categoryInDb == null)
                return Json(new { success =false,message= "Something wrong while delete data? !!!" });
            _unitofwork.Category.Remove(categoryInDb);
            _unitofwork.Save();
            return Json(new { success = true, message="Data Deleted Successfully !!!" });
        }
        #endregion
        public IActionResult Upsert(int? id)
        {
            Category category = new Category();
            if (id == null) return View(category);  //create

            //Edit
            category = _unitofwork.Category.Get(id.GetValueOrDefault());
            if(category== null) return NotFound();
            return View(category);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Upsert(Category category)
        {
            if(category== null) return NotFound();
            if (!ModelState.IsValid) return View(category);
            if (category.Id == 0)
                _unitofwork.Category.Add(category);
            else
                _unitofwork.Category.Update(category);
            _unitofwork.Save();
            return RedirectToAction(nameof(Index));
        }
    }
}
