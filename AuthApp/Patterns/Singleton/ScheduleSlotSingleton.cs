using System;
using System.Collections.Generic;

namespace AuthApp.Patterns.Singleton
{
    /// <summary>
    /// Singleton cung cấp danh sách time slot chuẩn dùng chung trong toàn hệ thống.
    /// Ý tưởng: chỉ có 1 object ScheduleSlotSingleton, mọi nơi muốn dùng time slot
    /// sẽ truy cập qua ScheduleSlotSingleton.Instance thay vì tự tính lại.
    /// </summary>
    public sealed class ScheduleSlotSingleton
    {
        // Field static lưu trữ instance duy nhất của Singleton
        private static ScheduleSlotSingleton? _instance;
        // Đối tượng lock dùng cho thread-safe khi khởi tạo lần đầu
        private static readonly object _lock = new();

        // Thuộc tính static trả về instance duy nhất
        public static ScheduleSlotSingleton Instance
        {
            get
            {
                // Double-check locking: chỉ khởi tạo khi _instance == null
                if (_instance == null)
                {
                    lock (_lock)
                    {
                        _instance ??= new ScheduleSlotSingleton();
                    }
                }

                return _instance;
            }
        }

        // Danh sách các time slot chuẩn (Start, End) chỉ đọc từ bên ngoài
        private readonly IReadOnlyList<(TimeSpan Start, TimeSpan End)> _standardSlots;

        // Constructor private: bên ngoài không thể new ScheduleSlotSingleton,
        // chỉ có class này tự tạo instance bên trong.
        private ScheduleSlotSingleton()
        {
            // Thiết lập cấu hình mặc định:
            // - Bắt đầu 7h sáng
            // - Mỗi slot học 2 tiếng
            // - Giữa 2 slot nghỉ 10 phút
            // - Mỗi ngày có 6 slot
            var slotStartTime = new TimeSpan(7, 0, 0);
            var slotDuration = TimeSpan.FromHours(2);
            var slotGap = TimeSpan.FromMinutes(10);
            const int slotsPerDay = 6;

            // Tính toán danh sách slot dựa trên cấu hình trên
            var slots = new List<(TimeSpan, TimeSpan)>();
            var currentStart = slotStartTime;
            for (var i = 0; i < slotsPerDay; i++)
            {
                var endTime = currentStart + slotDuration;
                slots.Add((currentStart, endTime));
                currentStart = endTime + slotGap;
            }

            // Lưu vào field readonly
            _standardSlots = slots;
        }

        // Hàm public cho phép controller lấy danh sách time slot chuẩn
        public IReadOnlyList<(TimeSpan Start, TimeSpan End)> GetStandardSlots() => _standardSlots;
    }
}


