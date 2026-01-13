
using Tickets.Domain.Entity;
using Tickets.Domain.IRepository;
using Tickets.Infra.Data;
using System;

namespace Tickets.Infra.Repository
{
    public class NoteRepository:INoteRepository
    {
        private readonly ApplicationDbContext _db;

        public NoteRepository(ApplicationDbContext db)
        {
            _db = db;
        }
        public async Task<int> AddAsync(string title, string details, CancellationToken ct)
        {
            var note = new Note
            {
                Title = title,
                Details = details
            };

            await _db.Note.AddAsync(note, ct);
            await _db.SaveChangesAsync(ct);

            return note.Id; // return created id (optional)
        }
    }
}
