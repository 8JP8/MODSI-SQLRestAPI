namespace MODSI_SQLRestAPI.UserAuth.Email
{
    public static class EmailTemplates
    {
        public static string VerificationEmail(string link, string code)
        {
            return $@"
                <p>Olá,</p>
                <p>Clique no link para verificar a sua conta:</p>
                <a href=""{link}"">{link}</a>
                <br><br>
                Código: {code}
                <br><br>
                Se não pediu esta verificação, ignore este email.
            ";
        }

        public static string PasswordResetEmail(string link, string code)
        {
            return $@"
                <p>Olá,</p>
                <p>Clique no link para resetar a sua senha:</p>
                <a href=""{link}"">{link}</a>
                <br><br>
                Código: {code}
                <br><br>
                Se não pediu esta alteração, ignore este email.
            ";
        }
    }
}