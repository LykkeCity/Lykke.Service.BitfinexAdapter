using Lykke.Service.BitfinexAdapter.Core.Domain.OrderBooks;
using Lykke.Service.BitfinexAdapter.Core.Domain.Settings;
using Lykke.Service.BitfinexAdapter.Core.Domain.Trading;
using Lykke.Service.BitfinexAdapter.Core.Domain.Trading.Enums;
using Lykke.Service.BitfinexAdapter.Core.Domain.WebSocketClient;
using System;
using System.Globalization;

namespace Lykke.Service.BitfinexAdapter.Core.Utils
{
    public sealed class BitfinexModelConverter : ExchangeConverters
    {

        public BitfinexModelConverter(BitfinexAdapterSettings configuration) : base(configuration.SupportedCurrencySymbols, configuration.UseSupportedCurrencySymbolsAsFilter)
        {
        }

        public static OrderBookItem ToOrderBookItem(OrderBookItemResponse response)
        {
            return new OrderBookItem
            {
                Id = response.Id.ToString(CultureInfo.InvariantCulture),
                IsBuy = response.Amount > 0,
                Price = response.Price,
                Symbol = response.Pair,
                Size = response.Amount
            };
        }

        public ExecutionReport ToOrderStatusUpdate(TradeExecutionUpdate eu)
        {
            var instrument = ExchangeSymbolToLykkeInstrument(eu.AssetPair);
            var transactionTime = eu.TimeStamp;
            var tradeType = ConvertTradeType(eu.Volume);
            var orderId = eu.OrderId;
            return new ExecutionReport(instrument, transactionTime, eu.Price, eu.Volume, tradeType, orderId, OrderExecutionStatus.Fill, eu.OrderType)
            {
                Message = eu.OrderType,
                Fee = eu.Fee,
                ExecType = ExecType.Trade
            };
        }

        public string ConvertToMarginOrderType(OrderType type)
        {
            switch (type)
            {
                case OrderType.Market:
                    return "market";
                case OrderType.Limit:
                    return "limit";
                case OrderType.Stop:
                    return "stop";
                case OrderType.TrailingStop:
                    return "trailing-stop";
                case OrderType.FillOrKill:
                    return "fill-or-kill";

                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, $"Unrecognized order type: {type}");
            }
        }

        public string ConvertToSpotOrderType(OrderType type)
        {
            switch (type)
            {
                case OrderType.Market:
                    return "exchange market";
                case OrderType.Limit:
                    return "exchange limit";
                case OrderType.Stop:
                    return "exchange stop";
                case OrderType.TrailingStop:
                    return "exchange trailing-stop";
                case OrderType.FillOrKill:
                    return "exchange fill-or-kill";
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, $"Unrecognized order type: {type}");
            }
        }

        public string ConvertTradeType(TradeSide signalTradeSide)
        {
            switch (signalTradeSide)
            {
                case TradeSide.Buy:
                    return "buy";
                case TradeSide.Sell:
                    return "sell";
                default:
                    throw new ArgumentOutOfRangeException(nameof(signalTradeSide), signalTradeSide, $"Unrecognized order side: {signalTradeSide}");
            }
        }

        public static TradeSide ConvertTradeType(string signalTradeType)
        {
            switch (signalTradeType)
            {
                case "buy":
                    return TradeSide.Buy;
                case "sell":
                    return TradeSide.Sell;
                default:
                    throw new ArgumentOutOfRangeException(nameof(signalTradeType), signalTradeType, null);
            }
        }

        public static TradeSide ConvertTradeType(decimal amount)
        {
            return amount > 0 ? TradeSide.Buy : TradeSide.Sell;
        }

    }
}
