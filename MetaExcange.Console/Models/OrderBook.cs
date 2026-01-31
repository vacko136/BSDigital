using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MetaExcange.Console.Models
{
    public class OrderBook
    {
        public string Exchange { get; init; } = string.Empty;
        public List<OrderLevel> Bids { get; init; } = new();
        public List<OrderLevel> Asks { get; init; } = new();
    }
}
