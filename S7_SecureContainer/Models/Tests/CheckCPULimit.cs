using Docker.DotNet.Models;
using S7_SecureContainer.Models.Docker;
using S7_SecureContainer.Models.Test;
using S7_SecureContainer.Services;

namespace S7_SecureContainer.Models.Tests
{
	public static class CheckCPULimit
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
