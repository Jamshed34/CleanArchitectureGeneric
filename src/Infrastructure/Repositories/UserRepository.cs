using Domain.Entities;
using Domain.Interfaces;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Primitives;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Infrastructure.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly IMemoryCache _cache;
        private readonly CancellationTokenSource _cacheEvictionTokenSource = new();

        public UserRepository(ApplicationDbContext context, IMemoryCache cache)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
        }

        public async Task AddAsync(User user, CancellationToken cancellationToken = default)
        {
            await _context.Users.AddAsync(user, cancellationToken);
            InvalidateCache(); // 🚨 Invalidate on write
        }

        public async Task<int> CountAsync(CancellationToken cancellationToken = default)
        {
            const string cacheKey = "users_count";

            return await _cache.GetOrCreateAsync(cacheKey, async entry =>
            {
                entry.SlidingExpiration = TimeSpan.FromMinutes(5);
                entry.AddExpirationToken(GetEvictionToken());
                return await _context.Users.CountAsync(cancellationToken);
            });
        }

        public async Task DeleteAsync(User user, CancellationToken cancellationToken = default)
        {
            _context.Users.Remove(user);
            InvalidateCache(); // 🚨 Invalidate on delete
            await Task.CompletedTask;
        }

        public async Task<IEnumerable<User>> GetAllAsync(int pageNumber, int pageSize, CancellationToken cancellationToken = default)
        {
            var cacheKey = $"users_page_{pageNumber}_{pageSize}";

            return await _cache.GetOrCreateAsync(cacheKey, async entry =>
            {
                entry.SlidingExpiration = TimeSpan.FromMinutes(5);
                entry.AddExpirationToken(GetEvictionToken());

                return await _context.Users
                    .OrderBy(u => u.Email)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync(cancellationToken);
            });
        }

        public async Task<User> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
        {
            var cacheKey = $"user_email_{email.ToLower()}";

            return await _cache.GetOrCreateAsync(cacheKey, async entry =>
            {
                entry.SlidingExpiration = TimeSpan.FromMinutes(5);
                entry.AddExpirationToken(GetEvictionToken());

                var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email, cancellationToken);
                return user ?? throw new InvalidOperationException($"No user found with email {email}");
            });
        }

        public async Task<User> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var cacheKey = $"user_id_{id}";

            return await _cache.GetOrCreateAsync(cacheKey, async entry =>
            {
                entry.SlidingExpiration = TimeSpan.FromMinutes(5);
                entry.AddExpirationToken(GetEvictionToken());

                var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == id, cancellationToken);
                return user ?? throw new InvalidOperationException($"No user found with ID {id}");
            });
        }

        public async Task<bool> IsEmailUniqueAsync(string email, CancellationToken cancellationToken = default)
        {
            var cacheKey = $"user_unique_email_{email.ToLower()}";

            return await _cache.GetOrCreateAsync(cacheKey, async entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(2);
                entry.AddExpirationToken(GetEvictionToken());

                return !await _context.Users.AnyAsync(u => u.Email == email, cancellationToken);
            });
        }

        public async Task UpdateAsync(User user, CancellationToken cancellationToken = default)
        {
            _context.Users.Update(user);
            InvalidateCache(); // 🚨 Invalidate on update
            await Task.CompletedTask;
        }

        public IUnitOfWork UnitOfWork => _context;

        private void InvalidateCache()
        {
            _cacheEvictionTokenSource.Cancel();
        }

        private IChangeToken GetEvictionToken()
        {
            return new CancellationChangeToken(_cacheEvictionTokenSource.Token);
        }
    }
}
