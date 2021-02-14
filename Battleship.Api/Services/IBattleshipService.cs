using Battleship.Api.Models;
using System.Threading.Tasks;

namespace Battleship.Api.Services
{
    public interface IBattleshipService
    {
        void Start(int matrixRange);
        void End();
        void AddShips(string coordinates);
        Task<ShotResultDto> ShootShipAsync(string coordinate);
        GameStatisticDto GetStatistic();
    }
}
