using KT10.Models;
using System.ComponentModel.DataAnnotations;

namespace KT10.Models;

public abstract class UserInputBase
{
    [Required(ErrorMessage = "Введите имя пользователя.")]
    [StringLength(50, MinimumLength = 3, ErrorMessage = "Имя пользователя должно содержать от {2} до {1} символов.")]
    [RegularExpression(Rules.UsernamePattern, ErrorMessage = "Имя пользователя может содержать только буквы.")]
    [Display(Name = "Имя пользователя")]
    public string Username { get; set; } = string.Empty;

    [Required(ErrorMessage = "Введите адрес электронной почты.")]
    [StringLength(254, ErrorMessage = "Email не должен быть длиннее {1} символов.")]
    [RegularExpression(Rules.EmailPattern,
        ErrorMessage = "Email должен содержать символ @ и точку в домене, например name@example.com.")]
    [Display(Name = "Электронная почта")]
    public string Email { get; set; } = string.Empty;
}

public class RegisterViewModel : UserInputBase
{
    [Required(ErrorMessage = "Введите пароль.")]
    [StringLength(Rules.PasswordMaxLength, MinimumLength = Rules.PasswordMinLength,
        ErrorMessage = Rules.PasswordLengthMessage)]
    [RegularExpression(Rules.PasswordPattern, ErrorMessage = Rules.PasswordPatternMessage)]
    [DataType(DataType.Password)]
    [Display(Name = "Пароль")]
    public string Password { get; set; } = string.Empty;

    [Required(ErrorMessage = "Подтвердите пароль.")]
    [Compare(nameof(Password), ErrorMessage = "Пароли не совпадают.")]
    [DataType(DataType.Password)]
    [Display(Name = "Подтверждение пароля")]
    public string ConfirmPassword { get; set; } = string.Empty;
}

public class CreateUserRequest : UserInputBase
{
    [Required(ErrorMessage = "Введите пароль.")]
    [StringLength(Rules.PasswordMaxLength, MinimumLength = Rules.PasswordMinLength,
        ErrorMessage = Rules.PasswordLengthMessage)]
    [RegularExpression(Rules.PasswordPattern, ErrorMessage = Rules.PasswordPatternMessage)]
    public string Password { get; set; } = string.Empty;
}

public class UpdateUserRequest : UserInputBase
{
    [StringLength(Rules.PasswordMaxLength, MinimumLength = Rules.PasswordMinLength,
        ErrorMessage = Rules.PasswordLengthMessage)]
    [RegularExpression(Rules.PasswordPattern, ErrorMessage = Rules.PasswordPatternMessage)]
    public string? Password { get; set; }
}

public record UserResponse(int Id, string Username, string Email, DateTime CreatedAt)
{
    public static UserResponse From(AppUser u) => new(u.Id, u.Username, u.Email, u.CreatedAt);
}