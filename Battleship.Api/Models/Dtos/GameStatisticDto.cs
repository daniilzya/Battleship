namespace Battleship.Api.Models
{
    public sealed class GameStatisticDto
    {
        public int Ship_count { get; set; }
        public int Destroyed { get; set; }
        public int Knocked { get; set; }
        public int Shot_count { get; set; }
    }
}
