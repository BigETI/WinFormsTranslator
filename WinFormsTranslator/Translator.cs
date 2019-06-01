using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Json;
using System.Windows.Forms;

/// <summary>
/// Windows forms translator namespace
/// </summary>
namespace WinFormsTranslator
{
    /// <summary>
    /// Translator class
    /// </summary>
    public class Translator
    {
        /// <summary>
        /// Serializer
        /// </summary>
        private static readonly DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(LanguageData), new DataContractJsonSerializerSettings { UseSimpleDictionaryFormat = true });

        /// <summary>
        /// Languages directory
        /// </summary>
        private static readonly string languagesDirectory = "./languages";

        /// <summary>
        /// Languages
        /// </summary>
        private Dictionary<string, LanguageData> languages;

        /// <summary>
        /// Languages
        /// </summary>
        public IReadOnlyDictionary<string, LanguageData> Languages
        {
            get
            {
                InitLanguages();
                return languages;
            }
        }

        /// <summary>
        /// Selected culture
        /// </summary>
        public string Culture { get; private set; } = "en-GB";

        /// <summary>
        /// Fallback culture
        /// </summary>
        public string FallbackCulture { get; private set; } = "en-GB";

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="culture">Culture</param>
        /// <param name="fallbackCulture"></param>
        public Translator(string culture, string fallbackCulture)
        {
            if (culture != null)
            {
                Culture = culture;
            }
            if (fallbackCulture != null)
            {
                FallbackCulture = fallbackCulture;
            }
        }

