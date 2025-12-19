using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using UnityEngine;

namespace Network
{
    public static class NetworkIPHelper
    {
        /// <summary>
        /// Получает локальный IP адрес хоста через NetworkInterface
        /// </summary>
        public static string GetLocalIPAddress()
        {
            // Метод 1: Через NetworkInterface (рекомендуемый)
            foreach (NetworkInterface ni in NetworkInterface.GetAllNetworkInterfaces())
            {
                // Проверяем только активные интерфейсы (WiFi и Ethernet)
                if (ni.NetworkInterfaceType == NetworkInterfaceType.Wireless80211 || 
                    ni.NetworkInterfaceType == NetworkInterfaceType.Ethernet)
                {
                    // Проверяем, что интерфейс работает и подключен
                    if (ni.OperationalStatus == OperationalStatus.Up)
                    {
                        foreach (UnicastIPAddressInformation ip in ni.GetIPProperties().UnicastAddresses)
                        {
                            // Возвращаем первый IPv4 адрес (не localhost)
                            if (ip.Address.AddressFamily == AddressFamily.InterNetwork && 
                                !IPAddress.IsLoopback(ip.Address))
                            {
                                return ip.Address.ToString();
                            }
                        }
                    }
                }
            }

            // Метод 2: Fallback через Dns (если NetworkInterface не сработал)
            try
            {
                var host = Dns.GetHostEntry(Dns.GetHostName());
                foreach (var ip in host.AddressList)
                {
                    if (ip.AddressFamily == AddressFamily.InterNetwork && 
                        !IPAddress.IsLoopback(ip))
                    {
                        return ip.ToString();
                    }
                }
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"Failed to get IP via Dns: {e.Message}");
            }

            // Fallback: возвращаем localhost
            Debug.LogWarning("Could not find local IP address, using localhost");
            return "127.0.0.1";
        }

        /// <summary>
        /// Возвращает порт по умолчанию для сетевого подключения
        /// </summary>
        public static int GetDefaultPort() => 7777;
    }
}

