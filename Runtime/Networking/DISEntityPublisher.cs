using System.Collections;
using System;
using UnityEngine;

namespace DuckbearLab.UniSim.Networking
{
    public class DISEntityPublisher
    {
        public EntityStatePDU State;

        public float Heartbeat = 5f;
        public double MovementThreshold = 0.05;
        public double RotationThreshold = 1;
        public float MaxPDUsPerSecond = 60;

        private DISExerciseConnection _disExerciseConnection;
        private float _lastSendTime;
        public EntityStatePDU _lastSentState = null;

        public DISEntityPublisher(DISExerciseConnection disExerciseConnection)
        {
            _disExerciseConnection = disExerciseConnection;
        }

        public void Tick()
        {
            if (ShouldSendNewPDU)
            {
                _disExerciseConnection.SendPDU(State);

                _lastSendTime = Time.time;
                if (_lastSentState == null)
                    _lastSentState = new EntityStatePDU();
                _lastSentState.CopyFrom(State);
            }
        }

        private bool ShouldSendNewPDU =>
            MinPeriodPassed && (HeartbeatPeriodPassed || StateWasChanged() || DeadRecoknedPositionOrRotationIsWrong());

        private bool MinPeriodPassed => Time.time - _lastSendTime >= 1 / MaxPDUsPerSecond;

        private bool HeartbeatPeriodPassed => Time.time - _lastSendTime >= Heartbeat;

        private bool StateWasChanged()
        {
            if (_lastSentState == null) return true;
            if (State.EntityId != _lastSentState.EntityId) return true;
            if (State.ForceId != _lastSentState.ForceId) return true;
            if (State.EntityType != _lastSentState.EntityType) return true;
            if (State.AlternativeEntityType != _lastSentState.AlternativeEntityType) return true;
            if (State.Appearance.BitValue != _lastSentState.Appearance.BitValue) return true;
            if (State.MarkingText.CharacterSet != _lastSentState.MarkingText.CharacterSet) return true;
            if (State.MarkingText.String != _lastSentState.MarkingText.String) return true;
            if (State.Capabilities != _lastSentState.Capabilities) return true;
            if (State.ArticulatedParts.Length != _lastSentState.ArticulatedParts.Length) return true;
            for (int i = 0; i < State.ArticulatedParts.Length; i++)
            {
                var newState = State.ArticulatedParts[i];
                var sentState = _lastSentState.ArticulatedParts[i];

                if (newState.TypeDesignator != sentState.TypeDesignator) return true;
                if (newState.ChangeIndicator != sentState.ChangeIndicator) return true;
                if (newState.TypeVariantAttached != sentState.TypeVariantAttached) return true;
                if (newState.TypeVariantArticulated != sentState.TypeVariantArticulated) return true;
                if (newState.value != sentState.value) return true;
            }
            return false;
        }        

        private bool DeadRecoknedPositionOrRotationIsWrong()
        {
            GeocentricCoord deadReckonedPosition;
            Vector3Float deadReckonedRotation;
            DeadReckoningCalculator.Calculate(_lastSentState, Time.time - _lastSendTime, out deadReckonedPosition, out deadReckonedRotation);

            double positionDeltaSquared = Math.Pow(deadReckonedPosition.X - State.Location.X, 2) + Math.Pow(deadReckonedPosition.Y - State.Location.Y, 2) + Math.Pow(deadReckonedPosition.Z - State.Location.Z, 2);
            if (positionDeltaSquared > MovementThreshold * MovementThreshold) return true;

            var deadRecoknedLatLon = (LatLonCoord) CoordConverter.GeocentricToGeodetic(deadReckonedPosition);
            var currentLatLon = (LatLonCoord)CoordConverter.GeocentricToGeodetic((GeocentricCoord) State.Location);

            Vector3 deadReckonedEuler = CoordConverter.OrientationToUnityEuler(deadReckonedRotation, deadRecoknedLatLon);
            Vector3 currentEuler = CoordConverter.OrientationToUnityEuler(State.Orientation, currentLatLon);

            float rotationDelta = Quaternion.Angle(Quaternion.Euler(deadReckonedEuler), Quaternion.Euler(currentEuler));
            if (rotationDelta > RotationThreshold) return true;

            return false;
        }

    }
}
