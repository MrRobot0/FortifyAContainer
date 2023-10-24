using Docker.DotNet;
using Docker.DotNet.Models;

namespace S7_SecureContainer.Services
{
    public class DockerService
    {
        public String ConnectionString { get; set; }
        public DockerClient Client { get; set; }
    }
}
