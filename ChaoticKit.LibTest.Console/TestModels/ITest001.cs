using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChaoticKit.LibTest.Console.TestModels
{
    public interface ITest001
    {
        int M1(int x, int y);
        string M2(string x, string y);

        Task<int> M3(int x, int y, int z);

        Task M4(string str);
    }
}
