﻿using Common;
using Lykke.Service.BitfinexAdapter.Core.Domain.Trading.Enums;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;

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
        /// A side of the order
        /// </summary>
        [JsonConverter(typeof(StringEnumConverter))]
        public string TradeType { get; internal set; }

        /// <summary>
        /// A trade direction
        /// </summary>
        [JsonConverter(typeof(StringEnumConverter))]
        public TradeSide Side { get; internal set; }

        /// <summary>
        /// Transaction time
        /// </summary>
        public DateTime Time { get; internal set; }

        /// <summary>
        /// An actual price of the execution or order
        /// </summary>
        public decimal Price { get; internal set; }

        /// <summary>
        /// Trade volume
        /// </summary>
        public decimal Volume { get; internal set; }


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
        public OrderExecutionStatus ExecutionStatus { get; internal set; }

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
            decimal volume, TradeSide side, long orderId, OrderExecutionStatus executionStatus, string tradeType)
        {
            Instrument = instrument;
            Time = time;
            Price = price;
            Volume = volume;
            Side = side;
            Fee = 0; // TODO
            ExchangeOrderId = orderId;
            ExecutionStatus = executionStatus;
            TradeType = tradeType;
        }

        public override string ToString()
        {
            return this.ToJson();
        }
    }
}