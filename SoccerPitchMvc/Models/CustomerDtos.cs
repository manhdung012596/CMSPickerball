using System.ComponentModel.DataAnnotations;

namespace SoccerPitchMvc.Models;

/// <summary>
/// DTO dùng để hiển thị danh sách khách hàng.
/// </summary>
public class CustomerListDto
{
    public int Id { get; set; }
    public string FullName { get; set; } = null!;
    public string PhoneNumber { get; set; } = null!;
    public string? Address { get; set; }
}

/// <summary>
/// DTO dùng khi tạo mới khách hàng.
/// </summary>
public class CreateCustomerDto
{
    [Required(ErrorMessage = "Họ và tên là bắt buộc.")]
    [StringLength(100, ErrorMessage = "Họ và tên không được dài quá 100 ký tự.")]
    public string FullName { get; set; } = null!;

    [Required(ErrorMessage = "Số điện thoại là bắt buộc.")]
    [RegularExpression(@"^(0[3|5|7|8|9])[0-9]{8}$", ErrorMessage = "Số điện thoại Việt Nam không hợp lệ.")]
    public string PhoneNumber { get; set; } = null!;

    [StringLength(200, ErrorMessage = "Địa chỉ không được dài quá 200 ký tự.")]
    public string? Address { get; set; }
}

/// <summary>
/// DTO dùng khi cập nhật thông tin khách hàng.
/// </summary>
public class UpdateCustomerDto
{
    [Required]
    public int Id { get; set; }

    [Required(ErrorMessage = "Họ và tên là bắt buộc.")]
    [StringLength(100, ErrorMessage = "Họ và tên không được dài quá 100 ký tự.")]
    public string FullName { get; set; } = null!;

    [Required(ErrorMessage = "Số điện thoại là bắt buộc.")]
    [RegularExpression(@"^(0[3|5|7|8|9])[0-9]{8}$", ErrorMessage = "Số điện thoại Việt Nam không hợp lệ.")]
    public string PhoneNumber { get; set; } = null!;

    [StringLength(200, ErrorMessage = "Địa chỉ không được dài quá 200 ký tự.")]
    public string? Address { get; set; }
}
