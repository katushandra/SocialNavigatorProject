using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Common.Helpers
{
    public static class PasswordValidator
    {
        public static List<string> Valid(string password)
        {
            var errors = new List<string>();

            if (password.Length < 6)
            {
                errors.Add("Пароль должен быть не менее 6 символов");
            }

            if (!password.Any(char.IsDigit))
            {
                errors.Add("Пароль должен содержать хотя бы одну цифру");
            }

            if (!password.Any(char.IsLower))
            {
                errors.Add("Пароль должен содержать хотя бы одну строчную букву");
            }

            if (!password.Any(char.IsUpper))
            {
                errors.Add("Пароль должен содержать хотя бы одну заглавную букву");
            }

            if (!password.Any(c => !char.IsLetterOrDigit(c)))
            {
                errors.Add("Пароль должен содержать хотя бы один специальный символ");
            }
            return errors;
        }
    }
}
