using System.Diagnostics;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Saul.DataAccess.Repository.IRepository;
using Saul.Models;
using Saul.Models.ViewModels;
using Saul.Utility;
namespace Saul.Areas.Customer.Controllers
{
    [Area("Customer")]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private IUnitOfWork _unitOfWork;

        public HomeController(ILogger<HomeController> logger, IUnitOfWork unitOfWork)
        {
            _logger = logger;
            _unitOfWork = unitOfWork;
        }

        public IActionResult Index()
        {

            IEnumerable<Product> products = _unitOfWork.product.GetAll(includeProperties:"Category,ProductImages");
            foreach (var item in products)
            {
                if (item.ProductImages==null)
                {
                    item.ProductImages = new List<ProductImage>();
                }
            }
            return View(products);
        }
        
        public IActionResult Details(int ProductId)
        {
           
            Product product = _unitOfWork.product.Get(u=>u.Id == ProductId, includeProperties: "Category,ProductImages");
            if (product == null)
            {
                return NotFound();
            }

            ShoppingCard shopping = new()
            {
                Product = product,
                ProductId= ProductId,
                Count =1
                
            };

            if (shopping.Product.ProductImages == null)
            {
                shopping.Product.ProductImages = new List<ProductImage>();
            }

            return View(shopping);
        }

        [HttpPost]
        [Authorize]
        public IActionResult Details(ShoppingCard obj)
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var usedId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

            if (usedId == null)
            {
                return NotFound();
            }

            obj.UserId = usedId;
            ShoppingCard shoppingCard = _unitOfWork.shoppingCard.Get(u => u.ProductId == obj.ProductId && u.UserId == usedId);

            if (shoppingCard == null)
            {
                _unitOfWork.shoppingCard.Add(obj);
            }
            else
            {
                shoppingCard.Count += obj.Count;
                _unitOfWork.shoppingCard.update(shoppingCard);
            }
            _unitOfWork.save();
            return RedirectToAction("Index");
        }
        private double? SpecifyPriceRange(ShoppingCard shoppingCard)
        {

            if (shoppingCard.Count <= 50)
            {
                return shoppingCard.Product?.Price;
            }
            else
            {
                if (shoppingCard.Count <= 100)
                {
                    return shoppingCard.Product?.Price50;
                }
                else
                {
                    return shoppingCard.Product?.Price100;
                }
            }

        }


        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
