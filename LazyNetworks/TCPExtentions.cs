using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net;
using System.Text;

namespace AdvancedTCP
{
    static class TCPExtentions
    {
        public static IPEndPoint ParseIPEndPoint(string endPoint)
        {
            // https://stackoverflow.com/questions/2727609/best-way-to-create-ipendpoint-from-string/2727880#2727880.
            string[] ep = endPoint.Split(':');
            if (ep.Length < 2) throw new FormatException("Invalid endpoint format");
            IPAddress ip;
            if (ep.Length > 2)
            {
                if (!IPAddress.TryParse(string.Join(":", ep, 0, ep.Length - 1), out ip))
                {
                    throw new FormatException("Invalid ip-adress");
                }
            }
            else
            {
                if (!IPAddress.TryParse(ep[0], out ip))
                {
                    throw new FormatException("Invalid ip-adress");
                }
            }
            if (!int.TryParse(ep[ep.Length - 1], NumberStyles.None, NumberFormatInfo.CurrentInfo, out int port))
            {
                throw new FormatException("Invalid port");
            }
            return new IPEndPoint(ip, port);
        }
    }
}
