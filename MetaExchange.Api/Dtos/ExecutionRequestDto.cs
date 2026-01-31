using MetaExcange.Console.Enums;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace MetaExchange.Api.Dtos
{
    public class ExecutionRequestDto
    {
        [Required]
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public OrderSide Side { get; init; }

        [Required]
        public decimal AmountBtc { get; init; }
    }
}
