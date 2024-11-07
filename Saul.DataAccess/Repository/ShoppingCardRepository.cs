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
    public class ShoppingCardRepository : Repository<ShoppingCard>, IShoppingCardRepository
    {
        private readonly ApplicationDbContext _db;
        public ShoppingCardRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

        public void update(ShoppingCard obj)
        {
            _db.ShoppingCards.Update(obj); 
        }
    }
}
