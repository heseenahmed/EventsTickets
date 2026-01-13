using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace FleetLinker.Application.Command.Core
{
    public abstract class Message : IRequest<bool>
    {
        public string MessageType { get; protected set; }
        protected Message()
        {
            MessageType = GetType().Name;
        }
    }
}
