namespace PrintServiceLibrary
{
    using System.Collections.Generic;
    using System.Data;
    using System.IO;
    using System.Linq;
    using System.Xml;
    using System.Xml.Linq;
    using Newtonsoft.Json;

    /// <summary>
    /// Class that converts various files and structures into DataSets (for Data Binding)
    /// </summary>
    internal class DataBinder
    {
        /// <summary>
        /// Binds XML data to DataSet
        /// </summary>
        /// <param name="json">Input xml data</param>
        /// <returns>Returns <see cref="DataSet"/> with XML data, null if the conversion fails</returns>
        internal DataSet BindXmlDataToDataSet(string xmlData)
        {
            using (StringReader sr = new StringReader(xmlData))
            {
                DataSet data = new DataSet();
                data.ReadXml(sr);

                return data;
            }
        }

        /// <summary>
        /// Binds JSON data to DataSet
        /// </summary>
        /// <param name="json">Input json</param>
        /// <returns>Returns <see cref="DataSet"/> with JSON data, null and error message if the conversion fails</returns>
        internal (DataSet data, string errorMessage) BindJsonDataToDataSet(string json)
        {
            if (string.IsNullOrEmpty(json))
            {
                return (null, Properties.Localization.CannotDeserializeEmptyJson);
            }

            XmlDocument convertedXml = JsonConvert.DeserializeXmlNode(json);
            var dataSet = this.BindXmlDataToDataSet(convertedXml.InnerXml);
            return (dataSet, string.Empty);
        }

        /// <summary>
        /// Replaces XML datafields by given values
        /// </summary>
        /// <param name="xml">XML with label template</param>
        /// <param name="source">Source tags in XML</param>
        /// <param name="data">Data to replace the tags</param>
        /// <returns>Returns modified XML</returns>
        internal string ReplaceDataFieldsByValuesInXml(string xml, List<string> source, List<string> data)
        {
            Utility.RemoveZeroWidthNoBreakCharacter(ref xml);

            var xmlLinqData = XElement.Parse(xml);
            var items = xmlLinqData.Element("Items").Elements().ToList();

            foreach (var item in items)
            {
                var dataFieldAttribute = item.Attribute("DataField");
                if (dataFieldAttribute != null)
                {
                    int index = source.IndexOf(dataFieldAttribute.Value);
                    if (index < 0)
                    {
                        continue;
                    }

                    switch (item.Name.ToString())
                    {
                        case "TextItem":
                            item.Attribute("Text").SetValue(data[index]);
                            break;

                        case "BarcodeItem":
                            item.Attribute("Code").SetValue(data[index]);
                            break;

                        case "ImageItem":
                            if (item.Attribute("SourceBase64") != null)
                            {
                                item.Attribute("SourceBase64").SetValue(data[index]);
                            }
                            else
                            {
                                XAttribute newSourceAttribute = new XAttribute("SourceBase64", data[index]);
                                item.Add(newSourceAttribute);
                            }

                            break;
                    }

                    dataFieldAttribute.SetValue(string.Empty);
                }
            }

            return "<?xml version=\"1.0\" encoding=\"utf-8\"?>" + xmlLinqData.ToString();
        }

        /// <summary>
        /// Roughly validates ZPL input
        /// </summary>
        /// <param name="zpl">ZPL to validate</param>
        /// <returns>Returns true if the ZPL is valid, false otherwise</returns>
        private (bool isValid, string errorMessage) ValidateZplInput(string zpl)
        {
            if (string.IsNullOrEmpty(zpl))
            {
                return (false, Properties.Localization.CannotBindToEmptyZPLTemplate);
            }

            if (!zpl.StartsWith("^XA") && zpl.EndsWith("^XZ"))
            {
                return (false, Properties.Localization.InvalidZPL);
            }

            return (true, string.Empty);
        }
    }
}
