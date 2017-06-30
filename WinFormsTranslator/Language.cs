namespace WinFormsTranslator
{
    public class Language
    {

        private string nameKey;

        private string culture;

        public string Name
        {
            get
            {
                return Translator.GetTranslation(nameKey);
            }
        }

        public string Culture
        {
            get
            {
                return culture;
            }
        }

        public Language(string nameKey, string culture)
        {
            this.nameKey = nameKey;
            this.culture = culture;
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
