using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Windows;
using System.Xml.Linq;
using SimpleLocalization.Annotations;

namespace SimpleLocalization.Localization
{
    /// <summary>
    /// Controls localization process.
    /// </summary>
    public class StringLocalizer : INotifyPropertyChanged
    {
        private LocalizedStrings _localizedStrings;
        private CultureInfo _currentCultureInfo;

        private readonly Dictionary<string, string> _languageDictionary = new Dictionary<string, string>
        {
            {LanguageEnum.English.ToString(), "en"},
            {LanguageEnum.Russian.ToString(), "ru"},
            {LanguageEnum.Belarusian.ToString(), "be"},
        };

        /// <summary>
        /// ctor
        /// </summary>
        public StringLocalizer()
        {
#if DEBUG
            GenerateNewXmlResources();
#endif
        }

        public LocalizedStrings Strings => _localizedStrings ?? (_localizedStrings =
            (bool)DesignerProperties.IsInDesignModeProperty.GetMetadata(typeof(DependencyObject)).DefaultValue
                ? new LocalizedStrings() : LoadStrings());

        /// <summary>
        /// Updates current application localized strings and refreshes it in UI
        /// </summary>
        public void UpdateStrings()
        {
            _localizedStrings = null;
            OnPropertyChanged(nameof(Strings));
        }

        private CultureInfo CurrentCultureInfo
        {
            get => _currentCultureInfo;
            set
            {
                _currentCultureInfo = value;
                if (Application.Current.MainWindow != null)
                {
                    Thread.CurrentThread.CurrentCulture = _currentCultureInfo;
                    Thread.CurrentThread.CurrentUICulture = _currentCultureInfo;
                }
            }
        }

        /// <summary>
        /// Loads localized strings from localized files according to language from settings
        /// </summary>
        /// <returns></returns>
        private LocalizedStrings LoadStrings()
        {
            var localStrings = new LocalizedStrings();
            Type localType = localStrings.GetType();
            var props = localType.GetProperties();
            
            if (_languageDictionary.TryGetValue(MainSettings.Instance.Language.ToString(), out string lang))
            {
                CurrentCultureInfo = CultureInfo.GetCultureInfoByIetfLanguageTag(lang);
            }

            string languagePath = Path.Combine(MainConstants.LanguageDirectory,
                $"{CurrentCultureInfo.TwoLetterISOLanguageName}.xml");
            if (File.Exists(languagePath))
            {
                var xmlDoc = XDocument.Load(languagePath);
                foreach (PropertyInfo info in props)
                {
                    if (info.PropertyType != typeof(string))
                    {
                        if (Debugger.IsAttached)
                        {
                            Debug.WriteLine("\n\tOnly string properties are supported", "Exception");
                            Debugger.Break();
                        }
                        continue;
                    }

                    if (!info.CanWrite)
                    {
                        if (Debugger.IsAttached)
                        {
                            Debug.WriteLine($"\n\tProperty must be read-write: {localType} -> {info.Name}", "Exception");
                            Debugger.Break();
                        }
                        continue;
                    }

                    var element = xmlDoc.Root?.Descendants("string").FirstOrDefault(p =>
                        (string)p.Attribute("key") == info.Name);
                    if (element != null)
                    {
                        string value = (string)element.Attribute("value");
                        info.SetValue(localStrings, value);
                    }
                }
            }
            return localStrings;
        }

        /// <summary>
        /// Generates new localized strings in localized files
        /// </summary>
        private void GenerateNewXmlResources()
        {
            var localStrings = new LocalizedStrings();
            Type localType = localStrings.GetType();
            var props = localType.GetProperties();
            int startIndex =
                AppDomain.CurrentDomain.BaseDirectory.IndexOf("bin", StringComparison.InvariantCulture);
            if (startIndex < 1)
            {
                return;
            }
            string basePath = AppDomain.CurrentDomain.BaseDirectory.Remove(startIndex);
            foreach (var lang in _languageDictionary)
            {
                string languagePath = Path.Combine(MainConstants.LanguageDirectory,
                    $"{lang.Value}.xml");
                string appPath = Path.Combine(basePath, languagePath);
                if (File.Exists(appPath))
                {
                    var xmlDoc = XDocument.Load(languagePath);
                    bool isChangedDoc = false;
                    foreach (PropertyInfo info in props)
                    {
                        if (info.PropertyType != typeof(string))
                        {
                            if (Debugger.IsAttached)
                            {
                                Debug.WriteLine("\n\tOnly string properties are supported", "Exception");
                                Debugger.Break();
                            }
                            continue;
                        }

                        if (!info.CanWrite)
                        {
                            if (Debugger.IsAttached)
                            {
                                Debug.WriteLine($"\n\tProperty must be read-write: {localType} -> {info.Name}", "Exception");
                                Debugger.Break();
                            }
                            continue;
                        }

                        var element = xmlDoc.Root?.Descendants("string").FirstOrDefault(p =>
                            (string)p.Attribute("key") == info.Name);
                        if (element == null)
                        {
                            xmlDoc.Root?.Add(
                                new XElement("string",
                                    new XAttribute("key", info.Name),
                                    new XAttribute("value", info.GetValue(localStrings))));

                            if (!isChangedDoc) isChangedDoc = true;
                        }
                    }
                    if (isChangedDoc)
                    {
                        xmlDoc.Save(appPath, SaveOptions.None);
                    }
                }
            }
        }
        #region INotifyPropertyChanged implementation

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }
}
