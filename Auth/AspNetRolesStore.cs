using LinqToDB;
using LinqToDB.Data;
using Microsoft.AspNetCore.Identity;
using LINQ2DB_MVC_Core_5.Auth.DB;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace LINQ2DB_MVC_Core_5.Auth
{
    public class AspNetRolesStore : IRoleStore<AspNetRoles>
    {
        private bool _disposed = false;
        private readonly DataConnection db;

        public AspNetRolesStore()
        {
            db = new DataConnection();
        }

        public async Task<IdentityResult> CreateAsync(AspNetRoles role, CancellationToken cancellationToken)
        {
            await db.InsertAsync<AspNetRoles>(role);

            return IdentityResult.Success;
        }

        public async Task<IdentityResult> DeleteAsync(AspNetRoles role, CancellationToken cancellationToken)
        {
            await db.DeleteAsync(role);

            return IdentityResult.Success;
        }

        public async Task<AspNetRoles> FindByIdAsync(string roleId, CancellationToken cancellationToken)
        {
            return await db.GetTable<AspNetRoles>().FirstOrDefaultAsync(_ => _.Id.Equals(roleId), cancellationToken);
        }

        public async Task<AspNetRoles> FindByNameAsync(string normalizedRoleName, CancellationToken cancellationToken)
        {
            return await db.GetTable<AspNetRoles>()
            .FirstOrDefaultAsync(u => u.NormalizedName == normalizedRoleName, cancellationToken);
        }

        public Task<string> GetNormalizedRoleNameAsync(AspNetRoles role, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();
            if (role == null)
                throw new ArgumentNullException(nameof(role));
            return Task.FromResult(role.NormalizedName);
        }

        public Task<string> GetRoleIdAsync(AspNetRoles role, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();
            if (role == null)
                throw new ArgumentNullException(nameof(role));
            return Task.FromResult(role.Id);
        }

        public Task<string> GetRoleNameAsync(AspNetRoles role, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();
            if (role == null)
                throw new ArgumentNullException(nameof(role));
            return Task.FromResult(role.Name);
        }

        public Task SetNormalizedRoleNameAsync(AspNetRoles role, string normalizedName, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();
            if (role == null)
                throw new ArgumentNullException(nameof(role));
            role.NormalizedName = normalizedName;
            return Task.CompletedTask;
        }

        public Task SetRoleNameAsync(AspNetRoles role, string roleName, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();
            if (role == null)
                throw new ArgumentNullException(nameof(role));
            role.Name = roleName;
            return Task.CompletedTask;
        }

        public async Task<IdentityResult> UpdateAsync(AspNetRoles role, CancellationToken cancellationToken)
        {
            await db.UpdateAsync(role);

            return IdentityResult.Success;
        }
        public void Dispose()
        {
            db.Dispose();
            _disposed = true;
        }

        protected void ThrowIfDisposed()
        {
            if (_disposed)
                throw new ObjectDisposedException(GetType().Name);
        }
    }
}
