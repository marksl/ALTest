using System;
using System.Collections.Generic;
using System.Globalization;
using System.Security.Principal;
using System.Text;
using System.Xml;
using ALTest.Core;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ALTest.MsTest
{
    internal static class MsTestResultsFile
    {
        internal static void WriteResults(DateTime start, DateTime finish,
                                          ICollection<TestResult> results,
                                          Dictionary<string, ICollection<TestResult>> assemblyResults,
                                          string fileName)
        {
            string machineName = Environment.MachineName;
            string testListId = Guid.NewGuid().ToString();

            using (var w = new XmlTextWriter(fileName, Encoding.UTF8))
            {
                w.Formatting = Formatting.Indented;
                w.Indentation = 4;
                w.WriteStartDocument();
                {
                    w.WriteStartElement("TestRun", "http://microsoft.com/schemas/VisualStudio/TeamTest/2010");
                    {
                        w.WriteAttributeString("id", Guid.NewGuid().ToString());

                        WindowsIdentity identity = WindowsIdentity.GetCurrent();
                        string windowsIdentity = identity != null ? identity.Name : "Unknown";

                        string localName = string.Format("{0} {1}", windowsIdentity, DateTime.Now.ToString("yyyy-MM-dd HH-mm-ss"));

                        w.WriteAttributeString("name", localName);
                        w.WriteAttributeString("runUser", string.Format("{0}", windowsIdentity));

                        w.WriteStartElement("TestSettings");
                        {
                            w.WriteAttributeString("name", "Default Test Settings");
                            w.WriteAttributeString("id", Guid.NewGuid().ToString());

                            w.WriteStartElement("Execution");
                            {
                                w.WriteElementString("TestTypeSpecific", string.Empty);
                                w.WriteStartElement("AgentRule");
                                w.WriteAttributeString("name", "Execution Agents");
                                w.WriteEndElement();
                            }
                            w.WriteEndElement();

                            w.WriteStartElement("Deployment");
                            w.WriteAttributeString("runDeploymentRoot", localName);
                            w.WriteEndElement();
                        }
                        w.WriteEndElement();

                        w.WriteStartElement("Times");
                        {
                            string startTime = start.ToString(CultureInfo.InvariantCulture);
                            w.WriteAttributeString("creation", startTime);
                            w.WriteAttributeString("queuing", startTime);
                            w.WriteAttributeString("start", startTime);
                            w.WriteAttributeString("finish", finish.ToString(CultureInfo.InvariantCulture));
                        }
                        w.WriteEndElement();


                        w.WriteStartElement("ResultSummary");
                        //FOO
                        {
                            int passed = 0;
                            int failed = 0;
                            foreach (TestResult result in results)
                            {
                                if (result.TestPassed)
                                {
                                    passed++;
                                }
                                else
                                {
                                    failed++;
                                }
                            }
                            w.WriteAttributeString("outcome",
                                                   failed > 0
                                                       ? UnitTestOutcome.Failed.ToString()
                                                       : UnitTestOutcome.Passed.ToString());

                            w.WriteStartElement("Counters");
                            {
                                string total = results.Count.ToString(CultureInfo.InvariantCulture);
                                w.WriteAttributeString("total", total);
                                w.WriteAttributeString("executed", total);
                                w.WriteAttributeString("passed", passed.ToString(CultureInfo.InvariantCulture));
                                w.WriteAttributeString("error", "0");
                                w.WriteAttributeString("failed", failed.ToString(CultureInfo.InvariantCulture));
                                w.WriteAttributeString("timeout", "0");
                                w.WriteAttributeString("aborted", "0");
                                w.WriteAttributeString("inconclusive", "0");
                                w.WriteAttributeString("passedButRunAborted", "0");
                                w.WriteAttributeString("notRunnable", "0");
                                w.WriteAttributeString("notExecuted", "0");
                                w.WriteAttributeString("disconnected", "0");
                                w.WriteAttributeString("warning", "0");
                                w.WriteAttributeString("completed", "0");
                                w.WriteAttributeString("inProgress", "0");
                                w.WriteAttributeString("pending", "0");
                            }
                            w.WriteEndElement();
                        }
                        w.WriteEndElement();

                        var testIdToExecutionId = new Dictionary<string, Tuple<string, TestResult>>();

                        w.WriteStartElement("TestDefinitions");
                        foreach (var assemblyName in assemblyResults.Keys)
                        {
                            foreach (TestResult r in assemblyResults[assemblyName])
                            {
                                w.WriteStartElement("UnitTest");
                                {
                                    string testId = Guid.NewGuid().ToString();
                                    string executionId = Guid.NewGuid().ToString();

                                    testIdToExecutionId.Add(testId, new Tuple<string, TestResult>(executionId, r));

                                    w.WriteAttributeString("name", r.TestName);
                                    w.WriteAttributeString("storage", assemblyName);
                                    w.WriteAttributeString("id", testId);

                                    w.WriteStartElement("Execution");
                                    w.WriteAttributeString("id", executionId);
                                    w.WriteEndElement();

                                    w.WriteStartElement("TestMethod");
                                    w.WriteAttributeString("codeBase", assemblyName);
                                    w.WriteAttributeString("adapterTypeName",
                                                           "Microsoft.VisualStudio.TestTools.TestTypes.Unit.UnitTestAdapter, Microsoft.VisualStudio.QualityTools.Tips.UnitTest.Adapter, Version=11.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a");
                                    w.WriteAttributeString("className", r.ClassName);
                                    w.WriteAttributeString("name", r.TestName);
                                    w.WriteEndElement(); // </TestMethod>
                                }
                                w.WriteEndElement(); // </UnitTest>
                            }
                        }
                        w.WriteEndElement(); // </TestDefinitions>

                        w.WriteStartElement("TestLists");
                        {
                            w.WriteStartElement("TestList");
                            {
                                w.WriteAttributeString("name", "Results Not in a List");
                                w.WriteAttributeString("id", testListId);
                            }
                            w.WriteEndElement(); // </TestList>

                            w.WriteStartElement("TestList");
                            {
                                w.WriteAttributeString("name", "All Loaded Results");
                                w.WriteAttributeString("id", Guid.NewGuid().ToString());
                            }
                            w.WriteEndElement(); // </TestList>
                        }
                        w.WriteEndElement(); // </TestLists>

                        w.WriteStartElement("TestEntries");
                        foreach (string testId in testIdToExecutionId.Keys)
                        {
                            string executionId = testIdToExecutionId[testId].Item1;
                            w.WriteStartElement("TestEntry");
                            {
                                w.WriteAttributeString("testId", testId);
                                w.WriteAttributeString("executionId", executionId);
                                w.WriteAttributeString("testListId", testListId);
                            }
                            w.WriteEndElement(); // </TestEntry>
                        }
                        w.WriteEndElement(); // <TestEntries>

                        w.WriteStartElement("Results");

                        foreach (string testId in testIdToExecutionId.Keys)
                        {
                            var tuple = testIdToExecutionId[testId];

                            string executionId = tuple.Item1;
                            TestResult testResult = tuple.Item2;
                            
                            string testName = testResult.TestName;
                            bool testPassed = testResult.TestPassed;

                            w.WriteStartElement("UnitTestResult");
                            w.WriteAttributeString("executionId", executionId);
                            w.WriteAttributeString("testId", testId);
                            w.WriteAttributeString("testName", testName);
                            w.WriteAttributeString("computerName", machineName);
                            w.WriteAttributeString("duration", testResult.Duration.ToString());
                            w.WriteAttributeString("startTime", testResult.StartTime.ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss'.'fffffffzzz", CultureInfo.InvariantCulture));
                            w.WriteAttributeString("endTime", testResult.EndTime.ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss'.'fffffffzzz", CultureInfo.InvariantCulture));
                            w.WriteAttributeString("testType", "13cdc9d9-ddb5-4fa4-a97d-d965ccfc6d4b");
                            w.WriteAttributeString("outcome", testPassed ? "Passed" : "Failed");
                            w.WriteAttributeString("testListId", testListId);
                            w.WriteAttributeString("relativeResultsDirectory", executionId);
                            if (!testPassed)
                            {
                                w.WriteStartElement("Output");
                                w.WriteStartElement("ErrorInfo");
                                w.WriteElementString("Message", testResult.ExceptionMessage);
                                w.WriteElementString("StackTrace", testResult.ExceptionStackTrace);
                                w.WriteEndElement(); // </ErrorInfo>
                                w.WriteEndElement(); // </Output>
                            }
                            w.WriteEndElement(); // </UnitTestResult>
                        }
                        w.WriteEndElement(); // </Results>
                    }
                    w.WriteEndElement(); // </TestRun>
                }
                w.WriteEndDocument();
            }            
        }
    }
}
