using LinqToDB;
using LinqToDB.Data;
using Microsoft.AspNetCore.Identity;
using LINQ2DB_MVC_Core_5.Auth.DB;
using LINQ2DB_MVC_Core_5.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace LINQ2DB_MVC_Core_5.Auth
{

    public class AspNetUsersStore : IUserStore<AspNetUsers>
        , IUserTwoFactorRecoveryCodeStore<AspNetUsers>
        , IUserPasswordStore<AspNetUsers>
        , IUserRoleStore<AspNetUsers>
        , IUserClaimStore<AspNetUsers>
        , IUserEmailStore<AspNetUsers>
        , IUserPhoneNumberStore<AspNetUsers>
        , IUserAuthenticatorKeyStore<AspNetUsers>
        , IUserTwoFactorStore<AspNetUsers>
        , IUserLoginStore<AspNetUsers>
    {
        private bool _disposed = false;
        private DataConnection db;
        // This leans heavily on understanding the Linq2DB Identity code: -
        //      (see: https://github.com/linq2db/LinqToDB.Identity)

        public AspNetUsersStore()
        {
            db = new DataConnection();
        }

        public async Task<IdentityResult> CreateAsync(AspNetUsers user, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();

            await db.InsertAsync<AspNetUsers>(user);

            return IdentityResult.Success;
        }

        public async Task<IdentityResult> DeleteAsync(AspNetUsers user, CancellationToken cancellationToken)
        {
            await db.DeleteAsync(user);

            return IdentityResult.Success;
        }

        public async Task<AspNetUsers> FindByIdAsync(string userId, CancellationToken cancellationToken)
        {
            return await db.GetTable<AspNetUsers>().FirstOrDefaultAsync(_ => _.Id.Equals(userId), cancellationToken);
        }

        public async Task<AspNetUsers> FindByNameAsync(string normalizedUserName, CancellationToken cancellationToken)
        {
            return await db.GetTable<AspNetUsers>()
            .FirstOrDefaultAsync(u => u.NormalizedUserName == normalizedUserName, cancellationToken);
        }

        public Task<string> GetNormalizedUserNameAsync(AspNetUsers user, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();
            if (user == null)
                throw new ArgumentNullException(nameof(user));
            return Task.FromResult(user.NormalizedUserName);
        }

        public Task<string> GetUserIdAsync(AspNetUsers user, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();
            if (user == null)
                throw new ArgumentNullException(nameof(user));
            return Task.FromResult(user.Id);
        }

        public Task<string> GetUserNameAsync(AspNetUsers user, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();
            if (user == null)
                throw new ArgumentNullException(nameof(user));
            return Task.FromResult(user.UserName);
        }

        public Task SetNormalizedUserNameAsync(AspNetUsers user, string normalizedName, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();
            if (user == null)
                throw new ArgumentNullException(nameof(user));
            user.NormalizedUserName = normalizedName;
            return Task.CompletedTask;
        }

        public Task SetUserNameAsync(AspNetUsers user, string userName, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();
            if (user == null)
                throw new ArgumentNullException(nameof(user));
            user.UserName = userName;
            return Task.CompletedTask;
        }

        public async Task<IdentityResult> UpdateAsync(AspNetUsers user, CancellationToken cancellationToken)
        {
            var result = await Task.Run(() => db.UpdateConcurrent<AspNetUsers, string>(user), cancellationToken);
            return result == 1 ? IdentityResult.Success : IdentityResult.Failed();
        }

        // Two Factor Authentication: BEGIN
        private const string InternalLoginProvider = "[AspNetUserStore]";
        private const string RecoveryCodeTokenName = "RecoveryCodes";

        // This leans heavily on understanding the Microsoft DotNet Core Identity code: -
        //      (see: https://github.com/dotnet/aspnetcore/tree/2.1.3)
        public async Task<int> CountCodesAsync(AspNetUsers user, CancellationToken cancellationToken)
        {
            // TwoFactorAuth logic copied from UserBaseStore: -
            //  https://github.com/dotnet/aspnetcore/blob/2.1.3/src/Identity/Extensions.Stores/src/UserStoreBase.cs.
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();

            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }
            var entry = await db
           .GetTable<AspNetUserTokens>()
           .Where(_ => _.UserId.Equals(user.Id) && _.LoginProvider == InternalLoginProvider && _.Name == RecoveryCodeTokenName)
           .FirstOrDefaultAsync(cancellationToken);

            var mergedCodes = entry?.Value ?? "";
            if (mergedCodes.Length > 0)
            {
                return mergedCodes.Split(';').Length;
            }

            return 0;
        }

        public async Task<bool> RedeemCodeAsync(AspNetUsers user, string code, CancellationToken cancellationToken)
        {
            // TwoFactorAuth logic copied from UserBaseStore: -
            //  https://github.com/dotnet/aspnetcore/blob/2.1.3/src/Identity/Extensions.Stores/src/UserStoreBase.cs.
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();

            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }
            if (code == null)
            {
                throw new ArgumentNullException(nameof(code));
            }

            var mergedCodes = await GetTokenAsync(user, InternalLoginProvider, RecoveryCodeTokenName, cancellationToken) ?? "";
            var splitCodes = mergedCodes.Split(';');
            if (splitCodes.Contains(code))
            {
                var updatedCodes = new List<string>(splitCodes.Where(s => s != code));
                await ReplaceCodesAsync(user, updatedCodes, cancellationToken);
                return true;
            }
            return false;
        }

        public async Task ReplaceCodesAsync(AspNetUsers user, IEnumerable<string> recoveryCodes, CancellationToken cancellationToken)
        {
            // TwoFactorAuth logic copied from UserBaseStore: -
            //  https://github.com/dotnet/aspnetcore/blob/2.1.3/src/Identity/Extensions.Stores/src/UserStoreBase.cs.
            var mergedCodes = string.Join(";", recoveryCodes);
            await SetTokenAsync(user, InternalLoginProvider, RecoveryCodeTokenName, mergedCodes, cancellationToken);
            return;
        }

        private async Task<string> GetTokenAsync(AspNetUsers user, string loginProvider, string name, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();

            var entry = await db
            .GetTable<AspNetUserTokens>()
            .Where(_ => _.UserId.Equals(user.Id) && _.LoginProvider == loginProvider && _.Name == name)
            .FirstOrDefaultAsync(cancellationToken);

            return entry?.Value;
        }

        private async Task SetTokenAsync(AspNetUsers user, string loginProvider, string name, string value, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();

            var q = db.GetTable<AspNetUserTokens>()
            .Where(_ => _.UserId.Equals(user.Id) && _.LoginProvider == loginProvider && _.Name == name);

            var token = q.FirstOrDefault();

            if (token == null)
            {
                await db.InsertAsync(new AspNetUserTokens()
                {
                    UserId = user.Id,
                    LoginProvider = loginProvider,
                    Name = name,
                    Value = value
                });
            }
            else
            {
                token.Value = value;
                q.Set(_ => _.Value, value)
                    .Update();
            }
        }

        public Task<string> GetPasswordHashAsync(AspNetUsers user, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();

            if (user == null)
                throw new ArgumentNullException(nameof(user));
            return Task.FromResult(user.PasswordHash);
        }

        public Task<bool> HasPasswordAsync(AspNetUsers user, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();

            if (user == null)
                throw new ArgumentNullException(nameof(user));
            return Task.FromResult(user.PasswordHash.TrimEnd().Length != 0);
        }

        public Task SetPasswordHashAsync(AspNetUsers user, string passwordHash, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();

            if (user == null)
                throw new ArgumentNullException(nameof(user));
            user.PasswordHash = passwordHash;
            return Task.CompletedTask;
        }

        public async Task AddToRoleAsync(AspNetUsers user, string roleName, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();

            var oRole = await db.GetTable<AspNetRoles>()
                    .SingleOrDefaultAsync(r => r.Name == roleName, cancellationToken);
            if (oRole == null)
                throw new InvalidOperationException("Role not found: " + roleName);
            await db.InsertAsync(new AspNetUserRoles()
            {
                UserId = user.Id,
                RoleId = oRole.Id
            });
        }

        public async Task<IList<string>> GetRolesAsync(AspNetUsers user, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();

            var userId = user.Id;
            var query = from userRole in db.GetTable<AspNetUserRoles>()
                        join role in db.GetTable<AspNetRoles>() on userRole.RoleId equals role.Id
                        where userRole.UserId.Equals(userId)
                        select role.Name;

            return await query.ToListAsync(cancellationToken);
        }

        public async Task<IList<AspNetUsers>> GetUsersInRoleAsync(string roleName, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();

            var query = from userrole in db.GetTable<AspNetUserRoles>()
                        join user in db.GetTable<AspNetUsers>() on userrole.UserId equals user.Id
                        join role in db.GetTable<AspNetRoles>() on userrole.RoleId equals role.Id
                        where role.Name == roleName
                        select user;

            return await query.ToListAsync(cancellationToken);
        }

        public async Task<bool> IsInRoleAsync(AspNetUsers user, string roleName, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();

            var q = from ur in db.GetTable<AspNetUserRoles>()
                    join r in db.GetTable<AspNetRoles>() on ur.RoleId equals r.Id
                    where r.Name == roleName && ur.UserId.Equals(user.Id)
                    select ur;

            return await q.AnyAsync(cancellationToken);
        }

        public async Task RemoveFromRoleAsync(AspNetUsers user, string roleName, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();

            var q = from ur in db.GetTable<AspNetUserRoles>()
                    join r in db.GetTable<AspNetRoles>() on ur.RoleId equals r.Id
                    where r.Name == roleName && ur.UserId.Equals(user.Id)
                    select ur;

            await q.DeleteAsync(cancellationToken);
        }

        public async Task AddClaimsAsync(AspNetUsers user, IEnumerable<Claim> claims, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();

            foreach (var claim in claims)
            {
                await db.InsertWithInt64IdentityAsync(new AspNetUserClaims()
                {
                    UserId = user.Id,
                    ClaimType = claim.Type,
                    ClaimValue = claim.Value
                }, token: cancellationToken);
            }
            return;
        }

        public async Task<IList<Claim>> GetClaimsAsync(AspNetUsers user, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();

            return await
                db.GetTable<AspNetUserClaims>()
                    .Where(uc => uc.UserId.Equals(user.Id))
                    .Select(c => c.ToClaim())
                    .ToListAsync(cancellationToken);

        }

        public Task<IList<AspNetUsers>> GetUsersForClaimAsync(Claim claim, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task RemoveClaimsAsync(AspNetUsers user, IEnumerable<Claim> claims, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task ReplaceClaimAsync(AspNetUsers user, Claim claim, Claim newClaim, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            db.Dispose();
            _disposed = true;
        }

        private string[] SplitHash(string psHashedPwd)
        {
            string[] sResult = psHashedPwd.Split("~~");
            return sResult;
        }

        protected void ThrowIfDisposed()
        {
            if (_disposed)
                throw new ObjectDisposedException(GetType().Name);
        }

        public Task SetEmailAsync(AspNetUsers user, string email, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();

            if (user == null)
                throw new ArgumentNullException(nameof(user));
            user.Email = email;
            return Task.CompletedTask;
        }

        public Task<string> GetEmailAsync(AspNetUsers user, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();
            if (user == null)
                throw new ArgumentNullException(nameof(user));
            return Task.FromResult(user.Email);
        }

        public Task<bool> GetEmailConfirmedAsync(AspNetUsers user, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();
            if (user == null)
                throw new ArgumentNullException(nameof(user));
            return Task.FromResult(user.EmailConfirmed);
        }

        public Task SetEmailConfirmedAsync(AspNetUsers user, bool confirmed, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();

            if (user == null)
                throw new ArgumentNullException(nameof(user));
            user.EmailConfirmed = confirmed;
            return Task.CompletedTask;
        }

        public async Task<AspNetUsers> FindByEmailAsync(string normalizedEmail, CancellationToken cancellationToken)
        {
            return await db.GetTable<AspNetUsers>()
            .FirstOrDefaultAsync(u => u.NormalizedEmail == normalizedEmail, cancellationToken);
        }

        public Task<string> GetNormalizedEmailAsync(AspNetUsers user, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();
            if (user == null)
                throw new ArgumentNullException(nameof(user));
            return Task.FromResult(user.NormalizedEmail);
        }

        public Task SetNormalizedEmailAsync(AspNetUsers user, string normalizedEmail, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();

            if (user == null)
                throw new ArgumentNullException(nameof(user));
            user.NormalizedEmail = normalizedEmail;
            return Task.CompletedTask;
        }

        public Task SetPhoneNumberAsync(AspNetUsers user, string phoneNumber, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();

            if (user == null)
                throw new ArgumentNullException(nameof(user));
            user.PhoneNumber = phoneNumber;
            return Task.CompletedTask;
        }

        public Task<string> GetPhoneNumberAsync(AspNetUsers user, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();
            if (user == null)
                throw new ArgumentNullException(nameof(user));
            return Task.FromResult(user.PhoneNumber);
        }

        public Task<bool> GetPhoneNumberConfirmedAsync(AspNetUsers user, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();
            if (user == null)
                throw new ArgumentNullException(nameof(user));
            return Task.FromResult(user.PhoneNumberConfirmed);
        }

        public Task SetPhoneNumberConfirmedAsync(AspNetUsers user, bool confirmed, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();

            if (user == null)
                throw new ArgumentNullException(nameof(user));
            user.PhoneNumberConfirmed = confirmed;
            return Task.CompletedTask;
        }

        public async Task SetAuthenticatorKeyAsync(AspNetUsers user, string key, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();

            await SetTokenAsync(user, InternalLoginProvider, RecoveryCodeTokenName, key, cancellationToken);
        }

        public async Task<string> GetAuthenticatorKeyAsync(AspNetUsers user, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();

            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }
            var entry = await FindTokenAsync(user, cancellationToken);
            return entry?.Value;
        }

        private async Task<AspNetUserTokens> FindTokenAsync(AspNetUsers user, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();

            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            return await db.GetTable<AspNetUserTokens>().FirstOrDefaultAsync(
                t => t.UserId == user.Id 
                && t.LoginProvider == InternalLoginProvider 
                && t.Name == RecoveryCodeTokenName
                );
        }

        public Task SetTwoFactorEnabledAsync(AspNetUsers user, bool enabled, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();

            if (user == null)
                throw new ArgumentNullException(nameof(user));
            user.TwoFactorEnabled = enabled;
            return Task.CompletedTask;
        }

        public Task<bool> GetTwoFactorEnabledAsync(AspNetUsers user, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();
            if (user == null)
                throw new ArgumentNullException(nameof(user));
            return Task.FromResult(user.TwoFactorEnabled);
        }

        public async Task AddLoginAsync(AspNetUsers user, UserLoginInfo login, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }
            if (login == null)
            {
                throw new ArgumentNullException(nameof(login));
            }
            
            await db.InsertAsync(new AspNetUserLogins() { 
                LoginProvider = login.LoginProvider,
                ProviderDisplayName = login.ProviderDisplayName,
                ProviderKey = login.ProviderKey,
                User = user,
                UserId = user.Id
            });
            return;
        }

        public async Task RemoveLoginAsync(AspNetUsers user, string loginProvider, string providerKey, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }
            var entry = await db.GetTable<AspNetUserLogins>()
                .FirstOrDefaultAsync(l => l.UserId == user.Id && l.LoginProvider == loginProvider && l.ProviderKey == providerKey, cancellationToken);
            if (entry != null)
            {
                await db.DeleteAsync(entry);
            }
        }

        public async Task<IList<UserLoginInfo>> GetLoginsAsync(AspNetUsers user, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }
            var userId = user.Id;
            return await db.GetTable<AspNetUserLogins>()
                .Where(l => l.UserId.Equals(userId))
                .Select(l => new UserLoginInfo(l.LoginProvider, l.ProviderKey, l.ProviderDisplayName))
                .ToListAsync(cancellationToken);
        }

        public async Task<AspNetUsers> FindByLoginAsync(string loginProvider, string providerKey, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();
            var userLogin = await db.GetTable<AspNetUserLogins>()
                .SingleOrDefaultAsync(userLogin => userLogin.LoginProvider == loginProvider && userLogin.ProviderKey == providerKey, cancellationToken);
            if (userLogin != null)
            {
                return await db.GetTable<AspNetUsers>().SingleOrDefaultAsync(u => u.Id.Equals(userLogin.UserId), cancellationToken);
            }
            return null;
        }
    }

}
