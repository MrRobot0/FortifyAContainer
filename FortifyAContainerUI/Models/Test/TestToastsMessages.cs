using Blazored.Toast.Services;
using System.Diagnostics;

namespace FortifyAContainerUI.Models.Test
{
    public class TestToastsMessages
    {
        private readonly IToastService _toastService;
        public TestToastsMessages(IToastService toastService) {
            _toastService = toastService;
        }

        public void ShowToast(ToastModel toast)
        {
            _toastService.ShowToast(toast.Level, toast.Message);
        }

        public readonly Stopwatch stopwatch = new();
        public const int MaxRetries = 10;

        public ToastModel TestComplete()
        {
            return new(ToastLevel.Success,
                        string.Format(testComplete, stopwatch.ElapsedMilliseconds));
        }

        public ToastModel TestNotFullyComplete()
        {
            return new(ToastLevel.Error,
                        string.Format(testNotFullyComplete, MaxRetries));
        }

        private const string testComplete = "Test(s) complete in {0}ms!";
        private const string testNotFullyComplete = "Some tests were not succesfully run, retried {0} times. Please try again later!";
    }
}
