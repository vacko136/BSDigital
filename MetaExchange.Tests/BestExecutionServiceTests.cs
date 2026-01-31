using MetaExcange.Console.Enums;
using MetaExcange.Console.Models;
using MetaExcange.Console.Services;
using Xunit;

namespace MetaExchange.Tests;

public class BestExecutionServiceTests
{
    private readonly BestExecutionService _service = new();

    [Fact]
    public void BuyOrder_SingleExchange_ExecutesCorrectly()
    {
        // Arrange
        var orderBooks = new List<OrderBook>
        {
            new OrderBook
            {
                Exchange = "Ex1",
                Bids = new List<OrderLevel>(),
                Asks = new List<OrderLevel>
                {
                    new OrderLevel { Price = 1000, AmountBtc = 5 },
                    new OrderLevel { Price = 1200, AmountBtc = 3 }
                }
            }
        };

        var balances = new List<ExchangeBalance>
        {
            new ExchangeBalance { Exchange = "Ex1", Eur = 8000, Btc = 0 }
        };

        decimal requestedBtc = 6;

        // Act
        var orders = _service.Execute(orderBooks, balances, OrderSide.Buy, requestedBtc);

        // Assert
        Assert.Equal(2, orders.Count);
        Assert.Equal(5, orders[0].AmountBtc);  // first level fully used
        Assert.Equal(1, orders[1].AmountBtc);  // second level partially used
        Assert.Equal(1000m, orders[0].Price);
        Assert.Equal(1200m, orders[1].Price);
    }

    [Fact]
    public void SellOrder_InsufficientBalance_Throws()
    {
        var orderBooks = new List<OrderBook>
    {
        new OrderBook
        {
            Exchange = "Ex1",
            Bids = new List<OrderLevel> { new OrderLevel { Price = 2000, AmountBtc = 5 } },
            Asks = new List<OrderLevel>()
        }
    };

        var balances = new List<ExchangeBalance>
    {
        new ExchangeBalance { Exchange = "Ex1", Eur = 0, Btc = 3 } // Only 3 BTC available
    };

        decimal requestedBtc = 5;

        var ex = Assert.Throws<InvalidOperationException>(() =>
            _service.Execute(orderBooks, balances, OrderSide.Sell, requestedBtc));

        Assert.Contains("Insufficient BTC balance", ex.Message);
    }

    [Fact]
    public void BuyOrder_InsufficientLiquidity_Throws()
    {
        // Arrange
        var orderBooks = new List<OrderBook>
        {
            new OrderBook
            {
                Exchange = "Ex1",
                Bids = new List<OrderLevel>(),
                Asks = new List<OrderLevel>
                {
                    new OrderLevel { Price = 1000, AmountBtc = 1 }
                }
            }
        };

        var balances = new List<ExchangeBalance>
        {
            new ExchangeBalance { Exchange = "Ex1", Eur = 5000, Btc = 0 }
        };

        decimal requestedBtc = 2;

        // Act & Assert
        var ex = Assert.Throws<InvalidOperationException>(() =>
            _service.Execute(orderBooks, balances, OrderSide.Buy, requestedBtc));

        Assert.Contains("Insufficient market liquidity", ex.Message);
    }

    [Fact]
    public void BuyOrder_MultipleExchanges_SelectsBestPrice()
    {
        // Arrange
        var orderBooks = new List<OrderBook>
        {
            new OrderBook
            {
                Exchange = "Ex1",
                Bids = new List<OrderLevel>(),
                Asks = new List<OrderLevel>
                {
                    new OrderLevel { Price = 3000, AmountBtc = 3 }
                }
            },
            new OrderBook
            {
                Exchange = "Ex2",
                Bids = new List<OrderLevel>(),
                Asks = new List<OrderLevel>
                {
                    new OrderLevel { Price = 3200, AmountBtc = 5 }
                }
            }
        };

        var balances = new List<ExchangeBalance>
        {
            new ExchangeBalance { Exchange = "Ex1", Eur = 10000, Btc = 0 },
            new ExchangeBalance { Exchange = "Ex2", Eur = 20000, Btc = 0 }
        };

        decimal requestedBtc = 6;

        // Act
        var orders = _service.Execute(orderBooks, balances, OrderSide.Buy, requestedBtc);

        // Assert
        Assert.Equal(2, orders.Count);
        Assert.Equal("Ex1", orders[0].Exchange); // cheaper first
        Assert.Equal("Ex2", orders[1].Exchange); // remainder
    }
}
