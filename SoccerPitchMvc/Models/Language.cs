using System;
using System.Collections.Generic;

namespace SoccerPitchMvc.Models;

/// <summary>
/// Bảng lưu thông tin các ngôn ngữ hỗ trợ trong hệ thống CMS.
/// </summary>
public partial class Language
{
    /// <summary>
    /// Mã định danh duy nhất của ngôn ngữ.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Mã viết tắt của ngôn ngữ (Ví dụ: VI, EN, CN, FR).
    /// </summary>
    public string Code { get; set; } = null!;

    /// <summary>
    /// Tên đầy đủ của ngôn ngữ (Ví dụ: Tiếng Việt, English).
    /// </summary>
    public string Name { get; set; } = null!;

    /// <summary>
    /// Tên thay thế của ngôn ngữ (Ví dụ: Vietnamese, Chinese).
    /// </summary>
    public string? AlternateName { get; set; }

    /// <summary>
    /// Mô tả chi tiết về ngôn ngữ.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Thứ tự sắp xếp hiển thị trên giao diện.
    /// </summary>
    public int SortOrder { get; set; }

    /// <summary>
    /// Trạng thái đăng (1 = Đang Đăng, 0 = Ngừng Đăng).
    /// </summary>
    public int Status { get; set; }
}
