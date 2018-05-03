using Lykke.Service.BitfinexAdapter.Core.Domain;
using Lykke.Service.BitfinexAdapter.Core.Domain.Exceptions;
using Lykke.Service.BitfinexAdapter.Core.Domain.Settings;
using Lykke.Service.BitfinexAdapter.Core.Domain.Trading;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace Lykke.Service.BitfinexAdapter.Core.Utils
{
    public class ExchangeConverters
    {
        private readonly IReadOnlyCollection<CurrencySymbol> _currencySymbols;
        private readonly bool _useSupportedCurrencySymbolsAsFilter;

        public ExchangeConverters(IReadOnlyCollection<CurrencySymbol> currencySymbols,
            bool useSupportedCurrencySymbolsAsFilter)
        {
            _currencySymbols = currencySymbols;
            _useSupportedCurrencySymbolsAsFilter = useSupportedCurrencySymbolsAsFilter;
        }

        public string LykkeSymbolToExchangeSymbol(string lykkeSymbol)
        {
            var foundSymbol = _currencySymbols.FirstOrDefault(s => s.LykkeSymbol.Equals(lykkeSymbol, StringComparison.InvariantCultureIgnoreCase));
            if (foundSymbol == null && _useSupportedCurrencySymbolsAsFilter)
            {
                throw new ApiException($"Symbol {lykkeSymbol} is not mapped to {Constants.BitfinexExchangeName} value", HttpStatusCode.BadRequest, ApiErrorCode.IncorrectInstrument );
            }
            return foundSymbol?.ExchangeSymbol ?? lykkeSymbol;
        }

        public Instrument ExchangeSymbolToLykkeInstrument(string exchangeSymbol)
        {
            var foundSymbol = _currencySymbols.FirstOrDefault(s => s.ExchangeSymbol.Equals(exchangeSymbol, StringComparison.InvariantCultureIgnoreCase) );
            if (foundSymbol == null && _useSupportedCurrencySymbolsAsFilter)
            {
                throw new ApiException($"Symbol {exchangeSymbol} in {Constants.BitfinexExchangeName} is not mapped to lykke value", HttpStatusCode.InternalServerError, ApiErrorCode.IncorrectInstrument);
            }
            return new Instrument(foundSymbol?.LykkeSymbol ?? exchangeSymbol);
        }
    }
}
