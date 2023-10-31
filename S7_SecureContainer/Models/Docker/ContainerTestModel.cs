﻿using Docker.DotNet.Models;
using S7_SecureContainer.Models.Test;

namespace S7_SecureContainer.Models.Docker
{
    public class ContainerTestModel
    {
        public Dictionary<ContainerListResponse, List<TestResult>> ContainerTestResults { get; set; } = new();
        public List<ToastModel> Toasts { get; set; } = new();
    }
}