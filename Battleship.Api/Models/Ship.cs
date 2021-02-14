using System;
using System.Collections.Generic;
using System.Linq;

namespace Battleship.Api.Models
{
    internal sealed class Ship
    {
        private Dictionary<Coordinate, bool> _coordinates;
        private Ship(Guid id, Dictionary<Coordinate, bool> coordinatesDictionary)
        {
            Id = id;
            _coordinates = coordinatesDictionary;
        }

        public static Ship Create(IList<Coordinate> shipCoordinates) 
        {
            if (shipCoordinates == null)
                throw new ArgumentNullException(nameof(shipCoordinates));

            var coordinateDictionary = shipCoordinates.ToDictionary(c => c, _ => false);
            return new Ship(Guid.NewGuid(), coordinateDictionary);
        }

        public Guid Id { get; }
        public ShipState State { get; private set; }

        public IEnumerable<Coordinate> Coordinates
        {
            get { return _coordinates.Select(c => c.Key); } 
        }
        
        public void Damage(Coordinate coordinate)  
        {
            if (!_coordinates.TryGetValue(coordinate, out bool isHit))
                return;

            if (isHit)
                throw new Exception("Already hit.");

            _coordinates[coordinate] = true;

            var hitCount = _coordinates.Count(c => c.Value == true);
            if (hitCount == _coordinates.Count)
                State = ShipState.Destroyed;
            else
                State = ShipState.Knocked;
        }
    }
}
