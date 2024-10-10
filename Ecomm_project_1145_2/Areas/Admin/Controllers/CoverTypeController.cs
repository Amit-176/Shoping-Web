using Ecomm_project_1145_2.DataAccess.Repository.IRepository;
using Ecomm_project_1145_2.Model;
using Ecomm_project_1145_2.Utiltity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Ecomm_project_1145_2.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles=SD.Role_Admin + "," + SD.Role_Employee)]
    public class CoverTypeController : Controller
    {
        private readonly IUnitOfWork _unitofwork;

        public CoverTypeController(IUnitOfWork unitofwork)
        {
            _unitofwork = unitofwork;
        }

        public IActionResult Index()
        {
            return View();
        }
        public IActionResult Upsert(int? id)
        {
            CoverType coverType = new CoverType();
            if (id == null) return View(coverType); //create
            //Edit
            coverType = _unitofwork.CoverType.Get(id.GetValueOrDefault());
            if(coverType==null)return NotFound();
            return View(coverType);
        }
        [HttpPost]
        public IActionResult Upsert(CoverType coverType)
        {
            if (coverType == null) return NotFound();
            if (!ModelState.IsValid) return View(coverType);
            if (coverType.Id == 0)
                _unitofwork.CoverType.Add(coverType);
            else
               _unitofwork.CoverType.Update(coverType);
            _unitofwork.Save();
            return RedirectToAction(nameof(Index));

        }
        #region APIs
        [HttpGet]
        public IActionResult GetAll()
        {
            return Json(new {data = _unitofwork.CoverType.GetAll() });
        }
        [HttpDelete]
        public IActionResult Delete(int id)
        {
            var coverTypeInDb=_unitofwork.CoverType.Get(id);
            if (coverTypeInDb == null)
                return Json(new { success = false, message = "Something wrong while delete data? !!" });
            _unitofwork.CoverType.Remove(coverTypeInDb);
            _unitofwork.Save();
            return Json(new { success = true, message = "Delete Data Successfully" });
        }
        #endregion
    }
}
