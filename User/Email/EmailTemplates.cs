namespace MODSI_SQLRestAPI.UserAuth.Email
{
    public static class EmailTemplates
    {
        public static string VerificationEmail(string link, string code)
        {
            return $@"
                <p>Ol�,</p>
                <p>Clique no link para verificar a sua conta:</p>
                <a href=""{link}"">{link}</a>
                <br><br>
                C�digo: {code}
                <br><br>
                Se n�o pediu esta verifica��o, ignore este email.
            ";
        }

        public static string PasswordResetEmail(string link, string code)
        {
            return $@"
                <p>Ol�,</p>
                <p>Clique no link para resetar a sua senha:</p>
                <a href=""{link}"">{link}</a>
                <br><br>
                C�digo: {code}
                <br><br>
                Se n�o pediu esta altera��o, ignore este email.
            ";
        }
    }
}