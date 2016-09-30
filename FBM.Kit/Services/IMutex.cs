using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FBM.Kit.Services
{
    public interface IMutex
    {
        bool Aquire( int milliseconds = 0);
        Task<bool> AquireAsync(int milliseconds);
        bool Release();
    }
}
