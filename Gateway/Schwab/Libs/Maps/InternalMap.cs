using Schwab.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using Terminal.Core.Domains;
using Terminal.Core.Enums;
using Terminal.Core.Models;

namespace Schwab.Mappers
{
  public class InternalMap
  {
    /// <summary>
    /// Map fields in the stream
    /// </summary>
    /// <param name="instrument"></param>
    /// <returns></returns>
    public static IDictionary<string, string> GetStreamMap(InstrumentModel instrument)
    {
      switch (instrument.Type)
      {
        case InstrumentEnum.Shares: return StreamEquityMap.Map;
        case InstrumentEnum.Futures: return StreamFutureMap.Map;
        case InstrumentEnum.Currencies: return StreamCurrencyMap.Map;
        case InstrumentEnum.Options: return StreamOptionMap.Map;
        case InstrumentEnum.FuturesOptions: return StreamFutureOptionMap.Map;
      }

      return null;
    }

    /// <summary>
    /// Map fields in the stream
    /// </summary>
    /// <param name="assetType"></param>
    /// <returns></returns>
    public static IDictionary<string, string> GetStreamMap(string assetType)
    {
      switch (assetType)
      {
        case "LEVELONE_EQUITIES": return StreamEquityMap.Map;
        case "LEVELONE_FUTURES": return StreamFutureMap.Map;
        case "LEVELONE_FOREX": return StreamCurrencyMap.Map;
        case "LEVELONE_OPTIONS": return StreamOptionMap.Map;
        case "LEVELONE_FUTURES_OPTIONS": return StreamFutureOptionMap.Map;
      }

      return null;
    }

    /// <summary>
    /// External instrument type to local
    /// </summary>
    /// <param name="assetType"></param>
    /// <returns></returns>
    public static InstrumentEnum? GetSubscriptionType(string assetType)
    {
      switch (assetType)
      {
        case "LEVELONE_EQUITIES": return InstrumentEnum.Shares;
        case "LEVELONE_FUTURES": return InstrumentEnum.Futures;
        case "LEVELONE_FOREX": return InstrumentEnum.Currencies;
        case "LEVELONE_OPTIONS": return InstrumentEnum.Options;
        case "LEVELONE_FUTURES_OPTIONS": return InstrumentEnum.FuturesOptions;
      }

      return null;
    }

    /// <summary>
    /// Asset type
    /// </summary>
    /// <param name="assetType"></param>
    /// <returns></returns>
    public static InstrumentEnum? GetInstrumentType(string assetType)
    {
      switch (assetType)
      {
        case "ETF":
        case "INDEX":
        case "EQUITY":
        case "EXTENDED":
        case "INDICATOR":
        case "FUNDAMENTAL":
        case "MUTUAL_FUND": return InstrumentEnum.Shares;
        case "BOND": return InstrumentEnum.Bonds;
        case "FOREX": return InstrumentEnum.Currencies;
        case "FUTURE": return InstrumentEnum.Futures;
        case "FUTURE_OPTION": return InstrumentEnum.FuturesOptions;
        case "OPTION": return InstrumentEnum.Options;
      }

      return null;
    }

    /// <summary>
    /// Get order book
    /// </summary>
    /// <param name="message"></param>
    /// <returns></returns>
    public static PointModel GetPrice(AssetMessage message)
    {
      var o = message.Quote;
      var point = new PointModel
      {
        Ask = o.AskPrice,
        Bid = o.BidPrice,
        AskSize = o.AskSize,
        BidSize = o.BidSize,
        Last = o.AskPrice ?? o.BidPrice,
      };

      return point;
    }

