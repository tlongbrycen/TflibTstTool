using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using libxl;
using ToFCamera.Wrapper;

static class Constants
{
    public const string TESTPLAN_DIR = @"C:\Users\tlong\source\repos\TflibTstTool\TestCase\Input\";
    public const string INPUT_INIT_DEPTH_DATA_DIR = @"C:\Users\tlong\source\repos\TflibTstTool\TestCase\Input\InputInitDepthData\";
    public const string INPUT_DEPTH_DATA_DIR = @"C:\Users\tlong\source\repos\TflibTstTool\TestCase\Input\InputDepthData\";
    public const string OUTPUT_GET_GROUND_CLOUD_DIR = @"C:\Users\tlong\source\repos\TflibTstTool\TestCase\Output\OutputGetGroundCloud\";
    public const string OUTPUT_GET_PEOPLE_DATA_DIR = @"C:\Users\tlong\source\repos\TflibTstTool\TestCase\Output\OutputGetPeopleData\";
    public const string TEST_RESULT_DIR = @"C:\Users\tlong\source\repos\TflibTstTool\TestCase\Output\test_result.xls";
    public const string TEST_CONFIG_DIR = @"C:\Users\tlong\source\repos\TflibTstTool\TestCase\Input\test_config.txt";
}

namespace HelloExcel
{
    class ExcuteTest
    {
        ushort[] inputInitDepthData;
        ushort inputCameraAngle;
        ushort inputPeopleNumberMax;
        ushort[] inputDepthData;
        TFL_RESULT result;


        public ExcuteTest()
        {
            inputInitDepthData = null;
            inputCameraAngle = 0;
            inputPeopleNumberMax = 0;
            inputDepthData = null;
            result = TFL_RESULT.TFL_ERROR;
        }

        void SetInputInitDepthData(Sheet sheet, int sequenceIdx)
        {
            inputInitDepthData = new ushort[640 * 480];
            string initDepth_fileName = sheet.readStr(sequenceIdx, 5);
            initDepth_fileName = Constants.INPUT_INIT_DEPTH_DATA_DIR + initDepth_fileName;
            int cnt1 = 0;
            if (File.Exists(initDepth_fileName))
            {
                using (BinaryReader reader = new BinaryReader(File.Open(initDepth_fileName, FileMode.Open)))
                {
                    do
                    {
                        inputInitDepthData[cnt1] = reader.ReadUInt16();
                        cnt1++;
                    } while (reader.BaseStream.Position < reader.BaseStream.Length);
                }
            }
            else
            {
                Console.WriteLine("INPUT_INIT_DEPTH_DATA NOT FOUND");
                Console.WriteLine(initDepth_fileName);
            }
        }

        void SetInputCameraAngle(Sheet sheet, int sequenceIdx)
        {
            inputCameraAngle = (ushort)sheet.readNum(sequenceIdx, 6);
        }

        void SetInputPeopleNumberMax(Sheet sheet, int sequenceIdx)
        {
            inputPeopleNumberMax = (ushort)sheet.readNum(sequenceIdx, 7);
        }

        void SetInputDepthData(Sheet sheet, int sequenceIdx)
        {
            inputDepthData = new ushort[640 * 480];
            int cnt2 = 0;
            string Depth_fileName = sheet.readStr(sequenceIdx, 8);
            Depth_fileName = Constants.INPUT_DEPTH_DATA_DIR + Depth_fileName;
            if (File.Exists(Depth_fileName))
            {
                using (BinaryReader reader = new BinaryReader(File.Open(Depth_fileName, FileMode.Open)))
                {
                    do
                    {
                        inputDepthData[cnt2] = reader.ReadUInt16();
                        cnt2++;
                    } while (reader.BaseStream.Position < reader.BaseStream.Length);
                }
            }
            else
            {
                Console.WriteLine("INPUT_DEPTH_DATA NOT FOUND");
                Console.WriteLine(Depth_fileName);
            }
        }

        public void Run(PeopleDetector peoDtc, Sheet sheet, int sequenceIdx)
        {
            SetInputInitDepthData(sheet, sequenceIdx);
            SetInputCameraAngle(sheet, sequenceIdx);
            SetInputPeopleNumberMax(sheet, sequenceIdx);
            SetInputDepthData(sheet, sequenceIdx);
            result = peoDtc.Execute(inputInitDepthData, inputCameraAngle, inputPeopleNumberMax, inputDepthData);
            sheet.writeStr(sequenceIdx, 9, result.ToString());
        }
    }

    class GetGroundCloudTst
    {
        List<TFL_PointXYZ> outputGetGroundCloud;
        TFL_RESULT result;
        int PLYfileNamePosfix;
        string PLYfileName;

        public GetGroundCloudTst(int caseStart)
        {
            outputGetGroundCloud = null;
            PLYfileNamePosfix = caseStart - 1;
            PLYfileName = "outputGetGroundCloud_";
        }

        public void Run(PeopleDetector peoDtc, Sheet sheet, int sequenceIdx)
        {
            outputGetGroundCloud = new List<TFL_PointXYZ>();
            result = peoDtc.GetGroundCloud(outputGetGroundCloud);
            sheet.writeStr(sequenceIdx, 11, result.ToString());
            // Result is wrote to xls
            PLYfileNamePosfix = PLYfileNamePosfix + 1;
            PLYfileName = Constants.OUTPUT_GET_GROUND_CLOUD_DIR + "outputGround_" + PLYfileNamePosfix.ToString() + ".ply";
            TFL_Utilities.SavePLY(outputGetGroundCloud.ToArray(), (ulong)outputGetGroundCloud.Count(), PLYfileName);
            // output groud is saved as PLY
            sheet.writeStr(sequenceIdx, 10, "outputGround_" + PLYfileNamePosfix.ToString() + ".ply");
            // PLY filename is wrote to xls
        }
    }