        /// <summary>
        /// Initialize languages
        /// </summary>
        private void InitLanguages()
        {
            if (languages == null)
            {
                languages = new Dictionary<string, LanguageData>();
                if (Directory.Exists(languagesDirectory))
                {
                    try
                    {
                        string[] language_files = Directory.GetFiles(languagesDirectory, "*-*.json", SearchOption.AllDirectories);
                        if (language_files != null)
                        {
                            foreach (string language_file in language_files)
                            {
                                if (language_file != null)
                                {
                                    try
                                    {
                                        using (FileStream file_stream = File.OpenRead(language_file))
                                        {
                                            LanguageData language_data = serializer.ReadObject(file_stream) as LanguageData;
                                            if (language_data != null)
                                            {
                                                string culture = Path.GetFileNameWithoutExtension(language_file);
                                                if (languages.ContainsKey(culture))
                                                {
                                                    languages[culture] = language_data;
                                                }
                                                else
                                                {
                                                    languages.Add(culture, language_data);
                                                }
                                            }
                                        }
                                    }
                                    catch (Exception e)
                                    {
                                        Console.Error.WriteLine(e);
                                    }
                                }
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        Console.Error.WriteLine(e);
                    }
                }
            }
        }

        /// <summary>
        /// Try translate
        /// </summary>
        /// <param name="input">Input</param>
        /// <param name="output">Output</param>
        /// <returns>"true" if successful, otherwise "false"</returns>
        public bool TryTranslate(string input, out string output)
        {
            bool ret = false;
            output = input;
            if (input != null)
            {
                InitLanguages();
                if (input.StartsWith("{$") && input.EndsWith("$}") && (input.Length > 4))
                {
                    output = GetTranslation(input.Substring(2, input.Length - 4));
                    ret = (input != output);
                }
            }
            return ret;
        }

        /// <summary>
        /// Translate controls
        /// </summary>
        /// <param name="parent">Parent control</param>
        public void TranslateControls(Control parent)
        {
            InitLanguages();
            try
            {
                if (parent != null)
                {
                    string translated = "";
                    InitLanguages();
                    IEnumerable<Control> controls = GetSelfAndChildrenRecursive(parent);
                    foreach (Control c in controls)
                    {
                        if (TryTranslate(c.Text, out translated))
                        {
                            c.Text = translated;
                        }
                        IEnumerable<ToolStripMenuItem> tsmis = GetAllToolStripMenuItemsRecursive(c.ContextMenuStrip);
                        foreach (ToolStripMenuItem tsmi in tsmis)
                        {
                            if (TryTranslate(tsmi.Text, out translated))
                            {
                                tsmi.Text = translated;
                            }
                        }
                        if (c is ComboBox)
                        {
                            ComboBox cb = (ComboBox)c;
                            for (int i = 0; i < cb.Items.Count; i++)
                            {
                                if (cb.Items[i] is string)
                                {
                                    if (TryTranslate((string)(cb.Items[i]), out translated))
                                    {
                                        cb.Items[i] = translated;
                                    }
                                }
                                else if (cb.Items[i] is ITranslatable)
                                {
                                    if (TryTranslate(((ITranslatable)(cb.Items[i])).TranslatableText, out translated))
                                    {
                                        ((ITranslatable)(cb.Items[i])).TranslatableText = translated;
                                    }
                                }
                            }
                        }
                        else if (c is ListView)
                        {
                            ListView lv = (ListView)c;
                            foreach (ColumnHeader col in lv.Columns)
                            {
                                if (TryTranslate(col.Text, out translated))
                                {
                                    col.Text = translated;
                                }
                            }
                            foreach (ListViewGroup grp in lv.Groups)
                            {
                                if (TryTranslate(grp.Header, out translated))
                                {
                                    grp.Header = translated;
                                }
                            }
                        }
                        else if (c is ToolStrip)
                        {
                            foreach (ToolStripItem tsi in ((ToolStrip)c).Items)
                            {
                                if (TryTranslate(tsi.Text, out translated))
                                {
                                    tsi.Text = translated;
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.Error.WriteLine(e.Message);
            }
        }

        /// <summary>
        /// Get translation
        /// </summary>
        /// <param name="key">Key</param>
        /// <returns>Value</returns>
        public string GetTranslation(string key)
        {
            string ret = null;
            if (key != null)
            {
                if (Languages.ContainsKey(Culture))
                {
                    LanguageData language = Languages[Culture];
                    if (language != null)
                    {
                        if (language.Translations.ContainsKey(key))
                        {
                            ret = language.Translations[key];
                        }
                    }
                }
                if (ret == null)
                {
                    if (Languages.ContainsKey(FallbackCulture))
                    {
                        LanguageData language = Languages[FallbackCulture];
                        if (language != null)
                        {
                            if (language.Translations.ContainsKey(key))
                            {
                                ret = language.Translations[key];
                            }
                        }
                    }
                }
                if (ret == null)
                {
                    ret = "{$" + key + "$}";
                }
            }
            else
            {
                ret = "{$NULL$}";
            }
            return ret;
        }

        /// <summary>
        /// Get self and children recursive
        /// </summary>
        /// <param name="parent">Parent control</param>
        /// <returns>All children recursive</returns>
        public static IEnumerable<Control> GetSelfAndChildrenRecursive(Control parent)
        {
            List<Control> ret = new List<Control>();
            if (parent != null)
            {
                foreach (Control child in parent.Controls)
                {
                    ret.AddRange(GetSelfAndChildrenRecursive(child));
                }
                ret.Add(parent);
            }
            return ret;
        }

        /// <summary>
        /// Get all strip menu items recursive
        /// </summary>
        /// <param name="parent">Parent tool strip menu item</param>
        /// <returns>All tool strip menu items children recursive</returns>
        public static IEnumerable<ToolStripMenuItem> GetAllToolStripMenuItemsRecursive(ToolStripMenuItem parent)
        {
            List<ToolStripMenuItem> ret = new List<ToolStripMenuItem>();
            if (parent != null)
            {
                foreach (ToolStripItem child in parent.DropDownItems)
                {
                    if (child is ToolStripMenuItem)
                    {
                        ret.AddRange(GetAllToolStripMenuItemsRecursive((ToolStripMenuItem)child));
                    }
                }
                ret.Add(parent);
            }
            return ret;
        }

        /// <summary>
        /// Get all tool strip menu items recursive
        /// </summary>
        /// <param name="parent">Parent context menu strip</param>
        /// <returns>All tool strip menu item children recursive</returns>
        public static IEnumerable<ToolStripMenuItem> GetAllToolStripMenuItemsRecursive(ContextMenuStrip parent)
        {
            List<ToolStripMenuItem> ret = new List<ToolStripMenuItem>();
            if (parent != null)
            {
                foreach (ToolStripItem child in parent.Items)
                {
                    if (child is ToolStripMenuItem)
                    {
                        ret.AddRange(GetAllToolStripMenuItemsRecursive((ToolStripMenuItem)child));
                    }
                }
            }
            return ret;
        }

        /// <summary>
        /// Enumerator to combo box
        /// </summary>
        /// <typeparam name="T">Enumerator type</typeparam>
        /// <param name="comboBox">Combo box</param>
        public static void EnumToComboBox<T>(ComboBox comboBox)
        {
            EnumToComboBox<T>(comboBox, null);
        }

        /// <summary>
        /// Enumerator to combo box
        /// </summary>
        /// <typeparam name="T">Enumerator type</typeparam>
        /// <param name="comboBox">Combo box</param>
        /// <param name="exclusions">Exclusions</param>
        public static void EnumToComboBox<T>(ComboBox comboBox, T[] exclusions)
        {
            if (comboBox != null)
            {
                comboBox.Items.Clear();
                Array arr = Enum.GetValues(typeof(T));
                foreach (var e in arr)
                {
                    bool s = true;
                    if (exclusions != null)
                    {
                        foreach (var ex in exclusions)
                        {
                            if (ex.Equals(e))
                            {
                                s = false;
                                break;
                            }
                        }
                    }
                    if (s)
                    {
                        comboBox.Items.Add(e);
                    }
                }
            }
        }

        /// <summary>
        /// Enumerable to combo box
        /// </summary>
        /// <typeparam name="T">Type to enumerate</typeparam>
        /// <param name="comboBox">Combo box</param>
        /// <param name="items">Items</param>
        public static void EnumerableToComboBox<T>(ComboBox comboBox, IEnumerable<T> items)
        {
            if (comboBox != null)
            {
                comboBox.Items.Clear();
                foreach (var item in items)
                {
                    comboBox.Items.Add(item);
                }
            }
        }
    }
}
