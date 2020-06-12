using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DuckbearLab.UniSim.Networking
{
    public static class DeadReckoningCalculator
    {

        public static void Calculate(EntityStatePDU entityState, float elapsedTime, out GeocentricCoord outLocation, out Vector3Float outOrientation)
        {
            switch (entityState.DeadRecokning.Algorithm)
            {
                case DeadReckoningAlgorithm.Other:
                case DeadReckoningAlgorithm.Static:
                    outLocation = (GeocentricCoord) entityState.Location;
                    outOrientation = entityState.Orientation;
                    return;
                case DeadReckoningAlgorithm.FPW:
                    outLocation = (GeocentricCoord) (entityState.Location + entityState.LinearVelocity * elapsedTime);
                    outOrientation = entityState.Orientation;
                    return;
                case DeadReckoningAlgorithm.RPW:
                    outLocation = (GeocentricCoord) (entityState.Location + entityState.LinearVelocity * elapsedTime);
                    outOrientation = entityState.Orientation + entityState.DeadRecokning.AngularVelocity * elapsedTime;
                    return;
                case DeadReckoningAlgorithm.RVW:
                    outLocation = (GeocentricCoord) (entityState.Location + (entityState.LinearVelocity * elapsedTime + entityState.DeadRecokning.LinearAcceleration * (0.5 * elapsedTime * elapsedTime)));
                    outOrientation = entityState.Orientation + entityState.DeadRecokning.AngularVelocity * elapsedTime;
                    return;
                case DeadReckoningAlgorithm.FVW:
                    outLocation = (GeocentricCoord) (entityState.Location + (entityState.LinearVelocity * elapsedTime + entityState.DeadRecokning.LinearAcceleration * (0.5 * elapsedTime * elapsedTime)));
                    outOrientation = entityState.Orientation;
                    return;
            }

            outLocation = (GeocentricCoord) entityState.Location;
            outOrientation = entityState.Orientation;
        }

    }
}
