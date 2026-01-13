
using FleetLinker.Application.DTOs;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace FleetLinker.Application.Command.Note
{
    public class CreateNoteCommand : IRequest<APIResponse<Domain.Entity.Note>>
    {
        public string Title { get; set; } = string.Empty;
        public string Details { get; set; } = string.Empty;
    }
}
