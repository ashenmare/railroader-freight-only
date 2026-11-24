using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using HarmonyLib;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Railroader.Modding;

namespace FreightOnly
{
    internal sealed class Settings
    {
        // On: the railroad's reputation never counts passengers, so running them is optional. Off: the game decides, as
        // vanilla (passengers count once two connected stations are open). Default on: installing the mod is the choice.
        public bool freightOnly = true;
    }

    // Railroader 0.6 entry point. Host-side: the reputation is worked out once a game day on the host only
    // (ReputationTracker subscribes TimeDayDidChange under StateManager.IsHost) and stored in the replicated "_reputation"
    // object, so clients see the host's result without the mod.
    public sealed class Main : IModEntryPoint
    {
        private const string HarmonyId = "ashenmare.freight-only";
        internal static IModLogger Log;
        internal static Settings Settings = new Settings();
        private static readonly HashSet<string> Logged = new HashSet<string>();
        private Harmony _harmony;

        internal static void LogOnce(string key, string text) { if (Logged.Add(key)) Log?.Info(text); }

        public void OnModLoad(IModContext context)
        {
            Log = context.Log;
            string settingsPath = Path.Combine(context.ModDirectory, "settings.json");
            Settings = LoadSettings(settingsPath);
            Logged.Clear();
            _harmony = new Harmony(HarmonyId);
            try
            {
                _harmony.CreateClassProcessor(typeof(PassengerScore)).Patch();
                if (!PassengerScore.Hooked) return;   // Prepare() has said why
                Log.Info(Settings.freightOnly
                    ? "Freight Only loaded: the reputation counts freight and safety only (the game's own freight-only weights, 60% and 40%)"
                    : "Freight Only loaded but off: passengers count toward the reputation as in the base game");
            }
            catch (Exception e)
            {
                PassengerScore.Hooked = false;   // the prefix lets the original run if any of it went on
                Log.Warning("Freight Only: couldn't hook the reputation, so passengers count as in the base game: " + PathClean.CleanText(e.Message));
                try { SettingsUi.Stop(); } catch (Exception) { }
                return;
            }
            try { SettingsUi.Start(_harmony, settingsPath); }
            catch (Exception e)
            {
                Log.Warning("Freight Only: the setting isn't in Preferences > Mods on this game version (the mod still works): " + PathClean.CleanText(e.Message));
                try { SettingsUi.Stop(); } catch (Exception) { }
            }
        }

        // 0.1.2: the off state first (a prefix still on lets the game decide), then the settings section, then the unpatch
        // in its own try, so a throwing unpatch never leaves the mod forcing freight only after it's unticked.
        public void OnModUnload()
        {
            PassengerScore.Hooked = false;
            try { SettingsUi.Stop(); } catch (Exception) { }
            try { _harmony?.UnpatchAll(HarmonyId); } catch (Exception) { }
            _harmony = null;
        }

        // After a save in Preferences > Mods: the same read as at load, from the file just written.
        internal static void Applied(JObject saved)
        {
            Settings = saved.ToObject<Settings>() ?? new Settings();
            Logged.Remove("day");
            Log?.Info(Settings.freightOnly
                ? "Freight Only: on; from the next daily reputation update, passengers don't count"
                : "Freight Only: off; from the next daily reputation update, passengers count as in the base game");
        }

        private static Settings LoadSettings(string path)
        {
            try
            {
                if (File.Exists(path)) return JsonConvert.DeserializeObject<Settings>(File.ReadAllText(path)) ?? new Settings();
            }
            catch (Exception e) { Log?.Warning("Freight Only: couldn't read settings.json, using the default (on): " + PathClean.CleanText(e.Message)); return new Settings(); }
            // No file yet: write one with the default, so players can find it. Never in the zip, so an update can't
            // overwrite someone's choice; an existing file (even an unreadable one) is never touched.
            Settings d = new Settings();
            try
            {
                string tmp = path + ".tmp";
                File.WriteAllText(tmp, JsonConvert.SerializeObject(d, Formatting.Indented));
                File.Move(tmp, path);
                Log?.Info("Freight Only: wrote settings.json with the default (freightOnly true)");
            }
            catch (Exception e)
            {
                Log?.Warning("Freight Only: couldn't write a default settings.json (the default applies anyway): " + PathClean.CleanText(e.Message));
                try { File.Delete(path + ".tmp"); } catch (Exception) { }
            }
            return d;
        }
    }

    // Everything typed by SettingsSection.cs (and so by the game's UI) lives here, off Main, behind NoInlining methods
    // called inside a try, so a game update that breaks the section can't take the reputation patch with it (the house
    // pattern, as in Beaten Path).
    internal static class SettingsUi
    {
        private static object _section;   // a SettingsSection

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void Start(Harmony harmony, string settingsPath)
        {
            Stop();
            var fields = new[]
            {
                SettingField.Bool("freightOnly", "Freight only", true, true,
                    "Passengers don't count toward the reputation. Applies at the next daily update."),
            };
            _section = new SettingsSection("Freight Only", settingsPath, fields, Main.Applied, m => Main.Log?.Warning("Freight Only: " + m),
                "In multiplayer only the host's setting counts.");
            PreferencesModsTab.Add(harmony, b => (_section as SettingsSection)?.Build(b, true), m => Main.Log?.Warning("Freight Only: " + m));
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void Stop()
        {
            PreferencesModsTab.Remove();
            _section = null;
        }
    }
}
