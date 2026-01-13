
using Tickets.Domain.Entity;
using Tickets.Domain.IRepository;
using MediatR;

namespace Tickets.Application.Command.AdelApis
{
    public class CreateTokenCommandHandler : IRequestHandler<CreateTokenCommand, List<Tokens>>
    {
        private readonly IAdelTokenRepository _repo;

        public CreateTokenCommandHandler(IAdelTokenRepository repo)
        {
            _repo = repo;
        }
        public async Task<List<Tokens>> Handle(CreateTokenCommand request, CancellationToken ct)
        {
            var token = (request.Token ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(token))
                throw new ArgumentException("Token is required.");

            await _repo.AddAsync(token, ct);
            return await _repo.GetAllValuesAsync(ct);
        }
    }
}
