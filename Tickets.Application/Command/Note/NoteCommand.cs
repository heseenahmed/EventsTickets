
using Tickets.Application.DTOs;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace Tickets.Application.Command.Note
{
    public class CreateNoteCommand : IRequest<APIResponse<Domain.Entity.Note>>
    {
        public string Title { get; set; } = string.Empty;
        public string Details { get; set; } = string.Empty;
    }
}
