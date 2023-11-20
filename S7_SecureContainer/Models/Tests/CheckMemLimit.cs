using Docker.DotNet.Models;
using S7_SecureContainer.Models.Docker;
using S7_SecureContainer.Models.Test;
using S7_SecureContainer.Services;

namespace S7_SecureContainer.Models.Tests
{
	public static class CheckMemLimit
	{
		public static async Task<TestResult> Run(ContainerInspectResponse containerInspectResponse)
		{
			try
			{
				var mem = containerInspectResponse.HostConfig.Memory;
				if (mem == 0)
				{
					return new TestResult(ContainerTestTypes.MemLimit, TestResult.Status.Warning, "It's recommended to limit the memory used");
				}
				return new TestResult(ContainerTestTypes.MemLimit, TestResult.Status.Passed);
			}
			catch (Exception)
			{
				return new TestResult(ContainerTestTypes.MemLimit, TestResult.Status.Invalid);
			}
		}
	}
}
