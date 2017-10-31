using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using SimpleLocalization.Localization;

namespace SimpleLocalization
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        public MainWindow()
        {
            InitializeComponent();
            LanguageComboBox.ItemsSource = Enum.GetNames(typeof(LanguageEnum)).ToList();
            LanguageComboBox.SelectedIndex = (int)MainSettings.Instance.Language;
            LanguageComboBox.SelectionChanged += LanguageComboBox_OnSelectionChanged;
        }

        private void LanguageComboBox_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string lang = LanguageComboBox.SelectedValue.ToString();
            
            if (Enum.TryParse(lang, out LanguageEnum newLang))
            {
                MainSettings.Instance.Language = newLang;
                if (Application.Current.TryFindResource("StringLocalizer") is StringLocalizer loc)
                {
                    loc.UpdateStrings();
                }
            }
        }
    }
}