    /// <summary>
    /// Get internal option
    /// </summary>
    /// <param name="optionMessage"></param>
    /// <param name="message"></param>
    /// <returns></returns>
    public static InstrumentModel GetOption(OptionMessage optionMessage, OptionChainMessage message)
    {
      var asset = message.Underlying;
      var price = message.UnderlyingPrice;
      var point = new PointModel
      {
        Ask = asset?.Ask ?? price,
        Bid = asset?.Bid ?? price,
        AskSize = asset?.AskSize ?? 0,
        BidSize = asset?.BidSize ?? 0,
        Last = asset?.Last ?? price
      };

      var instrument = new InstrumentModel
      {
        Exchange = asset?.ExchangeName,
        Name = message.Symbol,
        Point = point
      };

      if (asset is not null)
      {
        point.Bar = new BarModel
        {
          Low = asset.LowPrice,
          High = asset.HighPrice,
          Open = asset.OpenPrice,
          Close = asset.Close
        };
      }

      var optionBar = new BarModel
      {
        Low = optionMessage.LowPrice,
        High = optionMessage.HighPrice,
        Open = optionMessage.OpenPrice,
        Close = optionMessage.ClosePrice
      };

      var optionPoint = new PointModel
      {
        Ask = optionMessage.Ask,
        Bid = optionMessage.Bid,
        AskSize = optionMessage.AskSize ?? 0,
        BidSize = optionMessage.BidSize ?? 0,
        Volume = optionMessage.TotalVolume ?? 0,
        Last = optionMessage.Last,
        Bar = optionBar
      };

      var optionInstrument = new InstrumentModel
      {
        Basis = instrument,
        Point = optionPoint,
        Leverage = optionMessage.Multiplier ?? 100,
        Name = optionMessage.Symbol
      };

      var greeks = new VariableModel
      {
        Rho = optionMessage.Rho ?? 0,
        Vega = optionMessage.Vega ?? 0,
        Delta = optionMessage.Delta ?? 0,
        Gamma = optionMessage.Gamma ?? 0,
        Theta = optionMessage.Theta ?? 0
      };

      var option = new DerivativeModel
      {
        Strike = optionMessage.StrikePrice,
        Expiration = optionMessage.ExpirationDate,
        OpenInterest = optionMessage.OpenInterest ?? 0,
        IntrinsicValue = optionMessage.IntrinsicValue ?? 0,
        Volatility = optionMessage.Volatility ?? 0,
        Variable = greeks
      };

      optionInstrument.Point = optionPoint;
      optionInstrument.Derivative = option;

      switch (optionMessage.PutCall.ToUpper())
      {
        case "PUT": option.Side = OptionSideEnum.Put; break;
        case "CALL": option.Side = OptionSideEnum.Call; break;
      }

      return optionInstrument;
    }

    /// <summary>
    /// Get internal point
    /// </summary>
    /// <param name="message"></param>
    /// <returns></returns>
    public static PointModel GetPoint(AssetMessage message)
    {
      var o = message.Quote;
      var point = new PointModel
      {
        Ask = o.AskPrice,
        Bid = o.BidPrice,
        AskSize = o.AskSize,
        BidSize = o.BidSize,
        Time = DateTimeOffset.FromUnixTimeMilliseconds(o.QuoteTime ?? DateTime.Now.Ticks).UtcDateTime,
        Last = o.AskTime > o.BidTime ? o.AskPrice : o.BidPrice,
        Instrument = new InstrumentModel { Name = message.Symbol }
      };

      return point;
    }

    /// <summary>
    /// Get internal point from bar
    /// </summary>
    /// <param name="message"></param>
    /// <returns></returns>
    public static PointModel GetBar(BarMessage message)
    {
      var point = new PointModel
      {
        Ask = message.Close,
        Bid = message.Close,
        AskSize = message.Volume,
        BidSize = message.Volume,
        Time = DateTimeOffset.FromUnixTimeMilliseconds(message.Datetime ?? DateTime.Now.Ticks).UtcDateTime,
        Last = message.Close
      };

      return point;
    }

    /// <summary>
    /// Convert remote position to local
    /// </summary>
    /// <param name="message"></param>
    /// <returns></returns>
    public static OrderModel GetPosition(PositionMessage message)
    {
      var instrument = new InstrumentModel
      {
        Name = message.Instrument.Symbol,
        Type = GetInstrumentType(message.Instrument.AssetType)
      };

      var action = new TransactionModel
      {
        Instrument = instrument,
        Price = message.AveragePrice,
        Descriptor = message.Instrument.Symbol,
        CurrentVolume = message.LongQuantity + message.ShortQuantity,
        Volume = message.LongQuantity + message.ShortQuantity
      };

      var order = new OrderModel
      {
        Transaction = action,
        Type = OrderTypeEnum.Market,
        Side = GetPositionSide(message)
      };

      var gainLoss = message.LongOpenProfitLoss;

      order.GainMax = gainLoss;
      order.GainMin = gainLoss;

      return order;
    }

    /// <summary>
    /// Convert remote position side to local
    /// </summary>
    /// <param name="message"></param>
    /// <returns></returns>
    public static OrderSideEnum GetPositionSide(PositionMessage message)
    {
      switch (true)
      {
        case true when message.LongQuantity > 0: return OrderSideEnum.Buy;
        case true when message.ShortQuantity > 0: return OrderSideEnum.Sell;
      }

      return OrderSideEnum.None;
    }

