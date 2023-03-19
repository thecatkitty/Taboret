using Discord.Rest;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace Taboret.Areas.Identity
{
    public static class DiscordExtensions
    {
        public static async Task<RestSelfUser> GetDiscordUser(this HttpContext context) => (await DiscordHelper.GetClientAsync(context)).CurrentUser;
    }
}