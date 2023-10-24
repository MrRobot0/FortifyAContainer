using Docker.DotNet.Models;
using System.Text;

namespace S7_SecureContainer.Models
{
    public class TestResult
    {
        public ContainerListResponse Container { get; set; }
        public String Message { get; set; }
        public Status State { get; set; }

        public enum Status
        {
            Passed,
            Failed,
            Invalid
        }

        public TestResult(String message, Status status, ContainerListResponse container) { 
            StringBuilder messageBuilder = new StringBuilder();
            messageBuilder.Append(message);
            messageBuilder.Append(" ");
            messageBuilder.Append("has ");
            messageBuilder.Append(status.ToString().ToLower());
            this.Message = messageBuilder.ToString();
            this.Container = container;
            this.State = status;
        }
    }
}
