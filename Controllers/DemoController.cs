using LINQ2DB_MVC_Core_5.Auth.DB;
using LINQ2DB_MVC_Core_5.DB;
using LinqToDB;
using LinqToDB.Async;
using LinqToDB.Data;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Threading.Tasks;

namespace LINQ2DB_MVC_Core_5.Controllers
{
    public class DemoController : Controller
    {
        private LinqDB _db;

        public DemoController(DataConnection db)
        {
            _db = (LinqDB)db;
        }

        public async Task<IActionResult> Index()
        {
            // return the current user count.
            var result = await _db.GetTable<AspNetUsers>().Select(u => u.Email).ToListAsync();
            return Ok(result);
        }

        protected override void Dispose(bool disposing)
        {
            _db?.Dispose();
            if (_db != null)
            {
                _db = null;
            }
        }
    }
}