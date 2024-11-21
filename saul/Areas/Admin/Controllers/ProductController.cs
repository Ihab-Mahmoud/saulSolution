using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
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
        private  IWebHostEnvironment _webHostEnvironment;
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
                
                    productVM.Product.ProductImages = new List<ProductImage>() { };
                
                return View(productVM);
            }
            else
            {
                // update
                Product product = _unitOfWork.product.Get(u => u.Id == id, includeProperties: "Category,ProductImages");
                if (product == null)
                {
                    return NotFound();
                }
                if (product.ProductImages == null)
                {
                    product.ProductImages = new List<ProductImage>() { };
                }
                productVM.Product = product;
                return View(productVM);

            }

        }



        [HttpPost]
        public IActionResult Upsert(ProductVM obj, List<IFormFile>? formFiles)
        {
            if (ModelState.IsValid && obj != null)
            {
                string wwwRootPath = _webHostEnvironment.WebRootPath;

                if (obj.Product.Id != 0)
                {
                    _unitOfWork.product.update(obj.Product);
                }
                else
                {
                    _unitOfWork.product.Add(obj.Product);
                }

                _unitOfWork.save();
                if (formFiles != null)
                {
                  
                    foreach (var formFile in formFiles)
                    {
                        string fileName = Guid.NewGuid().ToString() + Path.GetExtension(formFile.FileName);
                        string productPath = @"images\product\product-" + obj.Product.Id;
                        string finalPath = Path.Combine(wwwRootPath, productPath);

                        if (!Directory.Exists(finalPath))
                        {
                            Directory.CreateDirectory(finalPath);
                        }

                        using (var fileStream = new FileStream(Path.Combine(finalPath, fileName), FileMode.Create))
                        {
                            formFile.CopyTo(fileStream);
                        }

                        string imagePath = @"\" +productPath + @"\" + fileName;

                        ProductImage image = new ProductImage()
                        {
                            ImageURL = imagePath,
                            ProductId = obj.Product.Id,
                        };

                        if (obj.Product.ProductImages==null)
                        {
                            obj.Product.ProductImages = new List<ProductImage>() { };
                        }


                        obj.Product.ProductImages.Add(image) ;
                    }

                    _unitOfWork.product.update(obj.Product);
                    _unitOfWork.save();
                }


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

        public IActionResult DeleteImage(int imageId)
        {

            ProductImage productImage = _unitOfWork.productImage.Get(u=>u.Id==imageId);

            var oldImagePath = Path.Combine(_webHostEnvironment.WebRootPath, productImage.ImageURL.TrimStart('\\'));
            if (System.IO.File.Exists(oldImagePath))
            {
                System.IO.File.Delete(oldImagePath);
            }

            _unitOfWork.productImage.Remove(productImage);
            _unitOfWork.save();


            return RedirectToAction(nameof(Upsert), new { id = productImage.ProductId });

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
            Product product = _unitOfWork.product.Get(u => u.Id == id, includeProperties: "Category,ProductImages");
            if (product == null)
            {
                return Json(new { success = "true", message = "Error While Deleting" });
            }
            string productPath = @"images\product\product-" + product.Id;
            string finalPath = Path.Combine(_webHostEnvironment.WebRootPath, productPath);
          
            if (Directory.Exists(finalPath))
            {
                string[] filePaths = Directory.GetFiles(finalPath);
                foreach (string filePath in filePaths) { 
                System.IO.File.Delete(filePath);
                }

                Directory.Delete(finalPath);
            }


            _unitOfWork.product.Remove(product);
            _unitOfWork.save();
            return Json(new { success = "true", message = "Deleted Successfully" });
        }
        #endregion

    }
}
