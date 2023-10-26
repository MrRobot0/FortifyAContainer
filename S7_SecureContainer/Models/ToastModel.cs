using Blazored.Toast.Services;

namespace S7_SecureContainer.Models
{
    public class ToastModel
    {
        public ToastLevel Level { get; set; } = ToastLevel.Info;
        public string Message { get; set; } = "";
        public ToastModel(ToastLevel level, string message) { 
            Level = level;
            Message = message;
        }
    }
}
