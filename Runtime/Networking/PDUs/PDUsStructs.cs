using DuckbearLab.UniSim.Networking;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace DuckbearLab.UniSim.Networking
{
    [System.Serializable]
    public class PDUHeader : IPDUEncodable
    {
        public byte ProtocolVersion;
        public byte ExerciseId;
        public PDUType PDUType;
        public byte ProtocolFamilyType;
        public uint TimeStamp;
        public ushort PDULength;

        public void Operate(PDUEncoder pduEncoder)
        {
            pduEncoder.Operate(ref ProtocolVersion);
            pduEncoder.Operate(ref ExerciseId);
            pduEncoder.OperateEnumByte(ref PDUType);
            pduEncoder.Operate(ref ProtocolFamilyType);
            pduEncoder.Operate(ref TimeStamp);
            pduEncoder.Operate(ref PDULength);
            pduEncoder.Padding(2);
        }
    }

    [System.Serializable]
    public struct EntityId : IPDUEncodable, IEquatable<EntityId>
    {
        public ushort Site;
        public ushort Application;
        public ushort Entity;

        public EntityId(ushort site, ushort application, ushort entity)
        {
            Site = site;
            Application = application;
            Entity = entity;
        }

        public void Operate(PDUEncoder pduEncoder)
        {
            pduEncoder.Operate(ref Site);
            pduEncoder.Operate(ref Application);
            pduEncoder.Operate(ref Entity);
        }

        public static EntityId FromString(string entityId)
        {
            var parts = entityId.Split(':');

            if (parts.Length != 3) throw new System.Exception("Invalid entityId string");

            return new EntityId()
            {
                Site = ushort.Parse(parts[0]),
                Application = ushort.Parse(parts[1]),
                Entity = ushort.Parse(parts[2]),
            };
        }

        public override string ToString()
        {
            return $"{Site}:{Application}:{Entity}";
        }

        public override bool Equals(object obj) => (obj is EntityId) && Equals((EntityId)obj);

        public bool Equals(EntityId other)
        {
            return Site == other.Site &&
                Application == other.Application &&
                Entity == other.Entity;
        }

        public override int GetHashCode()
        {
            unchecked // Overflow is fine, just wrap
            {
                int hash = 17;
                hash = hash * 31 + Site.GetHashCode();
                hash = hash * 31 + Application.GetHashCode();
                hash = hash * 31 + Entity.GetHashCode();
                return hash;
            }
        }

        public static bool operator ==(EntityId left, EntityId right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(EntityId left, EntityId right)
        {
            return !left.Equals(right);
        }
    }

    [System.Serializable]
    public struct EntityType : IPDUEncodable
    {
        public byte Kind;
        public byte Domain;
        public ushort Country;
        public byte Category;
        public byte Subcategory;
        public byte Specific;
        public byte Extra;

        public EntityType(byte kind, byte domain, ushort country, byte category, byte subcategory, byte specific, byte extra)
        {
            Kind = kind;
            Domain = domain;
            Country = country;
            Category = category;
            Subcategory = subcategory;
            Specific = specific;
            Extra = extra;
        }

        public void Operate(PDUEncoder pduEncoder)
        {
            pduEncoder.Operate(ref Kind);
            pduEncoder.Operate(ref Domain);
            pduEncoder.Operate(ref Country);
            pduEncoder.Operate(ref Category);
            pduEncoder.Operate(ref Subcategory);
            pduEncoder.Operate(ref Specific);
            pduEncoder.Operate(ref Extra);
        }

        public static EntityType FromString(string entityType)
        {
            var parts = entityType.Split(':');

            if (parts.Length != 7) throw new System.Exception("Invalid entityType string");

            return new EntityType()
            {
                Kind = byte.Parse(parts[0]),
                Domain = byte.Parse(parts[1]),
                Country = ushort.Parse(parts[2]),
                Category = byte.Parse(parts[3]),
                Subcategory = byte.Parse(parts[4]),
                Specific = byte.Parse(parts[5]),
                Extra = byte.Parse(parts[6])
            };
        }

        public override string ToString()
        {
            return $"{Kind}:{Domain}:{Country}:{Category}:{Subcategory}:{Specific}:{Extra}";
        }

        public override bool Equals(object obj) => (obj is EntityType) && Equals((EntityType)obj);

        public bool Equals(EntityType other)
        {
            return Kind == other.Kind &&
                Domain == other.Domain &&
                Country == other.Country &&
                Category == other.Category &&
                Subcategory == other.Subcategory &&
                Specific == other.Specific &&
                Extra == other.Extra;
        }

        public override int GetHashCode()
        {
            unchecked // Overflow is fine, just wrap
            {
                int hash = 17;
                hash = hash * 31 + Kind.GetHashCode();
                hash = hash * 31 + Domain.GetHashCode();
                hash = hash * 31 + Country.GetHashCode();
                hash = hash * 31 + Category.GetHashCode();
                hash = hash * 31 + Subcategory.GetHashCode();
                hash = hash * 31 + Specific.GetHashCode();
                hash = hash * 31 + Extra.GetHashCode();
                return hash;
            }
        }

        public static bool operator ==(EntityType left, EntityType right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(EntityType left, EntityType right)
        {
            return !left.Equals(right);
        }
    }

    [System.Serializable]
    public struct Vector3Float : IPDUEncodable
    {
        public float X;
        public float Y;
        public float Z;

        public Vector3Float(float x, float y, float z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public Vector3Float(Vector3Float other)
        {
            X = other.X;
            Y = other.Y;
            Z = other.Z;
        }

        public void Operate(PDUEncoder pduEncoder)
        {
            pduEncoder.Operate(ref X);
            pduEncoder.Operate(ref Y);
            pduEncoder.Operate(ref Z);
        }

        public void Set(Vector3 v)
        {
            X = v.x;
            Y = v.y;
            Z = v.z;
        }

        public void Set(float x, float y, float z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public Vector3 ToUnityVector()
        {
            return new Vector3(X, Y, Z);
        }

        public static Vector3Float operator +(Vector3Float a, Vector3Float b) => new Vector3Float(a.X + b.X, a.Y + b.Y, a.Z + b.Z);
        public static Vector3Float operator -(Vector3Float a, Vector3Float b) => new Vector3Float(a.X - b.X, a.Y - b.Y, a.Z - b.Z);
        public static Vector3Float operator *(Vector3Float a, float f) => new Vector3Float(a.X * f, a.Y * f, a.Z * f);
        public static Vector3Float operator *(Vector3Float a, double d) => new Vector3Float((float)(a.X * d), (float)(a.Y * d), (float)(a.Z * d));
    }

    [System.Serializable]
    public struct Vector3Double : IPDUEncodable
    {
        public double X;
        public double Y;
        public double Z;

        public void Operate(PDUEncoder pduEncoder)
        {
            pduEncoder.Operate(ref X);
            pduEncoder.Operate(ref Y);
            pduEncoder.Operate(ref Z);
        }

        public Vector3Double(double x, double y, double z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public Vector3Double(Vector3Double d)
        {
            X = d.X;
            Y = d.Y;
            Z = d.Z;
        }

        public void Set(double x, double y, double z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public void Set(Vector3 v)
        {
            X = v.x;
            Y = v.y;
            Z = v.z;
        }

        public static explicit operator Vector3(Vector3Double v) =>
            new Vector3((float) v.X, (float) v.Y, (float) v.Z);

        public static explicit operator Vector3Double(Vector3 v) =>
            new Vector3Double(v.x, v.y, v.z);

        public static Vector3Double operator +(Vector3Double a, Vector3Double b) => new Vector3Double(a.X + b.X, a.Y + b.Y, a.Z + b.Z);
        public static Vector3Double operator +(Vector3Double a, Vector3Float b) => new Vector3Double(a.X + b.X, a.Y + b.Y, a.Z + b.Z);
        public static Vector3Double operator -(Vector3Double a, Vector3Double b) => new Vector3Double(a.X - b.X, a.Y - b.Y, a.Z - b.Z);
        public static Vector3Double operator *(Vector3Double a, double d) => new Vector3Double(a.X * d, a.Y * d, a.Z * d);
    }

    public enum DamageState
    {
        NoDamage,
        SlightDamage,
        ModerateDamage,
        Destroyed
    }

    public enum LifeformState
    {
        Null,
        UprightStandingStill,
        UprightWalking,
        UprightRunning,
        Kneeling,
        Prone,
        Crawling,
        Swimming,
        Parachuting,
        Jumping
    }

    public enum WeaponState
    {
        NotPresent,
        Stowed,
        Deployed,
        FiringPosition
    }

    [System.Serializable]
    public class Appearance : IPDUEncodable
    {
        public uint BitValue;

        public void Operate(PDUEncoder pduEncoder)
        {
            pduEncoder.Operate(ref BitValue);
        }

        public DamageState DamageState
        {
            get => (DamageState)GetBits(3, 2);
            set => SetBits(3, 2, (uint)value);
        }

        // Use only on Life Form entities
        public LifeformState LifeformState
        {
            get => (LifeformState)GetBits(16, 4);
            set => SetBits(16, 4, (uint)value);
        }

        // Use only on Life Form entities
        public WeaponState PrimaryWeaponState
        {
            get => (WeaponState)GetBits(16 + 8, 4);
            set => SetBits(16 + 8, 4, (uint)value);
        }

        private uint GetBits(int index, int size)
        {
            //For example, index = 3 and size = 4
            uint mask = (1u << size + 1) - 1u; // Turn on first [size] bits - 0b00000000_00001111
            mask = mask << index; // Shift [index] bits - 0b00000000_01111000
            return (BitValue & mask) >> index; // Apply mask on value and shift result to start
        }

        private void SetBits(int index, int size, uint value)
        {
            //For example, index = 3 and size = 4
            uint clearMask = (1u << size + 1) - 1u; // Turn on first [size] bits - 0b00000000_00001111
            clearMask = ~(clearMask << index); // Shift [index] bits and invert - 0b11111111_10000111
            BitValue &= clearMask; // Clear BitValue using the clearMask
            BitValue |= value << index; // Set the desired value bits
        }

    }

    public enum DeadReckoningAlgorithm : byte
    {
        Other = 0,
        Static,
        FPW,
        RPW,
        RVW,
        FVW,
        FPB,
        RPB,
        RVB,
        FVB
    }

    [System.Serializable]
    public class DeadReckoning : IPDUEncodable
    {
        public DeadReckoningAlgorithm Algorithm = DeadReckoningAlgorithm.RVW;
        public Vector3Float LinearAcceleration = new Vector3Float();
        public Vector3Float AngularVelocity = new Vector3Float();

        public void Operate(PDUEncoder pduEncoder)
        {
            pduEncoder.OperateEnumByte(ref Algorithm);
            pduEncoder.Padding(120 / 8);
            pduEncoder.Operate(ref LinearAcceleration);
            pduEncoder.Operate(ref AngularVelocity);
        }
    }

    [System.Serializable]
    public class MarkingText : IPDUEncodable
    {
        public byte CharacterSet = 1;
        public string String = "";

        public void Operate(PDUEncoder pduEncoder)
        {
            pduEncoder.Operate(ref CharacterSet);
            pduEncoder.Operate(ref String, 11);
        }
    }

    [System.Serializable]
    public class ArticulatedPart : IPDUEncodable
    {
        public byte TypeDesignator;
        public byte ChangeIndicator;
        public uint TypeVariantAttached;
        public uint TypeVariantArticulated;
        public double value;

        public void Operate(PDUEncoder pduEncoder)
        {
            pduEncoder.Operate(ref TypeDesignator);
            pduEncoder.Operate(ref ChangeIndicator);
            pduEncoder.Operate(ref TypeVariantAttached);
            pduEncoder.Operate(ref TypeVariantArticulated);
            pduEncoder.Operate(ref value);
        }
    }

    [System.Serializable]
    public class EventId : IPDUEncodable, IEquatable<EventId>
    {
        public ushort Site;
        public ushort Application;
        public ushort Event;

        public EventId()
        {

        }

        public EventId(ushort site, ushort application, ushort eventNum)
        {
            Site = site;
            Application = application;
            Event = eventNum;
        }

        public void Operate(PDUEncoder pduEncoder)
        {
            pduEncoder.Operate(ref Site);
            pduEncoder.Operate(ref Application);
            pduEncoder.Operate(ref Event);
        }

        public override string ToString()
        {
            return $"{Site}:{Application}:{Event}";
        }

        public override bool Equals(object obj) => (obj is EventId) && Equals((EventId)obj);

        public bool Equals(EventId other)
        {
            return this.Site == other.Site &&
                this.Application == other.Application &&
                this.Event == other.Event;
        }

        public override int GetHashCode()
        {
            unchecked // Overflow is fine, just wrap
            {
                int hash = 17;
                hash = hash * 31 + Site.GetHashCode();
                hash = hash * 31 + Application.GetHashCode();
                hash = hash * 31 + Event.GetHashCode();
                return hash;
            }
        }
    }

    public enum WarheadType : ushort
    {
        Other = 0,
        Cargo_VariableSubmunitions = 10,
        Fuel_AirExplosive = 20,
        GlassBeads = 30,
        _1um = 31,
        _5um = 32,
        _10um = 33,
        HighExplosive_HE = 1000,
        HE_Plastic = 1100,
        HE_Incendiary = 1200,
        HE_Fragmentation = 1300,
        HE_Antitank = 1400,
        HE_Bomblets = 1500,
        HE_ShapedCharge = 1600,
        HE_ContinuousRod = 1610,
        HE_TungstenBall = 1615,
        HE_BlastFragmentation = 1620,
        HE_SteerableDartswithHE = 1625,
        HE_Darts = 1630,
        HE_Flechettes = 1635,
        HE_DirectedFragmentation = 1640,
        HE_Semi_ArmorPiercing_SAP = 1645,
        HE_ShapedChargeFragmentation = 1650,
        HE_Semi_ArmorPiercing_Fragmentation = 1655,
        HE_HallowCharge = 1660,
        HE_DoubleHallowCharge = 1665,
        HE_GeneralPurpose = 1670,
        HE_BlastPenetrator = 1675,
        HE_RodPenetrator = 1680,
        HE_Antipersonnel = 1685,
        Smoke = 2000,
        Illumination = 3000,
        Practice = 4000,
        Kinetic = 5000,
        Mines = 6000,
        Nuclear = 7000,
        Nuclear_IMT = 7010,
        Chemical_General = 8000,
        Chemical_BlisterAgent = 8100,
        HD_Mustard = 8110,
        ThickenedHD_Mustard = 8115,
        DustyHD_Mustard = 8120,
        Chemical_BloodAgent = 8200,
        AC_HCN = 8210,
        CK_CNCI = 8215,
        CG_Phosgene = 8220,
        Chemical_NerveAgent = 8300,
        VX = 8310,
        ThickenedVX = 8315,
        DustyVX = 8320,
        GA_Tabun = 8325,
        ThickenedGA_Tabun = 8330,
        DustyGA_Tabun = 8335,
        GB_Sarin = 8340,
        ThickenedGB_Sarin = 8345,
        DustyGB_Sarin = 8350,
        GD_Soman = 8355,
        ThickenedGD_Soman = 8360,
        DustyGD_Soman = 8365,
        GF = 8370,
        ThickenedGF = 8375,
        DustyGF = 8380,
        Biological = 9000,
        Biological_Virus = 9100,
        Biological_Bacteria = 9200,
        Biological_Rickettsia = 9300,
        Biological_GeneticallyModifiedMicro_organisms = 9400,
        Biological_Toxin = 9500
    }

    public enum FuseType : ushort
    {
        Other = 0,
        IntelligentInfluence = 10,
        Sensor = 20,
        Self_destruct = 30,
        UltraQuick = 40,
        Body = 50,

        DeepIntrusion = 60,
        Multifunction = 100,

        PointDetonation_PD = 200,
        BaseDetonation_BD = 300,

        Contact = 1000,
        Contact_Instant_Impact = 1100,
        Contact_Delayed = 1200,
        _10msdelay = 1201,
        _20msdelay = 1202,
        _50msdelay = 1205,
        _60msdelay = 1206,
        _100msdelay = 1210,
        _125msdelay = 1212,
        _250msdelay = 1225,
        Contact_Electronic_ObliqueContact = 1300,
        Contact_Graze = 1400,
        Contact_Crush = 1500,
        Contact_Hydrostatic = 1600,
        Contact_Mechanical = 1700,
        Contact_Chemical = 1800,
        Contact_Piezoelectric = 1900,
        Contact_PointInitiating = 1910,
        Contact_PointInitiating_BaseDetonating = 1920,
        Contact_BaseDetonating = 1930,
        Contact_BallisticCapandBase = 1940,
        Contact_Base = 1950,
        Contact_Nose = 1960,
        Contact_FittedinStandoffProbe = 1970,
        Contact_Non_aligned = 1980,

        Timed = 2000,
        Timed_Programmable = 2100,
        Timed_Burnout = 2200,
        Timed_Pyrotechnic = 2300,
        Timed_Electronic = 2400,
        Timed_BaseDelay = 2500,
        Timed_ReinforcedNoseImpactDelay = 2600,
        Timed_ShortDelayImpact = 2700,
        _10msdelay1 = 2701,
        _20msdelay1 = 2702,
        _50msdelay1 = 2705,
        _60msdelay1 = 2706,
        _100msdelay1 = 2710,
        _125msdelay1 = 2712,
        _250msdelay1 = 2725,
        Timed_NoseMountedVariableDelay = 2800,
        Timed_LongDelaySide = 2900,
        Timed_SelectableDelay = 2910,
        Timed_Impact = 2920,
        Timed_Sequence = 2930,

        Proximity = 3000,
        Proximity_ActiveLaser = 3100,
        Proximity_Magnetic_Magpolarity = 3200,
        Proximity_ActiveRadar_DopplerRadar = 3300,
        Proximity_RadioFrequency_RF = 3400,
        Proximity_Programmable = 3500,
        Proximity_Programmable_Prefragmented = 3600,
        Proximity_Infrared = 3700,

        Command = 4000,
        Command_Electronic_RemotelySet = 4100,

        Altitude = 5000,
        Altitude_RadioAltimeter = 5100,
        Altitude_AirBurst = 5200,

        Depth = 6000,

        Acoustic = 7000,

        Pressure = 8000,
        Pressure_Delay = 8010,
        Inert = 8100,
        Dummy = 8110,
        Practice = 8120,
        PlugRepresenting = 8130,
        Training = 8150,

        Pyrotechnic = 9000,
        Pyrotechnic_Delay = 9010,
        Electro_optical = 9100,
        Electromechanical = 9110,
        Electromechanical_Nose = 9120,
        Strikerless = 9200,
        Strikerless_NoseImpact = 9210,
        Strikerless_Compression_Ignition = 9220,
        Compression_Ignition = 9300,
        Compression_Ignition_Strikerless_NoseImpact = 9310,
        Percussion = 9400,
        Percussion_Instantaneous = 9410,
        Electronic = 9500,
        Electronic_InternallyMounted = 9510,
        Electronic_RangeSetting = 9520,
        Electronic_Programmed = 9530,
        Mechanical = 9600,
        Mechanical_Nose = 9610,
        Mechanical_Tail = 9620
    }

    [System.Serializable]
    public class BurstDescriptor : IPDUEncodable
    {
        public EntityType Munition = new EntityType();
        public WarheadType Warhead;
        public FuseType Fuse;
        public ushort Quantity;
        public ushort Rate;

        public void Operate(PDUEncoder pduEncoder)
        {
            pduEncoder.Operate(ref Munition);
            pduEncoder.OperateEnumUShort(ref Warhead);
            pduEncoder.OperateEnumUShort(ref Fuse);
            pduEncoder.Operate(ref Quantity);
            pduEncoder.Operate(ref Rate);
        }
    }

    public enum DetonationResult : byte
    {
        Other = 0,
        EntityImpact = 1,
        EntityProximateDetonation = 2,
        GroundImpact = 3,
        GroundProximateDetonation = 4,
        Detonation = 5,
        None = 6,
        HEhit_Small = 7,
        HEhit_Medium = 8,
        HEhit_Large = 9,
        Armor_PiercingHit = 10,
        Dirtblast_Small = 11,
        Dirtblast_Medium = 12,
        Dirtblast_Large = 13,
        Waterblast_Small = 14,
        Waterblast_Medium = 15,
        Waterblast_Large = 16,
        AirHit = 17,
        BuildingHit_Small = 18,
        BuildingHit_Medium = 19,
        BuildingHit_Large = 20,
        MineClearingLineCharge = 21,
        EnvironmentObjectImpact = 22,
        EnvironmentObjectProximateDetonation = 23,
        WaterImpact = 24,
        AirBurst = 25,
        KillWithFragmentType1 = 26,
        KillWithFragmentType2 = 27,
        KillWithFragmentType3 = 28,
        KillWithFragmentType1AfterflyOutFailure = 29,
        KillWithFragmentType2AfterflyOutFailure = 30,
        MissDueToflyOutFailure = 31,
        MissDueToEndGameFailure = 32,
        MissDueToflyOutAndEndGameFailure = 33
    }

    public enum ParameterTypeDesignator : byte
    {
        ArticulatedPart = 0,
        AttachedPart
    }

    [System.Serializable]
    public class ArticulationParameter : IPDUEncodable
    {
        public ParameterTypeDesignator ParameterTypeDesignator;
        public byte ParameterChangeIndicator;
        public ushort ArticulationAttachmentID;
        public ulong ParameterTypeVarient; // Todo: Implement correctly
        public ulong ArticulationParameterValue; // Todo: Implement correctly

        public void Operate(PDUEncoder pduEncoder)
        {
            pduEncoder.OperateEnumByte(ref ParameterTypeDesignator);
            pduEncoder.Operate(ref ParameterChangeIndicator);
            pduEncoder.Operate(ref ArticulationAttachmentID);
            pduEncoder.Operate(ref ParameterTypeVarient);
            pduEncoder.Operate(ref ArticulationParameterValue);
        }
    }

    [System.Serializable]
    public class FixedDatumRecord : IPDUEncodable
    {
        public uint FixedDatumID;
        public int FixedDatumValue;

        public void Operate(PDUEncoder pduEncoder)
        {
            pduEncoder.Operate(ref FixedDatumID);
            pduEncoder.Operate(ref FixedDatumValue);
        }
    }

    [System.Serializable]
    public class VariableDatumRecord : IPDUEncodable
    {

        public uint VariableDatumID;
        public byte[] VariableDatumValue = new byte[0];

        public void Operate(PDUEncoder pduEncoder)
        {
            pduEncoder.Operate(ref VariableDatumID);

            pduEncoder.OperateArraySizeBits32(ref VariableDatumValue, sizeof(byte));

            for (int i = 0; i < VariableDatumValue.Length; i++)
                pduEncoder.Operate(ref VariableDatumValue[i]);

            // Padding to 64 bit (8 bytes)
            if (VariableDatumValue.Length % 8 > 0)
                pduEncoder.Padding(8 - (VariableDatumValue.Length % 8));
        }
    }
}