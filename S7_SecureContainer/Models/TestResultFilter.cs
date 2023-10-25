﻿using Docker.DotNet.Models;
using System.Collections.Generic;
using System.Reflection.Metadata.Ecma335;
using static S7_SecureContainer.Models.TestResult;

namespace S7_SecureContainer.Models
{
    public class TestResultFilter
    {
        public readonly Dictionary<Status, Dictionary<ContainerListResponse, List<TestResult>>> AllTestResults = new()
        {
            { Status.Passed, new() },
            { Status.Failed, new() },
            { Status.Invalid, new() }
        };
        public Dictionary<ContainerListResponse, List<TestResult>> TestResultsView { get; set; } = new();

        private Dictionary<Status, bool> _Options = new Dictionary<Status, bool>()
        {
            { Status.Passed, true },
            { Status.Failed, true },
            { Status.Invalid, true }
        };
        public void SetOption(Status key, bool value)
        {
            if (value)
            {
                foreach (var keyValuePair in AllTestResults[key])
                {
                    var container = keyValuePair.Key;
                    var TestResults = keyValuePair.Value;
                    if (!TestResultsView.ContainsKey(container))
                    {
                        TestResultsView.Add(container, TestResults.ToList());
                    }
                    else
                    {
                        TestResultsView[container].AddRange(TestResults.ToList());
                    }
                }
            }
            else
            {
                foreach (var keyValuePair in TestResultsView)
                {
                    var container = keyValuePair.Key;
                    var TestResults = keyValuePair.Value;
                    TestResults.RemoveAll(a => a.State == key);
                    if (TestResults.Count == 0)
                    {
                        TestResultsView.Remove(container);
                    }
                }
            }
            _Options[key] = value;
        }
        public bool GetOption(Status key)
        {
            return _Options[key];
        }

        public TestResultFilter(Dictionary<ContainerListResponse, List<TestResult>> containerTestResults)
        {
            foreach (var keyValuePair in containerTestResults)
            {
                var FailedTests = keyValuePair.Value
                    .Where(a => a.State == TestResult.Status.Failed)
                    .OrderBy(o => o.Message).ToList();
                if (FailedTests.Any()) AllTestResults[Status.Failed]
                        .Add(keyValuePair.Key, FailedTests);

                var PassedTests = keyValuePair.Value
                    .Where(a => a.State == TestResult.Status.Passed)
                    .OrderBy(o => o.Message).ToList();
                if (PassedTests.Any()) AllTestResults[Status.Passed]
                    .Add(keyValuePair.Key, PassedTests);

                var InvalidTests = keyValuePair.Value
                    .Where(a => a.State == TestResult.Status.Invalid)
                    .OrderBy(o => o.Message).ToList();
                if (InvalidTests.Any()) AllTestResults[Status.Invalid]
                    .Add(keyValuePair.Key, InvalidTests);
                TestResultsView.Add(keyValuePair.Key, keyValuePair.Value.OrderBy(o => o.Message).ToList());
            }
        }
        public TestResultFilter()
        {

        }
    }
}
