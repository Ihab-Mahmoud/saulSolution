using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Saul.Models.ViewModels;
using Saul.Models;
using System.Security.Claims;
using Saul.DataAccess.Repository;
using Saul.DataAccess.Repository.IRepository;
using Saul.Utility;
using Stripe.Checkout;
using Microsoft.IdentityModel.Tokens;

namespace Saul.Areas.Customer.Controllers
{
    [Authorize]
    [Area("Customer")]
    public class CardController : Controller
    {

        private readonly IUnitOfWork _unitOfWork;

        public CardController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public IActionResult Index()
        {
            var caimsIdentity = (ClaimsIdentity)User.Identity;
            var userId = caimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

            IEnumerable<ShoppingCard> shoppingCards = _unitOfWork.shoppingCard.GetAll(u => u.UserId == userId, includeProperties: "Product,ApplicationUser");
            double? priceTotal = 0;

            foreach (var item in shoppingCards)
            {
                priceTotal += SpecifyPriceRange(item) * item.Count;
                item.Price = SpecifyPriceRange(item);
            }

            ShoppingVM shoppingVM = new()
            {
                ShoppingCards = shoppingCards,
                OrderHeader=new()
            };
            shoppingVM.OrderHeader.OrderTotal = priceTotal;
            return View(shoppingVM);
        }

        public IActionResult Summary()
        {
            var caimsIdentity = (ClaimsIdentity)User.Identity;
            var userId = caimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

            IEnumerable<ShoppingCard> shoppingCards = _unitOfWork.shoppingCard.GetAll(u => u.UserId == userId, includeProperties: "Product,ApplicationUser");

            double? priceTotal = 0;

            foreach (var item in shoppingCards)
            {
                priceTotal += SpecifyPriceRange(item) * item.Count;
                item.Price = SpecifyPriceRange(item);
            }

            ShoppingVM shoppingVM = new()
            {
                ShoppingCards = shoppingCards,
                OrderHeader = new()
            };
            shoppingVM.OrderHeader.OrderTotal = priceTotal;

            ShoppingCard shoppingCard = _unitOfWork.shoppingCard.Get(u => u.UserId == userId, includeProperties: "Product,ApplicationUser");

            shoppingVM.OrderHeader.City = shoppingCard.ApplicationUser.City;
            shoppingVM.OrderHeader.PostalCode=shoppingCard.ApplicationUser.PostalCode;
            shoppingVM.OrderHeader.Region=shoppingCard.ApplicationUser.Region;
            shoppingVM.OrderHeader.StreetAddress=shoppingCard.ApplicationUser.StreetAddress;
            shoppingVM.OrderHeader.Name = shoppingCard.ApplicationUser.Name;
            shoppingVM.OrderHeader.PhoneNumber = shoppingCard.ApplicationUser.PhoneNumber;

            return View(shoppingVM);
        }

