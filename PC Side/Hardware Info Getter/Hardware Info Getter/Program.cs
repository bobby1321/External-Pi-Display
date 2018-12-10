using OpenHardwareMonitor.Hardware;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO.Ports;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Management;
using System.Windows;
using System.Collections;
using System.Threading;

namespace Hardware_Info_Getter
{
    static class Program
    {
        static SerialPort port;
        static Boolean dateActive = true;
        static Boolean timeActive = true;
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
                    List<Double> avgClockSum = new List<Double>();
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
                    CPU_Speed = "" + Math.Round(avgClockSum[0], 1);
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
            string[] outputs = { " ", CPU_Temp, CPU_Speed, CPU_Load, GPU_Temp, GPU_Speed, GPU_Load, dateActive.ToString(), timeActive.ToString() };
            computer.Close();
            return outputs;
        }

        class MyApplicationContext : ApplicationContext
        {
            NotifyIcon notifyIcon;
            MenuItem configMenuItem;
            MenuItem exitMenuItem;
            MenuItem dateMenuItem;
            MenuItem timeMenuItem;
            Thread t2;

            public MyApplicationContext()
            {
                notifyIcon = new NotifyIcon();
                configMenuItem = new MenuItem("Configure...");
                exitMenuItem = new MenuItem("Exit", new EventHandler(Exit));
                dateMenuItem = new MenuItem("Date", new EventHandler(Date));
                dateMenuItem.Checked = true;
                timeMenuItem = new MenuItem("Time", new EventHandler(Time));
                timeMenuItem.Checked = true;
                configMenuItem.MenuItems.Add(dateMenuItem);
                configMenuItem.MenuItems.Add(timeMenuItem);
                notifyIcon.Icon = new Icon("hiss_B6r_icon.ico");
                notifyIcon.ContextMenu = new ContextMenu(new MenuItem[] {configMenuItem, exitMenuItem});
                notifyIcon.Visible = true;
                t2 = new Thread(delegate ()
                {
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
                                if (s.Contains("PI USB to Serial"))
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
                        Console.WriteLine("pi");
                        port.WriteLine("pi");
                        System.Threading.Thread.Sleep(500);
                        port.WriteLine("raspberry");
                        Console.WriteLine("raspberry");
                        System.Threading.Thread.Sleep(1000);
                        port.WriteLine("python something.py");
                        Console.WriteLine("python something.py");
                        System.Threading.Thread.Sleep(500);
                        while (port.IsOpen)
                        {
                            string[] strings = GetSystemInfo();
                            for (int i = 0; i < strings.Length; i++)
                            {
                                port.WriteLine(strings[i]);
                                Console.WriteLine(strings[i]);
                            }
                            System.Threading.Thread.Sleep(1000);
                        }
                        port.Close();
                        Application.Exit();
                    }
                    catch
                    {
                        Console.Write("Well, that didn't work the second.");
                    }
                });
                t2.Start();
            }

            void Date(object sender, EventArgs e)
            {
                if (dateMenuItem.Checked)
                {
                    dateActive = false;
                    dateMenuItem.Checked = false;
                }
                else
                {
                    dateActive = true;
                    dateMenuItem.Checked = true;
                }
            }

            void Time(object sender, EventArgs e)
            {
                if (timeMenuItem.Checked)
                {
                    timeActive = false;
                    timeMenuItem.Checked = false;
                }
                else
                {
                    timeActive = true;
                    timeMenuItem.Checked = true;
                }
            }

            void Exit(object sender, EventArgs e)
            {
                notifyIcon.Visible = false;
                port.WriteLine("sudo reboot");
                t2.Abort();
                Application.Exit();
            }


        }
    
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MyApplicationContext());
        }
    }
}