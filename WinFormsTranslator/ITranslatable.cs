/// <summary>
/// WinForms translator namespace
/// </summary>
namespace WinFormsTranslator
{
    /// <summary>
    /// Translatable interface
    /// </summary>
    public interface ITranslatable
    {
        /// <summary>
        /// Translatable text
        /// </summary>
        string TranslatableText
        {
            get;
            set;
        }
    }
}
