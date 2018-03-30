using Lykke.Service.BitfinexAdapter.Core.Domain;
using Lykke.Service.BitfinexAdapter.Core.Domain.Settings;
using Lykke.Service.BitfinexAdapter.Core.Domain.Trading;
using System;
using System.Collections.Generic;
using System.Linq;

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
            var foundSymbol = _currencySymbols.FirstOrDefault(s => s.LykkeSymbol == lykkeSymbol);
            if (foundSymbol == null && _useSupportedCurrencySymbolsAsFilter)
            {
                throw new ArgumentException($"Symbol {lykkeSymbol} is not mapped to {Constants.BitfinexExchangeName} value");
            }
            return foundSymbol?.ExchangeSymbol ?? lykkeSymbol;
        }

        public Instrument ExchangeSymbolToLykkeInstrument(string exchangeSymbol)
        {
            var foundSymbol = _currencySymbols.FirstOrDefault(s => s.ExchangeSymbol == exchangeSymbol);
            if (foundSymbol == null && _useSupportedCurrencySymbolsAsFilter)
            {
                throw new ArgumentException(
                    $"Symbol {exchangeSymbol} in {Constants.BitfinexExchangeName} is not mapped to lykke value");
            }
            return new Instrument(foundSymbol?.LykkeSymbol ?? exchangeSymbol);
        }
    }
}
