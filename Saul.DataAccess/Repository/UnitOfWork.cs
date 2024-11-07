using Saul.DataAccess.Data;
using Saul.DataAccess.Repository.IRepository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Saul.DataAccess.Repository
{
    public class UnitOfWork : IUnitOfWork
    {

        private readonly ApplicationDbContext _db;
        public ICategoryRepository category { get; private set; }
        public IProductRepository product { get; private set; }
        public ICompanyRepository company { get; private set; }
        public IShoppingCardRepository shoppingCard { get; private set; }

        public IOrderHeaderRepository orderHeader { get; private set; }

        public IOrderDetailRepository orderDetail { get; private set; }

        public UnitOfWork (ApplicationDbContext db)
        {
            _db = db;
            category = new CategoryRepository(db);
            product = new ProductRepository(db);
            company = new CompanyRepository(db);
            shoppingCard = new ShoppingCardRepository(db);
            orderHeader = new OrderHeaderRepository(db);
            orderDetail = new OrderDetailRepository(db);

        }


        public void save()
        {
            _db.SaveChanges();
        }
    }
}
