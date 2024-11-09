using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Saul.DataAccess.Repository.IRepository;
using Saul.Models;
using Saul.Models.ViewModels;
using Saul.Utility;
using Stripe;
using Stripe.Checkout;
using Stripe.Climate;
using Stripe.V2;
using System.Security.Claims;

namespace Saul.Areas.Admin.Controllers
{
    [Authorize]
    [Area("Admin")]
    public class OrderController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        public OrderController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public IActionResult Index()
        {
            return View();
        }


        public IActionResult Details(int OrderId)
        {
            OrderHeader orderHeader = _unitOfWork.orderHeader.Get(u => u.Id == OrderId, includeProperties: "ApplicationUser");
            IEnumerable<OrderDetail> orderDetails = _unitOfWork.orderDetail.GetAll(u => u.OrderHeaderId == OrderId, includeProperties: "Product");

            OrderVM orderVM = new OrderVM()
            {
                OrderHeader = orderHeader,
                OrderDetail = orderDetails,

            };

            return View(orderVM);
        }

        [Authorize(Roles = SD.Role_Employee + "," + SD.Role_Admin)]
        [HttpPost]
        public IActionResult UpdateOrderDetails(OrderVM orderVM)
        {

            OrderHeader orderHeader = _unitOfWork.orderHeader.Get(u => u.Id == orderVM.OrderHeader.Id, includeProperties: "ApplicationUser");
            orderHeader.Name = orderVM.OrderHeader.Name;
            orderHeader.StreetAddress = orderVM.OrderHeader.StreetAddress;
            orderHeader.PhoneNumber = orderVM.OrderHeader.PhoneNumber;
            orderHeader.City = orderVM.OrderHeader.City;
            orderHeader.PostalCode = orderVM.OrderHeader.PostalCode;
            orderHeader.Region = orderVM.OrderHeader.Region;

            if (string.IsNullOrEmpty(orderVM.OrderHeader.Carrier))
            {
                orderHeader.Carrier = orderVM.OrderHeader.Carrier;
            }
            if (string.IsNullOrEmpty(orderVM.OrderHeader.TrackingNumber))
            {
                orderHeader.TrackingNumber = orderVM.OrderHeader.TrackingNumber;
            }

            TempData["success"] = "order pick up information has been updated successfully!";

            _unitOfWork.orderHeader.update(orderHeader);
            _unitOfWork.save();

            return RedirectToAction(nameof(Details), new { OrderId = orderVM.OrderHeader.Id });
        }

        [Authorize(Roles = SD.Role_Employee + "," + SD.Role_Admin)]
        [HttpPost]
        public IActionResult StartProcessing(OrderVM orderVM)
        {
            _unitOfWork.orderHeader.updateStatus(orderVM.OrderHeader.Id, SD.Status_InProcess);
            _unitOfWork.save();

            TempData["success"] = "order has been set to process successfully!";

            return RedirectToAction(nameof(Details), new { OrderId = orderVM.OrderHeader.Id });

        }

        [Authorize(Roles = SD.Role_Employee + "," + SD.Role_Admin)]
        [HttpPost]
        public IActionResult StartShipping(OrderVM orderVM)
        {
            OrderHeader orderHeader = _unitOfWork.orderHeader.Get(u => u.Id == orderVM.OrderHeader.Id, includeProperties: "ApplicationUser");

            orderHeader.OrderStatus = SD.Status_Shipped;
            orderHeader.TrackingNumber = orderVM.OrderHeader.TrackingNumber;
            orderHeader.Carrier = orderVM.OrderHeader.Carrier;
            orderHeader.ShippingDate = DateTime.Now;
            if (orderHeader.PaymentStatus == SD.Payment_Status_DelayedPayment)
            {
                orderHeader.PaymentDueDate = DateOnly.FromDateTime(DateTime.Now.AddDays(30));
            }
            _unitOfWork.orderHeader.update(orderHeader);
            _unitOfWork.save();

            TempData["success"] = "order has been set to be shipped successfully!";

            return RedirectToAction(nameof(Details), new { OrderId = orderVM.OrderHeader.Id });

        }

