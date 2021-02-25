using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Tools
{
    public class BytesIO
    {
        public int Seed;
        private List<byte> _bytes;
        private byte[] _byteArray;
        public BytesIO(byte[] bytes)
        {
            Seed = 0;
            _byteArray = bytes;
        }

        public BytesIO()
        {
            Seed = 0;
            _bytes = new List<byte>();
        }

        public int GetInt32()
        {
            int v = BitConverter.ToInt32(_byteArray, Seed);
            Seed += 4;
            return v;
        }

        public float GetSingle()
        {
            float v = BitConverter.ToSingle(_byteArray, Seed);
            Seed += 4;
            return v;
        }

        public byte[] GetBytes()
        {
            int length = GetInt32();
            byte[] bytes = new byte[length];
            Array.Copy(_byteArray, Seed, bytes, 0, length);
            Seed += length;
            return bytes;
        }

        public string GetString()
        {
            int length = GetInt32();
            string data = Encoding.Unicode.GetString(_byteArray, Seed, length);
            Seed += length;
            return data;
        }

        public List<byte[]> GetBytesList()
        {
            List<byte[]> list = new List<byte[]>();
            int count = GetInt32();
            for (int i = 0; i < count; ++i)
            {
                list.Add(GetBytes());
            }
            return list;
        }

        public Vector3 GetVector3()
        {
            Vector3 v = new Vector3(GetSingle(), GetSingle(), GetSingle());
            return v;
        }

        public BytesIO Set(Vector3 v)
        {
            Set(v.x).Set(v.y).Set(v.z);
            return this;
        }

        public BytesIO Set(List<byte[]> list)
        {
            Set(list.Count);
            foreach (byte[] bytes in list)
            {
                Set(bytes);
            }
            return this;
        }

        public BytesIO Set(int v)
        {
            _bytes.AddRange(BitConverter.GetBytes(v));
            return this;
        }

        public BytesIO Set(float v)
        {
            _bytes.AddRange(BitConverter.GetBytes(v));
            return this;
        }

        public BytesIO Set(byte[] bytes)
        {
            Set(bytes.Length);
            _bytes.AddRange(bytes);
            return this;
        }

        public BytesIO Set(string data)
        {
            byte[] bytes = Encoding.Unicode.GetBytes(data);
            Set(bytes.Length);
            _bytes.AddRange(bytes);
            return this;
        }

        public byte[] ToBytes()
        {
            return _bytes.ToArray();
        }
    }
}
