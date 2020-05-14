using System;
using System.Collections.Generic;
using ToFCamera.Wrapper;
using System.IO;
using System.Linq;

static class Constants 
{
	public const string CONFIG_DIR = @"C:\Users\tlong\source\repos\TflibTstTool\RAWsToPLYs\Raw2PLYConfig.txt";
}

namespace RAWsToPLYs
{
	/*uint16_t* ReadRawData(const char* path)
	{
		uint16_t* fileBuf = new uint16_t[307200];
		FILE* file = NULL;
		fopen_s(&file, path, "r");
		if (file == NULL)
		{
			std::cout << "" << std::endl;
			std::cout << "Could not open specified file" << std::endl;
			return 0;
		}
		else
		{
			std::cout << "" << std::endl;
			fread_s(fileBuf, 614400, 2, 307200, file);
			return fileBuf;
		}
		delete[] fileBuf;
		fclose(file);
	}*/

	class Raw
    {
		public ushort[] rawBuf;

		public Raw()
		{
			rawBuf = new ushort[640 * 480];
		}

		public void Load(string path)
		{
			int cnt = 0;
			if (File.Exists(path))
			{
				using (BinaryReader reader = new BinaryReader(File.Open(path, FileMode.Open)))
				{
					do
					{
						rawBuf[cnt] = reader.ReadUInt16();
						cnt++;
					} while (reader.BaseStream.Position < reader.BaseStream.Length);
				}
			}
		}
    }

	class PLY
	{
		public void Save(Raw rawData, string path)
		{
			TFL_PointXYZ[] pcdFullBuf = new TFL_PointXYZ[307200];
			TFL_RESULT result = TFL_Utilities.Depth2PCD(rawData.rawBuf, pcdFullBuf);
			result = TFL_Utilities.SavePLY(pcdFullBuf, 307200, path);
		}
	}
    
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");

			// Read config
			List<string> configStrLst = new List<string>(new string[] { "element1", "element2"});

			try
			{
				System.IO.StreamReader file = new System.IO.StreamReader(Constants.CONFIG_DIR);
				string line;
				int cnt = 0;
				while ((line = file.ReadLine()) != null)
				{
					configStrLst[cnt] = line;
					cnt++;
				}
			}
			catch (System.Exception e)
			{
				Console.WriteLine(e);
				Console.WriteLine("CONFIG FILE NOT FOUND");
				Console.WriteLine(Constants.CONFIG_DIR);
				return;
			}
			string rawDir = configStrLst[0];
			string plyDir = configStrLst[1];

			// Read raw
			Raw rawData = new Raw();
			PLY plyData = new PLY();
			string plyPath;

			string[] rawPaths = Directory.GetFiles(rawDir);
			for (int i = 0; i < rawPaths.Count(); i++)
			{
				Console.WriteLine(rawPaths[i]);
				rawData.Load(rawPaths[i]);
				plyPath = plyDir + Path.GetFileName(rawPaths[i]) + ".ply";
				plyData.Save(rawData, plyPath);
			}
		}
    }
}
