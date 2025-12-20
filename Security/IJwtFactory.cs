
using BIPL_RAASTP2M.Models;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
 namespace BIPL_RAASTP2M.Security
{
    public interface IJwtFactory
    {
        //Task<string> GenerateEncodedToken(SMSystemUsers smsystemUsers); 
        //Task<string> LoginToken(SMSystemUsers userLogin);
        Task<string> LoginToken(string Role, string UserID, string MerchantId);
        //Task<string> LoginAffiliateToken(SMSystemUsers userLogin);
        Task<getTokenDetails> ValidateJwtToken(string Token);
        Task<string> GetMobileNo(string token);
        public Task RevokeTokenAsync(string token);
    }
}
