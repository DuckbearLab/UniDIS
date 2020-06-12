using DuckbearLab.UniSim.CustomEventReports;
using DuckbearLab.UniSim.Networking;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using UnityEngine;

namespace DuckbearLab.UniSim
{
    public class CustomEventReportEncoder
    {
        public EventReportPDU EventReportPdu { get; private set; }

        public EntityId SenderId
        {
            get { return EventReportPdu.OriginatingEntityID; }
            set { EventReportPdu.OriginatingEntityID = value; }
        }

        public EntityId ReceiverId
        {
            get { return EventReportPdu.ReceivingEntityID; }
            set { EventReportPdu.ReceivingEntityID = value; }
        }

        private int readingFixedIndex;
        private int readingVarIndex;
        private object target;

        public CustomEventReportEncoder(uint eventType, object target)
        {
            EventReportPdu = new EventReportPDU();
            EventReportPdu.EventType = eventType;
            this.target = target;
        }

        public CustomEventReportEncoder(EventReportPDU pduToRead, object target)
        {
            readingFixedIndex = readingVarIndex = 0;
            EventReportPdu = pduToRead;
            this.target = target;
        }

        public object Read(Type type, FieldInfo field = null)
        {
            if (type == typeof(int)) return ReadParamInt();
            if (type == typeof(bool)) return ReadParamBool();
            if (type == typeof(byte)) return ReadParamByte();
            if (type == typeof(uint)) return ReadParamUInt();
            if (type == typeof(ushort)) return ReadParamUShort();
            if (type == typeof(float)) return ReadParamFloat();
            if (type == typeof(double)) return ReadParamDouble();
            if (type == typeof(string)) return ReadParamString();
            if (type == typeof(Vector3)) return ReadParamVector3();
            if (type == typeof(EntityId)) return ReadParamEntityId();
            if (type == typeof(EntityType)) return ReadParamEntityType();
            if (type == typeof(EventId)) return ReadParamEventID();
            if (type.IsEnum) return ReadParamInt(); //implicit conversion from int to enum, might break.
            if (type.IsArray) return ReadArray(field);
            if (type.IsValueType && !type.IsPrimitive) return ReadStruct(type);

            throw new Exception("Unknown type to read on field " + field.Name + "!");
        }

        public void Write(object value, FieldInfo field = null)
        {
            var valueType = field != null ? field.FieldType : value.GetType();
            if (valueType == typeof(int)) WriteParam((int)value, 0, field);
            else if (valueType == typeof(uint)) WriteParam((uint)value);
            else if (valueType == typeof(bool)) WriteParam((bool)value);
            else if (valueType == typeof(byte)) WriteParam((byte)value);
            else if (valueType == typeof(ushort)) WriteParam((ushort)value);
            else if (valueType == typeof(float)) WriteParam((float)value);
            else if (valueType == typeof(double)) WriteParam((double)value);
            else if (valueType == typeof(string)) WriteParam((string)value);
            else if (valueType == typeof(Vector3)) WriteParam((Vector3)value);
            else if (valueType == typeof(EntityId)) WriteParam((EntityId)value);
            else if (valueType == typeof(EntityType)) WriteParam((EntityType)value);
            else if (valueType == typeof(EventId)) WriteParam((EventId)value);
            else if (valueType.IsEnum) WriteParam((int)value);
            else if (valueType.IsArray) WriteParam((Array)value, field);
            else if (valueType.IsValueType && !valueType.IsPrimitive) WriteStruct(value);
            else throw new Exception("Unknown type to write on field " + field.Name + "!");
        }

