namespace PrintServiceLibrary
{
    /// <summary>
    /// Font structrue containing basic information about font
    /// </summary>
    public class FontInfo
    {
        /// <summary>
        /// Name of the font
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Short name of the font (8 characters)
        /// </summary>
        public string ShortName => this.GenerateShortName();

        /// <summary>
        /// Does font support bold characters?
        /// </summary>
        public bool Bold { get; set; }

        /// <summary>
        /// Does font support italic characters?
        /// </summary>
        public bool Italic { get; set; }

        /// <summary>
        /// ToString override to return shortname instead.
        /// </summary>
        /// <returns>Returns font's shortname</returns>
        public new string ToString()
        {
            return this.GenerateShortName();
        }

        /// <summary>
        /// Generates short name. ZEBRA printer has limit of 8 characters as a font name
        /// </summary>
        /// <returns></returns>
        private string GenerateShortName()
        {
            // ZEBRA printer has limit of 8 characters to Font Name
            if (this.Name.Length < 7 && !this.Bold && !this.Italic)
            {
                return this.Name;
            }

            string shortName = string.Empty;

            if (this.Name.Length <= 6)
            {
                shortName = this.Name;
            }
            else
            {
                shortName = this.Name.Substring(0, 6);
            }

            if (this.Bold)
            {
                shortName += "B";
            }

            if (this.Italic)
            {
                shortName += "I";
            }

            return shortName;
        }
    }
}
