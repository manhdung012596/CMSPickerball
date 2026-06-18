using System;
using System.Collections.Generic;

namespace SoccerPitchMvc.Models;

/// <summary>
/// Bảng lưu thông tin các lượt đặt sân bóng đá.
/// </summary>
public partial class Booking
{
    /// <summary>
    /// Mã định danh duy nhất cho lượt đặt sân.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Mã định danh của khách hàng thực hiện đặt sân.
    /// </summary>
    public int CustomerId { get; set; }

    /// <summary>
    /// Mã định danh của sân bóng được đặt.
    /// </summary>
    public int PitchId { get; set; }

    /// <summary>
    /// Mã định danh của khung giờ đặt sân.
    /// </summary>
    public int TimeSlotId { get; set; }

    /// <summary>
    /// Ngày thực hiện đặt sân (Chỉ lưu ngày).
    /// </summary>
    public DateOnly BookingDate { get; set; }

    /// <summary>
    /// Trạng thái đặt sân (Ví dụ: 0 = Chờ xác nhận, 1 = Đã xác nhận, 2 = Đã thanh toán, 3 = Hủy).
    /// </summary>
    public int Status { get; set; }

    /// <summary>
    /// Tổng số tiền của lượt đặt sân (Tự động tính toán dựa trên khung giờ/sân).
    /// </summary>
    public decimal TotalPrice { get; set; }

    /// <summary>
    /// Ghi chú thêm từ khách hàng hoặc quản trị viên.
    /// </summary>
    public string? Note { get; set; }

    /// <summary>
    /// Ngày giờ lượt đặt sân được tạo trên hệ thống.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Khách hàng sở hữu lượt đặt sân này.
    /// Mối quan hệ: Một lượt đặt sân thuộc về một khách hàng.
    /// </summary>
    public virtual Customer Customer { get; set; } = null!;

    /// <summary>
    /// Sân bóng của lượt đặt này.
    /// Mối quan hệ: Một lượt đặt sân thuộc về một sân bóng.
    /// </summary>
    public virtual Pitch Pitch { get; set; } = null!;

    /// <summary>
    /// Khung giờ của lượt đặt này.
    /// Mối quan hệ: Một lượt đặt sân thuộc về một khung giờ cụ thể.
    /// </summary>
    public virtual TimeSlot TimeSlot { get; set; } = null!;
}
