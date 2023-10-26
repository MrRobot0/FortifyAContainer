using Blazored.Toast.Services;
namespace S7_SecureContainer.Models
{
    public static class TestToastsMessages
    {
        public static ToastModel TestComplete = new(ToastLevel.Success, 
                        "Test(s) complete!");
        public static ToastModel TestNotFullyComplete = new(ToastLevel.Error, 
                        "Some tests were not succesfully run, retried five times. Please try again later!");
    }
}
