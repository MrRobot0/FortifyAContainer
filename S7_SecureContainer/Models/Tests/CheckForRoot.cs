using Docker.DotNet.Models;
using S7_SecureContainer.Models.Docker;
using S7_SecureContainer.Models.Test;
using S7_SecureContainer.Services;

namespace S7_SecureContainer.Models.Tests
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
