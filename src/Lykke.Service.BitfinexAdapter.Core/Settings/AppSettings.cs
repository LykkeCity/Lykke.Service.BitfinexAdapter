using Lykke.Service.BitfinexAdapter.Core.Domain.Settings;
using Lykke.Service.BitfinexAdapter.Core.Settings.ServiceSettings;
using Lykke.Service.BitfinexAdapter.Core.Settings.SlackNotifications;

namespace Lykke.Service.BitfinexAdapter.Core.Settings
{
    public class AppSettings
    {
        public BitfinexAdapterSettings BitfinexAdapterSettings { get; set; }
        public DbSettings Db { get; set; }
        public RabbitMqConfiguration RabbitMq { get; set; }
        public SlackNotificationsSettings SlackNotifications { get; set; }
    }
}
