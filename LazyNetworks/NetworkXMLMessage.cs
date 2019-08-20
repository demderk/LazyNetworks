using System.Xml;

namespace AdvancedTCP
{
    public class NetworkXMLMessage : IMessageBase<XmlNode>
    {


        public NetworkXMLMessage(Client remoteClient, XmlDocument xml, string messageName) : this(remoteClient, xml)
        {
            MessageName = messageName;
        }

        public NetworkXMLMessage(Client remoteClient, XmlDocument messageBody)
        {
            MessageBody = messageBody;
            RemoteClient = remoteClient;
        }

        public NetworkXMLMessage(Client remoteClient, string xmlCode)
        {
            RemoteClient = remoteClient;
            try
            {
                XmlDocument document = new XmlDocument();
                document.LoadXml(xmlCode);
                XmlNode mainNode = document;
                MessageName = mainNode.Name;
                MessageBody = mainNode;
            }
            catch (XmlException ex)
            {
                throw ex;
            }
        }

        public string MessageName { get; }

        public XmlNode MessageBody { get; }

        public Client RemoteClient { get; }

        public MessageQuestion Question { get; set; }

        public override string ToString()
        {
            if (MessageBody != null)
            {
                return MessageBody.InnerText;
            }
            else
            {
                return base.ToString();
            }
        }
    }

}
