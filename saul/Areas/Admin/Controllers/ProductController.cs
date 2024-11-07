using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Saul.DataAccess.Repository.IRepository;
using Saul.Models;
using Saul.Models.ViewModels;
using Saul.Utility;

namespace Saul.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin)]
    public class ProductController : Controller
    {
        private IUnitOfWork _unitOfWork;
        private readonly IWebHostEnvironment _webHostEnvironment;
        public ProductController(IUnitOfWork unitOfWork, IWebHostEnvironment webHostEnvironment)
        {
            _unitOfWork = unitOfWork;
            _webHostEnvironment = webHostEnvironment;
        }

        public IActionResult Index()
        {
            List<Product> products = [.. _unitOfWork.product.GetAll(includeProperties: "Category")];
            return View(products);
        }



        public IActionResult Upsert(int? id)
        {

            ProductVM productVM = new()
            {
                CategoryList = _unitOfWork.category.GetAll().Select(u => new SelectListItem
                {
                    Text = u.Name,
                    Value = u.Id.ToString()
                }),
                Product = new Product()
            };

            if (id == null)

            {
                // create

                return View(productVM);
            }
            else
            {
                // update
                Product product = _unitOfWork.product.Get(u => u.Id == id, includeProperties: "Category");
                if (product == null)
                {
                    return NotFound();
                }
                productVM.Product = product;
                return View(productVM);

            }

        }



        [HttpPost]
        public IActionResult Upsert(ProductVM obj, IFormFile? formFile)
        {
            if (ModelState.IsValid && obj != null)
            {
                string wwwRootPath = _webHostEnvironment.WebRootPath;

                if (formFile != null)
                {
                    if (!string.IsNullOrEmpty(obj?.Product?.ImageURL))
                    {
                        var oldImagePath = Path.Combine(wwwRootPath, obj?.Product?.ImageURL.TrimStart('\\'));
                        if (System.IO.File.Exists(oldImagePath))
                        {
                            System.IO.File.Delete(oldImagePath);
                        }
                    }


                    string fileName = Guid.NewGuid().ToString() + Path.GetExtension(formFile.FileName);
                    string productPath = Path.Combine(wwwRootPath, @"images\product");

                    using (var fileStream = new FileStream(Path.Combine(productPath, fileName), FileMode.Create))
                    {
                        formFile.CopyTo(fileStream);
                    }
                    obj.Product.ImageURL = @"\images\product\" + fileName;


                }


                if (obj.Product.Id != 0)
                {
                    _unitOfWork.product.update(obj.Product);
                }
                else
                {
                    _unitOfWork.product.Add(obj.Product);
                }

                _unitOfWork.save();
                TempData["success"] = "Product has been added successfully!";
                return RedirectToAction("Index");
            }
            else
            {

                obj.CategoryList = _unitOfWork.category.GetAll().Select(u => new SelectListItem
                {
                    Text = u.Name,
                    Value = u.Id.ToString()
                });

                return View(obj);

            }

        }

        #region API CALLS
        [HttpGet]
        public IActionResult GetAll()
        {
            List<Product> products = _unitOfWork.product.GetAll(includeProperties: "Category").ToList();
            return Json(new { data = products });
        }

        [HttpDelete]
        public IActionResult Delete(int id)
        {
            Product product = _unitOfWork.product.Get(u => u.Id == id, includeProperties: "Category");
            if (product == null)
            {
                return Json(new { success = "true", message = "Error While Deleting" });
            }

            var oldImagePath = Path.Combine(_webHostEnvironment.WebRootPath, product.ImageURL.TrimStart('\\'));
            if (System.IO.File.Exists(oldImagePath))
            {
                System.IO.File.Delete(oldImagePath);
            }
            _unitOfWork.product.Remove(product);
            _unitOfWork.save();
            return Json(new { success = "true", message = "Deleted Successfully" });
        }
        #endregion

    }
}
