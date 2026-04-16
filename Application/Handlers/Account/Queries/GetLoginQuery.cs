using Application.Common.Interfaces;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Handlers.Account.Queries
{
    public class GetLoginQuery : IQuery<string?>
    {
        public string? ReturnUrl { get; set; }
    }

    public class GetLoginQueryHandler : IRequestHandler<GetLoginQuery, string?>
    {
        public Task<string?> Handle(GetLoginQuery request, CancellationToken cancellationToken)
        {
            return Task.FromResult(request.ReturnUrl);
        }
    }
}
