using SimpleLocalization.Localization;

namespace SimpleLocalization
{
    /// <summary>
    /// Contains main settings of application
    /// </summary>
    public class MainSettings
    {
        private static MainSettings _instance;

        public static MainSettings Instance => _instance ?? (_instance = new MainSettings());

        /// <summary>
        /// Current app language
        /// </summary>
        public LanguageEnum Language { get; set; }

    }
}
