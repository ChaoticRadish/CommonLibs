using Common_Util.Extensions;
using Common_Util.Test.Console;
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace CommonLibTest_Console
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
            foreach (var str in args.SelectMany(s => s.Split('\n', ' ')).Where(s => s.IsNotEmpty()))
            {
                runner.Run(str);
            }

#if DEBUG
            FreeConsole();
#endif
        }
    }
}
