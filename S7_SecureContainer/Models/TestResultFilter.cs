using Docker.DotNet.Models;
using System.Collections.Generic;

namespace S7_SecureContainer.Models
{
    public class TestResultFilter
    {
        public bool showFailed { get; set; } = true;

        public bool showPassed { get; set; } = true;

        public bool showInvalid { get; set; } = true;

        public Dictionary<ContainerListResponse, List<TestResult>> filter(Dictionary<ContainerListResponse, List<TestResult>> containerTestResults)
        {
            Dictionary<ContainerListResponse, List<TestResult>> containerTestResultsView = new();

            foreach (var item in containerTestResults)
            {
                List<TestResult> results = new List<TestResult>();
                if (showFailed)
                {
                    results.AddRange(item.Value.Where(a => a.State == TestResult.Status.Failed));
                }
                if (showPassed)
                {
                    results.AddRange(item.Value.Where(a => a.State == TestResult.Status.Passed));
                }
                if (showInvalid)
                {
                    results.AddRange(item.Value.Where(a => a.State == TestResult.Status.Invalid));
                }
                if (results.Count > 0)
                {
                    containerTestResultsView.Add(item.Key, results);
                }
            }
            return containerTestResultsView;
        }
    }
}
