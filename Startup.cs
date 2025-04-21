using Microsoft.IdentityModel.Tokens;
using Microsoft.Owin;
using Microsoft.Owin.Cors;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Jwt;
using Owin;
using System.Text;
using System.Web.Http;

[assembly: OwinStartup(typeof(Startup))]

public class Startup
{
    public void Configuration(IAppBuilder app)
    {
        var config = new HttpConfiguration();

        var key = Encoding.ASCII.GetBytes("chave-super-secreta");

        app.UseCors(CorsOptions.AllowAll);

        app.UseJwtBearerAuthentication(new JwtBearerAuthenticationOptions
        {
            AuthenticationMode = AuthenticationMode.Active,
            TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = false,
                ValidateAudience = false,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key)
            }
        });

        config.MapHttpAttributeRoutes();

        app.UseWebApi(config);
    }
}
