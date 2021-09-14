using System.Collections.Generic;
using System.Threading.Tasks;
using Discord;
using Discord.Rest;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;

namespace KucykoweRodeo.Areas.Identity
{
    public static class DiscordHelper
    {
        private static readonly Dictionary<string, DiscordRestClient> s_cache = new();

        public static async Task<DiscordRestClient> GetClientAsync(string accessToken)
        {
            if (s_cache.ContainsKey(accessToken)) return s_cache[accessToken];

            s_cache[accessToken] = new DiscordRestClient();
            await s_cache[accessToken].LoginAsync(TokenType.Bearer, accessToken);
            return s_cache[accessToken];
        }

        public static async Task<DiscordRestClient> GetClientAsync(HttpContext context)
        {
            var accessToken = await context.GetTokenAsync("access_token");
            if (accessToken is null) return null;

            return await GetClientAsync(accessToken);
        }
    }
}
