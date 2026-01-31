using MetaExcange.Console.Models;

namespace MetaExchange.Api.Dtos
{
    public class ExecutionResponseDto
    {
        public IReadOnlyList<ExecutionOrder> Orders { get; init; } = [];
        public decimal TotalEur { get; init; }
    }
}
