using System.Diagnostics;

namespace Forester.Domain.Streams.GetCompletedOrders
{
    internal class LogMovementTracker
    {
        public readonly Dictionary<string, List<string>> LogMovements = new Dictionary<string, List<string>>();

        private struct Location
        {
            public int Latitude;
            public int Longitude;

            public override string ToString()
            {
                return $"({Latitude}|{Longitude})";
            }
        }

        private Dictionary<Location, List<string>> _logsOnGround = new Dictionary<Location, List<string>>();
        private Dictionary<string, List<string>> _logsOnBoard = new Dictionary<string, List<string>>();
        private Dictionary<string, List<string>> _logsOnTruck = new Dictionary<string, List<string>>();
        private Dictionary<string, List<string>> _logsInWarehouse = new Dictionary<string, List<string>>();

        public void CutToLength(string log, int latitude, int longitude)
        {
            var location = new Location 
            { 
                Latitude = latitude, 
                Longitude = longitude 
            };

            if (!_logsOnGround.ContainsKey(location))
                _logsOnGround.Add(location, new List<string>());
            _logsOnGround[location].Add(log);
            AddMovement(log, $"Cut and grounded to {location}");
        }

        public void Pick(int latitude, int longitude, string by)
        {
            var location = new Location
            {
                Latitude = latitude,
                Longitude = longitude
            };

            MoveLogs(location, by, _logsOnGround, _logsOnBoard, $"Picked from {location} to ({by})");
        }

        public void Ground(int latitude, int longitude, string by)
        {
            var location = new Location
            {
                Latitude = latitude,
                Longitude = longitude
            };

            MoveLogs(by, location, _logsOnBoard, _logsOnGround, $"Grounded from ({by}) to {location}");
        }

        public void LoadToTruck(int latitude, int longitude, string truck)
        {
            var location = new Location
            {
                Latitude = latitude,
                Longitude = longitude
            };

            MoveLogs(location, truck, _logsOnGround, _logsOnTruck, $"Loaded from {location} to truck ({truck})");
        }

        public void UnloadToWarehouse(string truck, string warehouse)
        {
            MoveLogs(truck, warehouse, _logsOnTruck, _logsInWarehouse, $"Stored from ({truck}) to {warehouse}");
        }

        private void MoveLogs<TKey1, TKey2>(TKey1 fromKey, TKey2 toKey, Dictionary<TKey1, List<string>> from, Dictionary<TKey2, List<string>> to, string description)
        {
            from.Remove(fromKey, out var logs);

            Debug.Assert(logs != null);

            if (!to.ContainsKey(toKey))
                to.Add(toKey, new List<string>());
            to[toKey].AddRange(logs);

            foreach (var log in logs)
            {
                AddMovement(log, description);
            }
        }

        private void AddMovement(string log, string description)
        {
            if (!LogMovements.ContainsKey(log))
                LogMovements.Add(log, new List<string>());
            LogMovements[log].Add(description);
        }
    }

}
