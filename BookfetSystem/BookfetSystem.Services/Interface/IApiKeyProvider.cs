using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookfetSystem.Services.Interface
{
    public interface IApiKeyProvider
    {
        string GetRandomKey();
    }
}
