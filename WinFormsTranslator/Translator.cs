using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Resources;
using System.Windows.Forms;

namespace WinFormsTranslator
{
    public class Translator
    {
        private static ResourceManager languageResourceManager = null;

        private static ITranslatorInterface translatorInterface;

        public static ITranslatorInterface TranslatorInterface
        {
            get
            {
                return translatorInterface;
            }
            set
            {
                if (value != null)
                    translatorInterface = value;
            }
        }

        private static void InitLanguage()
        {
            if (languageResourceManager == null)
            {
                try
                {
                    CultureInfo ci = new CultureInfo(translatorInterface.Language);
                    Assembly a = Assembly.Load(translatorInterface.AssemblyName);
                    languageResourceManager = new ResourceManager(translatorInterface.AssemblyName + ".Languages." + translatorInterface.Language, a);
                }
                catch (Exception e)
                {
                    Console.Error.WriteLine(e.Message);
                }
            }
        }

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

        private static void LoadLanguage(Control parent)
        {
            try
            {
                string translated = "";
                InitLanguage();
                ToolStripSeparator s;
                IEnumerable<Control> controls = GetSelfAndChildrenRecursive(parent);
                foreach (Control c in controls)
                {
                    if (TryTranslate(c.Text, out translated))
                        c.Text = translated;
                    IEnumerable<ToolStripMenuItem> tsmis = GetAllToolStripMenuItemsRecursive(c.ContextMenuStrip);
                    foreach (ToolStripMenuItem tsmi in tsmis)
                    {
                        if (TryTranslate(tsmi.Text, out translated))
                            tsmi.Text = translated;
                    }
                    if (c is ComboBox)
                    {
                        ComboBox cb = (ComboBox)c;
                        for (int i = 0; i < cb.Items.Count; i++)
                        {
                            if (TryTranslate((string)cb.Items[i], out translated))
                                cb.Items[i] = translated;
                        }
                    }
                    else if (c is ListView)
                    {
                        ListView lv = (ListView)c;
                        foreach (ColumnHeader col in lv.Columns)
                        {
                            if (TryTranslate(col.Text, out translated))
                                col.Text = translated;
                        }
                        foreach (ListViewGroup grp in lv.Groups)
                        {
                            if (TryTranslate(grp.Header, out translated))
                                grp.Header = translated;
                        }
                    }
                    else if (c is ToolStrip)
                    {
                        foreach (ToolStripItem tsi in ((ToolStrip)c).Items)
                        {
                            if (TryTranslate(tsi.Text, out translated))
                                tsi.Text = translated;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.Error.WriteLine(e.Message);
            }
        }

        public static string GetTranslation(string key)
        {
            string ret = "{$" + key + "$}";
            if (languageResourceManager != null)
            {
                try
                {
                    ret = languageResourceManager.GetString(key);
                    if (ret == null)
                        ret = "{$" + key + "$}";
                }
                catch (Exception e)
                {
                    Console.Error.WriteLine(e.Message);
                }
            }
            return ret;
        }

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

        public static void LoadTranslation(Control parent)
        {
            if (translatorInterface != null)
                LoadLanguage(parent);
        }

        public static IEnumerable<Control> GetSelfAndChildrenRecursive(Control parent)
        {
            List<Control> ret = new List<Control>();
            if (parent != null)
            {
                foreach (Control child in parent.Controls)
                    ret.AddRange(GetSelfAndChildrenRecursive(child));
                ret.Add(parent);
            }
            return ret;
        }

        public static IEnumerable<ToolStripMenuItem> GetAllToolStripMenuItemsRecursive(ToolStripMenuItem parent)
        {
            List<ToolStripMenuItem> ret = new List<ToolStripMenuItem>();
            if (parent != null)
            {
                foreach (ToolStripItem child in parent.DropDownItems)
                {
                    if (child is ToolStripMenuItem)
                        ret.AddRange(GetAllToolStripMenuItemsRecursive((ToolStripMenuItem)child));
                }
                ret.Add(parent);
            }
            return ret;
        }

        public static IEnumerable<ToolStripMenuItem> GetAllToolStripMenuItemsRecursive(ContextMenuStrip parent)
        {
            List<ToolStripMenuItem> ret = new List<ToolStripMenuItem>();
            if (parent != null)
            {
                foreach (ToolStripItem child in parent.Items)
                {
                    if (child is ToolStripMenuItem)
                        ret.AddRange(GetAllToolStripMenuItemsRecursive((ToolStripMenuItem)child));
                }
            }
            return ret;
        }

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
                    comboBox.Items.Add(e);
            }
        }

        public static void EnumerableToComboBox<T>(ComboBox comboBox, IEnumerable<T> arr)
        {
            comboBox.Items.Clear();
            foreach (var e in arr)
                comboBox.Items.Add(e);
        }
    }
}
