using System.Collections.Generic;
using System.Runtime.Serialization;

/// <summary>
/// WinForms translator namespace
/// </summary>
namespace WinFormsTranslator
{
    /// <summary>
    /// Language data class
    /// </summary>
    [DataContract]
    public class LanguageData
    {
        /// <summary>
        /// Language
        /// </summary>
        [DataMember]
        private string language;

        /// <summary>
        /// Translations
        /// </summary>
        [DataMember]
        private Dictionary<string, string> translations;

        /// <summary>
        /// Language
        /// </summary>
        public string Language
        {
            get
            {
                if (language == null)
                {
                    language = "";
                }
                return language;
            }
        }

        /// <summary>
        /// Translations
        /// </summary>
        public IReadOnlyDictionary<string, string> Translations
        {
            get
            {
                if (translations == null)
                {
                    translations = new Dictionary<string, string>();
                }
                return translations;
            }
        }
    }
}
