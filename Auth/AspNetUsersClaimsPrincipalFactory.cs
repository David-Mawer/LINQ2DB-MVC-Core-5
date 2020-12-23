using LinqToDB;
using LinqToDB.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using LINQ2DB_MVC_Core_5.Auth.DB;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace LINQ2DB_MVC_Core_5.Auth
{
    public class AspNetUsersClaimsPrincipalFactory : UserClaimsPrincipalFactory<AspNetUsers>, IDisposable
    {
        private DataConnection db;

        public AspNetUsersClaimsPrincipalFactory(UserManager<AspNetUsers> userManager
            , IOptions<IdentityOptions> optionsAccessor
            ) : base(userManager, optionsAccessor)
        {
            db = new DataConnection();
        }

        public void Dispose()
        {
            db.Dispose();
        }

        protected override async Task<ClaimsIdentity> GenerateClaimsAsync(AspNetUsers user)
        {
            var result = await base.GenerateClaimsAsync(user);

            var roleClaims = await (
                from ur in db.GetTable<AspNetUserRoles>()
                join rc in db.GetTable<AspNetRoleClaims>()
                on ur.RoleId equals rc.RoleId
                where ur.UserId == user.Id
                select rc.ToClaim()
                ).ToListAsync();

            if ((roleClaims != null) && (roleClaims.Count > 0))
            {
                result.AddClaims(roleClaims);
            }

            return result;
        }
    }
}
