﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ALTest.MsTest
{
    public class MsTestContext : TestContext
    {
        private readonly Dictionary<string, string> dict = new Dictionary<string, string>();

        public override void WriteLine(string format, params object[] args)
        {
            throw new NotImplementedException();
        }

        public override void AddResultFile(string fileName)
        {
            throw new NotImplementedException();
        }

        public override void BeginTimer(string timerName)
        {
            throw new NotImplementedException();
        }

        public override void EndTimer(string timerName)
        {
            throw new NotImplementedException();
        }

        public override IDictionary Properties
        {
            get { return dict; }
        }

        public override DataRow DataRow
        {
            get { throw new NotImplementedException(); }
        }

        public override DbConnection DataConnection
        {
            get { throw new NotImplementedException(); }
        }
    }
}