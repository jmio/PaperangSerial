using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Management;
using System.Text;
using System.Text.RegularExpressions;

namespace PaperangSerial
{
    /// <summary>
    /// Enumerate BT Serial COM port list
    /// </summary>
    public class EnumBluetoothSerial
    {
        /// <summary>
        /// Get BT Description from Registry
        /// </summary>
        /// <param name="address">MAC Address</param>
        /// <returns>BT Description</returns>
        public static string GetBluetoothRegistryName(string address)
        {
            string deviceName = "";
            string registryPath = @"SYSTEM\CurrentControlSet\Services\BTHPORT\Parameters\Devices";
            string devicePath = String.Format(@"{0}\{1}", registryPath, address);
            using (RegistryKey key = Registry.LocalMachine.OpenSubKey(devicePath))
            {
                if (key != null)
                {
                    var o = key.GetValue("Name");
                    byte[] raw = o as byte[];
                    if (raw != null)
                    {
                        deviceName = Encoding.ASCII.GetString(raw);
                    }
                }
            }
            return deviceName.TrimEnd('\0');
        }

        /// <summary>
        /// Enumerate BTSerial
        /// </summary>
        public static SortedDictionary<string, string> EnumBTSerial()
        {
            var result = new SortedDictionary<string, string>();

            // Get BT Serial List from Device manager
            Regex regexPortName = new Regex(@"(COM\d+)");
            using (ManagementObjectSearcher searchSerial = new ManagementObjectSearcher("SELECT * FROM Win32_PnPEntity"))
            {
                // Enumrate 
                foreach (ManagementObject obj in searchSerial.Get())
                {
                    string name = obj["Name"] as string;
                    string classGuid = obj["ClassGuid"] as string; // GUID
                    string devicePass = obj["DeviceID"] as string; // Device Instance Path
                    if (classGuid != null && devicePass != null)
                    {
                        // {4d36e978-e325-11ce-bfc1-08002be10318} is BT GUID
                        if (String.Equals(classGuid, "{4d36e978-e325-11ce-bfc1-08002be10318}", StringComparison.InvariantCulture))
                        {
                            // Get Device ID From Device Instance Path
                            string[] tokens = devicePass.Split('&');
                            if (tokens.Length < 5) break;
                            string[] addressToken = tokens[4].Split('_');
                            string bluetoothAddress = addressToken[0];
                            string comPortNumber = "";
                            string bluetoothName = "";
                            Match m = regexPortName.Match(name);
                            if (m.Success)
                            {
                                comPortNumber = m.Groups[1].ToString();
                            }
                            if (Convert.ToUInt64(bluetoothAddress, 16) > 0)
                            {
                                bluetoothName = GetBluetoothRegistryName(bluetoothAddress);
                            }
                            if ((comPortNumber != "") && (bluetoothName != ""))
                            {
                                // Add To List
                                result.Add(comPortNumber, bluetoothName + ":" + bluetoothAddress);
                            }
                        }
                    }
                }
            }

            return result;
        }

    }
}
