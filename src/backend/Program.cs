using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using Microsoft.Xna.Framework;

#if ANDROID
using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Views;
#else
using Steamworks;
#endif

namespace NonsensicalVideoGenerator
{
#if ANDROID
    [Activity(
        Label = "@string/app_name",
        MainLauncher = true,
        Icon = "@drawable/icon",
        AlwaysRetainTaskState = true,
        LaunchMode = LaunchMode.SingleInstance,
        ScreenOrientation = ScreenOrientation.FullUser,
        ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.Keyboard | ConfigChanges.KeyboardHidden | ConfigChanges.ScreenSize
    )]
    public class Program : AndroidGameActivity
#else
    static class Program
#endif
    {
#if ANDROID
        private UserInterface _game;
        private View _view;
        protected override void OnCreate(Bundle bundle)
#else
        [STAThread]
        static void Main(string[] args)
#endif
        {
#if ANDROID
            base.OnCreate(bundle);
#else
            // Launch options
            for (int i = 0; i < args.Length; i++)
            {
                Global.parameters.Add(args[i]);
            }
            if (Global.parameters.Contains("-console"))
            {
                string cwd = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? ".");
                string consoleTxt = Path.Combine(cwd, "console.txt");
                if (File.Exists(consoleTxt))
                {
                    ProcessStartInfo startInfo = new()
                    {
                        FileName = consoleTxt,
                        UseShellExecute = true
                    };
                    Process.Start(startInfo);
                    Environment.Exit(0);
                }
                else
                {
                    ConsoleOutput.Clear();
                }
            }
            else if (!Global.parameters.Contains("-keeplog"))
            {
                ConsoleOutput.Clear();
            }
            if (!Global.parameters.Contains("-nofrei0r"))
            {
                // Set FREI0R_PATH environment variable.
                Environment.SetEnvironmentVariable("FREI0R_PATH", Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? ".", "frei0r-1"), EnvironmentVariableTarget.User);
                ConsoleOutput.WriteLine("FREI0R_PATH: " + Environment.GetEnvironmentVariable("FREI0R_PATH", EnvironmentVariableTarget.User), Color.Transparent);
            }
#endif
            Global.randomSeed = Global.generator.globalRandom.Next();
            if (Global.parameters.Contains("-seed"))
            {
                int index = Global.parameters.IndexOf("-seed");
                if (index + 1 < Global.parameters.Count)
                {
                    if (int.TryParse(Global.parameters[index + 1], out int seed))
                    {
                        Global.randomSeed = seed;
                    }
                }
            }
            Global.generator.globalRandom = new Random(Global.randomSeed);
            SaveData.Load();
            if (Global.parameters.Contains("-v"))
            {
                SaveData.saveValues["HiddenVerbose"] = "true";
                SaveData.Save();
            }
            DisabledMedia.Load();
            if (Global.parameters.Count > 0)
                ConsoleOutput.WriteLine("Using command line parameters: " + String.Join(" ", Global.parameters.ToArray()));
#if DEBUG
            Debug.debugBuild = true;
            Debug.SetDebugMode(true);
#endif
#if !ANDROID
            // Initialize Steam
            try
            {
                SteamManager.Initialize();
            }
            catch (Exception ex)
            {
                ConsoleOutput.WriteLine("SteamManager failed to initialize: " + ex.Message, Color.Red);
            }
            if (Global.parameters.Contains("-debug"))
            {
                Debug.SetDebugMode(true);
            }
            if (Global.parameters.Contains("-locale"))
            {
                int index = Global.parameters.IndexOf("-locale");
                if (index + 1 < Global.parameters.Count)
                {
                    SaveData.saveValues["Locale"] = Global.parameters[index + 1];
                }
            }
            // Fix locales that had their names changed
            if (SaveData.saveValues["Locale"] == "en_us")
                SaveData.saveValues["Locale"] = "english";
            else if (SaveData.saveValues["Locale"] == "es_mx")
                SaveData.saveValues["Locale"] = "latam";
            else if (SaveData.saveValues["Locale"] == "de_de")
                SaveData.saveValues["Locale"] = "german";
            // Get language that Steam reports
            bool languageSet = false;
            if (SaveData.saveValues["Locale"] != "fixme")
                languageSet = true;
            if (SteamManager.initialized && !languageSet)
            {
                string steamLang = SteamApps.GetCurrentGameLanguage();
                if (steamLang != null)
                {
                    SaveData.saveValues["Locale"] = steamLang;
                    languageSet = true;
                }
            }
            // Default to English
            if (!languageSet)
#endif
                SaveData.saveValues["Locale"] = "english";
            L.ReloadLocales();
            if (Global.parameters.Contains("-intro"))
                SaveData.saveValues["FirstBoot"] = "true";
            if (Global.parameters.Contains("-fullscreen"))
                SaveData.saveValues["Fullscreen"] = "true";
            HolidayManager.CheckHolidays();
            Global.productVersion = (Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "0.0.0.0") + Global.productSku;
#if ANDROID
            _game = new UserInterface();
            _view = _game.Services.GetService(typeof(View)) as View;
            SetContentView(_view);
#else
            using (var _game = new UserInterface())
#endif
                _game.Run();
            // On graceful exit, save the game data
            SaveData.Save();
        }
    }
}
