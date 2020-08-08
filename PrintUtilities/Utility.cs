using System.Linq;

namespace PrintServiceLibrary
{
    /// <summary>
    /// Utility class for Communication API
    /// </summary>
    public static class Utility
    {
        /// <summary>
        /// Removes ZERO WIDTH NO-BREAK SPACE unicode character from XML. Neodynamic SDK generates &#65279; symbol at the beginning of the XML data
        /// </summary>
        /// <param name="xml">Input XML label template</param>
        public static void RemoveZeroWidthNoBreakCharacter(ref string xml)
        {
            if (xml.Contains('\uFEFF'))
            {
                xml = xml.Replace('\uFEFF', ' ');
                xml = xml.Trim();
            }
        }

        /// <summary>
        /// Roughly validates XML
        /// </summary>
        /// <param name="xml">XML to validate</param>
        /// <returns>Returns true if XML is valid, false otherwise</returns>
        public static bool IsXmlValid(string xml)
        {
            try
            {
                xml = xml.Trim();

                Utility.RemoveZeroWidthNoBreakCharacter(ref xml);
                System.Xml.Linq.XDocument.Parse(xml);

                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
