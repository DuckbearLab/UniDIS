using DuckbearLab.UniSim.Networking;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace DuckbearLab.UniSim.Networking
{
    public enum PDUType : byte
    {
        EntityState = 1,
        Fire = 2,
        Detonation = 3,
        EventReport = 21
    }

    public abstract class PDU : IPDUEncodable
    {
        public abstract PDUType PDUType { get; }
        public abstract void Operate(PDUEncoder pduEncoder);
    }

    [Serializable]
    public class EntityStatePDU : PDU
    {
        public override PDUType PDUType => PDUType.EntityState;

        public EntityId EntityId = new EntityId();
        public byte ForceId = 0;
        public EntityType EntityType = new EntityType();
        public EntityType AlternativeEntityType = new EntityType();
        public Vector3Float LinearVelocity = new Vector3Float();
        public Vector3Double Location = new Vector3Double();
        public Vector3Float Orientation = new Vector3Float();
        public Appearance Appearance = new Appearance();
        public DeadReckoning DeadRecokning = new DeadReckoning();
        public MarkingText MarkingText = new MarkingText();
        public uint Capabilities = 0;
        public ArticulatedPart[] ArticulatedParts = new ArticulatedPart[0];

        public override void Operate(PDUEncoder pduEncoder)
        {
            pduEncoder.Operate(ref EntityId);
            pduEncoder.Operate(ref ForceId);
            pduEncoder.OperateArraySize(ref ArticulatedParts);
            pduEncoder.Operate(ref EntityType);
            pduEncoder.Operate(ref AlternativeEntityType);
            pduEncoder.Operate(ref LinearVelocity);
            pduEncoder.Operate(ref Location);
            pduEncoder.Operate(ref Orientation);
            pduEncoder.Operate(ref Appearance);
            pduEncoder.Operate(ref DeadRecokning);
            pduEncoder.Operate(ref MarkingText);
            pduEncoder.Operate(ref Capabilities);
            pduEncoder.Operate(ref ArticulatedParts);
        }

        public void CopyFrom(EntityStatePDU other)
        {
            EntityId = other.EntityId;
            ForceId = other.ForceId;
            EntityType = other.EntityType;
            AlternativeEntityType = other.AlternativeEntityType;
            LinearVelocity = other.LinearVelocity;
            Location = other.Location;
            Orientation = other.Orientation;
            Appearance.BitValue = other.Appearance.BitValue;
            DeadRecokning.Algorithm = other.DeadRecokning.Algorithm;
            DeadRecokning.LinearAcceleration = other.DeadRecokning.LinearAcceleration;
            MarkingText.CharacterSet = other.MarkingText.CharacterSet;
            MarkingText.String = other.MarkingText.String;
            Capabilities = other.Capabilities;
            if (ArticulatedParts.Length != other.ArticulatedParts.Length)
                ArticulatedParts = new ArticulatedPart[other.ArticulatedParts.Length];
            for (int i = 0; i < other.ArticulatedParts.Length; i++)
            {
                ArticulatedParts[i].TypeDesignator = other.ArticulatedParts[i].TypeDesignator;
                ArticulatedParts[i].ChangeIndicator = other.ArticulatedParts[i].ChangeIndicator;
                ArticulatedParts[i].TypeVariantAttached = other.ArticulatedParts[i].TypeVariantAttached;
                ArticulatedParts[i].TypeVariantArticulated = other.ArticulatedParts[i].TypeVariantArticulated;
                ArticulatedParts[i].value = other.ArticulatedParts[i].value;
            }
        }
    }

    [Serializable]
    public class FirePDU : PDU
    {
        public override PDUType PDUType => PDUType.Fire;

        public EntityId FiringEntityId = new EntityId();
        public EntityId TargetEntityId = new EntityId();
        public EntityId MunitionId = new EntityId();
        public EventId EventId = new EventId();
        public uint FireMissionIndex = 0;
        public Vector3Double Location = new Vector3Double();
        public BurstDescriptor BurstDescriptor = new BurstDescriptor();
        public Vector3Float Velocity = new Vector3Float();
        public float Range = 0;

        public override void Operate(PDUEncoder pduEncoder)
        {
            pduEncoder.Operate(ref FiringEntityId);
            pduEncoder.Operate(ref TargetEntityId);
            pduEncoder.Operate(ref MunitionId);
            pduEncoder.Operate(ref EventId);
            pduEncoder.Operate(ref FireMissionIndex);
            pduEncoder.Operate(ref Location);
            pduEncoder.Operate(ref BurstDescriptor);
            pduEncoder.Operate(ref Velocity);
            pduEncoder.Operate(ref Range);
        }
    }

    [Serializable]
    public class DetonationPDU : PDU
    {
        public override PDUType PDUType => PDUType.Detonation;

        public EntityId FiringEntityId = new EntityId();
        public EntityId TargetEntityId = new EntityId();
        public EntityId MunitionId = new EntityId();
        public EventId EventId = new EventId();
        public Vector3Float Velocity = new Vector3Float();
        public Vector3Double Location = new Vector3Double();
        public BurstDescriptor BurstDescriptor = new BurstDescriptor();
        public Vector3Float LocationInEntityCoordinates = new Vector3Float();
        public DetonationResult DetonationResult;
        public ArticulationParameter[] ArticulationParameters = new ArticulationParameter[0];

        public override void Operate(PDUEncoder pduEncoder)
        {
            pduEncoder.Operate(ref FiringEntityId);
            pduEncoder.Operate(ref TargetEntityId);
            pduEncoder.Operate(ref MunitionId);
            pduEncoder.Operate(ref EventId);
            pduEncoder.Operate(ref Velocity);
            pduEncoder.Operate(ref Location);
            pduEncoder.Operate(ref BurstDescriptor);
            pduEncoder.Operate(ref LocationInEntityCoordinates);
            pduEncoder.OperateEnumByte(ref DetonationResult);
            pduEncoder.OperateArraySize(ref ArticulationParameters);
            pduEncoder.Padding(2);
            pduEncoder.Operate(ref ArticulationParameters);
        }
    }

    [Serializable]
    public class EventReportPDU : PDU
    {
        public override PDUType PDUType => PDUType.EventReport;

        public EntityId OriginatingEntityID = new EntityId();
        public EntityId ReceivingEntityID = new EntityId();
        public uint EventType;
        public List<FixedDatumRecord> FixedDatumRecords = new List<FixedDatumRecord>();
        public List<VariableDatumRecord> VariableDatumRecords = new List<VariableDatumRecord>();

        public override void Operate(PDUEncoder pduEncoder)
        {
            pduEncoder.Operate(ref OriginatingEntityID);
            pduEncoder.Operate(ref ReceivingEntityID);
            pduEncoder.Operate(ref EventType);
            pduEncoder.Padding(4);
            pduEncoder.OperateListSize32(ref FixedDatumRecords);
            pduEncoder.OperateListSize32(ref VariableDatumRecords);
            pduEncoder.Operate(ref FixedDatumRecords);
            pduEncoder.Operate(ref VariableDatumRecords);
        }
    }
}
