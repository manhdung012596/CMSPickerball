using System;
using System.Collections.Generic;

namespace SoccerPitchMvc.Models;

/// <summary>
/// Bảng lưu thông tin các khung giờ cố định trong ngày cho từng sân bóng.
/// </summary>
public partial class TimeSlot
{
    /// <summary>
    /// Mã định danh duy nhất cho khung giờ.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Mã định danh của sân bóng liên kết.
    /// </summary>
    public int PitchId { get; set; }

    /// <summary>
    /// Thời gian bắt đầu khung giờ.
    /// </summary>
    public TimeOnly StartTime { get; set; }

    /// <summary>
    /// Thời gian kết thúc khung giờ.
    /// </summary>
    public TimeOnly EndTime { get; set; }

    /// <summary>
    /// Giá thuê riêng cho khung giờ này (Ví dụ: Giờ cao điểm). Nếu NULL sẽ sử dụng giá mặc định của sân.
    /// </summary>
    public decimal? PriceOverride { get; set; }

    /// <summary>
    /// Danh sách các lượt đặt sân liên kết với khung giờ này.
    /// Mối quan hệ: Một khung giờ có nhiều lượt đặt sân (ở các ngày khác nhau).
    /// </summary>
    public virtual ICollection<Booking> Bookings { get; set; } = new List<Booking>();

    /// <summary>
    /// Thực thể sân bóng sở hữu khung giờ này.
    /// Mối quan hệ: Một khung giờ thuộc về một sân bóng.
    /// </summary>
    public virtual Pitch Pitch { get; set; } = null!;
}
