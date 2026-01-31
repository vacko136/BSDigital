using MetaExcange.Console.Enums;
using MetaExcange.Console.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MetaExcange.Console.Services
{
    public class LiquidityLevel
    {
        public string Exchange { get; init; } = string.Empty;
        public decimal Price { get; init; }
        public decimal AvailableBtc { get; init; }
    }

    public class BestExecutionService
    {
        private const decimal Epsilon = 0.00000001m;

        public IReadOnlyList<ExecutionOrder> Execute(IReadOnlyList<OrderBook> orderBooks,
        IReadOnlyList<ExchangeBalance> balances,
        OrderSide side,
        decimal requestedBtc)
        {
            if (requestedBtc <= 0)
            {
                throw new ArgumentException("Requested BTC amount must be positive.");
            }

            ValidateBalances(balances, side, requestedBtc);

            var liquidity = CollectLiquidity(orderBooks, side);

            ValidateLiquidity(liquidity, side, requestedBtc);

            return ExecuteOrders(liquidity, balances, side, requestedBtc);
        }

        private static void ValidateBalances(
            IReadOnlyList<ExchangeBalance> balances,
            OrderSide side,
            decimal requestedBtc)
        {
            if (side == OrderSide.Buy)
            {
                var totalEur = balances.Sum(b => b.Eur);
                if (totalEur <= 0)
                {
                    throw new InvalidOperationException("No EUR balance available to buy BTC.");
                }
            }
            else
            {
                var totalBtc = balances.Sum(b => b.Btc);
                if (totalBtc + Epsilon < requestedBtc)
                {
                    throw new InvalidOperationException($"Insufficient BTC balance to sell {requestedBtc} BTC.");
                }
            }
        }

        private static List<LiquidityLevel> CollectLiquidity(
            IReadOnlyList<OrderBook> orderBooks,
            OrderSide side)
        {
            var liquidity = new List<LiquidityLevel>();

            foreach (var book in orderBooks)
            {
                var levels = side == OrderSide.Buy
                    ? book.Asks
                    : book.Bids;

                foreach (var level in levels)
                {
                    liquidity.Add(new LiquidityLevel
                    {
                        Exchange = book.Exchange,
                        Price = level.Price,
                        AvailableBtc = level.AmountBtc
                    });
                }
            }

            return side == OrderSide.Buy? liquidity.OrderBy(l => l.Price).ToList() : liquidity.OrderByDescending(l => l.Price).ToList();
        }

        private static void ValidateLiquidity(
            IReadOnlyList<LiquidityLevel> liquidity,
            OrderSide side,
            decimal requestedBtc)
        {
            var totalAvailableBtc = liquidity.Sum(l => l.AvailableBtc);

            if (totalAvailableBtc + Epsilon < requestedBtc)
            {
                throw new InvalidOperationException( $"Insufficient market liquidity to {side} {requestedBtc} BTC.");
            }
        }

        private static IReadOnlyList<ExecutionOrder> ExecuteOrders(
            IReadOnlyList<LiquidityLevel> liquidity,
            IReadOnlyList<ExchangeBalance> balances,
            OrderSide side,
            decimal requestedBtc)
        {
            var balanceByExchange = balances.ToDictionary(b => b.Exchange);
            var remainingBtc = requestedBtc;
            var executionOrders = new List<ExecutionOrder>();

            foreach (var level in liquidity)
            {
                if (remainingBtc <= Epsilon)
                {
                    break;
                }
                    
                if (!balanceByExchange.TryGetValue(level.Exchange, out var balance))
                {
                    continue;
                }
                    
                decimal maxExecutableBtc;

                if (side == OrderSide.Buy)
                {
                    var maxByEur = balance.Eur / level.Price;
                    maxExecutableBtc = Math.Min(level.AvailableBtc, maxByEur);
                }
                else
                {
                    maxExecutableBtc = Math.Min(level.AvailableBtc, balance.Btc);
                }

                var executedBtc = Math.Min(maxExecutableBtc, remainingBtc);

                if (executedBtc <= Epsilon)
                {
                    continue;
                }
                    
                executionOrders.Add(new ExecutionOrder
                {
                    Exchange = level.Exchange,
                    Side = side,
                    Price = level.Price,
                    AmountBtc = executedBtc
                });

                if (side == OrderSide.Buy)
                {
                    balance.Eur -= executedBtc * level.Price;
                    balance.Btc += executedBtc;
                }
                else
                {
                    balance.Btc -= executedBtc;
                    balance.Eur += executedBtc * level.Price;
                }

                remainingBtc -= executedBtc;
            }

            if (remainingBtc > Epsilon)
            {
                string message = side == OrderSide.Buy ? $"Insufficient market liquidity to Buy {remainingBtc} BTC." : $"Insufficient BTC balance to Sell {remainingBtc} BTC.";
                throw new InvalidOperationException(message);
            }

            return executionOrders;
        }
    }
}
