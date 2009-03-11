using System;
using System.IO;
using System.Text;
using System.Diagnostics;

namespace FileWalk
{
	/// <summary>
	/// Utility functions
	/// </summary>
	public static class Util
	{
		public static string ReadFourCC(BinaryReader reader)
		{
			byte[] fourcc = new byte[4];
			reader.Read(fourcc, 0, fourcc.Length);
			return Encoding.ASCII.GetString(fourcc);
		}

        public static int EndianFlip(int i, int bytes)
        {
            return (int) EndianFlip((uint) i, bytes);
        }

        public static uint EndianFlip(uint u, int bytes)
        {
            Debug.Assert(bytes >= 2 && bytes <= 4);
            if (bytes == 4) 
            {
                return EndianFlip32(u);
            }
            else if (bytes == 3)
            {
                return EndianFlip24(u);
            }
            else if (bytes == 2)
            {
                return EndianFlip16(u);
            }

            return u;
        }

        public static uint EndianFlip16(uint u)
        {
            return (((u & 0x000000ff) << 8) +
                    ((u & 0x0000ff00) >> 8));
        }
        
        public static uint EndianFlip24(uint u)
        {
            return (((u & 0x000000ff) << 16) +
                    ((u & 0x0000ff00)) +
                    ((u & 0x00ff0000) >> 16));
        }

        public static uint EndianFlip32(uint u)
        {
            return (((u & 0x000000ff) << 24) +
                    ((u & 0x0000ff00) << 8) +
                    ((u & 0x00ff0000) >> 8) +
                    ((u & 0xff000000) >> 24));
        }

        public static uint ReadUimsbf(BinaryReader reader, int bytes)
        {
            Debug.Assert(bytes >= 1 && bytes <= 4);
            uint i = reader.ReadByte();
            for (int j = 0; j < bytes - 1; j++)
            {
                i = i << 8 | reader.ReadByte();
            }
            return i;
        }
	}
}
