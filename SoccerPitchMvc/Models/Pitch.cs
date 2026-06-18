using System;
using System.Collections.Generic;

namespace SoccerPitchMvc.Models;

/// <summary>
/// Bảng lưu thông tin của các sân bóng đá cỏ nhân tạo.
/// </summary>
public partial class Pitch
{
    /// <summary>
    /// Mã định danh duy nhất cho sân bóng.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Tên của sân bóng (Ví dụ: Sân A, Sân B).
    /// </summary>
    public string Name { get; set; } = null!;

    /// <summary>
    /// Loại sân bóng (Ví dụ: 5 người, 7 người, 11 người).
    /// </summary>
    public int Type { get; set; }

    /// <summary>
    /// Giá thuê mặc định theo giờ của sân bóng.
    /// </summary>
    public decimal PricePerHour { get; set; }

    /// <summary>
    /// Trạng thái hoạt động của sân (Ví dụ: Active = 0, Maintenance = 1).
    /// </summary>
    public int Status { get; set; }

    /// <summary>
    /// Ngày giờ sân bóng được khởi tạo trên hệ thống.
    /// </summary>
    public DateTime CreatedDate { get; set; }

    /// <summary>
    /// Danh sách các lượt đặt sân liên kết với sân bóng này.
    /// Mối quan hệ: Một sân có nhiều lượt đặt sân.
    /// </summary>
    public virtual ICollection<Booking> Bookings { get; set; } = new List<Booking>();

    /// <summary>
    /// Danh sách các khung giờ cấu hình cho sân bóng này.
    /// Mối quan hệ: Một sân có nhiều khung giờ hoạt động.
    /// </summary>
    public virtual ICollection<TimeSlot> TimeSlots { get; set; } = new List<TimeSlot>();
}
