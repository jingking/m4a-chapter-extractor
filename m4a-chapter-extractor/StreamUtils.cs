using System;
using System.IO;
using System.Text;


namespace M4a_chapter_extractor
{
	internal class StreamUtils
	{


		public static int GetAtomSize(Stream s)
		{
			return StreamUtils.ReadInt32BigEndian(s);
		}

		public static int GetAtomId(Stream s)
		{
			return StreamUtils.ReadInt32BigEndian(s);
		}

		public static string GetAtomType(Stream s)
		{
			return StreamUtils.ReadShortString(s, 4);
		}

		//parses ('stts') time list
		public static int[]? GetTimeToSample(Stream s)
		{
			return StreamUtils.ReadList(s, false, true);
		}

		//parses ('stsz') size list
		//public static int[]? GetSampleSize(Stream s)
		//{
		//	return StreamUtils.ReadList(s, true, false);
		//}

		//parses ('stco') offset list
		public static int[]? GetChunkOffset(Stream s)
		{
			return StreamUtils.ReadList(s);
		}

		public static string GetChapter(Stream s)
		{
			UInt16 len = StreamUtils.ReadUInt16BigEndian(s);
			if (len > 0)
				return StreamUtils.ReadShortString(s, len);
			else
				return "";
		}


		private static int[]? ReadList(Stream s, bool checksize = false, bool checkcount = false)
		{
			int[]? result = null;

			int version;
			version = StreamUtils.ReadInt32BigEndian(s);

			if (checksize)
			{
				int sample_size;
				sample_size = StreamUtils.ReadInt32BigEndian(s);
			}

			int entry_count;
			entry_count = StreamUtils.ReadInt32BigEndian(s);


			if (entry_count > 0 && version == 0)
			{
				result = new int[entry_count];
				for (var i = 0; i < entry_count; i++)
				{
					if (checkcount)
					{
						var samplecount = StreamUtils.ReadInt32BigEndian(s);
						if (samplecount == 1)
							result[i] = StreamUtils.ReadInt32BigEndian(s);
					}
					else
						result[i] = StreamUtils.ReadInt32BigEndian(s);
				}
			}
			return result;
		}



		private static long ReadUInt64asLong(Stream s)
		{
			return (long)ReadUInt64(s);
		}

		private static UInt64 ReadUInt64(Stream s)
		{
			byte[] buff = new byte[8];
			s.Read(buff, 0, buff.Length);
			return BitConverter.ToUInt64(buff, 0);
		}

		private static int ReadUInt32asInt(Stream s)
		{
			return (int)ReadUInt32(s);
		}

		private static UInt32 ReadUInt32(Stream s)
		{
			byte[] buff = new byte[4];
			s.Read(buff, 0, buff.Length);
			return BitConverter.ToUInt32(buff, 0);
		}

		private static Int32 ReadInt32BigEndian(Stream s)
		{
			byte[] buff = new byte[4];
			s.Read(buff, 0, buff.Length);
			if (BitConverter.IsLittleEndian)
				Array.Reverse(buff);
			return BitConverter.ToInt32(buff, 0);
		}

		private static UInt16 ReadUInt16(Stream s)
		{
			byte[] buff = new byte[2];
			s.Read(buff, 0, buff.Length);
			return BitConverter.ToUInt16(buff, 0);
		}

		private static UInt16 ReadUInt16BigEndian(Stream s)
		{
			byte[] buff = new byte[2];
			s.Read(buff, 0, buff.Length);
			if (BitConverter.IsLittleEndian)
				Array.Reverse(buff);
			return BitConverter.ToUInt16(buff, 0);
		}

		private static string ReadShortString(Stream s, int len)
		{
			byte[] buff = new byte[len];
			s.Read(buff, 0, buff.Length);
			for (int i = 0; i < len; i++)
			{
				if (buff[i] < 32 && buff[i] != 10 && buff[i] != 13)
					buff[i] = (byte)'?';
			}
			return Encoding.ASCII.GetString(buff);
		}
	}
}