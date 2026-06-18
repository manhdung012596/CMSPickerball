using System;
using System.Collections.Generic;

namespace SoccerPitchMvc.Models;

/// <summary>
/// Bảng lưu thông tin khách hàng sử dụng dịch vụ đặt sân.
/// </summary>
public partial class Customer
{
    /// <summary>
    /// Mã định danh duy nhất cho khách hàng.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Mã định danh tài khoản liên kết với Identity User.
    /// </summary>
    public string? UserId { get; set; }

    /// <summary>
    /// Họ và tên đầy đủ của khách hàng.
    /// </summary>
    public string FullName { get; set; } = null!;

    /// <summary>
    /// Số điện thoại liên lạc của khách hàng (Bắt buộc).
    /// </summary>
    public string PhoneNumber { get; set; } = null!;

    /// <summary>
    /// Địa chỉ của khách hàng.
    /// </summary>
    public string? Address { get; set; }

    /// <summary>
    /// Danh sách các lượt đặt sân mà khách hàng này đã thực hiện.
    /// Mối quan hệ: Một khách hàng có thể đặt sân nhiều lần.
    /// </summary>
    public virtual ICollection<Booking> Bookings { get; set; } = new List<Booking>();
}
