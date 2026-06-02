using ChaoticKit.Extensions;
using ChaoticKit.Test.Console;
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace ChaoticKit.LibTest.Console
{
    partial class Program
    {
        [LibraryImport("kernel32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static partial bool AllocConsole();
        [LibraryImport("kernel32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static partial bool FreeConsole();

        static void Main(string[] args)
        {
#if DEBUG
            AllocConsole();
#endif
            var runner = new TestRunner();
            var tests = args.SelectMany(s => s.Split('\n', ' ')).Where(s => s.IsNotEmpty() && !s.StartsWith("//"));
            if (!tests.Any())
            {
                throw new InvalidOperationException("未指定测试项");
            }
            else
            {
                foreach (var str in tests)
                {
                    runner.Run(str);
                }
            }

#if DEBUG
            FreeConsole();
#endif
        }
    }
}
