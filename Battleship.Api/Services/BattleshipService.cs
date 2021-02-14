using Battleship.Api.Exceptions;
using Battleship.Api.Extensions;
using Battleship.Api.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Battleship.Api.Services
{
    public class BattleshipService : IBattleshipService
    {
        private readonly SemaphoreSlim semaphore = new SemaphoreSlim(1, 1);

        private int _isGameStarted = 0; // 0 means false, 1 means true
        private int _areShipsAdded = 0; // 0 means false, 1 means true
        private int _shotCount = 0;

        private readonly Dictionary<char, int> _columns = new Dictionary<char, int> 
        {
            {'A', 0}, 
            {'B', 1}, 
            {'C', 2}, 
            {'D', 3},
            {'E', 4}, 
            {'F', 5}, 
            {'G', 6}, 
            {'H', 7}, 
            {'I', 8}, 
            {'K', 9}, 
            {'L', 10}, 
            {'M', 11}, 
            {'N', 12}, 
            {'O', 13}, 
            {'P', 14}, 
            {'Q', 15}, 
            {'R', 16}, 
            {'S', 17}, 
            {'T', 18}, 
            {'V', 19}, 
            {'X', 20}, 
            {'Y', 21}, 
            {'Z', 22} 
        };

        private Ship[,] _matrix = null; // y,x

        public void Start(int matrixRange) 
        {
            if (matrixRange < 1 || matrixRange > _columns.Count)
                throw new OperationException(ErrorCode.BadRequest, $"Matrix range is outside of boundaries. Max range is {_columns.Count}.");

            if (Interlocked.Exchange(ref _isGameStarted, 1) == 1)
                return;

            _shotCount = 0;
            _areShipsAdded = 0;
            _matrix = new Ship[matrixRange, matrixRange];
        }

        public void End()
        {
            _matrix = null;
            Volatile.Write(ref _isGameStarted, 0);
        }

        private void Restart() 
        {
            var range = _matrix.GetLength(0);
            End();
            Start(range);
        }

        public void AddShips(string coordinates)
        {
            if (string.IsNullOrEmpty(coordinates))
                throw new OperationException(ErrorCode.BadRequest, $"{nameof(coordinates)} is required.");
            
            if (_isGameStarted == 0 || Interlocked.Exchange(ref _areShipsAdded, 1) == 1)
                return;

            try
            {
                AddShipsToMatrix(coordinates);
            }
            catch(Exception) 
            {
                Restart();
                throw;
            }
        }

        private void AddShipsToMatrix(string coordinates) 
        {
            foreach (var shipCoordinates in ParseCoordinatePairs(coordinates))
            {
                var shipSmallestCoordinate = shipCoordinates.OrderBy(c => c.Y).First();
                if (_matrix[shipSmallestCoordinate.Y, shipSmallestCoordinate.X] != null)
                    throw new OperationException(ErrorCode.BadRequest, "Ship has been added already.");

                if (shipCoordinates[0].X != shipCoordinates[1].X && shipCoordinates[0].Y != shipCoordinates[1].Y)
                {
                    shipCoordinates.Add(new Coordinate(shipSmallestCoordinate.X, shipSmallestCoordinate.Y + 1));
                    shipCoordinates.Add(new Coordinate(shipSmallestCoordinate.X + 1, shipSmallestCoordinate.Y));
                }

                var ship = Ship.Create(shipCoordinates);
                foreach (var c in ship.Coordinates)
                {
                    _matrix[c.Y, c.X] = ship;
                }
            }
        }

        public async Task<ShotResultDto> ShootShipAsync(string coordinates)
        {
            Interlocked.Add(ref _shotCount, 1);
            
            if (string.IsNullOrEmpty(coordinates))
                throw new OperationException(ErrorCode.BadRequest, $"{nameof(coordinates)} is required.");

            var statistic = GetStatistic();
            if (statistic.Ship_count == statistic.Destroyed)
                throw new OperationException(ErrorCode.InvalidState, $"Game is finished.");

            var hitShips = new Dictionary<Guid, Ship>();
            var shipCoordinates = ParseCoordinates(coordinates, ',').ToList();
            
            await semaphore.WaitAsync();
            try
            {
                foreach (var c in shipCoordinates)
                {
                    var ship = _matrix[c.Y, c.X];
                    if (ship == null)
                        continue;

                    if (!hitShips.ContainsKey(ship.Id))
                        hitShips.Add(ship.Id, ship);

                    ship.Damage(c);
                }
            }
            finally 
            {
                semaphore.Release();
            }

            statistic = GetStatistic();
            var shotResut = new ShotResultDto();
            shotResut.End = statistic.Ship_count == statistic.Destroyed;
            if (hitShips.Any()) 
            {
                shotResut.Destroy = hitShips.All(s => s.Value.State == ShipState.Destroyed);
                shotResut.Knock = shotResut.Destroy || hitShips.Any(s => s.Value.State == ShipState.Knocked);
            }

            return shotResut;
        }

        public GameStatisticDto GetStatistic()
        {
            if (_isGameStarted == 0)
                return new GameStatisticDto();

            var ships = _matrix.OfType<Ship>()
                               .GroupBy(s => s.Id)
                               .Select(g => g.First())
                               .ToList();

            var statistic = new GameStatisticDto();
            statistic.Ship_count = ships.Count;
            statistic.Destroyed = ships.Count(s => s.State == ShipState.Destroyed);
            statistic.Knocked = ships.Count(s => s.State == ShipState.Knocked);
            statistic.Shot_count = _shotCount;

            return statistic;
        }

        private IEnumerable<IList<Coordinate>> ParseCoordinatePairs(string coordinates) 
        {
            var coordinatePairs = coordinates.TrimSplit(',');
            foreach (var cp in coordinatePairs)
            {
                var coordinatePairList = new List<Coordinate>();
                foreach (var c in ParseCoordinates(cp, ' '))
                {
                    coordinatePairList.Add(c);
                }

                if (coordinatePairList.Count != 2)
                    continue;

                yield return coordinatePairList;
            }
        }

        private IEnumerable<Coordinate> ParseCoordinates(string coordinates, char separator) 
        {
            var deepestCoordinate = _matrix.GetLength(0) - 1;
            var coordinateList = coordinates.TrimSplit(separator);
            foreach (var c in coordinateList)
            {
                if (!TryParseCoordinate(c, out Coordinate coordinate))
                    continue;

                if (coordinate.X > deepestCoordinate || coordinate.Y > deepestCoordinate)
                    throw new OperationException(ErrorCode.BadRequest, $"Coordinate '{c}' out of range.");

                yield return coordinate;
            }
        }

        private bool TryParseCoordinate(string coordinate, out Coordinate outCoordinate) 
        {
            if (coordinate.Length > 3) 
            {
                outCoordinate = new Coordinate(-1, -1);
                return false;
            }

            var xCoordinate = default(char);
            var yCoordinate = string.Empty;
            var coordinateCleaned = coordinate.Trim();
            if (coordinateCleaned.Length == 3)
            {
                yCoordinate = coordinateCleaned.Substring(0, 2);
                xCoordinate = coordinateCleaned[2];
            }
            else if (coordinateCleaned.Length == 2) 
            {
                yCoordinate = coordinateCleaned.Substring(0, 1);
                xCoordinate = coordinateCleaned[1];
            }

            if (!int.TryParse(yCoordinate, out int y) || !_columns.ContainsKey(xCoordinate)) 
            {
                outCoordinate = new Coordinate(-1, -1);
                return false;
            }

            outCoordinate = new Coordinate(_columns[xCoordinate], y-1);
            return true;
        }
    }
}

