using DuckbearLab.UniSim.CustomEventReports;
using DuckbearLab.UniSim.Networking;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DuckbearLab.UniSim.Example
{
    [AddComponentMenu("UniSim/UniSim Example")]
    public class Example : MonoBehaviour
    {

        [SerializeField] private PublishedEntity _publishedEntity = null;

        private void Start()
        {
            CoordConverter.SetRefLatLon(new LatLonCoord(32.055304, 34.7564563));
            _publishedEntity.State.EntityId.Entity = (ushort)UnityEngine.Random.Range(0, ushort.MaxValue);

            ExerciseConnection.Instance.Subscribe<DetonationPDU>(ReceivedDetonationPDU);
            CustomEventReportsManager.Instance.Subscribe<SampleEventReport>(ReceivedSampleEventReport);
        }

        private void OnDestroy()
        {
            ExerciseConnection.Instance.Unsubscribe<DetonationPDU>(ReceivedDetonationPDU);
        }

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.Z))
            {
                FirePDU fire = new FirePDU();

                fire.EventId = new EventId(8, 9, 10);
                fire.BurstDescriptor.Munition = new EntityType(1, 2, 3, 4, 5, 6, 7);
                fire.Location = new Vector3Double(11, 22, 33);

                ExerciseConnection.Instance.SendPDU(fire);
            }
            if (Input.GetKeyDown(KeyCode.X))
            {
                DetonationPDU detoantion = new DetonationPDU();

                detoantion.EventId = new EventId(999, 888, 777);
                detoantion.BurstDescriptor.Munition = new EntityType(9, 8, 7, 6, 5, 4, 3);
                detoantion.Location = new Vector3Double(11, 22, 33);

                ExerciseConnection.Instance.SendPDU(detoantion);
            }
            if (Input.GetKeyDown(KeyCode.C))
            {
                EventReportPDU eventReport = new EventReportPDU();

                eventReport.EventType = 33;
                eventReport.FixedDatumRecords.Add(new FixedDatumRecord()
                {
                    FixedDatumID = 12,
                    FixedDatumValue = 34
                });
                eventReport.VariableDatumRecords.Add(new VariableDatumRecord()
                {
                    VariableDatumID = 55,
                    VariableDatumValue = new byte[] { 0x11, 0x22, 0x33, 0x44 }
                });
                eventReport.VariableDatumRecords.Add(new VariableDatumRecord()
                {
                    VariableDatumID = 70,
                    VariableDatumValue = System.Text.Encoding.UTF8.GetBytes("Hello!")
                });

                ExerciseConnection.Instance.SendPDU(eventReport);
            }
            if (Input.GetKeyDown(KeyCode.V))
            {
                var sampleEventReport = new SampleEventReport();
                sampleEventReport.A = 123.456f;
                sampleEventReport.B = 789;
                CustomEventReportsManager.Instance.Send(sampleEventReport);
            }
            if (Input.GetKeyDown(KeyCode.B))
            {
                var damageState = (DamageState)UnityEngine.Random.Range(0, 4);
                var lifeformState = (LifeformState)UnityEngine.Random.Range(0, 10);
                var primaryWeaponState = (WeaponState)UnityEngine.Random.Range(0, 4);

                _publishedEntity.State.Appearance.DamageState = damageState;
                _publishedEntity.State.Appearance.LifeformState = lifeformState;
                _publishedEntity.State.Appearance.PrimaryWeaponState = primaryWeaponState;

                Debug.Log($"Set damageState={damageState} lifeformState={lifeformState} primaryWeaponState={primaryWeaponState}");
            }
            if (Input.GetKeyDown(KeyCode.N))
            {
                var reflectedEntity = FindObjectOfType<ReflectedEntity>();

                var damageState = reflectedEntity.State.Appearance.DamageState;
                var lifeformState = reflectedEntity.State.Appearance.LifeformState;
                var primaryWeaponState = reflectedEntity.State.Appearance.PrimaryWeaponState;

                Debug.Log($"RefletedEntity damageState={damageState} lifeformState={lifeformState} primaryWeaponState={primaryWeaponState}");
            }

            UpdateMovement();
        }

        private void UpdateMovement()
        {
            Vector3 move = Vector3.zero;

            if (Input.GetKey(KeyCode.W)) move += Vector3.up;
            if (Input.GetKey(KeyCode.S)) move += Vector3.down;
            if (Input.GetKey(KeyCode.D)) move += Vector3.right;
            if (Input.GetKey(KeyCode.A)) move += Vector3.left;
            if (Input.GetKey(KeyCode.E)) move += Vector3.forward;
            if (Input.GetKey(KeyCode.Q)) move += Vector3.back;

            _publishedEntity.transform.position += move * 4f * Time.deltaTime;
        }

        private void ReceivedDetonationPDU(DetonationPDU detonation)
        {
            Debug.Log($"Received detonation PDU! ({detonation.Location.X}, {detonation.Location.Y}, {detonation.Location.Z})");
        }

        private void ReceivedSampleEventReport(SampleEventReport sampleEventReport)
        {
            Debug.Log($"Received sample event report! A: {sampleEventReport.A} B: {sampleEventReport.B}");
        }
    }
}