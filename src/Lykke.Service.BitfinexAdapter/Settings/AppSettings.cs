using Lykke.Service.BitfinexAdapter.Settings.ServiceSettings;
using Lykke.Service.BitfinexAdapter.Settings.SlackNotifications;

namespace Lykke.Service.BitfinexAdapter.Settings
{
    public class AppSettings
    {
        public BitfinexAdapterSettings BitfinexAdapterService { get; set; }
        public SlackNotificationsSettings SlackNotifications { get; set; }
    }
}
