using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenHardwareMonitor.Hardware;
namespace Get_CPU_Temp5
{
    class Program
    {
        public class UpdateVisitor : IVisitor
        {
            public void VisitComputer(IComputer computer)
            {
                computer.Traverse(this);
            }
            public void VisitHardware(IHardware hardware)
            {
                hardware.Update();
                foreach (IHardware subHardware in hardware.SubHardware) subHardware.Accept(this);
            }
            public void VisitSensor(ISensor sensor) { }
            public void VisitParameter(IParameter parameter) { }
        }
        static void GetSystemInfo()
        {
            UpdateVisitor updateVisitor = new UpdateVisitor();
            Computer computer = new Computer();
            computer.Open();
            computer.CPUEnabled = true;
            computer.GPUEnabled = true;
            computer.Accept(updateVisitor);
            String CPU_Temp = "";
            String CPU_Speed = "";
            String CPU_Load = "";
            String GPU_Temp = "";
            String GPU_Speed = "";
            String GPU_Load = "";
            for (int i = 0; i < computer.Hardware.Length; i++)
            {
                //CPU Info
                if (computer.Hardware[i].HardwareType == HardwareType.CPU)
                {
                    float avgClockSum = 0;
                    int coreTempSensorCount = 0;
                    for (int j = 0; j < computer.Hardware[i].Sensors.Length; j++)
                    {
                        //Package Temp
                        if (computer.Hardware[i].Sensors[j].SensorType == SensorType.Temperature && computer.Hardware[i].Sensors[j].Name == "CPU Package")
                            //CPU_Temp = (computer.Hardware[i].Sensors[j].Name + ":" + computer.Hardware[i].Sensors[j].Value.ToString() + " °C" + "\r");
                            CPU_Temp = (computer.Hardware[i].Sensors[j].Value.ToString()+ "\r");
                        //Average Clock Speed
                        if (computer.Hardware[i].Sensors[j].SensorType == SensorType.Clock && computer.Hardware[i].Sensors[j].Name != "Bus Speed")
                        {
                            avgClockSum += (float)computer.Hardware[i].Sensors[j].Value;
                            coreTempSensorCount++;
                        }
                        //Load Percentages
                        if (computer.Hardware[i].Sensors[j].SensorType == SensorType.Load && computer.Hardware[i].Sensors[j].Name == "CPU Total")
                           // CPU_Load = (computer.Hardware[i].Sensors[j].Name + ":" + computer.Hardware[i].Sensors[j].Value.ToString() + " %" + "\r");
                           CPU_Load = (computer.Hardware[i].Sensors[j].Value.ToString()+ "\r");
                    }
                    //Calculating Average CPU Clock Speeds across all cores
                    //CPU_Speed = ("CPU Clock: " + (avgClockSum / coreTempSensorCount) + " MHz" + "\r");
                    CPU_Speed = ((avgClockSum / coreTempSensorCount) + "\r");
                }
                //NVidia GPU Info
                else if (computer.Hardware[i].HardwareType == HardwareType.GpuNvidia)
                {
                    for (int j = 0; j < computer.Hardware[i].Sensors.Length; j++)
                    {
                        //GPU Core Temp
                        if (computer.Hardware[i].Sensors[j].SensorType == SensorType.Temperature && computer.Hardware[i].Sensors[j].Name == "GPU Core")
                            //GPU_Temp = (computer.Hardware[i].Sensors[j].Name + ":" + computer.Hardware[i].Sensors[j].Value.ToString() + " °C" + "\r");
                            GPU_Temp = (computer.Hardware[i].Sensors[j].Value.ToString() + "\r");
                        //GPU Core Load Percentage
                        if (computer.Hardware[i].Sensors[j].SensorType == SensorType.Load && computer.Hardware[i].Sensors[j].Name == "GPU Core")
                            //GPU_Load = (computer.Hardware[i].Sensors[j].Name + ":" + computer.Hardware[i].Sensors[j].Value.ToString() + " %" + "\r");
                            GPU_Load = (computer.Hardware[i].Sensors[j].Value.ToString() + "\r");
                        //GPU Clock Speed
                        if (computer.Hardware[i].Sensors[j].SensorType == SensorType.Clock && computer.Hardware[i].Sensors[j].Name == "GPU Core")
                            //GPU_Speed = (computer.Hardware[i].Sensors[j].Name + ":" + computer.Hardware[i].Sensors[j].Value.ToString() + " MHz" + "\r");
                            GPU_Speed = (computer.Hardware[i].Sensors[j].Value.ToString() + "\r");
                    }
                }
                //AMD GPU Info
                else if (computer.Hardware[i].HardwareType == HardwareType.GpuAti)
                {
                    for (int j = 0; j < computer.Hardware[i].Sensors.Length; j++)
                    {
                        //GPU Core Temp
                        if (computer.Hardware[i].Sensors[j].SensorType == SensorType.Temperature && computer.Hardware[i].Sensors[j].Name == "GPU Core")
                            //GPU_Temp = (computer.Hardware[i].Sensors[j].Name + ":" + computer.Hardware[i].Sensors[j].Value.ToString() + " °C" + "\r");
                            GPU_Temp = (computer.Hardware[i].Sensors[j].Value.ToString() + "\r");
                        //GPU Core Load Percentage
                        if (computer.Hardware[i].Sensors[j].SensorType == SensorType.Load && computer.Hardware[i].Sensors[j].Name == "GPU Core")
                            //GPU_Load = (computer.Hardware[i].Sensors[j].Name + ":" + computer.Hardware[i].Sensors[j].Value.ToString() + " %" + "\r");
                            GPU_Load = (computer.Hardware[i].Sensors[j].Value.ToString() + "\r");
                        //GPU Clock Speed
                        if (computer.Hardware[i].Sensors[j].SensorType == SensorType.Clock && computer.Hardware[i].Sensors[j].Name == "GPU Core")
                            //GPU_Speed = (computer.Hardware[i].Sensors[j].Name + ":" + computer.Hardware[i].Sensors[j].Value.ToString() + " MHz" + "\r");
                            GPU_Speed = (computer.Hardware[i].Sensors[j].Value.ToString() + "\r");
                    }
                }
            }
            //Writing to a document

            //Console.WriteLine(CPU_Temp);
            //Console.WriteLine(CPU_Speed);
            //Console.WriteLine(CPU_Load);
            //Console.WriteLine(GPU_Temp);
            //Console.WriteLine(GPU_Speed);
            //Console.WriteLine(GPU_Load);
            string[] outputs = {CPU_Temp, CPU_Speed, CPU_Load, GPU_Temp, GPU_Speed, GPU_Load };

            //File Location can be changed
            System.IO.File.WriteAllLines(@"D:\WriteOutputs.txt", outputs);
            computer.Close();
        }
        static void Main(string[] args)
        {
            while (true)
            {
                GetSystemInfo();

                //Wait time can be changed to be more or less frequent
                System.Threading.Thread.Sleep(1000);
            }
        }
    }
}