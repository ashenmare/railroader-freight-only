using System;
using System.Reflection;
using HarmonyLib;

namespace FreightOnly
{
    // The whole mod. Railroader 0.6.2 works out the reputation once a game day on the host
    // (Game.Reputation.ReputationTracker.UpdateReputation). It asks the private static
    // ShouldIncludePassengerScore() (true when any open, reachable passenger stop has an open, reachable
    // neighbor). True: Passenger Network Service 30% + Passenger Equipment Condition 10% + Freight Performance 40% +
    // Safety 30%. False: Freight Performance 60% + Safety 40%, the game's own weights for a railroad without passenger
    // service. With Freight Only on, the answer is always false, so passengers never count and running them is optional.
    //
    // ShouldIncludePassengerScore is 42 bytes of IL (code size 0x2a, 0.6.2) with a lambda: over Mono's default inline
    // limit of 20, so the prefix holds. Its only caller is UpdateReputation (:175). Off, the prefix lets the original run.
    [HarmonyPatch]
    internal static class PassengerScore
    {
        private const string TrackerType = "Game.Reputation.ReputationTracker";
        // True from Prepare (the target is here) until OnModUnload, which clears it before it unpatches (0.1.2): while it's
        // false a prefix still on lets the original run.
        internal static bool Hooked { get; set; }

        private static bool Prepare()
        {
            Hooked = TargetMethod() != null;
            if (!Hooked) Main.Log?.Warning("Freight Only: ReputationTracker.ShouldIncludePassengerScore() isn't on this game version, so passengers count as in the base game");
            return Hooked;
        }

        private static MethodBase TargetMethod()
        {
            Type t = AccessTools.TypeByName(TrackerType);
            MethodInfo m = t == null ? null : AccessTools.Method(t, "ShouldIncludePassengerScore", Type.EmptyTypes);
            return m != null && m.IsStatic && m.ReturnType == typeof(bool) ? m : null;
        }

        private static bool Prefix(ref bool __result)
        {
            try
            {
                if (!Hooked || !Main.Settings.freightOnly) return true;
                __result = false;
                Main.LogOnce("day", "Freight Only: today's reputation counts freight (60%) and safety (40%), not passengers");
                return false;
            }
            catch (Exception) { return true; }   // never break the game's daily update: let the original decide
        }
    }
}
