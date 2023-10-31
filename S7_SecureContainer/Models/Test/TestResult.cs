﻿using Docker.DotNet.Models;
using System.Text;

namespace S7_SecureContainer.Models.Test
{
    public class TestResult
    {
        public ContainerListResponse Container { get; set; }
        public string Message { get; set; }
        public Status State { get; set; }

        public enum Status
        {
            Passed,
            Failed,
            Invalid
        }

        public TestResult(string message, Status status, ContainerListResponse container)
        {
            StringBuilder messageBuilder = new StringBuilder();
            messageBuilder.Append(message);
            messageBuilder.Append(" ");
            messageBuilder.Append("has ");
            messageBuilder.Append(status.ToString().ToLower());
            Message = messageBuilder.ToString();
            Container = container;
            State = status;
        }
    }
}