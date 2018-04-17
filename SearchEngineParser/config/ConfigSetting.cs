using SearchEngineParser.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Web;
using System.Xml;
using System.Xml.Serialization;

public class ConfigSettings

{
   
    public ConfigSettings()
    {
       // var d = System.Reflection.Assembly.GetExecutingAssembly().Location;
       // d = Path.GetDirectoryName(d);
        var d = HttpRuntime.AppDomainAppPath;
        
        ConfigFile =Path.Combine(d, "SearchEngins.xml");
    }

    public string ConfigFile { get; set; }
    /// <summary>
    /// Serializes an object.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="serializableObject"></param>
    /// <param name="fileName"></param>
    public void SerializeObject<T>(T serializableObject )
    {
        var fileName = ConfigFile;
        if (serializableObject == null) { return; }

        try
        {
            XmlDocument xmlDocument = new XmlDocument();
            XmlSerializer serializer = new XmlSerializer(serializableObject.GetType());
            using (MemoryStream stream = new MemoryStream())
            {
                serializer.Serialize(stream, serializableObject);
                stream.Position = 0;
                xmlDocument.Load(stream);
                xmlDocument.Save(fileName);
                stream.Close();
            }
        }
        catch (Exception ex)
        {
            //Log exception here
        }
    }


    /// <summary>
    /// Deserializes an xml file into an object list
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="fileName"></param>
    /// <returns></returns>
    public T DeSerializeObject<T>()
    {
        var fileName = ConfigFile;

        if (string.IsNullOrEmpty(fileName)) { return default(T); }

        T objectOut = default(T);

        try
        {
            string attributeXml = string.Empty;

            XmlDocument xmlDocument = new XmlDocument();
            xmlDocument.Load(fileName);
            string xmlString = xmlDocument.OuterXml;

            using (StringReader read = new StringReader(xmlString))
            {
                Type outType = typeof(T);

                XmlSerializer serializer = new XmlSerializer(outType);
                using (XmlReader reader = new XmlTextReader(read))
                {
                    objectOut = (T)serializer.Deserialize(reader);
                    reader.Close();
                }

                read.Close();
            }
        }
        catch (Exception ex)
        {
            //Log exception here
        }

        return objectOut;
    }
}