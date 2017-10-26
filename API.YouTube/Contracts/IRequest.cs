using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace API.YouTube.Contracts
{
    public interface IRequest<T>
    {
        string GetRequestUrl();
    }
}
