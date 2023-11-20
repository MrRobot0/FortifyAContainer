using Docker.DotNet.Models;
using S7_SecureContainer.Models.Docker;
using S7_SecureContainer.Models.Test;
using S7_SecureContainer.Services;
using System.Collections.Generic;

namespace S7_SecureContainer.Models.Tests
{
	public static class CheckForDefaultNetwork
	{
		public static async Task<TestResult> Run(ContainerInspectResponse containerInspectResponse)
		{
			try
			{
				var networks = containerInspectResponse.NetworkSettings.Networks;

				foreach (var network in networks)
				{
					if (network.Key == "bridge")
					{		
						return new TestResult(ContainerTestTypes.DefaultNetwork, TestResult.Status.Failed);
					}
				}
				return new TestResult(ContainerTestTypes.DefaultNetwork, TestResult.Status.Passed);
			}
			catch (Exception)
			{
				return new TestResult(ContainerTestTypes.DefaultNetwork, TestResult.Status.Invalid);
			}
		}
	}
}
