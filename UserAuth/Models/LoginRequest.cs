namespace MODSI_SQLRestAPI.UserAuth.Models.User
{
    public class LoginRequest
    {
        public string Email { get; set; }
        public string Password { get; set; }
        public string Username { get; set; } // Novo atributo

        public LoginRequest() { }

        public LoginRequest(string email, string password, string username)
        {
            Email = email;
            Password = password;
            Username = username;
        }

        // Métodos Get
        public string GetEmail()
        {
            return Email;
        }

        public string GetPassword()
        {
            return Password;
        }

        public string GetUsername()
        {
            return Username;
        }

        // Métodos Set
        public void SetEmail(string email)
        {
            Email = email;
        }

        public void SetPassword(string password)
        {
            Password = password;
        }

        public void SetUsername(string username)
        {
            Username = username;
        }
    }
}

