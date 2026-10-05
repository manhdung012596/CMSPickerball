using System.ComponentModel.DataAnnotations;

namespace SoccerPitchMvc.Models;

public class RegisterViewModel
{
    [Required(ErrorMessage = "Họ và tên là bắt buộc.")]
    [StringLength(100, ErrorMessage = "Họ và tên không được dài quá 100 ký tự.")]
    public string FullName { get; set; } = null!;

    [Required(ErrorMessage = "Email là bắt buộc.")]
    [EmailAddress(ErrorMessage = "Email không đúng định dạng.")]
    public string Email { get; set; } = null!;

    [Required(ErrorMessage = "Số điện thoại là bắt buộc.")]
    [RegularExpression(@"^(0[3|5|7|8|9])[0-9]{8}$", ErrorMessage = "Số điện thoại Việt Nam không hợp lệ.")]
    public string PhoneNumber { get; set; } = null!;

    [Required(ErrorMessage = "Mật khẩu là bắt buộc.")]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "Mật khẩu phải từ 6 ký tự trở lên.")]
    [DataType(DataType.Password)]
    public string Password { get; set; } = null!;

    [DataType(DataType.Password)]
    [Display(Name = "Xác nhận mật khẩu")]
    [Compare("Password", ErrorMessage = "Mật khẩu và xác nhận mật khẩu không giống nhau.")]
    public string ConfirmPassword { get; set; } = null!;
}

public class LoginViewModel
{
    [Required(ErrorMessage = "Email hoặc Số điện thoại là bắt buộc.")]
    public string EmailOrPhone { get; set; } = null!;

    [Required(ErrorMessage = "Mật khẩu là bắt buộc.")]
    [DataType(DataType.Password)]
    public string Password { get; set; } = null!;

    [Display(Name = "Ghi nhớ đăng nhập")]
    public bool RememberMe { get; set; }
}

public class ProfileViewModel
{
    [Required(ErrorMessage = "Họ và tên là bắt buộc.")]
    [StringLength(100, ErrorMessage = "Họ và tên không được dài quá 100 ký tự.")]
    public string FullName { get; set; } = null!;

    [Required(ErrorMessage = "Số điện thoại là bắt buộc.")]
    [RegularExpression(@"^(0[3|5|7|8|9])[0-9]{8}$", ErrorMessage = "Số điện thoại Việt Nam không hợp lệ.")]
    public string PhoneNumber { get; set; } = null!;

    public string? Email { get; set; }

    [StringLength(200, ErrorMessage = "Địa chỉ không được dài quá 200 ký tự.")]
    public string? Address { get; set; }
}

public class ChangePasswordViewModel
{
    [Required(ErrorMessage = "Mật khẩu hiện tại là bắt buộc.")]
    [DataType(DataType.Password)]
    public string OldPassword { get; set; } = null!;

    [Required(ErrorMessage = "Mật khẩu mới là bắt buộc.")]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "Mật khẩu mới phải từ 6 ký tự trở lên.")]
    [DataType(DataType.Password)]
    public string NewPassword { get; set; } = null!;

    [DataType(DataType.Password)]
    [Compare("NewPassword", ErrorMessage = "Mật khẩu mới và xác nhận mật khẩu mới không giống nhau.")]
    public string ConfirmNewPassword { get; set; } = null!;
}
