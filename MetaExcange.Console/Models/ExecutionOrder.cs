using MetaExcange.Console.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace MetaExcange.Console.Models
{
    public class ExecutionOrder
    {
        public string Exchange { get; init; } = string.Empty;

        [JsonConverter(typeof(JsonStringEnumConverter))]
        public OrderSide Side { get; init; }

        public decimal Price { get; init; }
        public decimal AmountBtc { get; init; }
    }
}
