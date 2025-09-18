using System;

namespace EventManagement.Application.Helpers
{
    public static class PasswordHelper
    {
        /// <summary>
        /// Hash mật khẩu sử dụng BCrypt
        /// </summary>
        public static string HashPassword(string password, int workFactor = 12)
        {
            if (string.IsNullOrEmpty(password))
                throw new ArgumentException("Mật khẩu không được để trống", nameof(password));

            if (workFactor < 4 || workFactor > 31)
                throw new ArgumentException("Work factor phải nằm trong khoảng 4-31", nameof(workFactor));

            return BCrypt.Net.BCrypt.HashPassword(password, workFactor);
        }

        /// <summary>
        /// Xác thực mật khẩu với hash đã lưu
        /// </summary>
        public static bool VerifyPassword(string password, string hashedPassword)
        {
            if (string.IsNullOrEmpty(password))
                throw new ArgumentException("Mật khẩu không được để trống", nameof(password));

            if (string.IsNullOrEmpty(hashedPassword))
                throw new ArgumentException("Hash không được để trống", nameof(hashedPassword));

            return BCrypt.Net.BCrypt.Verify(password, hashedPassword);
        }
    }
}