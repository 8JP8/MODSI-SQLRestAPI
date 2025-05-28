namespace MODSI_SQLRestAPI.UserAuth.Email
{
    public static class EmailTemplates
    {
        public static string VerificationEmail(string link, string code)
        {
            return $@"
                <p>Clique no link para verificar a sua conta:</p>
                C�digo: {code}
                <br><br>
                <a href=""{link}"">{link}</a>
                <br><br>
            ";
        }

        public static string PasswordResetEmail(string link, string code)
        {
            return $@"
                <p>Clique no link para resetar a sua senha:</p>
                C�digo: {code}
                <br><br>
                <a href=""{link}"">{link}</a>
                <br><br>
                Se n�o pediu esta altera��o, ignore este email.
            ";
        }
    }
}