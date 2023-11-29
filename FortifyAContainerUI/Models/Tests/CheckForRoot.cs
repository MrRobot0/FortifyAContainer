using Docker.DotNet.Models;
using FortifyAContainerUI.Models.Docker;
using FortifyAContainerUI.Models.Test;
using FortifyAContainerUI.Services;

namespace FortifyAContainerUI.Models.Tests
{
    public static class CheckForRoot
    {
        public static async Task<TestResult> Run(ContainerInspectResponse containerInspectResponse)
        {
            try
            {
                var user = containerInspectResponse.Config.User;
                if (user != "0:0" && user != "root")
                {
                    return new TestResult(ContainerTestTypes.Root, TestResult.Status.Passed);
                }
                else
                {
                    return new TestResult(ContainerTestTypes.Root, TestResult.Status.Failed);
                }
            }
            catch (Exception)
            {
                return new TestResult(ContainerTestTypes.Root, TestResult.Status.Invalid);
            }
        }
    }
}
