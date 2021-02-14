namespace Battleship.Api.Models
{
    public sealed class ShotResultDto
    {
        public bool Destroy { get; set; }
        public bool Knock { get; set; }
        public bool End { get; set; }
    }
}
