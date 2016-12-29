using System;
using System.Collections.Generic;
using System.Linq;
using Common;

namespace Server
{
    sealed class TestData : ITestData
    {
        string[] ITestData.TestArray { get { throw new NotImplementedException(); } }

        int ITestData.TestInt { get { throw new NotImplementedException(); } }

        string ITestData.TestString => "TestStringValue";
        string ITestData.TestFunction(int arg1, string arg2) => "arg1=" + arg1 + " arg2=" + arg2;
    }

    sealed class TestData1 : ITestData
    {
        string[] ITestData.TestArray { get { throw new NotImplementedException(); } }

        int ITestData.TestInt { get { throw new NotImplementedException(); } }

        string ITestData.TestString => "TestStringValue";
        string ITestData.TestFunction(int arg1, string arg2) => "1 arg1=" + arg1 + " arg2=" + arg2;
    }

    sealed class TestData2 : ITestData1
    {
        string[] ITestData1.TestArray { get { throw new NotImplementedException(); } }

        int ITestData1.TestInt { get { throw new NotImplementedException(); } }

        string ITestData1.TestString => "TestStringValue";
        string ITestData1.TestFunction(int arg1, string arg2) => "2 arg1=" + arg1 + " arg2=" + arg2;
    }
}