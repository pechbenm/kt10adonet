namespace KT10.Models;

public static class Rules
{
    public const string UsernamePattern = @"^\p{L}+\z";                       
    public const string EmailPattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+\z";       
    public const string PasswordPattern = @"^(?=.*\p{L})(?=.*\d).+\z";        

    public const int PasswordMinLength = 8;
    public const int PasswordMaxLength = 100;   

    public const string PasswordLengthMessage = "Пароль должен содержать от {2} до {1} символов.";
    public const string PasswordPatternMessage = "Пароль должен содержать и буквы, и цифры.";
}