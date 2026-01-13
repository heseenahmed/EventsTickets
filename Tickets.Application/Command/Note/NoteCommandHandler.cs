
using FleetLinker.Application.DTOs;
using FleetLinker.Domain.IRepository;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace FleetLinker.Application.Command.Note
{
    public class NoteCommandHandler : IRequestHandler<CreateNoteCommand, APIResponse<Domain.Entity.Note>>
    {
        private readonly INoteRepository _repo;

        public NoteCommandHandler(INoteRepository repo)
        {
            _repo = repo;
        }
        public async Task<APIResponse<Domain.Entity.Note>> Handle(CreateNoteCommand request, CancellationToken ct)
        {
            try
            {
                var title = (request.Title ?? string.Empty).Trim();
                var details = (request.Details ?? string.Empty).Trim();

                if (string.IsNullOrWhiteSpace(title))
                    return APIResponse<Domain.Entity.Note>.Fail(404 , new List<string>{ },
                "Title is required.");

                if (string.IsNullOrWhiteSpace(details))
                    return APIResponse<Domain.Entity.Note>.Fail(404, new List<string> { }, "Details is required.");

                int id = await _repo.AddAsync(title, details, ct);

                return APIResponse<Domain.Entity.Note>.Success(new Domain.Entity.Note {Id = id, Title = title, Details = details },"Saved successfully."  , 200);
            }
            catch
            {
                // You can log exception here
                return APIResponse<Domain.Entity.Note>.Fail(404, new List<string> { }, "Failed to save note.");

            }
        }
    }
}
