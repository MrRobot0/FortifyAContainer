using Docker.DotNet;
using Docker.DotNet.Models;

namespace S7_SecureContainer.Services
{
    public class DockerService
    {
        public String ConnectionString { get; set; } = string.Empty;
        public DockerClient? Client { get; set; } = null;

        public void Clear()
        {
            Client = null;
            ConnectionString = string.Empty;
        }
    }
}