    class GetPeopleDataTst
    {
        List<TFL_Human> outputGetPeopleData;
        TFL_RESULT result;
        int PLYfileNamePosfix;
        string PLYfileName;

        public GetPeopleDataTst(int caseStart)
        {
            outputGetPeopleData = null;
            PLYfileNamePosfix = caseStart - 1;
            PLYfileName = "outputGetPeopleData_";
        }

        public void Run(PeopleDetector peoDtc, Sheet sheet, int sequenceIdx)
        {
            outputGetPeopleData = new List<TFL_Human>();
            result = peoDtc.GetPeopleData(outputGetPeopleData);
            sheet.writeStr(sequenceIdx, 13, result.ToString());
            // Result is wrote to xls
            PLYfileNamePosfix = PLYfileNamePosfix + 1;
            string xlsStr = "";
            int numPeo = outputGetPeopleData.Count();
            for (int i = 0; i < numPeo; i++)
            {
                PLYfileName = PLYfileNamePosfix.ToString()
                    + "_person" + i.ToString() + ".ply";
                TFL_Utilities.SavePLY(outputGetPeopleData[i].peoplePointCloud.ToArray(),
                    (ulong)outputGetPeopleData[i].peoplePointCloud.Count(),
                    Constants.OUTPUT_GET_PEOPLE_DATA_DIR + PLYfileName);
                // output people is saved as PLY
                xlsStr = xlsStr + PLYfileName + ";";
            }
            sheet.writeStr(sequenceIdx, 12, xlsStr);
            // PLY filename is wrote to xls
        }
    }

    class Program
    {

        static void Main(string[] args)
        {
            try
            {
                // Read config
                List<string> configStrLst = new List<string>(new string[] { "element1", "element2", "element3" });

                try
                {
                    System.IO.StreamReader file = new System.IO.StreamReader(Constants.TEST_CONFIG_DIR);
                    string line;
                    int cnt = 0;
                    while ((line = file.ReadLine()) != null)
                    {
                        //System.Console.WriteLine(line);
                        configStrLst[cnt] = line;
                        cnt++;
                    }
                }
                catch (System.Exception e)
                {
                    Console.WriteLine(e);
                    Console.WriteLine("TEST CONFIG NOT FOUND");
                    Console.WriteLine(Constants.TEST_CONFIG_DIR);
                    return;
                }

                int caseStart = 1;
                int caseEnd = 3;
                caseStart = Int16.Parse(configStrLst[1]);
                caseEnd = Int16.Parse(configStrLst[2]);
                string tstPlanDir = Constants.TESTPLAN_DIR + configStrLst[0];

                Book book = new BinBook();
                PeopleDetector peoDtc = new PeopleDetector();
                ExcuteTest exTst = new ExcuteTest();
                GetGroundCloudTst gndTst = new GetGroundCloudTst(caseStart);
                GetPeopleDataTst peoTst = new GetPeopleDataTst(caseStart);

                if (book.load(tstPlanDir))
                {
                    Console.WriteLine("Testplan file name: " + configStrLst[0]);
                    Console.WriteLine("Start from case number: " + configStrLst[1]);
                    Console.WriteLine("End at case number: " + configStrLst[2]);
                    Sheet sheet = book.getSheet(0);

                    /* Parse the sequences */
                    int tstCaseCnt = 0;
                    int sequenceIdx = 10;
                    double sequenceNum = sheet.readNum(sequenceIdx, 4);
                    while (true)
                    {
                        switch (sequenceNum)
                        {
                            case 1: // Run function 1
                                if (tstCaseCnt >= caseStart)
                                {
                                    exTst.Run(peoDtc, sheet, sequenceIdx);
                                    book.save(Constants.TEST_RESULT_DIR);
                                }
                                break;
                            case 2: // Run function 2
                                if (tstCaseCnt >= caseStart)
                                {
                                    gndTst.Run(peoDtc, sheet, sequenceIdx);
                                    book.save(Constants.TEST_RESULT_DIR);
                                }
                                break;
                            case 3: // Run function 3
                                if (tstCaseCnt >= caseStart)
                                {
                                    peoTst.Run(peoDtc, sheet, sequenceIdx);
                                    book.save(Constants.TEST_RESULT_DIR);
                                }
                                break;
                            case -1: // Start testcase
                                tstCaseCnt = tstCaseCnt + 1;
                                if (tstCaseCnt > caseEnd)
                                {
                                    goto ENDTEST;
                                }
                                else if (tstCaseCnt >= caseStart)
                                {
                                    System.Console.WriteLine("Start testcase number " + tstCaseCnt);
                                }
                                break;
                            case -2: // End testcase
                                if (tstCaseCnt >= caseStart)
                                {
                                    System.Console.WriteLine("End testcase number " + tstCaseCnt);
                                }
                                break;
                            case -3: // Stop
                                goto ENDTEST;
                            default: // Invalid
                                return;
                        }
                        //Console.WriteLine(sequenceNum);
                        sequenceIdx = sequenceIdx + 1;
                        sequenceNum = sheet.readNum(sequenceIdx, 4);
                    }
                ENDTEST:
                    Console.WriteLine("END TEST");

                }
                else
                {
                    Console.WriteLine("TESTPLAN_DIR NOT FOUND");
                    Console.WriteLine(tstPlanDir);
                }
            }
            catch (System.Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }
    }
}