    /// <summary>
    /// Convert remote order to local
    /// </summary>
    /// <param name="message"></param>
    /// <returns></returns>
    public static OrderModel GetOrder(OrderMessage message)
    {
      var subOrders = message
        ?.OrderLegCollection
        ?.ToList();

      var instrument = new InstrumentModel
      {
        Name = string.Join($" / ", subOrders.Select(o => o?.Instrument?.Symbol))
      };

      var action = new TransactionModel
      {
        Id = message.OrderId,
        Descriptor = message.OrderId,
        Instrument = instrument,
        CurrentVolume = message.FilledQuantity,
        Volume = message.Quantity,
        Time = message.EnteredTime,
        Status = GetStatus(message)
      };

      var inOrder = new OrderModel
      {
        Transaction = action,
        Type = OrderTypeEnum.Market,
        Side = GetOrderSide(message),
        TimeSpan = GetTimeSpan(message)
      };

      switch (message.OrderType.ToUpper())
      {
        case "STOP":
          inOrder.Type = OrderTypeEnum.Stop;
          inOrder.Price = message.Price;
          break;

        case "LIMIT":
          inOrder.Type = OrderTypeEnum.Limit;
          inOrder.Price = message.Price;
          break;

        case "STOP_LIMIT":
          inOrder.Type = OrderTypeEnum.StopLimit;
          inOrder.Price = message.Price;
          inOrder.ActivationPrice = message.StopPrice;
          break;

        case "NET_DEBIT":
        case "NET_CREDIT":
          inOrder.Type = OrderTypeEnum.Limit;
          inOrder.Price = message.Price;
          break;
      }

      if (subOrders.Count > 0)
      {
        inOrder.Instruction = InstructionEnum.Group;

        foreach (var subOrder in subOrders)
        {
          var subInstrument = new InstrumentModel
          {
            Name = subOrder.Instrument.Symbol
          };

          var subAction = new TransactionModel
          {
            Instrument = subInstrument,
            CurrentVolume = subOrder.Quantity,
            Volume = subOrder.Quantity
          };

          inOrder.Orders.Add(new OrderModel
          {
            Transaction = subAction,
            Side = GetSubOrderSide(subOrder.Instruction)
          });
        }
      }

      return inOrder;
    }

    /// <summary>
    /// Convert remote order status to local
    /// </summary>
    /// <param name="message"></param>
    /// <returns></returns>
    public static OrderStatusEnum GetStatus(OrderMessage message)
    {
      switch (message.Status.ToUpper())
      {
        case "FILLED":
        case "REPLACED": return OrderStatusEnum.Filled;
        case "WORKING": return OrderStatusEnum.Partitioned;
        case "REJECTED":
        case "CANCELED":
        case "EXPIRED": return OrderStatusEnum.Canceled;
        case "NEW":
        case "PENDING_RECALL":
        case "PENDING_CANCEL":
        case "PENDING_REPLACE":
        case "PENDING_ACTIVATION":
        case "PENDING_ACKNOWLEDGEMENT":
        case "AWAITING_CONDITION":
        case "AWAITING_PARENT_ORDER":
        case "AWAITING_RELEASE_TIME":
        case "AWAITING_MANUAL_REVIEW":
        case "AWAITING_STOP_CONDITION": return OrderStatusEnum.Pending;
      }

      return OrderStatusEnum.None;
    }

    /// <summary>
    /// Convert remote order side to local
    /// </summary>
    /// <param name="message"></param>
    /// <returns></returns>
    public static OrderSideEnum GetOrderSide(OrderMessage message)
    {
      switch (message.OrderType.ToUpper())
      {
        case "NET_DEBIT": return OrderSideEnum.Buy;
        case "NET_CREDIT": return OrderSideEnum.Sell;
      }

      var position = message
        ?.OrderLegCollection
        ?.FirstOrDefault();

      return GetSubOrderSide(position.Instruction);
    }

    /// <summary>
    /// Convert remote order side to local
    /// </summary>
    /// <param name="status"></param>
    /// <returns></returns>
    public static OrderSideEnum GetSubOrderSide(string status)
    {
      switch (status?.ToUpper())
      {
        case "BUY":
        case "BUY_TO_OPEN":
        case "BUY_TO_CLOSE":
        case "NET_DEBIT": return OrderSideEnum.Buy;

        case "SELL":
        case "SELL_SHORT":
        case "SELL_TO_OPEN":
        case "SELL_TO_CLOSE":
        case "NET_CREDIT": return OrderSideEnum.Sell;
      }

      return OrderSideEnum.None;
    }

    /// <summary>
    /// Convert remote time in force to local
    /// </summary>
    /// <param name="message"></param>
    /// <returns></returns>
    public static OrderTimeSpanEnum? GetTimeSpan(OrderMessage message)
    {
      var span = message.Duration;
      var session = message.Session;

      switch (true)
      {
        case true when E(span, "DAY"): return OrderTimeSpanEnum.Day;
        case true when E(span, "FILL_OR_KILL"): return OrderTimeSpanEnum.Fok;
        case true when E(span, "GOOD_TILL_CANCEL"): return OrderTimeSpanEnum.Gtc;
        case true when E(span, "IMMEDIATE_OR_CANCEL"): return OrderTimeSpanEnum.Ioc;
        case true when E(session, "AM"): return OrderTimeSpanEnum.Am;
        case true when E(session, "PM"): return OrderTimeSpanEnum.Pm;
      }

      return null;
    }

    /// <summary>
    /// Case insensitive comparison
    /// </summary>
    /// <param name="x"></param>
    /// <param name="o"></param>
    /// <returns></returns>
    public static bool E(string x, string o) => string.Equals(x, o, StringComparison.OrdinalIgnoreCase);
  }
}
