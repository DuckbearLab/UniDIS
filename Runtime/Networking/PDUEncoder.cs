using UnityEngine;
using System.Collections;
using System.IO;
using System.Text;
using System;
using System.Collections.Generic;

namespace DuckbearLab.UniSim.Networking
{

    public interface IPDUEncodable
    {
        void Operate(PDUEncoder pduEncoder);
    }

    public class PDUEncoder
    {
        private enum OperateState
        {
            Read,
            Write,
            Size
        }

        private OperateState _state;
        private BinaryWriter _writer;
        private BinaryReader _reader;

        private int _size = 0;

        public static void Encode(IPDUEncodable target, BinaryWriter writer)
        {
            PDUEncoder pduEncoder = new PDUEncoder();
            pduEncoder._state = OperateState.Write;
            pduEncoder._writer = writer;
            target.Operate(pduEncoder);
        }

        public static void Decode(IPDUEncodable target, BinaryReader reader)
        {
            PDUEncoder pduEncoder = new PDUEncoder();
            pduEncoder._state = OperateState.Read;
            pduEncoder._reader = reader;
            target.Operate(pduEncoder);
        }

        public static int Size(IPDUEncodable target)
        {
            PDUEncoder pduEncoder = new PDUEncoder();
            pduEncoder._state = OperateState.Size;
            pduEncoder._size = 0;
            target.Operate(pduEncoder);
            return pduEncoder._size;
        }

        private PDUEncoder()
        {

        }

        public void Operate(ref byte b)
        {
            if (_state == OperateState.Write)
                _writer.Write(b);
            else if (_state == OperateState.Read)
                b = _reader.ReadByte();
            else if (_state == OperateState.Size)
                _size += sizeof(byte);
        }

        public void OperateEnumByte<TEnum>(ref TEnum e) where TEnum : Enum
        {

            if (_state == OperateState.Write)
                _writer.Write(Convert.ToByte(e));
            else if (_state == OperateState.Read)
                e = (TEnum)Enum.ToObject(typeof(TEnum), _reader.ReadByte());
            else if (_state == OperateState.Size)
                _size += sizeof(byte);
        }

        public void OperateEnumUShort<TEnum>(ref TEnum e) where TEnum : Enum
        {

            if (_state == OperateState.Write)
                _writer.Write(Convert.ToUInt16(e));
            else if (_state == OperateState.Read)
                e = (TEnum)Enum.ToObject(typeof(TEnum), _reader.ReadUInt16());
            else if (_state == OperateState.Size)
                _size += sizeof(ushort);
        }

        public void Operate(ref sbyte b)
        {
            if (_state == OperateState.Write)
                _writer.Write(b);
            else if (_state == OperateState.Read)
                b = _reader.ReadSByte();
            else if (_state == OperateState.Size)
                _size += sizeof(sbyte);
        }

        public void Operate(ref char c)
        {
            if (_state == OperateState.Write)
                _writer.Write(c);
            else if (_state == OperateState.Read)
                c = _reader.ReadChar();
            else if (_state == OperateState.Size)
                _size += sizeof(char);
        }

        public void Operate(ref short s)
        {
            if (_state == OperateState.Write)
                _writer.Write(Reverse(BitConverter.GetBytes(s)));
            else if (_state == OperateState.Read)
                s = BitConverter.ToInt16(Reverse(_reader.ReadBytes(2)), 0);
            else if (_state == OperateState.Size)
                _size += sizeof(short);
        }

        public void Operate(ref ushort s)
        {
            if (_state == OperateState.Write)
                _writer.Write(Reverse(BitConverter.GetBytes(s)));
            else if (_state == OperateState.Read)
                s = BitConverter.ToUInt16(Reverse(_reader.ReadBytes(2)), 0);
            else if (_state == OperateState.Size)
                _size += sizeof(ushort);
        }

        public void Operate(ref int i)
        {
            if (_state == OperateState.Write)
                _writer.Write(Reverse(BitConverter.GetBytes(i)));
            else if (_state == OperateState.Read)
                i = BitConverter.ToInt32(Reverse(_reader.ReadBytes(4)), 0);
            else if (_state == OperateState.Size)
                _size += sizeof(int);
        }

        public void Operate(ref uint i)
        {
            if (_state == OperateState.Write)
                _writer.Write(Reverse(BitConverter.GetBytes(i)));
            else if (_state == OperateState.Read)
                i = BitConverter.ToUInt32(Reverse(_reader.ReadBytes(4)), 0);
            else if (_state == OperateState.Size)
                _size += sizeof(uint);
        }

