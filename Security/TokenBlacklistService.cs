using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BIPL_RAASTP2M.Security
{
    public class TokenBlacklistService
    {
        private readonly HashSet<string> _blacklist;

        public TokenBlacklistService()
        {
            _blacklist = new HashSet<string>();
        }

        public async Task RevokeTokenAsync(string token)
        {
            await Task.Run(() => _blacklist.Add(token));
        }

        public bool IsTokenRevoked(string token)
        {
            return _blacklist.Contains(token);
        }


    }
   
}
