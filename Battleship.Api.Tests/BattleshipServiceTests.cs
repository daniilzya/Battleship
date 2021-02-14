using Battleship.Api.Exceptions;
using Battleship.Api.Extensions;
using Battleship.Api.Models;
using Battleship.Api.Services;
using NUnit.Framework;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Battleship.Api.Tests
{
    [TestFixture]
    public class BattleshipServiceTests
    {
        private IBattleshipService _sut;

        [SetUp]
        public void Setup() 
        {
            _sut = new BattleshipService();
        }

        [TestCase(5)]
        [TestCase(23)]
        public void Start_Success(int range)
        {
            _sut.Start(range);
            var stats = _sut.GetStatistic();

            AssertStatistic(new GameStatisticDto(), stats);
        }

        [TestCase(0)]
        [TestCase(24)]
        public void Start_BadRequest(int range)
        {
            var ex = Assert.Throws<OperationException>(() => _sut.Start(range));
            Assert.AreEqual(ErrorCode.BadRequest, ex.ErrorCode, ex.Message);
        }


        [TestCase("1A 2B", 1)]
        [TestCase("1A 2B, 3D 3E", 2)]
        [TestCase("4E 5F, 2A 2B, 9A 10A, 9I 10K, 3H 3I", 5)]
        public void AddShip_Success(string coordinates, int shipCount)
        {
            _sut.Start(10);
            _sut.AddShips(coordinates);

            var stats = _sut.GetStatistic();
            AssertStatistic(new GameStatisticDto() { Ship_count = shipCount }, stats);
        }

        [TestCase("")]
        [TestCase("1A 2B, 1K 2L")]
        [TestCase("4E 5F, 2A 2B, 9A 10A, 9I 10K, 10A 11A")]
        [TestCase("1A 2B, 1A 2B")]
        public void AddShip_BadRequest(string coordinates)
        {
            _sut.Start(10);
            var ex = Assert.Throws<OperationException>(() => _sut.AddShips(coordinates));
            Assert.AreEqual(ErrorCode.BadRequest, ex.ErrorCode, ex.Message);
        }

        [TestCase("1A 2B", "1A, 2B", 1, 0, 1, false, true, false)]
        [TestCase("1A 2B", "1A, 2A, 2B, 1B", 1, 1, 0, true, true, true)]
        [TestCase("4E 5F, 2A 2B, 9A 10A, 9I 10K, 3H 3I", "7C", 5, 0, 0, false, false, false)]
        public async Task ShootShip_Success(string shipCoordinates, string shotCoordinates, int shipCount, int destroyedCount, int knockedCount, bool destroyed, bool knocked, bool isEnd)
        {
            _sut.Start(10);
            _sut.AddShips(shipCoordinates);
            var shotResult = await _sut.ShootShipAsync(shotCoordinates);
            Assert.AreEqual(destroyed, shotResult.Destroy);
            Assert.AreEqual(knocked, shotResult.Knock);
            Assert.AreEqual(isEnd, shotResult.End);

            var stats = _sut.GetStatistic();
            AssertStatistic(new GameStatisticDto() { Ship_count = shipCount, Destroyed = destroyedCount, Knocked = knockedCount, Shot_count = 1 }, stats);
        }

        [TestCase("1A 2B", "1A, 2B", 1, 0, 1)]
        [TestCase("1A 2B", "1A, 2A, 2B, 1B", 1, 1, 0)]
        [TestCase("4E 5F, 2A 2B, 9A 10A, 9I 10K, 3H 3I", "7C", 5, 0, 0)]
        public async Task ShootShip_ManyTimes_Success(string shipCoordinates, string shotCoordinates, int shipCount, int destroyedCount, int knockedCount)
        {
            _sut.Start(10);
            _sut.AddShips(shipCoordinates);

            int shotCounter = 0;
            foreach (var shotCoordinate in shotCoordinates.TrimSplit(',')) 
            {
                 await _sut.ShootShipAsync(shotCoordinate);
                shotCounter++;
            }

            var stats = _sut.GetStatistic();
            AssertStatistic(new GameStatisticDto() { Ship_count = shipCount, Destroyed = destroyedCount, Knocked = knockedCount, Shot_count = shotCounter }, stats);
        }

        [TestCase("1A 2B", "1A, 2B", 1, 0, 1)]
        [TestCase("1A 2B", "1A, 2A, 2B, 1B", 1, 1, 0)]
        [TestCase("4E 5F, 2A 2B, 9A 10A, 9I 10K, 3H 3I", "7C", 5, 0, 0)]
        public async Task ShootShip_ManyTimes_InParallel_Success(string shipCoordinates, string shotCoordinates, int shipCount, int destroyedCount, int knockedCount)
        {
            _sut.Start(10);
            _sut.AddShips(shipCoordinates);

            List<Task> shootTasks = new List<Task>();
            int shotCounter = 0;
            foreach (var shotCoordinate in shotCoordinates.TrimSplit(','))
            {
                shootTasks.Add(_sut.ShootShipAsync(shotCoordinate));
                shotCounter++;
            }

            await Task.WhenAll(shootTasks);

            var stats = _sut.GetStatistic();
            AssertStatistic(new GameStatisticDto() { Ship_count = shipCount, Destroyed = destroyedCount, Knocked = knockedCount, Shot_count = shotCounter }, stats);
        }


        [TestCase("1A 2B", "1A, 2A, 2B, 1B", "3D")]
        public async Task ShootShip_InvalidState(string shipCoordinates, string shotCoordinates, string lateShotCoordinates)
        {
            _sut.Start(10);
            _sut.AddShips(shipCoordinates);
            var shotResult = await _sut.ShootShipAsync(shotCoordinates);
            Assert.IsTrue(shotResult.Destroy);
            Assert.IsTrue(shotResult.Knock);
            Assert.IsTrue(shotResult.End);

            var ex = Assert.ThrowsAsync<OperationException>(async () => await _sut.ShootShipAsync(lateShotCoordinates));
            Assert.AreEqual(ErrorCode.InvalidState, ex.ErrorCode, ex.Message);
        }

        [Test]
        public void GetStatistic_WhenGameNotStarted()
        {
            var stats = _sut.GetStatistic();
            AssertStatistic(new GameStatisticDto(), stats);
        }

        private void AssertStatistic(GameStatisticDto expectedStats, GameStatisticDto actualStats) 
        {
            Assert.AreEqual(expectedStats.Ship_count, actualStats.Ship_count);
            Assert.AreEqual(expectedStats.Destroyed, actualStats.Destroyed);
            Assert.AreEqual(expectedStats.Knocked, actualStats.Knocked);
            Assert.AreEqual(expectedStats.Shot_count, actualStats.Shot_count);
        }
    }
}