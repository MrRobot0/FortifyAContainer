using Docker.DotNet.Models;
using S7_SecureContainer.Models.Docker;
using S7_SecureContainer.Models.Test;
using S7_SecureContainer.Services;

namespace S7_SecureContainer.Models.Tests
{
	public static class CheckDockerSocket
	{
		public static async Task<TestResult> Run(ContainerInspectResponse containerInspectResponse)
		{
			try
			{
				var mounts = containerInspectResponse.Mounts;
				foreach (var mount in mounts)
				{
					if (mount.Source == "/var/run/docker.sock")
					{
						if (mount.RW)
						{
							return new TestResult(ContainerTestTypes.DockerSocket, TestResult.Status.Failed);
						}
						else
						{
							return new TestResult(ContainerTestTypes.DockerSocket, TestResult.Status.Warning, "Is mounted, but as read-only");
						}
					}
				}
				return new TestResult(ContainerTestTypes.DockerSocket, TestResult.Status.Passed);
			}
			catch (Exception)
			{
				return new TestResult(ContainerTestTypes.DockerSocket, TestResult.Status.Invalid);
			}
		}
	}
}
