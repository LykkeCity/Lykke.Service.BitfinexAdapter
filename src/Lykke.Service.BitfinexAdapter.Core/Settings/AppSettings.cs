using Lykke.Service.BitfinexAdapter.Core.Domain.Settings;
using Lykke.Service.BitfinexAdapter.Core.Settings.SlackNotifications;

namespace Lykke.Service.BitfinexAdapter.Core.Settings
{
    public class AppSettings
    {
        public BitfinexAdapterSettings BitfinexAdapterService { get; set; }
        public SlackNotificationsSettings SlackNotifications { get; set; }
    }
}
