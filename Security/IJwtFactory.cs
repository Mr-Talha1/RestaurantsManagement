
using TBAppBackend.Models;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
 namespace TBAppBackend.Security
{
    public interface IJwtFactory
    {
        //Task<string> GenerateEncodedToken(SMSystemUsers smsystemUsers); 
        //Task<string> LoginToken(SMSystemUsers userLogin);
        Task<string> LoginToken(string Role, string UserID, string MerchantId, string BranchId);
        //Task<string> LoginAffiliateToken(SMSystemUsers userLogin);
        Task<getTokenDetails> ValidateJwtToken(string Token);
        Task<string> GetMobileNo(string token);
        public Task RevokeTokenAsync(string token);
    }
}
