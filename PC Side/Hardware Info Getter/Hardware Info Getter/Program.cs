using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Net.Sockets;
using OpenHardwareMonitor.Hardware;
namespace Get_CPU_Temp5
{
    class Program
    {
        static TcpClient tcpClient;
        static Stream stream;
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
            //String CPU_Temp = "";
            //String CPU_Speed = "";
            //String CPU_Load = "";
            //String GPU_Temp = "";
            //String GPU_Speed = "";
            //String GPU_Load = "";
            float? CPU_Temp = 0;
            float? CPU_Speed = 0;
            float? CPU_Load = 0;
            float? GPU_Temp = 0;
            float? GPU_Load = 0;
            float? GPU_Speed = 0;
            for (int i = 0; i < computer.Hardware.Length; i++)
            {
                if (computer.Hardware[i].HardwareType == HardwareType.CPU)
                {
                    float avgClockSum = 0;
                    int coreTempSensorCount = 0;
                    for (int j = 0; j < computer.Hardware[i].Sensors.Length; j++)
                    {
                        if (computer.Hardware[i].Sensors[j].SensorType == SensorType.Temperature && computer.Hardware[i].Sensors[j].Name == "CPU Package")
                            //CPU_Temp = (computer.Hardware[i].Sensors[j].Name + ":" + computer.Hardware[i].Sensors[j].Value.ToString() + " °C" + "\r");
                            //CPU_Temp = (computer.Hardware[i].Sensors[j].Value.ToString()+ "\r");
                            CPU_Temp = computer.Hardware[i].Sensors[j].Value;
                        if (computer.Hardware[i].Sensors[j].SensorType == SensorType.Clock && computer.Hardware[i].Sensors[j].Name != "Bus Speed")
                        {
                            avgClockSum += (float)computer.Hardware[i].Sensors[j].Value;
                            coreTempSensorCount++;
                        }
                        if (computer.Hardware[i].Sensors[j].SensorType == SensorType.Load && computer.Hardware[i].Sensors[j].Name == "CPU Total")
                            // CPU_Load = (computer.Hardware[i].Sensors[j].Name + ":" + computer.Hardware[i].Sensors[j].Value.ToString() + " %" + "\r");
                            //CPU_Load = (computer.Hardware[i].Sensors[j].Value.ToString()+ "\r");
                            CPU_Load = computer.Hardware[i].Sensors[j].Value;
                    }
                    //CPU_Speed = ("CPU Clock: " + (avgClockSum / coreTempSensorCount) + " MHz" + "\r");
                    //CPU_Speed = ((avgClockSum / coreTempSensorCount) + "\r");
                    CPU_Speed = ((int)avgClockSum / coreTempSensorCount);
                }
                else if (computer.Hardware[i].HardwareType == HardwareType.GpuNvidia)
                {
                    for (int j = 0; j < computer.Hardware[i].Sensors.Length; j++)
                    {
                        if (computer.Hardware[i].Sensors[j].SensorType == SensorType.Temperature && computer.Hardware[i].Sensors[j].Name == "GPU Core")
                            //GPU_Temp = (computer.Hardware[i].Sensors[j].Name + ":" + computer.Hardware[i].Sensors[j].Value.ToString() + " °C" + "\r");
                            //GPU_Temp = (computer.Hardware[i].Sensors[j].Value.ToString() + "\r");
                            GPU_Temp = computer.Hardware[i].Sensors[j].Value;
                        if (computer.Hardware[i].Sensors[j].SensorType == SensorType.Load && computer.Hardware[i].Sensors[j].Name == "GPU Core")
                            //GPU_Load = (computer.Hardware[i].Sensors[j].Name + ":" + computer.Hardware[i].Sensors[j].Value.ToString() + " %" + "\r");
                            //GPU_Load = (computer.Hardware[i].Sensors[j].Value.ToString() + "\r");
                            GPU_Load = computer.Hardware[i].Sensors[j].Value;
                        if (computer.Hardware[i].Sensors[j].SensorType == SensorType.Clock && computer.Hardware[i].Sensors[j].Name == "GPU Core")
                            //GPU_Speed = (computer.Hardware[i].Sensors[j].Name + ":" + computer.Hardware[i].Sensors[j].Value.ToString() + " MHz" + "\r");
                            //GPU_Speed = (computer.Hardware[i].Sensors[j].Value.ToString() + "\r");
                            GPU_Speed = computer.Hardware[i].Sensors[j].Value;
                    }
                }
                else if (computer.Hardware[i].HardwareType == HardwareType.GpuAti)
                {
                    for (int j = 0; j < computer.Hardware[i].Sensors.Length; j++)
                    {
                        if (computer.Hardware[i].Sensors[j].SensorType == SensorType.Temperature && computer.Hardware[i].Sensors[j].Name == "GPU Core")
                            //GPU_Temp = (computer.Hardware[i].Sensors[j].Name + ":" + computer.Hardware[i].Sensors[j].Value.ToString() + " °C" + "\r");
                            //GPU_Temp = (computer.Hardware[i].Sensors[j].Value.ToString() + "\r");
                            GPU_Temp = computer.Hardware[i].Sensors[j].Value;
                        if (computer.Hardware[i].Sensors[j].SensorType == SensorType.Load && computer.Hardware[i].Sensors[j].Name == "GPU Core")
                            //GPU_Load = (computer.Hardware[i].Sensors[j].Name + ":" + computer.Hardware[i].Sensors[j].Value.ToString() + " %" + "\r");
                            //GPU_Load = (computer.Hardware[i].Sensors[j].Value.ToString() + "\r");
                            GPU_Load = computer.Hardware[i].Sensors[j].Value;
                        if (computer.Hardware[i].Sensors[j].SensorType == SensorType.Clock && computer.Hardware[i].Sensors[j].Name == "GPU Core")
                            //GPU_Speed = (computer.Hardware[i].Sensors[j].Name + ":" + computer.Hardware[i].Sensors[j].Value.ToString() + " MHz" + "\r");
                            //GPU_Speed = (computer.Hardware[i].Sensors[j].Value.ToString() + "\r");
                            GPU_Speed = computer.Hardware[i].Sensors[j].Value;
                    }
                }
            }
            //Console.WriteLine(CPU_Temp);
            //Console.WriteLine(CPU_Speed);
            //Console.WriteLine(CPU_Load);
            //Console.WriteLine(GPU_Temp);
            //Console.WriteLine(GPU_Speed);
            //Console.WriteLine(GPU_Load);
            //string[] outputs = {CPU_Temp, CPU_Speed, CPU_Load, GPU_Temp, GPU_Speed, GPU_Load };
            float[] outputs = {(float)CPU_Temp, (float)CPU_Speed, (float)CPU_Load, (float)GPU_Temp, (float)GPU_Speed, (float)GPU_Load };
            byte[] bytes = new byte[outputs.Length * sizeof(float)];
            Buffer.BlockCopy(outputs, 0, bytes, 0, bytes.Length);
            stream.Write(bytes, 0, bytes.Length);
            //System.IO.File.WriteAllLines(@"D:\WriteOutputs.txt", outputs);
            computer.Close();
        }
        static void Main(string[] args)
        {
            try
            {
                tcpClient = new TcpClient();
                tcpClient.Connect("raspberrypi.local", 22);
                stream = tcpClient.GetStream();
                Console.WriteLine("Success!");
            }
            catch
            {
                Console.WriteLine("Failed");
            }
            while (true)
            {
                GetSystemInfo();
                Console.WriteLine("Sent");
                System.Threading.Thread.Sleep(1000);
            }
        }
    }
}