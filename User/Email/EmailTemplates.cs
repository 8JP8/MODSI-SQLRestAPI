namespace MODSI_SQLRestAPI.UserAuth.Email
{
    public static class EmailTemplates
    {
        public static string VerificationEmail(string link, string code)
        {
            return $@"
                <p>Clique no link para verificar a sua conta:</p>
                Código: {code}
                <br><br>
                <a href=""{link}"">{link}</a>
                <br><br>
            ";
        }

        public static string PasswordResetEmail(string link, string code)
        {
            return $@"
                <p>Clique no link para resetar a sua senha:</p>
                Código: {code}
                <br><br>
                <a href=""{link}"">{link}</a>
                <br><br>
                Se não pediu esta alteração, ignore este email.
            ";
        }
    }
}