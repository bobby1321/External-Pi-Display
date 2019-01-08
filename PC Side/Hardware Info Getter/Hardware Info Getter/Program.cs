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
        //Serial communication port to the pi
        static SerialPort port;

        //Variables that control display of config options
        static Boolean dateActive = true;
        static Boolean timeActive = true;
        static Boolean autoRebootActive = false;
        static Boolean time24hrActive = false;

        //Does something
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

        //Actually gets the system info, returns string array with values to send
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

            //Cycle through all of the sensor arrays detected in the system
            for (int i = 0; i < computer.Hardware.Length; i++)
            {
                //If sensor array is a CPU sensor array
                if (computer.Hardware[i].HardwareType == HardwareType.CPU)
                {
                    List<Double> avgClockSum = new List<Double>();
                    int coreTempSensorCount = 0;

                    //Cycle through all CPU sensors
                    for (int j = 0; j < computer.Hardware[i].Sensors.Length; j++)
                    {
                        //If the sensor is a temperature sensor and named "CPU Package"
                        if (computer.Hardware[i].Sensors[j].SensorType == SensorType.Temperature && computer.Hardware[i].Sensors[j].Name == "CPU Package")
                            CPU_Temp = (computer.Hardware[i].Sensors[j].Value.ToString());

                        //If the sensor is a clock speed sensor not named "Bus Speed"
                        if (computer.Hardware[i].Sensors[j].SensorType == SensorType.Clock && computer.Hardware[i].Sensors[j].Name != "Bus Speed")
                        {
                            avgClockSum.Add((double)computer.Hardware[i].Sensors[j].Value);
                            coreTempSensorCount++;
                        }

                        //If the sensor is a load percentage sensor and is named "CPU Total"
                        if (computer.Hardware[i].Sensors[j].SensorType == SensorType.Load && computer.Hardware[i].Sensors[j].Name == "CPU Total")
                            CPU_Load = (computer.Hardware[i].Sensors[j].Value.ToString());
                    }
                    CPU_Speed = "" + Math.Round(avgClockSum[0], 1);
                }

                //If sensor array is an NVidia GPU sensor array
                else if (computer.Hardware[i].HardwareType == HardwareType.GpuNvidia)
                {
                    //Cycle through all sensors in the array
                    for (int j = 0; j < computer.Hardware[i].Sensors.Length; j++)
                    {
                        //If the sensor is a temperature sensor named "GPU Core"
                        if (computer.Hardware[i].Sensors[j].SensorType == SensorType.Temperature && computer.Hardware[i].Sensors[j].Name == "GPU Core")
                            GPU_Temp = (computer.Hardware[i].Sensors[j].Value.ToString());

                        //If the sensor is a load percentage sensor named "GPU Core"
                        if (computer.Hardware[i].Sensors[j].SensorType == SensorType.Load && computer.Hardware[i].Sensors[j].Name == "GPU Core")
                            GPU_Load = (computer.Hardware[i].Sensors[j].Value.ToString());

                        //If the sensor is a clock speed sensor named "GPU Core"
                        if (computer.Hardware[i].Sensors[j].SensorType == SensorType.Clock && computer.Hardware[i].Sensors[j].Name == "GPU Core")
                            GPU_Speed = (Math.Round((float)computer.Hardware[i].Sensors[j].Value, 1).ToString());
                    }
                }

                //If sensor array is an AMD GPU sensor array
                else if (computer.Hardware[i].HardwareType == HardwareType.GpuAti)
                {
                    //Cycle throguh all sensors in the array
                    for (int j = 0; j < computer.Hardware[i].Sensors.Length; j++)
                    {
                        //If the sensor is a temperature sensor named "GPU Core"
                        if (computer.Hardware[i].Sensors[j].SensorType == SensorType.Temperature && computer.Hardware[i].Sensors[j].Name == "GPU Core")
                            GPU_Temp = (computer.Hardware[i].Sensors[j].Value.ToString());

                        //If the sensor is a load percentage sensor named "GPU Core"
                        if (computer.Hardware[i].Sensors[j].SensorType == SensorType.Load && computer.Hardware[i].Sensors[j].Name == "GPU Core")
                            GPU_Load = (computer.Hardware[i].Sensors[j].Value.ToString());

                        //If the sensor is a clock speed sensor named "GPU Core"
                        if (computer.Hardware[i].Sensors[j].SensorType == SensorType.Clock && computer.Hardware[i].Sensors[j].Name == "GPU Core")
                            GPU_Speed = (Math.Round((float)computer.Hardware[i].Sensors[j].Value, 1).ToString());
                    }
                }
            }

            //String array of outputs to be sent to the pi
            string[] outputs = { " ", CPU_Temp, CPU_Speed, CPU_Load, GPU_Temp, GPU_Speed, GPU_Load, dateActive.ToString(), timeActive.ToString(), time24hrActive.ToString() };
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
            MenuItem autoRebootMenuItem;
            MenuItem time12hrMenuItem;
            MenuItem time24hrMenuItem;
            Thread t2;

            //Constructing the taskbar app
            public MyApplicationContext()
            {
                notifyIcon = new NotifyIcon();

                //Top level menu
                configMenuItem = new MenuItem("Configure...");
                exitMenuItem = new MenuItem("Exit", new EventHandler(Exit));

                //Date toggle
                dateMenuItem = new MenuItem("Date", new EventHandler(Date));
                dateMenuItem.Checked = true; //Change to set default value

                //Time menu
                timeMenuItem = new MenuItem("Time");
                time12hrMenuItem = new MenuItem("12-Hour", new EventHandler(Time24Hr));
                time12hrMenuItem.Checked = true; //Change to set default value
                time24hrMenuItem = new MenuItem("24-Hour", new EventHandler(Time24Hr));
                time24hrMenuItem.Checked = false; //Change to set default value

                //Auto Reboot toggle
                autoRebootMenuItem = new MenuItem("Auto Reboot After Exit", new EventHandler(AutoReboot));
                autoRebootMenuItem.Checked = false; //Change to set default value

                configMenuItem.MenuItems.Add(dateMenuItem);
                configMenuItem.MenuItems.Add(timeMenuItem);
                configMenuItem.MenuItems.Add(autoRebootMenuItem);

                timeMenuItem.MenuItems.Add(time12hrMenuItem);
                timeMenuItem.MenuItems.Add(time24hrMenuItem);

                notifyIcon.Icon = new Icon("hiss_B6r_icon.ico"); //Change to set custom icon
                notifyIcon.ContextMenu = new ContextMenu(new MenuItem[] {configMenuItem, exitMenuItem});
                notifyIcon.Visible = true;

                //Defining new thread for serial commands
                t2 = new Thread(delegate ()
                {
                    string portNum = "";

                    //Seraching for the pi from a list of serial devices
                    try
                    {
                        using (var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_PnPEntity WHERE Caption like '%(COM%'"))
                        {
                            var portnames = SerialPort.GetPortNames();
                            var ports = searcher.Get().Cast<ManagementBaseObject>().ToList().Select(p => p["Caption"].ToString());

                            var portList = portnames.Select(n => n + " - " + ports.FirstOrDefault(s => s.Contains(n))).ToList();

                            foreach (string s in portList)
                            {
                                if (s.Contains("PI USB to Serial")) //Change this if your pi shows up as a different name
                                {
                                    Console.WriteLine(s);
                                    portNum = s.Substring(0, s.IndexOf(" "));
                                }
                            }
                        }
                    }
                    catch
                    {
                        Console.Write("Serial Connection not established");
                    }
                    try
                    {
                        //Opening the connection at the previously defined port
                        port = new SerialPort(portNum, 115200, Parity.None, 8, StopBits.One);
                        port.Open();
                        System.Threading.Thread.Sleep(500);

                        Console.WriteLine("pi");
                        port.WriteLine("pi");   //I know, this is a really stupid way to do this. Change the string if you have a different username for your pi
                        System.Threading.Thread.Sleep(500);
                        port.WriteLine("raspberry");  //Change this string if you have a different password for your pi
                        Console.WriteLine("raspberry");
                        System.Threading.Thread.Sleep(1000);

                        port.WriteLine("python something.py"); //Runs the program on board the pi. Change this string if your program is called something different
                        Console.WriteLine("python something.py");
                        System.Threading.Thread.Sleep(500);

                        //Get and send updates to the pi
                        while (port.IsOpen)
                        {
                            string[] strings = GetSystemInfo(); //Reads the string array from GetSystemInfo and sends them across the serial port
                            for (int i = 0; i < strings.Length; i++)
                            {
                                port.WriteLine(strings[i]);
                                Console.WriteLine(strings[i]);
                            }
                            System.Threading.Thread.Sleep(1000); //Change this value if you want to change the update rate
                        }
                        port.Close();
                        Application.Exit();
                    }
                    catch
                    {
                        Console.Write("Unable to write to Pi");
                    }
                });
                t2.Start();
            }

            //Defines the action that occurs when the date button is clicked
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

            //Defines the action that occurs when the auto reboot button is clicked
            void AutoReboot(object sender, EventArgs e)
            {
                if (autoRebootMenuItem.Checked)
                {
                    autoRebootActive = false;
                    autoRebootMenuItem.Checked = false;
                }
                else
                {
                    autoRebootActive = true;
                    autoRebootMenuItem.Checked = true;
                }
            }

            //Defines the action that occurs when either of the time buttons are clicked
            void Time24Hr(object sender, EventArgs e)
            {
                if (!timeActive)
                {
                    timeActive = true;
                    if (sender == time12hrMenuItem)
                    {
                        time24hrActive = false;
                        time24hrMenuItem.Checked = false;
                        time12hrMenuItem.Checked = true;
                    }
                    else if (sender == time24hrMenuItem)
                    {
                        time24hrActive = true;
                        time24hrMenuItem.Checked = true;
                        time12hrMenuItem.Checked = false;
                    }
                    return;
                }
                else if (sender == time12hrMenuItem && time24hrActive)
                {
                    time24hrActive = false;
                    time24hrMenuItem.Checked = false;
                    time12hrMenuItem.Checked = true;
                }
                else if (sender == time12hrMenuItem && !time24hrActive)
                {
                    time12hrMenuItem.Checked = false;
                    time24hrActive = false;
                    timeActive = false;
                }
                else if (sender == time24hrMenuItem && !time24hrActive)
                {
                    time24hrActive = true;
                    time24hrMenuItem.Checked = true;
                    time12hrMenuItem.Checked = false;
                }
                else if (sender == time24hrMenuItem && time24hrActive)
                {
                    time24hrMenuItem.Checked = false;
                    time24hrActive = false;
                    timeActive = false;
                }
            }

            //Defines the action that occurs when the exit button is clicked
            void Exit(object sender, EventArgs e)
            {
                notifyIcon.Visible = false;
                if (autoRebootActive)
                {
                    port.WriteLine("Stop");
                    port.WriteLine("sudo reboot");
                }
                t2.Abort();
                Application.Exit();
            }


        }

        //Run all the things    
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MyApplicationContext());
        }
    }
}