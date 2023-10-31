using Blazored.Toast.Services;
using Docker.DotNet;
using Docker.DotNet.Models;
using S7_SecureContainer.Models;
using System.Net.Sockets;
using System.Reflection;

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


        public async Task<List<ToastModel>> Connect(string connectionString)
        {
            List<ToastModel> toasts = new();
            if (connectionString == string.Empty)
            {
                toasts.Add(new ToastModel(ToastLevel.Warning, "Please enter a value.."));
                return toasts;
            }
            DockerClient client;
            try
            {
                client = new DockerClientConfiguration(
                new Uri(connectionString))
                    .CreateClient();
                IList<ContainerListResponse> containers = await client.Containers.ListContainersAsync(
                new ContainersListParameters()
                {
                    Limit = 1,
                });
                if (containers.Count == 0)
                {
                    toasts.Add(new ToastModel(ToastLevel.Error, "No containers found!"));
                    return toasts;
                }
            }
            catch (Exception)
            {
                toasts.Add(new ToastModel(ToastLevel.Error, "Could not connect to server url: " + connectionString));
                return toasts;
            }
            toasts.Add(new ToastModel(ToastLevel.Success, "Succesfully connected to: " + connectionString));
            ConnectionString = connectionString;
            Client = client;
            return toasts;
        }
    }
}
