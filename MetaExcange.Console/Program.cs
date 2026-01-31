using MetaExcange.Console.Enums;
using MetaExcange.Console.Services;

Console.WriteLine("MetaExchange Console App");

// Hardcoded for now (can be user input later)
OrderSide side = OrderSide.Buy;
decimal requestedBtc = 9m;

// Runner orchestrates loading + execution
var runner = new OrderExecutionRunner();

try
{
    var executionPlan = runner.ExecuteOrder(side, requestedBtc);

    Console.WriteLine($"Execution Plan for {side} {requestedBtc} BTC:");
    decimal totalEur = 0m;

    foreach (var order in executionPlan)
    {
        Console.WriteLine(
            $"{side} {order.AmountBtc} BTC @ {order.Price} EUR on {order.Exchange}"
        );

        totalEur += order.AmountBtc * order.Price;
    }

    Console.WriteLine($"Total EUR: {totalEur}");
}
catch (Exception ex)
{
    Console.WriteLine($"Error: {ex.Message}");
}
