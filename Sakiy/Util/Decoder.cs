﻿using System.Net;
using System.Security.Cryptography;
using System.Text;

namespace Sakiy.Util
{
    public sealed class Decoder : IDisposable
    {
        private Stream BaseStream;
        public Decoder(Stream baseStream)
        {
            BaseStream = baseStream;
        }
        public void Decrypt(byte[] sharedSecret)
        {
            Aes aes = Aes.Create();
            aes.Mode = CipherMode.CFB;
            aes.Padding = PaddingMode.None;
            aes.KeySize = 128;
            aes.FeedbackSize = 8;
            aes.Key = sharedSecret;
            aes.IV = sharedSecret;
            BaseStream = new CryptoStream(BaseStream, aes.CreateDecryptor(), CryptoStreamMode.Read);
        }
        public void Dispose()
        {
            BaseStream.Dispose();
        }
        public byte[] ReadBuffer(int length, bool reverse)
        {
            byte[] buffer = new byte[length];
            int offset = 0;
            while (offset < length)
            {
                int read = BaseStream.Read(buffer, offset, length - offset);
                if (read < 1) throw new EndOfStreamException();
                offset += read;
            }
            if (reverse) Array.Reverse(buffer);
            return buffer;
        }
        public bool ReadBool()
        {
            return BitConverter.ToBoolean(ReadBuffer(1, false));
        }
        public byte ReadByte()
        {
            return ReadBuffer(1, false)[0];
        }
        public sbyte ReadSByte()
        {
            return (sbyte)ReadBuffer(1, false)[0];
        }
        public ushort ReadUShort()
        {
            return BitConverter.ToUInt16(ReadBuffer(2, BitConverter.IsLittleEndian));
        }
        public short ReadShort()
        {
            return BitConverter.ToInt16(ReadBuffer(2, BitConverter.IsLittleEndian));
        }
        public uint ReadUInt()
        {
            return BitConverter.ToUInt32(ReadBuffer(4, BitConverter.IsLittleEndian));
        }
        public int ReadInt()
        {
            return BitConverter.ToInt32(ReadBuffer(4, BitConverter.IsLittleEndian));
        }
        public ulong ReadULong()
        {
            return BitConverter.ToUInt64(ReadBuffer(8, BitConverter.IsLittleEndian));
        }
        public long ReadLong()
        {
            return BitConverter.ToInt64(ReadBuffer(8, BitConverter.IsLittleEndian));
        }
        public float ReadFloat()
        {
            return BitConverter.ToSingle(ReadBuffer(4, BitConverter.IsLittleEndian));
        }
        public double ReadDouble()
        {
            return BitConverter.ToDouble(ReadBuffer(8, BitConverter.IsLittleEndian));
        }
        public int ReadVarInt()
        {
            int position = 0;
            int result = 0;
            while (position < 32)
            {
                byte current = ReadByte();
                result |= (current & 0x7F) << position;
                if ((current & 0x80) == 0) return result;
                position += 7;
            }
            throw new ProtocolViolationException();
        }
        public string ReadString()
        {
            return Encoding.UTF8.GetString(ReadBuffer(ReadVarInt(), false));
        }
        public Guid ReadGuid()
        {
            return new Guid(ReadUInt(), ReadUShort(), ReadUShort(), ReadByte(), ReadByte(), ReadByte(), ReadByte(), ReadByte(), ReadByte(), ReadByte(), ReadByte());
        }
    }
}
