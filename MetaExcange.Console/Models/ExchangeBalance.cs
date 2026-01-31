using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MetaExcange.Console.Models
{
    public class ExchangeBalance
    {
        public string Exchange { get; init; } = string.Empty;
        public decimal Eur { get; set; }
        public decimal Btc { get; set; }
    }
}
