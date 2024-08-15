using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Net;
using System.Linq;

public class UITest : MonoBehaviour
{
    public TextMeshProUGUI txtTip;
    // Start is called before the first frame update
    void Start()
    {
        //string CommandLine = Environment.CommandLine;

        //txtTip.text += CommandLine;
        //string[] commandLineArgs = Environment.GetCommandLineArgs();


        //string str = "";
        //foreach (var item in commandLineArgs)
        //{
        //    str += item + "\n\t";
        //}
        //txtTip.text += str;

        txtTip.text += "Start \n\t";
        txtTip.text += "GetLocalIPAddress: " + GetLocalIPAddress() + "\n\t";
        txtTip.text += "GetLocalIPv4: " + GetLocalIPv4() + "\n\t";
        GetIp();
        InitIpAddress();
        txtTip.text += "InitIpAddress: " + ipAddress;
    }

    string ipAddress;
    private void InitIpAddress()
    {
        ipAddress = "";
#if UNITY_STANDALONE_WIN
        ipAddress = GetIp();
#endif

#if UNITY_ANDROID || UNITY_IPHONE
        ipAddress = Dns.GetHostEntry(Dns.GetHostName()).AddressList.First(f => f.AddressFamily == AddressFamily.InterNetwork || f.AddressFamily == AddressFamily.InterNetworkV6).ToString();
#endif
    }

    //获取ipAddress
    public string GetIpAddress()
    {
        return ipAddress;
    }

    private string GetIp()
    {
        string _ip = string.Empty;
        foreach (var item in NetworkInterface.GetAllNetworkInterfaces())
        {
            var _type1 = NetworkInterfaceType.Wireless80211;
            var _type2 = NetworkInterfaceType.Ethernet;
            if ((item.NetworkInterfaceType == _type1 || item.NetworkInterfaceType == _type2) && item.OperationalStatus == OperationalStatus.Up)
            {
                foreach (var ip in item.GetIPProperties().UnicastAddresses)
                {
                    switch (ip.Address.AddressFamily)
                    {
                        case AddressFamily.InterNetwork:
                            _ip = ip.Address.ToString();
                            break;
                        case AddressFamily.InterNetworkV6:
                            _ip = ip.Address.ToString();
                            break;
                    }
                }
            }
        }
        return _ip;
    }
    // Update is called once per frame
    void Update()
    {

    }
    //private void GetIp()
    //{
    //    IPAddress[] ip = Dns.GetHostAddresses(Dns.GetHostName());
    //    foreach (IPAddress ipAddr in ip)
    //    {
    //        if (ipAddr.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork ||
    //            ipAddr.AddressFamily == System.Net.Sockets.AddressFamily.InterNetworkV6)
    //        {
    //            txtTip.text += "GetIp: "  + ipAddr.ToString() + "\r\n";
    //        }
    //    }
    //}
    public string GetLocalIPv4()
    {
        return Dns.GetHostEntry(Dns.GetHostName())
            .AddressList.First(
                f => f.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
            .ToString();
    }

    string GetLocalIPAddress()
    {
        string localIP = null;

        // 获取所有网络接口
        NetworkInterface[] networkInterfaces = NetworkInterface.GetAllNetworkInterfaces();

        foreach (var networkInterface in networkInterfaces)
        {
            // 排除虚拟网络接口和回环接口
            if (networkInterface.NetworkInterfaceType == NetworkInterfaceType.Wireless80211 ||
                networkInterface.NetworkInterfaceType == NetworkInterfaceType.Ethernet &&
                networkInterface.OperationalStatus == OperationalStatus.Up)
            {
                // 获取网络接口的 IP 属性
                IPInterfaceProperties properties = networkInterface.GetIPProperties();

                // 获取每个 IP 地址
                foreach (var address in properties.UnicastAddresses)
                {
                    // 排除 IPv6 地址
                    if (address.Address.AddressFamily == AddressFamily.InterNetwork)
                    {
                        localIP = address.Address.ToString();
                        return localIP;
                    }
                }
            }
        }

        return localIP;
    }

}
