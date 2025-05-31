using System;

namespace MODSI_SQLRestAPI.UserAuth.DTO
{
    public class PasswordResetRequestDTO
    {
        public string Email { get; set; }
    }

    public class EmailVerificationDTO
    {
        public string Email { get; set; }
    }

    public class PasswordResetCodeEntry
    {
        public int UserId { get; set; }
        public string Code { get; set; }
        public DateTime Expiration { get; set; }
    }
}
