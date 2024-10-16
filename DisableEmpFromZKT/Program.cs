using System;
using System.Collections.Generic;
using DisableEmpFromZKT.Utilities;
using Oracle.ManagedDataAccess.Client;
using System.IO;

namespace DisableEmpFromZKT
{
    class Program
    {
        private static ZkemClient? objZkeeper;
        private const string ConnectionString = "User Id=SOLEHRE;Password=SOLEHRESOLAPPS;Data Source=(DESCRIPTION=(ADDRESS=(PROTOCOL=TCP)(HOST=192.168.1.236)(PORT=1521))(CONNECT_DATA=(SERVICE_NAME=orcl)))";

        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("No employee ID provided.");
                Console.WriteLine("Press Enter To Close Application");
                Console.ReadLine();
                return;
            }
            string employeeId = args[0];
            
            bool newEnabled;
            if (!bool.TryParse(args[1], out newEnabled))
            {
                Console.WriteLine("Invalid value for 'Enabled'. Please provide true or false.");
                Console.WriteLine("Press Enter To Close Application");
                Console.ReadLine();
                return;
            }
            string empId_u = args[2];
            DisableEmployeeOnAllDevices(employeeId, newEnabled, empId_u);
            //Console.WriteLine("Press Enter To Close Application");
            //Console.ReadLine();
        }

        private static void DisableEmployeeOnAllDevices(string employeeId, bool newEnabled, string empId_u)
        {
            objZkeeper = new ZkemClient((sender, message) => Console.WriteLine($"Event raised: {message}"));
            var devices = GetDevicesFromDatabase();

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
                            Console.WriteLine("");
                            Console.WriteLine("****************************************************************");
                            Console.WriteLine($"OPERATION SUCCESSFULL EMPLOYEE: {employeeId} ENABLED : {newEnabled}");
                            Console.WriteLine("****************************************************************");
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
            UpdateEmployeeZktStatus(empId_u, newEnabled);
        }
        private static void UpdateEmployeeZktStatus(string employeeId, bool newEnabled)
        {
            try
            {
                using (var connection = new OracleConnection(ConnectionString))
                {
                    connection.Open();
                    string query = "UPDATE HR_EMPLOYEES SET zkt_status = :zkt_status WHERE empid = :empid";
                    using (var command = new OracleCommand(query, connection))
                    {
                        int zktStatusValue = newEnabled ? 1 : 0;
                        command.Parameters.Add(new OracleParameter("zkt_status", zktStatusValue));
                        command.Parameters.Add(new OracleParameter("empid", employeeId));

                        int rowsAffected = command.ExecuteNonQuery();

                        if (rowsAffected > 0)
                        {
                            Console.WriteLine($"Successfully updated zkt_status for employee ID: {employeeId} to {zktStatusValue}");
                        }
                        else
                        {
                            Console.WriteLine($"No records found for employee ID: {employeeId} to update.");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error updating zkt_status in database: " + ex.Message);
            }
        }
        private static List<DeviceInfo> GetDevicesFromDatabase()
        {
            var devices = new List<DeviceInfo>();
            try
            {
                using (var connection = new OracleConnection(ConnectionString))
                {
                    connection.Open();
                    string query = "SELECT IP, PORT, DEVICE_ID FROM DEVICE_TABLE"; 
                    using (var command = new OracleCommand(query, connection))
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            devices.Add(new DeviceInfo
                            {
                                IP = reader.GetString(0),
                                Port = reader.GetInt32(1),
                                DeviceId = reader.GetInt32(2)
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error retrieving devices from database: " + ex.Message);
            }
            return devices;
        }

        //private static List<DeviceInfo> GetDevices()
        //{
        //    return new List<DeviceInfo>
        //    {
        //        new DeviceInfo { IP = "192.168.1.201", Port = 4370, DeviceId = 1 },
        //        new DeviceInfo { IP = "192.168.1.52", Port = 4370, DeviceId = 1 },
        //        new DeviceInfo { IP = "192.168.1.200", Port = 4370, DeviceId = 1 },
        //        new DeviceInfo { IP = "192.168.1.207", Port = 4370, DeviceId = 1 },
        //        new DeviceInfo { IP = "192.168.1.205", Port = 4370, DeviceId = 1 },
        //        new DeviceInfo { IP = "192.168.1.203", Port = 4370, DeviceId = 1 },
        //        new DeviceInfo { IP = "192.168.1.202", Port = 4370, DeviceId = 1 },

        //    };
        //}


    }
}
