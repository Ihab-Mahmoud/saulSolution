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
    public class CompanyRepository : Repository<Company>,ICompanyRepository
    {
        private readonly ApplicationDbContext _db;

        public CompanyRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

        public void update(Company obj)
        {
            _db.Companies.Update(obj);
        }
    }
}
