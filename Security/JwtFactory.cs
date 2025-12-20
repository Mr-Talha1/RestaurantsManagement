
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using BIPL_RAASTP2M.Security;
using System.IO;
using Microsoft.Extensions.Configuration;
using BIPL_RAASTP2M.Models;

namespace BIPL_RAASTP2M.Security
{
    public class JwtFactory : IJwtFactory
    {
        private readonly JwtIssuerOptions _jwtOptions;
        private readonly TokenBlacklistService _tokenBlacklistService;

        public JwtFactory(IOptions<JwtIssuerOptions> jwtOptions, TokenBlacklistService tokenBlacklistService)
        {
            _jwtOptions = jwtOptions.Value;
            ThrowIfInvalidOptions(_jwtOptions);
            _tokenBlacklistService = tokenBlacklistService;
        }

        //comment for wrapper
        //public async Task<string> GenerateEncodedToken(SMSystemUsers smsystemUsers)
        //{
        //    _jwtOptions.ResetTokenIssuanceDate();
        //    var claims = new[] {
        //         new Claim(JwtRegisteredClaimNames.Sub, smsystemUsers.UserID),
        //         new Claim(JwtRegisteredClaimNames.Jti, await _jwtOptions.JtiGenerator()),
        //         new Claim(JwtRegisteredClaimNames.Iat, ToUnixEpochDate(_jwtOptions.IssuedAt).ToString(), ClaimValueTypes.Integer64),
        //         ////identity.FindFirst(Helpers.Constants.Strings.JwtClaimIdentifiers.Rol),
        //         ////identity.FindFirst(Helpers.Constants.Strings.JwtClaimIdentifiers.Id)
        //     };

        //    // Create the JWT security token and encode it.
        //    var jwt = new JwtSecurityToken(
        //        issuer: _jwtOptions.Issuer,
        //        audience: _jwtOptions.Audience,
        //        claims: claims,
        //        notBefore: _jwtOptions.NotBefore,
        //        expires: _jwtOptions.Expiration,
        //        signingCredentials: _jwtOptions.SigningCredentials);

        //    var encodedJwt = new JwtSecurityTokenHandler().WriteToken(jwt); //token

        //    return encodedJwt;
        //}

        //public async Task<string> LoginToken(SMSystemUsers userLogin)
        //{
        //    string roleid;
        //    string groupid;
        //    if (userLogin.RoleID == null)
        //    {
        //        roleid = "";
        //    }
        //    else
        //    {
        //        roleid = userLogin.RoleID.ToString();
        //    }
        //    if (userLogin.GroupID == null)
        //    {
        //        groupid = "";
        //    }
        //    else
        //    {
        //        groupid = userLogin.GroupID.ToString();
        //    }

        //    _jwtOptions.ResetTokenIssuanceDate();
        //    var claims = new[] {
        //         new Claim(JwtRegisteredClaimNames.Sub, userLogin.UserID),
        //         new Claim(JwtRegisteredClaimNames.Jti, await _jwtOptions.JtiGenerator()),
        //         new Claim(JwtRegisteredClaimNames.Iat, ToUnixEpochDate(_jwtOptions.IssuedAt).ToString(), ClaimValueTypes.Integer64),
        //         new Claim(ClaimTypes.Name, userLogin.InstitutionID),
        //         new Claim(ClaimTypes.Role, roleid),
        //         new Claim(ClaimTypes.GivenName, groupid),
        //         ////identity.FindFirst(Helpers.Constants.Strings.JwtClaimIdentifiers.Rol),
        //         ////identity.FindFirst(Helpers.Constants.Strings.JwtClaimIdentifiers.Id)
        //     };
        //    var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("64A63153-11C1-4919-9133-EFAF99A9B456"));
        //    // Create the JWT security token and encode it.
        //    var jwt = new JwtSecurityToken(
        //        issuer: _jwtOptions.Issuer,
        //        audience: _jwtOptions.Audience,
        //        claims: claims,
        //        notBefore: DateTime.Now,
        //        expires: DateTime.Now.AddHours(3),
        //        signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
        //            );

        //    //DateTime.Now.AddHours(3)
        //    var encodedJwt = new JwtSecurityTokenHandler().WriteToken(jwt); //token

        //    return encodedJwt;
        //}

