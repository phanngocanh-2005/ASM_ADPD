using System;

namespace AuthApp.Patterns.Factory
{
    /// <summary>
    /// Interface chung cho mọi loại thông báo (success, error, ...).
    /// Mục tiêu: client chỉ làm việc với INotificationMessage, không cần biết
    /// class cụ thể nào đang được tạo ra (áp dụng Factory Method).
    /// </summary>
    public interface INotificationMessage
    {
        string Type { get; }
        string Message { get; }
    }

    /// <summary>
    /// Concrete Product: thông báo thành công.
    /// </summary>
    public class SuccessNotification : INotificationMessage
    {
        public string Type => "Success";
        public string Message { get; }

        public SuccessNotification(string message)
        {
            Message = message;
        }
    }

    /// <summary>
    /// Concrete Product: thông báo lỗi.
    /// </summary>
    public class ErrorNotification : INotificationMessage
    {
        public string Type => "Error";
        public string Message { get; }

        public ErrorNotification(string message)
        {
            Message = message;
        }
    }

    /// <summary>
    /// Factory Method tạo ra các loại thông báo dùng chung cho TempData / ModelState.
    /// Thay vì new SuccessNotification / ErrorNotification trực tiếp ở controller,
    /// ta gom logic tạo object vào đây → controller gọn hơn, dễ thay đổi / mở rộng.
    /// </summary>
    public static class NotificationFactory
    {
        // Tạo thông báo thành công
        public static INotificationMessage CreateSuccess(string message) =>
            new SuccessNotification(message);

        // Tạo thông báo lỗi
        public static INotificationMessage CreateError(string message) =>
            new ErrorNotification(message);
    }
}


