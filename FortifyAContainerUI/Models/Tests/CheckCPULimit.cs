using Docker.DotNet.Models;
using FortifyAContainerUI.Models.Docker;
using FortifyAContainerUI.Models.Test;
using FortifyAContainerUI.Services;

namespace FortifyAContainerUI.Models.Tests
{
	public class CheckCPULimit
	{
		public static async Task<TestResult> Run(ContainerInspectResponse containerInspectResponse)
		{
			try
			{
				var cpu = containerInspectResponse.HostConfig.NanoCPUs;
				if (cpu == 0)
				{
					return new TestResult(ContainerTestTypes.CPULimit, TestResult.Status.Warning, "It's recommended to limit CPUs used");
				}
				return new TestResult(ContainerTestTypes.CPULimit, TestResult.Status.Passed);
			}
			catch (Exception)
			{
				return new TestResult(ContainerTestTypes.CPULimit, TestResult.Status.Invalid);
			}
		}
	}
}
