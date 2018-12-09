﻿using System;
using System.Linq;
using System.IO.Ports;
using System.Management;
using System.Windows;
using System.Collections;
using OpenHardwareMonitor.Hardware;
using System.Collections.Generic;

namespace Get_CPU_Temp5
{
    class Program
    {
        static SerialPort port;
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
        static string[] GetSystemInfo()
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
            String GPU_Load = "";
            String GPU_Speed = "";
            for (int i = 0; i < computer.Hardware.Length; i++)
            {
                if (computer.Hardware[i].HardwareType == HardwareType.CPU)
                {
                    List <Double> avgClockSum = new List<Double>();
                    int coreTempSensorCount = 0;
                    for (int j = 0; j < computer.Hardware[i].Sensors.Length; j++)
                    {
                        if (computer.Hardware[i].Sensors[j].SensorType == SensorType.Temperature && computer.Hardware[i].Sensors[j].Name == "CPU Package")
                            CPU_Temp = (computer.Hardware[i].Sensors[j].Value.ToString());
                        //CPU_Temp = computer.Hardware[i].Sensors[j].Value;
                        if (computer.Hardware[i].Sensors[j].SensorType == SensorType.Clock && computer.Hardware[i].Sensors[j].Name != "Bus Speed")
                        { 
                            avgClockSum.Add((double)computer.Hardware[i].Sensors[j].Value);
                            coreTempSensorCount++;
                        }
                        if (computer.Hardware[i].Sensors[j].SensorType == SensorType.Load && computer.Hardware[i].Sensors[j].Name == "CPU Total")
                            CPU_Load = (computer.Hardware[i].Sensors[j].Value.ToString());
                            //CPU_Load = computer.Hardware[i].Sensors[j].Value;
                    }
                    CPU_Speed = "" + Math.Round(avgClockSum[0],1);
                    //CPU_Speed = ((int)avgClockSum / coreTempSensorCount);
                }
                else if (computer.Hardware[i].HardwareType == HardwareType.GpuNvidia)
                {
                    for (int j = 0; j < computer.Hardware[i].Sensors.Length; j++)
                    {
                        if (computer.Hardware[i].Sensors[j].SensorType == SensorType.Temperature && computer.Hardware[i].Sensors[j].Name == "GPU Core")
                            GPU_Temp = (computer.Hardware[i].Sensors[j].Value.ToString());
                            //GPU_Temp = computer.Hardware[i].Sensors[j].Value;
                        if (computer.Hardware[i].Sensors[j].SensorType == SensorType.Load && computer.Hardware[i].Sensors[j].Name == "GPU Core")
                            GPU_Load = (computer.Hardware[i].Sensors[j].Value.ToString());
                            //GPU_Load = computer.Hardware[i].Sensors[j].Value;
                        if (computer.Hardware[i].Sensors[j].SensorType == SensorType.Clock && computer.Hardware[i].Sensors[j].Name == "GPU Core")
                            GPU_Speed = (Math.Round((float)computer.Hardware[i].Sensors[j].Value, 1).ToString());
                            //GPU_Speed = computer.Hardware[i].Sensors[j].Value;
                    }
                }
                else if (computer.Hardware[i].HardwareType == HardwareType.GpuAti)
                {
                    for (int j = 0; j < computer.Hardware[i].Sensors.Length; j++)
                    {
                        if (computer.Hardware[i].Sensors[j].SensorType == SensorType.Temperature && computer.Hardware[i].Sensors[j].Name == "GPU Core")
                            GPU_Temp = (computer.Hardware[i].Sensors[j].Value.ToString());
                            //GPU_Temp = computer.Hardware[i].Sensors[j].Value;
                        if (computer.Hardware[i].Sensors[j].SensorType == SensorType.Load && computer.Hardware[i].Sensors[j].Name == "GPU Core")
                            GPU_Load = (computer.Hardware[i].Sensors[j].Value.ToString());
                            //GPU_Load = computer.Hardware[i].Sensors[j].Value;
                        if (computer.Hardware[i].Sensors[j].SensorType == SensorType.Clock && computer.Hardware[i].Sensors[j].Name == "GPU Core")
                            GPU_Speed = (Math.Round((float)computer.Hardware[i].Sensors[j].Value, 1).ToString());
                            //GPU_Speed = computer.Hardware[i].Sensors[j].Value;
                    }
                }
            }
            string[] outputs = {" ", CPU_Temp, CPU_Speed, CPU_Load, GPU_Temp, GPU_Speed, GPU_Load };
            computer.Close();
            return outputs;
        }
        static void Main(string[] args)
        {
            //Application.EnableVisualStyles();
            //Application.SetCompatibleTextRenderingDefault(false);
            //Application.Run(new MyCustomApplicationContext());
            string portNum = "";
            try
            {
                using (var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_PnPEntity WHERE Caption like '%(COM%'"))
                {
                    var portnames = SerialPort.GetPortNames();
                    var ports = searcher.Get().Cast<ManagementBaseObject>().ToList().Select(p => p["Caption"].ToString());

                    var portList = portnames.Select(n => n + " - " + ports.FirstOrDefault(s => s.Contains(n))).ToList();

                    foreach (string s in portList)
                    {
                        if(s.Contains("PI USB to Serial"))
                        {
                            Console.WriteLine(s);
                            portNum = s.Substring(0, s.IndexOf(" "));
                        }
                    }
                }
            }
            catch
            {
                Console.Write("Well, that didn't work the first.");
            }
            try
            {
                port = new SerialPort(portNum, 115200, Parity.None, 8, StopBits.One);
                port.Open();
                System.Threading.Thread.Sleep(500);
                port.WriteLine("pi");
                System.Threading.Thread.Sleep(500);
                port.WriteLine("raspberry");
                System.Threading.Thread.Sleep(1000);
                port.WriteLine("python something.py");
                System.Threading.Thread.Sleep(500);
                while (port.IsOpen)
                {
                    string[] strings = GetSystemInfo();
                    Console.WriteLine("Howdy");
                    for (int i = 0; i < strings.Length; i++)
                    {
                        port.WriteLine(strings[i]);
                        Console.WriteLine(strings[i]);
                    }
                    System.Threading.Thread.Sleep(1000);
                }
                port.Close();
            }
            catch
            {
                Console.Write("Well, that didn't work the second.");
            }
        }
    }

    /*public class MyCustomApplicationContext : ApplicationContext
    {
        private NotifyIcon trayIcon;

        public MyCustomApplicationContext()
        {
            // Initialize Tray Icon
            trayIcon = new NotifyIcon()
            {
                Icon = Resources.AppIcon,
                ContextMenu = new ContextMenu(new MenuItem[] {
                new MenuItem("Exit", Exit)
            }),
                Visible = true
            };
        }

        void Exit(object sender, EventArgs e)
        {
            // Hide tray icon, otherwise it will remain shown until user mouses over it
            trayIcon.Visible = false;

            Application.Exit();
        }
    }*/
}