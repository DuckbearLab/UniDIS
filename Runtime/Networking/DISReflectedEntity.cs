using UnityEngine;
using System.Collections;

namespace DuckbearLab.UniSim.Networking
{
    public class DISReflectedEntity
    {
        public EntityStatePDU State;

        private EntitiesManager _entitiesManager;
        private float _lastUpdateTime;

        public void UpdateState(EntityStatePDU newState)
        {
            State = newState;
            _lastUpdateTime = Time.time;
        }

        public void GetDeadRecoknedPositionAndRotation(out Vector3 position, out Quaternion rotation)
        {
            float elapsedTime = TimeSinceLastHeartbeat;

            GeocentricCoord location;
            Vector3Float orientation;

            DeadReckoningCalculator.Calculate(State, elapsedTime, out location, out orientation);

            var geod = CoordConverter.GeocentricToGeodetic(location);
            var databaseLocation = CoordConverter.GeodeticToDatabase(geod);

            position = (Vector3) databaseLocation;
            rotation = Quaternion.Euler(CoordConverter.OrientationToUnityEuler(orientation, (LatLonCoord) geod));
        }

        public float TimeSinceLastHeartbeat => Time.time - _lastUpdateTime;

    }
}