        public async Task<string> LoginToken(string Role, string UserID, string MerchantId)
        {
            _jwtOptions.ResetTokenIssuanceDate();
            var claims = new[] {
                 new Claim(JwtRegisteredClaimNames.Sub, Role),
                  new Claim(JwtRegisteredClaimNames.UniqueName, UserID),
                  //new Claim(JwtRegisteredClaimNames.GivenName, Iban),
                  //new Claim(JwtRegisteredClaimNames.Name, AccountNumber),
                  new Claim(JwtRegisteredClaimNames.NameId, MerchantId),
                 new Claim(JwtRegisteredClaimNames.Jti, await _jwtOptions.JtiGenerator()),
                 new Claim(JwtRegisteredClaimNames.Iat, ToUnixEpochDate(_jwtOptions.IssuedAt).ToString(), ClaimValueTypes.Integer64),
                 
                 ////identity.FindFirst(Helpers.Constants.Strings.JwtClaimIdentifiers.Rol),
                 ////identity.FindFirst(Helpers.Constants.Strings.JwtClaimIdentifiers.Id)
             };
            var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("64A63153-11C1-4919-9133-EFAF99A9B456"));
            // Create the JWT security token and encode it.
            var jwt = new JwtSecurityToken(
                issuer: _jwtOptions.Issuer,
                audience: _jwtOptions.Audience,
                claims: claims,
                notBefore: _jwtOptions.NotBefore,
                expires: DateTime.Now.AddHours(3),
                signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
                    );

            var encodedJwt = new JwtSecurityTokenHandler().WriteToken(jwt); //token

            return encodedJwt;
        }

        public async Task<string> GetMobileNo(string token)
        {
            if (token == null)
                return null;

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes("64A63153-11C1-4919-9133-EFAF99A9B456");
            try
            {
                tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    // set clockskew to zero so tokens expire exactly at token expiration time (instead of 5 minutes later)
                    ClockSkew = TimeSpan.Zero
                }, out SecurityToken validatedToken);

                var jwtToken = (JwtSecurityToken)validatedToken;
                var MobileNo = jwtToken.Claims.First(x => x.Type == ClaimTypes.Name).Value;

                // return user id from JWT token if validation successful
                return MobileNo;
            }
            catch
            {
                // return null if validation fails
                return null;
            }
        }


        public async Task<getTokenDetails> ValidateJwtToken(string token)
        {
            if (token == null)
                return null;

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes("64A63153-11C1-4919-9133-EFAF99A9B456");
            try
            {
                tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    // set clockskew to zero so tokens expire exactly at token expiration time (instead of 5 minutes later)
                    ClockSkew = TimeSpan.Zero
                }, out SecurityToken validatedToken);

                var jwtToken = (JwtSecurityToken)validatedToken;
                // var userId = jwtToken.Claims.First(x => x.Type == ClaimTypes.Name).Value;
                var UserID = jwtToken.Claims.First(x => x.Type == JwtRegisteredClaimNames.UniqueName).Value;
                var Role = jwtToken.Claims.First(x => x.Type == JwtRegisteredClaimNames.Sub).Value;
                //var Iban = jwtToken.Claims.First(x => x.Type == JwtRegisteredClaimNames.GivenName).Value;
                //var AccountNumber = jwtToken.Claims.First(x => x.Type == JwtRegisteredClaimNames.Name).Value;
                var MerchantId = jwtToken.Claims.First(x => x.Type == JwtRegisteredClaimNames.NameId).Value;

                if (_tokenBlacklistService.IsTokenRevoked(token))
                {
                    // Token is revoked, deny access
                    return null;
                }
                getTokenDetails dt = new getTokenDetails();
                dt.UserID = UserID;
                dt.Role = Role;
                dt.MerchantId = long.Parse(MerchantId);
                // return user id from JWT token if validation successful
                return dt;
            }
            catch
            {
                getTokenDetails dt = new getTokenDetails();// return null if validation fails
                return dt;
            }
        }


        public async Task RevokeTokenAsync(string token)
        {
            await _tokenBlacklistService.RevokeTokenAsync(token);
            // Optionally perform additional logout logic here
        }


        private static long ToUnixEpochDate(DateTime date)
            => (long)Math.Round((date.ToUniversalTime() -
                new DateTimeOffset(1970, 1, 1, 0, 0, 0, TimeSpan.Zero))
                .TotalSeconds);
        private static void ThrowIfInvalidOptions(JwtIssuerOptions options)
        {
            if (options == null) throw new ArgumentNullException(nameof(options));

            if (options.ValidFor <= TimeSpan.Zero)
            {
                throw new ArgumentException("Must be a non-zero TimeSpan.", nameof(JwtIssuerOptions.ValidFor));
            }

            if (options.SigningCredentials == null)
            {
                throw new ArgumentNullException(nameof(JwtIssuerOptions.SigningCredentials));
            }

            if (options.JtiGenerator == null)
            {
                throw new ArgumentNullException(nameof(JwtIssuerOptions.JtiGenerator));
            }
        }
    }
}