        public void Operate(ref long i)
        {
            if (_state == OperateState.Write)
                _writer.Write(Reverse(BitConverter.GetBytes(i)));
            else if (_state == OperateState.Read)
                i = BitConverter.ToInt64(Reverse(_reader.ReadBytes(4)), 0);
            else if (_state == OperateState.Size)
                _size += sizeof(long);
        }

        public void Operate(ref ulong i)
        {
            if (_state == OperateState.Write)
                _writer.Write(Reverse(BitConverter.GetBytes(i)));
            else if (_state == OperateState.Read)
                i = BitConverter.ToUInt64(Reverse(_reader.ReadBytes(4)), 0);
            else if (_state == OperateState.Size)
                _size += sizeof(ulong);
        }

        public void Operate(ref float f)
        {
            if (_state == OperateState.Write)
                _writer.Write(Reverse(BitConverter.GetBytes(f)));
            else if (_state == OperateState.Read)
                f = BitConverter.ToSingle(Reverse(_reader.ReadBytes(4)), 0);
            else if (_state == OperateState.Size)
                _size += sizeof(float);
        }

        public void Operate(ref double d)
        {
            if (_state == OperateState.Write)
                _writer.Write(Reverse(BitConverter.GetBytes(d)));
            else if (_state == OperateState.Read)
                d = BitConverter.ToDouble(Reverse(_reader.ReadBytes(8)), 0);
            else if (_state == OperateState.Size)
                _size += sizeof(double);
        }

        public void Operate(ref string s, int length)
        {
            if (_state == OperateState.Write)
            {
                byte[] bytes = new byte[length];
                Encoding.ASCII.GetBytes(s, 0, System.Math.Min(11, s.Length), bytes, 0);
                _writer.Write(bytes);
            }
            else if (_state == OperateState.Read)
            {
                s = Encoding.ASCII.GetString(_reader.ReadBytes(length));
            }
            else if (_state == OperateState.Size)
                _size += length;
        }

        public void Operate<T>(ref T pduEncodable) where T : IPDUEncodable
        {
            pduEncodable.Operate(this);
        }

        public void OperateArraySize<T>(ref T[] array) where T : new()
        {
            if (_state == OperateState.Write)
            {
                _writer.Write((byte)array.Length);
            }
            else if (_state == OperateState.Read)
            {
                array = new T[_reader.ReadByte()];
                for (int i = 0; i < array.Length; i++)
                    array[i] = new T();
            }
            else if (_state == OperateState.Size)
                _size += sizeof(byte);
        }

        public void OperateListSize32<T>(ref List<T> list) where T : new()
        {
            int count = 0;
            if (_state == OperateState.Write)
            {
                count = list.Count;
                Operate(ref count);
            }
            else if (_state == OperateState.Read)
            {
                Operate(ref count);

                list = new List<T>();
                for (int i = 0; i < count; i++)
                    list.Add(new T());
            }
            else if (_state == OperateState.Size)
                _size += sizeof(int);
        }

        public void OperateArraySizeBits32<T>(ref T[] array, int elementSize) where T : new()
        {
            int size = 0;
            if (_state == OperateState.Write)
            {
                size = array.Length * elementSize * 8;
                Operate(ref size);
            }
            else if (_state == OperateState.Read)
            {
                Operate(ref size);

                array = new T[size / (elementSize * 8)];
                for (int i = 0; i < array.Length; i++)
                    array[i] = new T();
            }
            else if (_state == OperateState.Size)
                _size += sizeof(int);
        }

        public void Operate<T>(ref T[] array) where T : IPDUEncodable
        {
            for (int i = 0; i < array.Length; i++)
                Operate(ref array[i]);
        }

        public void Operate<T>(ref List<T> list) where T : IPDUEncodable
        {
            for (int i = 0; i < list.Count; i++)
            {
                T value = list[i];
                Operate(ref value);
                if (_state == OperateState.Write)
                    list[i] = value;
            }
        }

        private byte[] Reverse(byte[] bytes)
        {
            Array.Reverse(bytes);
            return bytes;
        }

        public void Padding(int bytes)
        {
            if (_state == OperateState.Write)
                _writer.Write(new byte[bytes]);
            else if (_state == OperateState.Read)
                _reader.ReadBytes(bytes);
            else if (_state == OperateState.Size)
                _size += bytes;
        }

    }
}