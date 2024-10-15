using System;
using System.Collections.Generic;
using DisableEmpFromZKT.Utilities;
using System.IO;

namespace DisableEmpFromZKT
{
    class Program
    {
        private static ZkemClient? objZkeeper;

        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("No employee ID provided.");
                return;
            }
            string employeeId = args[0];
            bool newEnabled;
            if (!bool.TryParse(args[1], out newEnabled))
            {
                Console.WriteLine("Invalid value for 'Enabled'. Please provide true or false.");
                return;
            }
            DisableEmployeeOnAllDevices(employeeId, newEnabled);
        }

        private static void DisableEmployeeOnAllDevices(string employeeId, bool newEnabled)
        {
            objZkeeper = new ZkemClient((sender, message) => Console.WriteLine($"Event raised: {message}"));
            var devices = GetDevices();

            foreach (var device in devices)
            {
                Console.WriteLine($"Attempting to connect to device IP: {device.IP}");
                bool isConnected = objZkeeper.Connect_Net(device.IP, device.Port);

                if (isConnected)
                {
                    Console.WriteLine($"Connected to Device IP: {device.IP}");
  
                    int machineNumber = device.DeviceId;
                    string enrollNumber = employeeId;

                    bool userExists = objZkeeper.SSR_GetUserInfo(machineNumber, enrollNumber, out string name, out string password, out int privilege, out bool enabled);

                    if (userExists)
                    {

                        bool updateResult = objZkeeper.SSR_SetUserInfo(machineNumber, enrollNumber, name, password, privilege, newEnabled);

                        if (updateResult)
                        {
                            Console.WriteLine($"OPERATION SUCCESSFULL EMPLOYEE: {employeeId} ENABLED : {newEnabled}");
                        }
                        else
                        {
                            Console.WriteLine($"OPERATION FAILED EMPLOYEE: {employeeId} ENABLED : {newEnabled}");
                        }
                    }
                    else 
                    {
                        Console.WriteLine($"USER DOES NOT EXIST : {employeeId}");
                    }

                    objZkeeper.Disconnect();
                }
                else
                {
                    Console.WriteLine($"Connection failed with Device IP: {device.IP}");
                }
            }
        }

        private static List<DeviceInfo> GetDevices()
        {
            return new List<DeviceInfo>
            {
                new DeviceInfo { IP = "192.168.1.201", Port = 4370, DeviceId = 1 },
                new DeviceInfo { IP = "192.168.1.52", Port = 4370, DeviceId = 1 },
                new DeviceInfo { IP = "192.168.1.200", Port = 4370, DeviceId = 1 },
                new DeviceInfo { IP = "192.168.1.207", Port = 4370, DeviceId = 1 },
                new DeviceInfo { IP = "192.168.1.205", Port = 4370, DeviceId = 1 },
                new DeviceInfo { IP = "192.168.1.203", Port = 4370, DeviceId = 1 },
                new DeviceInfo { IP = "192.168.1.202", Port = 4370, DeviceId = 1 },
                
            };
        }
    }
}