        #region int
        private void WriteParam(int data, uint datumId = 0, FieldInfo field = null)
        {
            if (field != null && field.GetCustomAttributes(typeof(CustomEventReportCalculatedAttribute), true).Length > 0)
            {
                foreach (var targetField in target.GetType().GetFields())
                {
                    foreach (CustomEventReportArrayAttribute attribute in targetField.GetCustomAttributes(typeof(CustomEventReportArrayAttribute), true))
                    {
                        if (attribute.ArrayLengthField == field.Name)
                        {
                            Array arr = (Array)targetField.GetValue(target);
                            if (arr != null)
                                EventReportPdu.FixedDatumRecords.Add(new FixedDatumRecord() { FixedDatumID = datumId, FixedDatumValue = arr.Length });
                            else
                                EventReportPdu.FixedDatumRecords.Add(new FixedDatumRecord() { FixedDatumID = datumId, FixedDatumValue = 0 });
                            return;
                        }
                    }
                }
                throw new Exception("Could not calculate field " + field.Name + "!");
            }
            else
            {
                EventReportPdu.FixedDatumRecords.Add(new FixedDatumRecord() { FixedDatumID = datumId, FixedDatumValue = data });
            }
        }

        private int ReadParamInt()
        {
            return EventReportPdu.FixedDatumRecords[readingFixedIndex++].FixedDatumValue;
        }
        #endregion

        #region bool
        private void WriteParam(bool data, uint datumId = 0)
        {
            EventReportPdu.FixedDatumRecords.Add(new FixedDatumRecord() { FixedDatumID = datumId, FixedDatumValue = data ? 1 : 0 });
        }

        private bool ReadParamBool()
        {
            return EventReportPdu.FixedDatumRecords[readingFixedIndex++].FixedDatumValue % 2 == 1;
        }
        #endregion

        #region byte
        private void WriteParam(byte data, uint datumId = 0)
        {
            EventReportPdu.FixedDatumRecords.Add(new FixedDatumRecord() { FixedDatumID = datumId, FixedDatumValue = unchecked((int)data) });
        }

        private byte ReadParamByte()
        {
            return unchecked((byte)EventReportPdu.FixedDatumRecords[readingFixedIndex++].FixedDatumValue);
        }
        #endregion

        #region uint
        private void WriteParam(uint data, uint datumId = 0)
        {
            EventReportPdu.FixedDatumRecords.Add(new FixedDatumRecord() { FixedDatumID = datumId, FixedDatumValue = unchecked((int)data) });
        }

        private uint ReadParamUInt()
        {
            return unchecked((uint)EventReportPdu.FixedDatumRecords[readingFixedIndex++].FixedDatumValue);
        }
        #endregion

        #region ushort
        private void WriteParam(ushort data, uint datumId = 0)
        {
            EventReportPdu.FixedDatumRecords.Add(new FixedDatumRecord() { FixedDatumID = datumId, FixedDatumValue = unchecked((int)data) });
        }

        private ushort ReadParamUShort()
        {
            return unchecked((ushort)EventReportPdu.FixedDatumRecords[readingFixedIndex++].FixedDatumValue);
        }
        #endregion

        #region float
        private void WriteParam(float data, uint datumId = 0)
        {
            EventReportPdu.FixedDatumRecords.Add(new FixedDatumRecord() { FixedDatumID = datumId, FixedDatumValue = BitConverter.ToInt32(BitConverter.GetBytes(data), 0) });
        }

        private float ReadParamFloat()
        {
            return BitConverter.ToSingle(BitConverter.GetBytes(EventReportPdu.FixedDatumRecords[readingFixedIndex++].FixedDatumValue), 0);
        }
        #endregion

        #region double
        //loss of data, but the same as c++
        private void WriteParam(double data, uint datumId = 0)
        {
            float fData = (float)data;
            EventReportPdu.FixedDatumRecords.Add(new FixedDatumRecord() { FixedDatumID = datumId, FixedDatumValue = BitConverter.ToInt32(BitConverter.GetBytes(fData), 0) });
        }

        private double ReadParamDouble()
        {
            return (double)BitConverter.ToSingle(BitConverter.GetBytes(EventReportPdu.FixedDatumRecords[readingFixedIndex++].FixedDatumValue), 0);
        }
        #endregion

        #region string
        private void WriteParam(string data, uint datumId = 0)
        {
            if (data != null)
            {
                var bytes = Encoding.UTF8.GetBytes(data);

                EventReportPdu.VariableDatumRecords.Add(new VariableDatumRecord { VariableDatumID = datumId, VariableDatumValue = bytes });
            }
            else
                EventReportPdu.VariableDatumRecords.Add(new VariableDatumRecord { VariableDatumID = datumId, VariableDatumValue = new byte[0] });
        }

