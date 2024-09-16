﻿using System;
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

            TestBase test = new DataWrapper.Loadable001();
            
            test.Run();
            test.Finish();

#if DEBUG
            FreeConsole();
#endif
        }
    }
}