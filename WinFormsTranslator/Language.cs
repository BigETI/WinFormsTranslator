/// <summary>
/// Windows forms translator namespace
/// </summary>
namespace WinFormsTranslator
{
    /// <summary>
    /// Language class
    /// </summary>
    public class Language
    {
        /// <summary>
        /// Name key
        /// </summary>
        private readonly string nameKey;

        /// <summary>
        /// Culture
        /// </summary>
        private readonly string culture;

        /// <summary>
        /// Name
        /// </summary>
        public string Name
        {
            get
            {
                return Translator.GetTranslation(nameKey);
            }
        }

        /// <summary>
        /// Culture
        /// </summary>
        public string Culture
        {
            get
            {
                return culture;
            }
        }

        /// <summary>
        /// Language
        /// </summary>
        /// <param name="nameKey">Name key</param>
        /// <param name="culture">Culture</param>
        public Language(string nameKey, string culture)
        {
            this.nameKey = nameKey;
            this.culture = culture;
        }

        /// <summary>
        /// To string
        /// </summary>
        /// <returns>String representation</returns>
        public override string ToString()
        {
            return Name;
        }
    }
}
