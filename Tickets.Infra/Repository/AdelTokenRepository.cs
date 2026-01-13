
using Tickets.Domain.Entity;
using Tickets.Domain.IRepository;
using Tickets.Infra.Data;
using Microsoft.EntityFrameworkCore;
using NuGet.Common;
using System;

namespace Tickets.Infra.Repository
{
    public class AdelTokenRepository:IAdelTokenRepository
    {
        private readonly ApplicationDbContext _db;

        public AdelTokenRepository(ApplicationDbContext db)
        {
            _db = db;
        }
        public async Task AddAsync(string tokenValue, CancellationToken ct)
        {
            var token = new Tokens { Token = tokenValue };
            await _db.Tokens.AddAsync(token, ct);
            await _db.SaveChangesAsync(ct);
        }

        public async Task<List<Tokens>> GetAllValuesAsync(CancellationToken ct)
        {
            return await _db.Tokens
                .OrderByDescending(x => x.Id)
                .ToListAsync(ct);
        }
    }
}
