using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Windows.Forms;

namespace GMS2IDGen
{
    class Program
    {
		[STAThread]
        static void Main(string[] args)
        {
			var filename = "";
			if (args.Length > 1)
			{
				filename = args[1];
			}
			else
			{
				using (var dialog = new OpenFileDialog())
				{
					dialog.Filter = "GMS2 data files (*.win)|*.win|All files (*.*)|*.*";
					dialog.RestoreDirectory = true;

					if (dialog.ShowDialog() == DialogResult.OK)
					{
						filename = dialog.FileName;
					}
				}
			}

			if (filename == "")
				return;

			Console.WriteLine("Reading...");
			var f = File.OpenRead(filename);
			var reader = new BinaryReader(f);
			f.Seek(0x4C, SeekOrigin.Begin);
			var width = reader.ReadInt32();
			var height = reader.ReadInt32();
			f.Seek(0x24, SeekOrigin.Begin);
			var gameid = reader.ReadInt32();
			f.Seek(0x6C, SeekOrigin.Begin);
			var time = reader.ReadInt64();
			f.Seek(0x54, SeekOrigin.Begin);
			var info = reader.ReadInt32();
			f.Seek(0x90, SeekOrigin.Begin);
			var roomcount = reader.ReadInt32();
            for (int i = 0; i < roomcount; i++)
            {
				reader.ReadInt32();
			}
			var pos = f.Position;
			reader.Close();

			Console.WriteLine("Writing...");
			f = File.OpenWrite(filename);
			var writer = new BinaryWriter(f);

			f.Seek(pos, SeekOrigin.Begin);
			WriteNum(writer, time, gameid, width, height, info, roomcount);
			writer.Close();

            Console.WriteLine("OK!");
			Console.ReadKey();
		}
		static void WriteNum(BinaryWriter writer,long time, int gameid, int width, int height, int info, int roomcount)
        {
			Random random = new Random((int)(time & 4294967295L));
			long randomNum = (long)random.Next() << 32 | (long)random.Next();
			long specialNum = time;
			specialNum += -1000L;
			ulong initializeNum = (ulong)specialNum;
			initializeNum = 
				((initializeNum << 56 & 18374686479671623680UL) | 
				(initializeNum >> 8 & 71776119061217280UL) | 
				(initializeNum << 32 & 280375465082880UL) | 
				(initializeNum >> 16 & 1095216660480UL) | 
				(initializeNum << 8 & 4278190080UL) | 
				(initializeNum >> 24 & 16711680UL) | 
				(initializeNum >> 16 & 65280UL) | 
				(initializeNum >> 32 & 255UL));
			specialNum = (long)initializeNum;
			specialNum ^= randomNum;
			specialNum = ~specialNum;
			specialNum ^= ((long)gameid << 32 | (long)gameid);
			specialNum ^= ((long)(width + info) << 48 | (long)(height + info) << 32 | (long)(height + info) << 16 | (long)(width + info));
			specialNum ^= 17L;
			int specialIndex = Math.Abs((int)(time & 65535L) / 7 + (gameid - width) + roomcount);
			specialIndex %= 4;

			writer.Write(randomNum);
			for (int i = 0; i < 4; i++)
			{
				if (i == specialIndex)
				{
					writer.Write(specialNum);
				}
				else
				{
					writer.Write(random.Next());
					writer.Write(random.Next());
				}
			}
		} 
    }
}
