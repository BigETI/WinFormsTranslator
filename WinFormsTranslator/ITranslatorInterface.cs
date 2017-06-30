using System.Collections.Generic;

namespace WinFormsTranslator
{
    public interface ITranslatorInterface
    {
        string Language
        {
            get;
            set;
        }

        string AssemblyName
        {
            get;
        }

        IEnumerable<Language> Languages
        {
            get;
        }

        void SaveSettings();
    }
}
