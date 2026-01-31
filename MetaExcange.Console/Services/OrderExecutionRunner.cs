using MetaExcange.Console.Enums;
using MetaExcange.Console.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace MetaExcange.Console.Services
{
    public class OrderExecutionRunner
    {
        private readonly BestExecutionService _service;

        public OrderExecutionRunner()
        {
            _service = new BestExecutionService();
        }
        

        public IReadOnlyList<ExecutionOrder> ExecuteOrder(OrderSide side, decimal requestedBtc)
        {
            var basePath = AppContext.BaseDirectory;

            var orderBooksPath = Path.Combine(basePath, "Data", "orderbooks.json");
            var orderBookLines = File.ReadAllLines(orderBooksPath);
            var orderBooks = orderBookLines
                .Select(line => JsonSerializer.Deserialize<OrderBook>(line)!)
                .ToList();

            var balancesPath = Path.Combine(basePath, "Data", "balances.json");
            var balancesJson = File.ReadAllText(balancesPath);
            var balances = JsonSerializer.Deserialize<List<ExchangeBalance>>(balancesJson)!;

            // Execute best execution plan
            return _service.Execute(orderBooks, balances, side, requestedBtc);
        }
    }
}
