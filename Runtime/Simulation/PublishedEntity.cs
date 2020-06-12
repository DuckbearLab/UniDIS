using DuckbearLab.UniSim.Networking;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DuckbearLab.UniSim
{
    [AddComponentMenu("UniSim/Published Entity")]
    public class PublishedEntity : MonoBehaviour
    {
        public EntityStatePDU State;

        [Header("Thresholds")]
        public float Heartbeat = 5f;
        public double MovementThreshold = 0.05;
        public double RotationThreshold = 1;
        public float MaxPdusPerSecond = 60;

        private DISEntityPublisher _disEntitypublisher;

        private void OnEnable()
        {
            _disEntitypublisher = EntitiesManager.Instance.CreateEntityPublisher();
            _disEntitypublisher.State = State;
        }

        private void OnDisable()
        {
            EntitiesManager.Instance?.DisposeEntityPublisher(_disEntitypublisher);
        }

        private void LateUpdate()
        {
            var geod = CoordConverter.DatabaseToGeodetic((Vector3Double) transform.position);
            var geoc = CoordConverter.GeodeticToGeocentric(geod);

            State.Location = (Vector3Double) geoc;
            State.Orientation = CoordConverter.UnityEulerToOrientation(transform.eulerAngles, (LatLonCoord)geod);

            _disEntitypublisher.State = State;
            _disEntitypublisher.Heartbeat = Heartbeat;
            _disEntitypublisher.MovementThreshold = MovementThreshold;
            _disEntitypublisher.RotationThreshold = RotationThreshold;
            _disEntitypublisher.MaxPDUsPerSecond = MaxPdusPerSecond;
        }
    }
}
