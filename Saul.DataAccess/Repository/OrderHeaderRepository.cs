using Saul.DataAccess.Data;
using Saul.DataAccess.Repository.IRepository;
using Saul.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Saul.DataAccess.Repository
{
    public class OrderHeaderRepository: Repository<OrderHeader>, IOrderHeaderRepository
    {
        private readonly ApplicationDbContext _db;
        public OrderHeaderRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

        public void update(OrderHeader obj)
        {
            _db.OrderHeaders.Update(obj);
        }

        public void updatePaymentId(int id, string paymentIntentId, string sessionId)
        {
            OrderHeader orderHeader = _db.OrderHeaders.Find(id);
            if (!string.IsNullOrEmpty(paymentIntentId))
            {
                orderHeader.PaymentIntentId = paymentIntentId;
                orderHeader.PaymentDate = DateTime.Now;
            }
            orderHeader.SessionId = sessionId;
        }

        public void updateStatus(int id, string orderStatus, string? paymentStatus = null)
        {
            OrderHeader orderHeader = _db.OrderHeaders.Find(id);
                if (!string.IsNullOrEmpty(paymentStatus))
                    {
                        orderHeader.PaymentStatus= paymentStatus;
                    }
                    orderHeader.OrderStatus = orderStatus;
        }
    }
}
