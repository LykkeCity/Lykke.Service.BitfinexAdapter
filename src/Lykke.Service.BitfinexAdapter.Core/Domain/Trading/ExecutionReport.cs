using Common;
using Lykke.Service.BitfinexAdapter.Core.Domain.Trading.Enums;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using Lykke.Common.ExchangeAdapter.Contracts;
using Lykke.Common.ExchangeAdapter.SpotController.Records;

namespace Lykke.Service.BitfinexAdapter.Core.Domain.Trading
{
    public class ExecutionReport
    {
        /// <summary>
        /// An exchange assigned ID of the order
        /// </summary>
        public long ExchangeOrderId { get; internal set; }

        /// <summary>
        /// An instrument description
        /// </summary>
        public Instrument Instrument { get; internal set; }

        /// <summary>
        /// OrderType
        /// </summary>
        public string OrderType { get; internal set; }

        /// <summary>
        /// A trade direction
        /// </summary>
        [JsonConverter(typeof(StringEnumConverter))]
        public TradeType tradeType { get; internal set; }

        /// <summary>
        /// Transaction time
        /// </summary>
        public DateTime Time { get; internal set; }

        /// <summary>
        /// An actual price of the execution or order
        /// </summary>
        public decimal Price { get; internal set; }

        public decimal AvgExecutionPrice { get; set; }

        /// <summary>
        /// Trade volume
        /// </summary>
        public decimal OriginalVolume { get; internal set; }

        public decimal ExecutedVolume { get; set; }

        public decimal RemainingVolume { get; set; }

        /// <summary>
        /// Execution fee
        /// </summary>
        public decimal Fee { get; internal set; }

        /// <summary>
        /// Fee currency
        /// </summary>
        public string FeeCurrency { get; internal set; }

        /// <summary>
        /// Indicates that operation was successful
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// Current status of the order
        /// </summary>
        [JsonConverter(typeof(StringEnumConverter))]
        public OrderStatus ExecutionStatus { get; internal set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public OrderStatusUpdateFailureType FailureType { get; set; }

        /// <summary>
        /// An arbitrary message from the exchange related to the execution|order 
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// A side of the execution. ExecType = Trade means it is an execution, otherwise it is an order
        /// </summary>
        [JsonConverter(typeof(StringEnumConverter))]
        public ExecType ExecType { get; set; }

        public ExecutionReport()
        {

        }

        [JsonConstructor]
        public ExecutionReport(Instrument instrument, DateTime time, decimal price,
            decimal originalVolume, decimal executedVolume, TradeType _tradeType, long orderId, OrderStatus executionStatus, string orderType, decimal avgExecutionPrice)
        {
            Instrument = instrument;
            Time = time;
            Price = price;
            OriginalVolume = originalVolume;
            tradeType = _tradeType;
            Fee = 0; // TODO
            ExchangeOrderId = orderId;
            ExecutionStatus = executionStatus;
            OrderType = orderType;
            AvgExecutionPrice = avgExecutionPrice;
            ExecutedVolume = executedVolume;
        }

        public override string ToString()
        {
            return this.ToJson();
        }
    }
}
