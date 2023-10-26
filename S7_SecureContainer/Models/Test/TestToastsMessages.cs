using Blazored.Toast.Services;

namespace S7_SecureContainer.Models.Test
{
    public static class TestToastsMessages
    {
        public static ToastModel TestComplete = new(ToastLevel.Success,
                        "Test(s) complete!");
        public static ToastModel TestNotFullyComplete = new(ToastLevel.Error,
                        String.Format("Some tests were not succesfully run, retried {0} times. Please try again later!", RetryCount));
        public const int RetryCount = 10;
    }
}
