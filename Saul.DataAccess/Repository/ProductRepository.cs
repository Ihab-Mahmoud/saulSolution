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
    public class ProductRepository : Repository<Product>, IProductRepository
    {
        private readonly ApplicationDbContext _db;

        public ProductRepository(ApplicationDbContext db) : base(db) 
        {
            _db = db;
        }

        public void update(Product obj)
        {
             //_db.Products.Update(obj);
            Product product= _db.Products.Find(obj.Id);
            if (product != null)
            {
                product.Author = obj.Author;
                product.ISBN = obj.ISBN;
                product.Description = obj.Description;
                product.Category = obj.Category;
                product.CategoryId = obj.CategoryId;
                product.ListPrice = obj.ListPrice;
                product.Price = obj.Price;
                product.Price50 = obj.Price50; 
                product.Price100 = obj.Price100;
                product.Title = obj.Title;
                if (obj.ImageURL != null)
                {
                    product.ImageURL = obj.ImageURL;
                }
            }

        }
    }
}
