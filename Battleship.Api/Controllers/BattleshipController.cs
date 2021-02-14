using Battleship.Api.Models.Dtos;
using Battleship.Api.Services;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Battleship.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class BattleshipController : ControllerBase
    {
        private readonly IBattleshipService _service;

        public BattleshipController(IBattleshipService service)
        {
            _service = service;
        }

        [HttpPost]
        [Route("create-matrix")]
        public IActionResult Create([FromBody] CreateGameRequestDto request)
        {
            _service.Start(request.Range);
            return Ok();
        }

        [HttpPost]
        [Route("ship")]
        public async Task<IActionResult> AddShips([FromBody] AddShipsRequestDto request)
        {
            await Task.Run(() => _service.AddShips(request.Coordinates));
            return Ok();
        }

        [HttpPost]
        [Route("shot")]
        public async Task<IActionResult> ShootShip([FromBody] ShootRequestDto request)
        {
            var result = await _service.ShootShipAsync(request.Coord);
            return Ok(result);
        }

        [HttpPost]
        [Route("clear")]
        public IActionResult Clear()
        {
            _service.End();
            return Ok();
        }

        [HttpGet]
        [Route("state")]
        public IActionResult State()
        {
            var stats = _service.GetStatistic();
            return Ok(stats);
        }
    }
}
