using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Saul.DataAccess.Repository.IRepository;
using Saul.Models;
using Saul.Models.ViewModels;
using Saul.Utility;
using System.Collections;

namespace Saul.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin)]

    public class CompanyController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;


        public CompanyController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }


        public IActionResult Index()
        {   
            IEnumerable<Company> companies =_unitOfWork.company.GetAll() ;
            return View(companies);
        }


        public IActionResult Upsert(int? id) {
            if (id == null)
            {
                // create
                Company company=new Company();
                return View(company);
            }
            else
            {
                // update
                Company company = _unitOfWork.company.Get(u => u.Id == id);
                if (company == null)
                {
                    return NotFound();
                }
                return View(company);

            }
        }



        [HttpPost]
        public IActionResult Upsert(Company obj)
        {
            if (ModelState.IsValid && obj != null)
            {
                if (obj.Id != 0)
                {
                    _unitOfWork.company.update(obj);
                }
                else
                {

                    _unitOfWork.company.Add(obj);
                }

                _unitOfWork.save();
                TempData["success"] = "Company has been added successfully!";
                return RedirectToAction("Index");
            }
            else
            {
                return View(obj);
            }

        }


        #region API CALLS

        [HttpGet]
        public IActionResult GetAll()
        {
            List<Company> companies = _unitOfWork.company.GetAll().ToList();
            return Json(new { data = companies });
        }

        [HttpDelete]
        public IActionResult Delete(int id)
        {
            Company company = _unitOfWork.company.Get(u => u.Id == id);
            if (company == null)
            {
                return Json(new { success = "true", message = "Error While Deleting" });
            }
           
            _unitOfWork.company.Remove(company);
            _unitOfWork.save();
            return Json(new { success = "true", message = "Deleted Successfully" });
        }
        #endregion
    }
}
