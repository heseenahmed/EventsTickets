
namespace Tickets.Domain.Entity
{
    public class Note
    {
        public int Id { get; set; }
        public string Title { get; set; } = null!;
        public string Details { get; set; } = null!;
    }
}
