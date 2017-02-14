using System;
using System.Text;
using System.IO;
using System.Xml;
using System.Windows;

namespace WPFImageViewer
{
    #region XMLSettings class
    class XMLSettings
    {
        public double Volume { get; set; }
        public short Zoom { get; set; }
        private WindowState windowState;
        public double Left { get; set; }
        public double Top { get; set; }
        public double Width { get; set; }
        public double Height { get; set; }
        public bool IsMuted { get; set; }
        public AfterP AfterPlayback { get; set; }
        public OrderB OrderBy { get; set; }
        public bool KeepRatio { get; set; }

        public WindowState WindowState
        {
            get
            {
                return windowState;
            }
            set
            {
                if (value != WindowState.Minimized)
                {
                    windowState = value;
                }
                else
                {
                    windowState = WindowState.Normal;
                }
            }
        }


        public XMLSettings()
        {
            Volume = 100;
            Zoom = 100;
            windowState = WindowState.Normal;
            Left = 0;
            Top = 0;
            Width = 640;
            Height = 360;
            IsMuted = false;
            AfterPlayback = AfterP.Loop;
            OrderBy = OrderB.Name;
            KeepRatio = false;
        }


        public enum AfterP : int
        {
            Loop,
            Nothing,
            Next,
        }

        public enum OrderB : int
        {
            Name,
            Date,
        }
    } 
    #endregion


    static class XMLHandler
    {
        #region createXML
        private static void createXML()
        {
            XmlTextWriter writer = new XmlTextWriter(AppDomain.CurrentDomain.BaseDirectory + "WPFImgConfig.xml", Encoding.UTF8);
            writer.WriteStartDocument(true);
            writer.Formatting = Formatting.Indented;
            writer.Indentation = 2;
            writer.WriteStartElement("Table");
            createNode("Normal", "100", "100", "0", "0", "640", "360", "False", "Loop", "Name", "False", writer);
            writer.WriteEndElement();
            writer.WriteEndDocument();
            writer.Close();
        }
        #endregion


        #region createNode
        private static void createNode(string WindowState, string Volume, string Zoom, string sPositionX, string sPositionY, string Width, string Height, string Mute, string AfterP, string OrderBy, string KeepRatio, XmlTextWriter writer)
        {
            writer.WriteStartElement("Config");
            writer.WriteStartElement("WindowState");
            writer.WriteString(WindowState);
            writer.WriteEndElement();
            writer.WriteStartElement("Volume");
            writer.WriteString(Volume);
            writer.WriteEndElement();
            writer.WriteStartElement("Zoom");
            writer.WriteString(Zoom);
            writer.WriteEndElement();
            writer.WriteStartElement("Left");
            writer.WriteString(sPositionX);
            writer.WriteEndElement();
            writer.WriteStartElement("Top");
            writer.WriteString(sPositionY);
            writer.WriteEndElement();
            writer.WriteStartElement("Width");
            writer.WriteString(Width);
            writer.WriteEndElement();
            writer.WriteStartElement("Height");
            writer.WriteString(Height);
            writer.WriteEndElement();
            writer.WriteStartElement("Mute");
            writer.WriteString(Mute);
            writer.WriteEndElement();
            writer.WriteStartElement("AfterPlayback");
            writer.WriteString(AfterP);
            writer.WriteEndElement();
            writer.WriteStartElement("OrderBy");
            writer.WriteString(OrderBy);
            writer.WriteEndElement();
            writer.WriteStartElement("KeepRatio");
            writer.WriteString(KeepRatio);
            writer.WriteEndElement();
            writer.WriteEndElement();
        }
        #endregion