        private string ReadParamString()
        {
            return Encoding.UTF8.GetString(EventReportPdu.VariableDatumRecords[readingVarIndex++].VariableDatumValue);
        }
        #endregion

        #region Vector3
        private void WriteParam(Vector3 vec)
        {
            WriteParam(vec.x);
            WriteParam(vec.y);
            WriteParam(vec.z);
        }

        private Vector3 ReadParamVector3()
        {
            return new Vector3()
            {
                x = (float)ReadParamDouble(),
                y = (float)ReadParamDouble(),
                z = (float)ReadParamDouble()
            };
        }
        #endregion

        #region EntityId
        private void WriteParam(EntityId entityId)
        {
            WriteParam(entityId.Site, 15100);
            WriteParam(entityId.Application, 15200);
            WriteParam(entityId.Entity, 15300);
        }

        private EntityId ReadParamEntityId()
        {
            return new EntityId()
            {
                Site = ReadParamUShort(),
                Application = ReadParamUShort(),
                Entity = ReadParamUShort()
            };
        }
        #endregion

        #region EntityType
        private void WriteParam(EntityType entityType)
        {
            WriteParam(entityType.Kind, 11110);
            WriteParam(entityType.Domain, 11120);
            WriteParam(entityType.Country, 11130);
            WriteParam(entityType.Category, 11140);
            WriteParam(entityType.Subcategory, 11150);
            WriteParam(entityType.Specific, 11160);
            WriteParam(entityType.Extra, 11170);
        }

        private EntityType ReadParamEntityType()
        {
            return new EntityType()
            {
                Kind = ReadParamByte(),
                Domain = ReadParamByte(),
                Country = ReadParamUShort(),
                Category = ReadParamByte(),
                Subcategory = ReadParamByte(),
                Specific = ReadParamByte(),
                Extra = ReadParamByte()
            };
        }
        #endregion

        #region EventID
        private void WriteParam(EventId eventId)
        {
            WriteParam(eventId.Site, 500000);
            WriteParam(eventId.Application, 500010);
            WriteParam(eventId.Event, 500020);
        }

        private EventId ReadParamEventID()
        {
            return new EventId()
            {
                Site = ReadParamUShort(),
                Application = ReadParamUShort(),
                Event = ReadParamUShort()
            };
        }
        #endregion

        #region Array
        private void WriteParam(Array array, FieldInfo field)
        {
            if (array != null)
                foreach (var item in array)
                    Write(item);
        }

        private Array ReadArray(FieldInfo field)
        {
            var arrayLengthAttributes = field.GetCustomAttributes(typeof(CustomEventReportArrayAttribute), true);
            if (arrayLengthAttributes.Length > 0)
            {
                var arrayLengthAttribute = (CustomEventReportArrayAttribute)arrayLengthAttributes[0];
                var arrayLengh = (int)target.GetType().GetField(arrayLengthAttribute.ArrayLengthField).GetValue(target);
                var arrayElementType = field.FieldType.GetElementType();

                if (arrayLengh < 0) // Is this the right thing to do?
                    arrayLengh = 0;

                var array = Array.CreateInstance(arrayElementType, arrayLengh);

                for (int i = 0; i < arrayLengh; i++)
                    array.SetValue(Read(arrayElementType), i);

                return array;
            }
            else
            {
                Debug.Log("EventReportArray Attribute not found on field" + field.Name + "!");
            }
            return new int[] { 1, 2, 3 };
        }
        #endregion

        #region Struct
        private void WriteStruct(object obj)
        {
            var prevTarget = target;

            target = obj;

            foreach (var field in obj.GetType().GetFields())
            {
                var value = field.GetValue(obj);
                Write(value, field);
            }

            target = prevTarget;
        }

        private object ReadStruct(Type fieldType)
        {
            var prevTarget = target;

            object o = Activator.CreateInstance(fieldType);

            target = o;

            foreach (var field in fieldType.GetFields())
            {
                field.SetValue(o, Read(field.FieldType, field));
            }

            target = prevTarget;

            return o;
        }
        #endregion

    }
}