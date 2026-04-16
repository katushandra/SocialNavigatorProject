using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Common.Interfaces
{
    public interface ICommand<TResponse> : IRequest<TResponse> { }
    public interface ICommand : IRequest { }
    public interface IQuery<TResponse> : IRequest<TResponse> { }
}
