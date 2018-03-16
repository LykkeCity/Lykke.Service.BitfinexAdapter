using System;
using Common.Log;

namespace Lykke.Service.BitfinexAdapter.Client
{
    public class BitfinexAdapterClient : IBitfinexAdapterClient, IDisposable
    {
        private readonly ILog _log;

        public BitfinexAdapterClient(string serviceUrl, ILog log)
        {
            _log = log;
        }

        public void Dispose()
        {
            //if (_service == null)
            //    return;
            //_service.Dispose();
            //_service = null;
        }
    }
}
