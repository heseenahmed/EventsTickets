using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace Tickets.Application.Command.Core
{
    public abstract class Commands : Message
    {
        public DateTime TimeSpan { get; protected set; }
        protected Commands()
        {
            TimeSpan = DateTime.UtcNow;
        }
    }
}
