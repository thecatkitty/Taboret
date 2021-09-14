using System.Threading.Tasks;
using Discord.Rest;
using Microsoft.AspNetCore.Http;

namespace KucykoweRodeo.Areas.Identity
{
    public static class DiscordExtensions
    {
        public static async Task<RestSelfUser> GetDiscordUser(this HttpContext context) => (await DiscordHelper.GetClientAsync(context)).CurrentUser;
    }
}