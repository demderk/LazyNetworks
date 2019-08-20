using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Net.NetworkInformation;
using System.Linq;
using System.Threading;
using System.Globalization;
using System.IO;

namespace AdvancedTCP
{
    public enum MessageDataType
    {
        Bytes = 0x0000,
        XMLDocument = 0x0001,
        Question = 0x0002
    }

}
