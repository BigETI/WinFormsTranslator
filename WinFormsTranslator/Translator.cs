using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Resources;
using System.Windows.Forms;

/// <summary>
/// Windows forms translator namespace
/// </summary>
namespace WinFormsTranslator
{
    /// <summary>
    /// Translator class
    /// </summary>
    public static class Translator
    {
        /// <summary>
        /// Language resource manager
        /// </summary>
        private static ResourceManager languageResourceManager;

        /// <summary>
        /// Fallback language resource manager
        /// </summary>
        private static ResourceManager fallbackLanguageResourceManager;

        /// <summary>
        /// Translator interface
        /// </summary>
        private static ITranslatorInterface translatorInterface;

        /// <summary>
        /// Translator interface
        /// </summary>
        public static ITranslatorInterface TranslatorInterface
        {
            get
            {
                return translatorInterface;
            }
            set
            {
                if (value != null)
                {
                    translatorInterface = value;
                }
            }
        }

        /// <summary>
        /// Initialize language
        /// </summary>
        private static void InitLanguage()
        {
            if (translatorInterface != null)
            {
                if (languageResourceManager == null)
                {
                    try
                    {
                        Assembly a = Assembly.Load(translatorInterface.AssemblyName);
                        languageResourceManager = new ResourceManager(translatorInterface.AssemblyName + ".Languages." + translatorInterface.Language, a);
                    }
                    catch (Exception e)
                    {
                        Console.Error.WriteLine(e.Message);
                    }
                }
                if (fallbackLanguageResourceManager == null)
                {
                    try
                    {
                        Assembly a = Assembly.Load(translatorInterface.AssemblyName);
                        fallbackLanguageResourceManager = new ResourceManager(translatorInterface.AssemblyName + ".Languages." + translatorInterface.FallbackLanguage, a);
                    }
                    catch (Exception e)
                    {
                        Console.Error.WriteLine(e.Message);
                    }
                }
            }
        }

        /// <summary>
        /// Try translatie
        /// </summary>
        /// <param name="input">Input</param>
        /// <param name="output">Output</param>
        /// <returns>Success</returns>
        public static bool TryTranslate(string input, out string output)
        {
            bool ret = false;
            output = input;
            if (input.StartsWith("{$") && input.EndsWith("$}") && (input.Length > 4))
            {
                output = GetTranslation(input.Substring(2, input.Length - 4));
                ret = (input != output);
            }
            return ret;
        }

        /// <summary>
        /// Load language
        /// </summary>
        /// <param name="parent">Parent control</param>
        private static void LoadLanguage(Control parent)
        {
            try
            {
                string translated = "";
                InitLanguage();
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
        public static string GetTranslation(string key)
        {
            string ret = null;
            if (languageResourceManager != null)
            {
                try
                {
                    ret = languageResourceManager.GetString(key);
                }
                catch (Exception e)
                {
                    Console.Error.WriteLine(e.Message);
                }
                if (ret == null)
                {
                    if (fallbackLanguageResourceManager != null)
                    {
                        try
                        {
                            ret = fallbackLanguageResourceManager.GetString(key);
                        }
                        catch (Exception e)
                        {
                            Console.Error.WriteLine(e.Message);
                        }
                    }
                }
            }
            else if (fallbackLanguageResourceManager != null)
            {
                try
                {
                    ret = fallbackLanguageResourceManager.GetString(key);
                }
                catch (Exception e)
                {
                    Console.Error.WriteLine(e.Message);
                }
            }
            if (ret == null)
            {
                ret = "{$" + key + "$}";
            }
            return ret;
        }

        /// <summary>
        /// Change language
        /// </summary>
        /// <param name="language">Language</param>
        /// <returns>Success</returns>
        public static bool ChangeLanguage(Language language)
        {
            bool ret = false;
            if (translatorInterface.Language != language.Culture)
            {
                translatorInterface.Language = language.Culture;
                translatorInterface.SaveSettings();
                ret = true;
            }
            return ret;
        }

        /// <summary>
        /// Load translation
        /// </summary>
        /// <param name="parent">Parent control</param>
        public static void LoadTranslation(Control parent)
        {
            if (translatorInterface != null)
            {
                LoadLanguage(parent);
            }
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
        /// <param name="exclusions">Exclusions</param>
        public static void EnumToComboBox<T>(ComboBox comboBox, T[] exclusions = null)
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

        /// <summary>
        /// Enumerable to combo box
        /// </summary>
        /// <typeparam name="T">Type to enumerate</typeparam>
        /// <param name="comboBox">Combo box</param>
        /// <param name="items">Items</param>
        public static void EnumerableToComboBox<T>(ComboBox comboBox, IEnumerable<T> items)
        {
            comboBox.Items.Clear();
            foreach (var item in items)
            {
                comboBox.Items.Add(item);
            }
        }
    }
}
