
using Microsoft.AspNetCore.Mvc;
using Saul.DataAccess.Repository.IRepository;
using Saul.Models;
using Saul.Utility;
using System.Security.Claims;

namespace Saul.ViewComponents
{
    public class ShoppingCartViewComponent :ViewComponent
    {
        private readonly IUnitOfWork _unitOfWork;

        public ShoppingCartViewComponent(IUnitOfWork unitOfWork )
        {
            _unitOfWork = unitOfWork;
        }


        public async Task<IViewComponentResult> InvokeAsync()
        {
            var caimsIdentity = (ClaimsIdentity)User.Identity;
            var userId = caimsIdentity.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (userId ==null)
            {
                
                HttpContext.Session.Clear();
            }
            else
            {
               
                IEnumerable<ShoppingCard> shoppingCards = _unitOfWork.shoppingCard.GetAll(u => u.UserId == userId, includeProperties: "Product,ApplicationUser");
                HttpContext.Session.SetInt32(SD.ShoppingCart, shoppingCards.Count());
            }
            return View(HttpContext.Session.GetInt32(SD.ShoppingCart));
        }
    }
}
