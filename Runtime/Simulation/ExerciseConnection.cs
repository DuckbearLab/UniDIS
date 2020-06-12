using DuckbearLab.UniSim.Networking;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DuckbearLab.UniSim
{
    [DefaultExecutionOrder(-100)]
    [AddComponentMenu("UniSim/Exercise Connection")]
    public class ExerciseConnection : MonoBehaviour
    {
        public static ExerciseConnection Instance;

        public byte ExerciseID;

        public DISExerciseConnection DISExerciseConnection { get; private set; }

        private void Awake()
        {
            Instance = this;
            DISExerciseConnection = new DISExerciseConnection();
        }

        private void Start()
        {
            DISExerciseConnection.ExerciseID = ExerciseID;
            DISExerciseConnection.Start();
        }

        private void OnDestroy()
        {
            DISExerciseConnection.Dispose();
            DISExerciseConnection = null;
        }

        private void Update()
        {
            DISExerciseConnection.Tick();
        }

        public void Subscribe<PDUType>(Action<PDUType> callback) where PDUType : PDU, new()
        {
            DISExerciseConnection.Subscribe(callback);
        }

        public void Unsubscribe<PDUType>(Action<PDUType> callback) where PDUType : PDU, new()
        {
            DISExerciseConnection?.Unsubscribe(callback);
        }

        public void SendPDU(PDU pdu)
        {
            DISExerciseConnection.SendPDU(pdu);
        }

    }
}