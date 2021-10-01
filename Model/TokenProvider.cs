using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Threading.Tasks;

namespace JWT_Calisma.Model
{
    public class TokenProvider
    {
        static IHttpContextAccessor contextAccessor;
        public TokenProvider(IHttpContextAccessor x)
        {
            contextAccessor = x;
        }

        public static string Token = null;

        public static string GetToken()
        {
            var handler = new JwtSecurityTokenHandler();
            var result = handler.ReadJwtToken(Token);
            return result.ToString();
        }
    }
}
