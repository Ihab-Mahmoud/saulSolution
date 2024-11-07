﻿using Saul.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Saul.DataAccess.Repository.IRepository
{
    public interface IOrderHeaderRepository : IRepository<OrderHeader> 
    {
     
        void update(OrderHeader obj);
        void updateStatus(int id, string orderStatus,string? paymentStatus=null);

        void updatePaymentId(int id, string paymentIntentId, string sessionId);

    }
}