        [HttpPost]
        public IActionResult Summary(ShoppingVM obj)
        {
            var caimsIdentity = (ClaimsIdentity)User.Identity;
            var userId = caimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

            IEnumerable<ShoppingCard> shoppingCards = _unitOfWork.shoppingCard.GetAll(u => u.UserId == userId, includeProperties: "Product,ApplicationUser");

            double? priceTotal = 0;

           
            ShoppingCard shoppingCard = _unitOfWork.shoppingCard.Get(u => u.UserId == userId, includeProperties: "Product,ApplicationUser");

            obj.OrderHeader.OrderDate = DateTime.Now;
            obj.OrderHeader.ApplicationUserId = userId;
            ApplicationUser applicationUser = shoppingCard.ApplicationUser;

            foreach (var item in shoppingCards)
            {
                priceTotal += SpecifyPriceRange(item) * item.Count;
                item.Price = SpecifyPriceRange(item);
            }
            obj.OrderHeader.OrderTotal= priceTotal;

            if (applicationUser.CompanyId.GetValueOrDefault()==0)
            {
                //regular customer
                obj.OrderHeader.OrderStatus = SD.Status_Pending;
                obj.OrderHeader.PaymentStatus = SD.Payment_Status_Pending;

            }
            else
            {
                //company customer
                obj.OrderHeader.OrderStatus = SD.Status_Approved;
                obj.OrderHeader.PaymentStatus = SD.Payment_Status_DelayedPayment;
            }
            _unitOfWork.orderHeader.Add(obj.OrderHeader);
            _unitOfWork.save();

            
            foreach (var item in shoppingCards)
            {
                OrderDetail orderDetail = new ()
                {
                    OrderHeaderId = obj.OrderHeader.Id,
                    ProductId = item.ProductId,
                    Count = (int)item.Count,
                    Price= item.Price,
                };
                _unitOfWork.orderDetail.Add(orderDetail);
                _unitOfWork.save();
            }


            if (applicationUser.CompanyId.GetValueOrDefault() == 0)
            {
                //regular customer
                string domain = "https://localhost:7000/";
                var options = new Stripe.Checkout.SessionCreateOptions
                {
                    SuccessUrl = domain + $"customer/card/OrderConfirmation?id={obj.OrderHeader.Id}",
                    CancelUrl = domain + $"customer/card/index",
                    LineItems = new List<Stripe.Checkout.SessionLineItemOptions>(),
                    Mode = "payment",
                };

                foreach (var item in shoppingCards)
                {
                    var sessionILineItem = new SessionLineItemOptions()
                    {
                        PriceData = new SessionLineItemPriceDataOptions()
                        {
                            Currency = "usd",
                            UnitAmount = (long)item.Price * 100,
                            ProductData = new SessionLineItemPriceDataProductDataOptions()
                            {
                                Name=item.Product.Title.ToString(),
                            }
                        },
                        Quantity =(long) item.Count
                    };
                    options.LineItems.Add(sessionILineItem);
                }

                var service = new Stripe.Checkout.SessionService();
                Session session= service.Create(options);
                _unitOfWork.orderHeader.updatePaymentId(obj.OrderHeader.Id,session.PaymentIntentId,session.Id);
                _unitOfWork.save();    
                Response.Headers.Append("Location", session.Url);
                return new StatusCodeResult(303);
            }
            ;

            HttpContext.Session.Clear();
            return RedirectToAction(nameof(OrderConfirmation),new {id=obj.OrderHeader.Id});
        }

        public IActionResult OrderConfirmation(int id)
        {
            OrderDetail orderDetail = _unitOfWork.orderDetail.Get(u=>u.OrderHeaderId == id,includeProperties:"Product");
            OrderHeader orderHeader = _unitOfWork.orderHeader.Get(u => u.Id == id);

            if (orderHeader.PaymentStatus != SD.Payment_Status_DelayedPayment)
            {
                // regular customer
                var service = new Stripe.Checkout.SessionService();
                Session session = service.Get(orderHeader.SessionId);

                if (session.PaymentStatus.ToLower() == "paid")
                {

                    _unitOfWork.orderHeader.updateStatus(id, SD.Status_Approved, SD.Payment_Status_Approved);
                    _unitOfWork.orderHeader.updatePaymentId(id, session.PaymentIntentId, session.Id);
                    _unitOfWork.save();

                    TempData["success"] = "order has been confirmed!";

                    List<ShoppingCard> shoppingCards = _unitOfWork.shoppingCard.GetAll(u => u.UserId == orderHeader.ApplicationUserId).ToList();
                    _unitOfWork.shoppingCard.RemoveRange(shoppingCards);
                    _unitOfWork.save();
                }

            }
            else
            {

            TempData["success"] = "order has been confirmed!";
            List<ShoppingCard> shoppingCards = _unitOfWork.shoppingCard.GetAll(u => u.UserId == orderHeader.ApplicationUserId).ToList();
            _unitOfWork.shoppingCard.RemoveRange(shoppingCards);
            _unitOfWork.save();
            }
            return View(orderDetail);
        }

        public IActionResult Plus(int CardId)
        {
            ShoppingCard shoppingCard = _unitOfWork.shoppingCard.Get(u => u.Id == CardId, includeProperties: "Product,ApplicationUser");

            shoppingCard.Count ++;
            _unitOfWork.shoppingCard.update(shoppingCard);
            _unitOfWork.save();
            return RedirectToAction("Index");
        }
        public IActionResult Minus(int CardId)
        {
            ShoppingCard shoppingCard = _unitOfWork.shoppingCard.Get(u => u.Id == CardId, includeProperties: "Product,ApplicationUser");
            if (shoppingCard.Count <=1 )
            {
                _unitOfWork.shoppingCard.Remove(shoppingCard);
            }
            else
            {
                shoppingCard.Count--;
                _unitOfWork.shoppingCard.update(shoppingCard);
            }
            _unitOfWork.save();
            return RedirectToAction("Index");
        }
        public IActionResult Remove(int CardId)
        {
            ShoppingCard shoppingCard = _unitOfWork.shoppingCard.Get(u => u.Id == CardId, includeProperties: "Product,ApplicationUser");
            _unitOfWork.shoppingCard.Remove(shoppingCard);
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
    }
}