        [HttpPost]
        public IActionResult DelayedPayment(OrderVM orderVM)
        {
            OrderHeader orderHeader = _unitOfWork.orderHeader.Get(u => u.Id == orderVM.OrderHeader.Id, includeProperties: "ApplicationUser");
            IEnumerable<OrderDetail> orderDetails = _unitOfWork.orderDetail.GetAll(u => u.OrderHeaderId == orderVM.OrderHeader.Id, includeProperties: "Product");

            var domain = $"{Request.Scheme}://{Request.Host}/";

            var options = new Stripe.Checkout.SessionCreateOptions
            {
                SuccessUrl = domain + $"admin/order/PaymentConfirmation?OrderId={orderVM.OrderHeader.Id}",
                CancelUrl = domain + $"admin/order/details?orderId={orderVM.OrderHeader.Id}",
                LineItems = new List<Stripe.Checkout.SessionLineItemOptions>(),
                Mode = "payment",
            };

            foreach (var item in orderDetails)
            {
                var sessionILineItem = new SessionLineItemOptions()
                {
                    PriceData = new SessionLineItemPriceDataOptions()
                    {
                        Currency = "usd",
                        UnitAmount = (long)item.Price * 100,
                        ProductData = new SessionLineItemPriceDataProductDataOptions()
                        {
                            Name = item.Product.Title.ToString(),
                        }
                    },
                    Quantity = (long)item.Count
                };
                options.LineItems.Add(sessionILineItem);
            }

            var service = new Stripe.Checkout.SessionService();
            Session session = service.Create(options);
            _unitOfWork.orderHeader.updatePaymentId(orderVM.OrderHeader.Id, session.PaymentIntentId, session.Id);
            _unitOfWork.save();
            Response.Headers.Append("Location", session.Url);
            return new StatusCodeResult(303);

        }


        public IActionResult PaymentConfirmation(int orderId)
        {
            OrderDetail orderDetail = _unitOfWork.orderDetail.Get(u => u.OrderHeaderId == orderId, includeProperties: "Product");
            OrderHeader orderHeader = _unitOfWork.orderHeader.Get(u => u.Id == orderId);

            if (orderHeader.PaymentStatus == SD.Payment_Status_DelayedPayment)
            {
                // regular customer
                var service = new Stripe.Checkout.SessionService();
                Session session = service.Get(orderHeader.SessionId);

                if (session.PaymentStatus.ToLower() == "paid")
                {

                    _unitOfWork.orderHeader.updateStatus(orderId, orderHeader.OrderStatus, SD.Payment_Status_Approved);
                    _unitOfWork.orderHeader.updatePaymentId(orderId, session.PaymentIntentId, session.Id);
                    _unitOfWork.save();

                    TempData["success"] = "payment has been confirmed!";
                }


            }
            return View(orderDetail);
        }


        [HttpPost]
        public IActionResult CancelOrder(OrderVM orderVM)
        {
            OrderHeader orderHeader = _unitOfWork.orderHeader.Get(u => u.Id == orderVM.OrderHeader.Id, includeProperties: "ApplicationUser");
            IEnumerable<OrderDetail> orderDetails = _unitOfWork.orderDetail.GetAll(u => u.OrderHeaderId == orderVM.OrderHeader.Id, includeProperties: "Product");

            if (orderHeader.PaymentStatus == SD.Payment_Status_Approved)
            {
                var refundService = new RefundService();
                // Create refund options
                var options = new RefundCreateOptions
                {
                    Reason=RefundReasons.RequestedByCustomer,
                    PaymentIntent = orderHeader.PaymentIntentId,
                };
                Refund refund = refundService.Create(options);
                TempData["success"] = " Payment has been refunded successfully!";
                _unitOfWork.orderHeader.updateStatus(orderVM.OrderHeader.Id,SD.Status_Cancelled,SD.Status_Refunded);
            }
            else
            {
                TempData["success"] = " Order has been  successfully!";
                _unitOfWork.orderHeader.updateStatus(orderVM.OrderHeader.Id, SD.Status_Cancelled, SD.Status_Refunded);
            }
                _unitOfWork.save();

            return RedirectToAction(nameof(Details), new { OrderId = orderVM.OrderHeader.Id });




        }

        #region API CALLS
        [HttpGet]
        public IActionResult GetAll(string orderStatus)
        {

            var caimsIdentity = (ClaimsIdentity)User.Identity;
            var userId = caimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

            IEnumerable<OrderHeader> orderHeaders;

            if (User.IsInRole(SD.Role_Admin) || User.IsInRole(SD.Role_Employee))
            {
                orderHeaders = _unitOfWork.orderHeader.GetAll(includeProperties: "ApplicationUser");
            }
            else
            {
                orderHeaders = _unitOfWork.orderHeader.GetAll( u=>u.ApplicationUserId== userId, includeProperties: "ApplicationUser");
            }

            switch (orderStatus)
            {
                case "inprocess":
                    orderHeaders = orderHeaders.Where(u => u.OrderStatus == SD.Status_InProcess).ToList();
                    break;
                case "pending":
                    orderHeaders = orderHeaders.Where(u => u.PaymentStatus == SD.Payment_Status_DelayedPayment).ToList();
                    break;
                case "completed":
                    orderHeaders = orderHeaders.Where(u => u.OrderStatus == SD.Status_Shipped).ToList();
                    break;
                case "approved":
                    orderHeaders = orderHeaders.Where(u => u.OrderStatus == SD.Status_Approved).ToList();
                    break;
                default:
                    break;
            }

            return Json(new { data = orderHeaders });
        }

        #endregion
    }
}
