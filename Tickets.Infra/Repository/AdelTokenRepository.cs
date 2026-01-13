
using FleetLinker.Domain.Entity;
using FleetLinker.Domain.IRepository;
using FleetLinker.Infra.Data;
using Microsoft.EntityFrameworkCore;
using NuGet.Common;
using System;

namespace FleetLinker.Infra.Repository
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
