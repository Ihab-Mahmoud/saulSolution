﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Saul.DataAccess.Repository.IRepository
{
    public interface IUnitOfWork
    {
        ICategoryRepository category { get; }
        IProductRepository product { get; }
        ICompanyRepository company { get; }
        IShoppingCardRepository shoppingCard { get; }
        IOrderHeaderRepository orderHeader { get; }
        IOrderDetailRepository orderDetail { get; }
        IProductImageRepository productImage { get; }
        IApplicationUserRepository applicationUser { get; }
        void save();
    }
}
