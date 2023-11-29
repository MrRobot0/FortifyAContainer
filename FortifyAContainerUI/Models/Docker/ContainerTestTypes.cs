namespace FortifyAContainerUI.Models.Docker
{
    public static class ContainerTestTypes
    {
		public const string DefaultNetwork = "Bridge network";
		public const string CPULimit = "CPU limit";
		public const string DockerSocket = "Docker Socket";
		public const string MemLimit = "Memory limit";
		public const string Root = "Root user";

		// Place tests in alphabetical order
		public static List<ContainerTestType> All = new()
        {
			{ new() { Name = DefaultNetwork, Tooltip = "Check if the container uses 'bridge' as network" } },
			{ new() { Name = CPULimit, Tooltip = "Check if the container has a CPU limit set" } },
			{ new() { Name = DockerSocket, Tooltip = "Check if the container uses '/var/run/docker.sock'" } },
            { new() { Name = MemLimit, Tooltip = "Check if the container has a Memory limit set" } },
			{ new() { Name = Root, Tooltip = "Check if the container uses '0:0' or 'root' as user" } },
		};
        public static int GetTestCount() { return All.Count; }
    }
}
