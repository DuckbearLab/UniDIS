using DuckbearLab.UniSim.Networking;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DuckbearLab.UniSim
{
    [AddComponentMenu("UniSim/Reflected Entity")]
    public class ReflectedEntity : MonoBehaviour
    {
        public EntityStatePDU State { get; private set; }

        private const float SmoothingTime = 0.1f;

        private DISReflectedEntity _disReflectedEntity;

        public void Initialize(DISReflectedEntity disReflectedEntity)
        {
            _disReflectedEntity = disReflectedEntity;

            gameObject.name = _disReflectedEntity.State.MarkingText.String;
            Tick();
        }

        public void Tick()
        {
            State = _disReflectedEntity.State;

            Vector3 desiredPosition;
            Quaternion desiredRotation;

            _disReflectedEntity.GetDeadRecoknedPositionAndRotation(out desiredPosition, out desiredRotation);

            if (_disReflectedEntity.TimeSinceLastHeartbeat < SmoothingTime && Vector3.Distance(transform.position, desiredPosition) < 5)
            {
                float elapsedTime = _disReflectedEntity.TimeSinceLastHeartbeat;
                transform.position = Vector3.Lerp(transform.position, desiredPosition, elapsedTime / SmoothingTime);
                transform.rotation = Quaternion.Slerp(transform.rotation, desiredRotation, elapsedTime / SmoothingTime);
            }
            else
            {
                transform.position = desiredPosition;
                transform.rotation = desiredRotation;
            }
        }

    }
}
