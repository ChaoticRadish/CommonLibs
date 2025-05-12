using Common_Util.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonLibTest_Console.DataStruct
{
    internal class Guid001() : TestBase("GuidBuild 的测试")
    {
        protected override void RunImpl()
        {
            RunTestMark();
        }

        [TestMethod]
        private void test1()
        {
            var builder = GuidHelper.Builder();
            builder.Add(1, 2, 3, 4);
            builder.Add(5, 6, 7, 8);
            builder.Add(9, 10, 11, 12);
            builder.Add(13, 14, 15, 16);
            Guid guid = builder.Build();    // 04030201-0605-0807-090a-0b0c0d0e0f10
            WritePair(guid);
        }
        [TestMethod]
        private void test2_1()
        {
            var builder = GuidHelper.Builder();
            builder.Add(99);
            Guid guid = builder.Build();    // 63000000-0000-0000-0000-000000000000
            WritePair(guid);
        }
        [TestMethod]
        private void test2_2()
        {
            var builder = GuidHelper.Builder();
            builder.Add(99, false);
            Guid guid = builder.Build();    // 00000063-0000-0000-0000-000000000000
            WritePair(guid);
        }
        [TestMethod]
        private void test3()
        {
            var builder = GuidHelper.Builder();
            builder.Add(99L);
            Guid guid = builder.Build();    // 00000000-0000-6300-0000-000000000000
            WritePair(guid);
        }
        [TestMethod]
        private void test4()
        {
            var builder = GuidHelper.Builder();
            builder.AddRandom();
            Guid guid = builder.Build(); 
            WritePair(guid);
        }
        [TestMethod]
        private void test5()
        {
            var builder = GuidHelper.Builder();
            builder.Add(99164L);
            WritePair("out 1", builder.Build());    // 00000000-0100-5c83-0000-000000000000
            builder.Add(12345);
            WritePair("out 2", builder.Build());    // 00000000-0100-5c83-0000-303900000000
            builder.Add((byte)12);
            WritePair("out 3", builder.Build());    // 00000000-0100-5c83-0000-30390c000000
            builder.Add(113513155312345_5613L);
            WritePair("out 4", builder.Build());    // 00000000-0100-5c83-0000-30390c0fc0cc
            WritePair(builder.CurrentIndex);    // 16

            builder.Reset();
            WriteLine("Reset");
            WritePair(builder.CurrentIndex);    // 0
            WritePair("out 5", builder.Build());    // 00000000-0000-0000-0000-000000000000
            builder.Add(12345);
            WritePair("out 6", builder.Build());    // 39300000-0000-0000-0000-000000000000
            builder.Add((byte)12);
            WritePair("out 7", builder.Build());    // 39300000-000c-0000-0000-000000000000
            builder.Add(113513155312345_5613L);
            WritePair("out 8", builder.Build());    // 39300000-0f0c-ccc0-2225-42d27d000000
            builder.Add(113513155312345_5613L);
            WritePair("out 8", builder.Build());    // 39300000-0f0c-ccc0-2225-42d27d0fc0cc
            WritePair(builder.CurrentIndex);    // 16
        }
    }
}
