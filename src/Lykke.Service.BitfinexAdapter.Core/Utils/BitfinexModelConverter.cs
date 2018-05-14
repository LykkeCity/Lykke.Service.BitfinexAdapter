using Lykke.Service.BitfinexAdapter.Core.Domain.Settings;
using Lykke.Service.BitfinexAdapter.Core.Domain.Trading;
using Lykke.Service.BitfinexAdapter.Core.Domain.Trading.Enums;
using Lykke.Service.BitfinexAdapter.Core.Domain.WebSocketClient;
using System;
using System.Globalization;
using Lykke.Common.ExchangeAdapter.Contracts;
using Lykke.Common.ExchangeAdapter.SpotController.Records;
using OrderBookItem = Lykke.Service.BitfinexAdapter.Core.Domain.OrderBooks.OrderBookItem;

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
            return new ExecutionReport(instrument, transactionTime, eu.OrderPrice ?? 0 /*could it be 0 when its a market order?*/, eu.Volume/*we set status to Fill, hence original and executed amount should be equal*/, eu.Volume, tradeType, orderId, OrderStatus.Fill, eu.OrderType, eu.Price)
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

        public OrderType GetOrderTypeFromString(string orderTypeString)
        {
            if (String.IsNullOrWhiteSpace(orderTypeString)) return OrderType.Unknown;
            switch (orderTypeString)
            {
                case "market":
                    return OrderType.Market;
                case "exchange market":
                    return OrderType.Market;
                case "limit":
                    return OrderType.Limit;
                case "exchange limit":
                    return OrderType.Limit;
                case "stop":
                    return OrderType.Stop;
                case "exchange stop":
                    return OrderType.Stop;
                case "trailing-stop":
                    return OrderType.TrailingStop;
                case "exchange trailing-stop":
                    return OrderType.TrailingStop;
                case "fill-or-kill":
                    return OrderType.FillOrKill;
                case "exchange fill-or-kill":
                    return OrderType.FillOrKill;
                default:
                    return OrderType.Unknown;
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

        public string ConvertTradeType(TradeType signalTradeType)
        {
            switch (signalTradeType)
            {
                case TradeType.Buy:
                    return "buy";
                case TradeType.Sell:
                    return "sell";
                default:
                    throw new ArgumentOutOfRangeException(nameof(signalTradeType), signalTradeType, $"Unrecognized trade type: {signalTradeType}");
            }
        }

        public static TradeType ConvertTradeType(string signalTradeType)
        {
            switch (signalTradeType)
            {
                case "buy":
                    return TradeType.Buy;
                case "sell":
                    return TradeType.Sell;
                default:
                    throw new ArgumentOutOfRangeException(nameof(signalTradeType), signalTradeType, null);
            }
        }

        public static TradeType ConvertTradeType(decimal amount)
        {
            return amount > 0 ? TradeType.Buy : TradeType.Sell;
        }

    }
}
