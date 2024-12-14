namespace TaskBoard.Application.Common.Models
{
    /// <summary>
    /// Represents a void result
    /// </summary>
    public record Unit
    {
        private Unit() { }
        public static Unit Value { get; } = new();
    }
}