        #region loadConfigXML
        public static XMLSettings loadConfigXML()
        {
            XMLSettings settings = new XMLSettings();

            try
            {
                XmlDocument myXmlDocument = new XmlDocument();
                myXmlDocument.Load(AppDomain.CurrentDomain.BaseDirectory + "WPFImgConfig.xml");

                XmlNode node;
                node = myXmlDocument.DocumentElement;

                foreach (XmlNode node1 in node.ChildNodes)
                {
                    foreach (XmlNode node2 in node1.ChildNodes)
                    {
                        switch (node2.Name)
                        {
                            case "WindowState":
                                switch (node2.InnerText)
                                {
                                    case "Maximized":
                                        settings.WindowState = WindowState.Maximized;
                                        break;
                                    case "Zoom":
                                        settings.WindowState = WindowState.Normal;
                                        break;
                                }
                                break;
                            case "Volume":
                                settings.Volume = Double.Parse(node2.InnerText);
                                break;
                            case "Zoom":
                                settings.Zoom = short.Parse(node2.InnerText);
                                break;
                            case "Left":
                                settings.Left = Double.Parse(node2.InnerText);
                                break;
                            case "Top":
                                settings.Top = Double.Parse(node2.InnerText);
                                break;
                            case "Width":
                                settings.Width = Double.Parse(node2.InnerText);
                                break;
                            case "Height":
                                settings.Height = Double.Parse(node2.InnerText);
                                break;
                            case "Mute":
                                settings.IsMuted = Convert.ToBoolean(node2.InnerText);
                                break;
                            case "AfterPlayback":
                                switch (node2.InnerText)
                                {
                                    case "Loop":
                                        settings.AfterPlayback = XMLSettings.AfterP.Loop;
                                        break;
                                    case "Next":
                                        settings.AfterPlayback = XMLSettings.AfterP.Next;
                                        break;
                                    case "Nothing":
                                        settings.AfterPlayback = XMLSettings.AfterP.Nothing;
                                        break;
                                }
                                break;
                            case "OrderBy":
                                switch (node2.InnerText)
                                {
                                    case "Name":
                                        settings.OrderBy = XMLSettings.OrderB.Name;
                                        break;
                                    case "Date":
                                        settings.OrderBy = XMLSettings.OrderB.Date;
                                        break;
                                }
                                break;
                            case "KeepRatio":
                                settings.KeepRatio = Convert.ToBoolean(node2.InnerText);
                                break;
                        }
                    }

                    return settings;
                }
            }
            catch (FileNotFoundException ex)
            {
                createXML();
                loadConfigXML();
            }

            return settings;
        }
        #endregion


        #region writeConfigXML
        public static void writeConfigXML(XMLSettings settings)
        {
            try
            {
                XmlDocument myXmlDocument = new XmlDocument();
                myXmlDocument.Load(AppDomain.CurrentDomain.BaseDirectory + "WPFImgConfig.xml");

                XmlNode node;
                node = myXmlDocument.DocumentElement;

                foreach (XmlNode node1 in node.ChildNodes)
                {
                    foreach (XmlNode node2 in node1.ChildNodes)
                    {
                        switch (node2.Name)
                        {
                            case "WindowState":
                                node2.InnerText = settings.WindowState.ToString();
                                break;
                            case "Volume":
                                node2.InnerText = settings.Volume.ToString();
                                break;
                            case "Zoom":
                                node2.InnerText = settings.Zoom.ToString();
                                break;
                            case "Left":
                                node2.InnerText = settings.Left.ToString();
                                break;
                            case "Top":
                                node2.InnerText = settings.Top.ToString();
                                break;
                            case "Width":
                                node2.InnerText = settings.Width.ToString();
                                break;
                            case "Height":
                                node2.InnerText = settings.Height.ToString();
                                break;
                            case "Mute":
                                node2.InnerText = settings.IsMuted.ToString();
                                break;
                            case "AfterPlayback":
                                node2.InnerText = settings.AfterPlayback.ToString();
                                break;
                            case "OrderBy":
                                node2.InnerText = settings.OrderBy.ToString();
                                break;
                            case "KeepRatio":
                                node2.InnerText = settings.KeepRatio.ToString();
                                break;
                        }
                    }

                    myXmlDocument.Save(AppDomain.CurrentDomain.BaseDirectory + "WPFImgConfig.xml");
                }
            }
            catch (FileNotFoundException ex)
            {
                MessageBox.Show("File \"WPFImgConfig.xml\" not found!");
            }
        } 
        #endregion
    }
}
