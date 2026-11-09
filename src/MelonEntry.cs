using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using MelonLoader;
using UnityEngine;

[assembly: MelonInfo(typeof(LongGoneDaysCheats.CheatMod), "Long Gone Days Cheat Menu", "1.1.2", "LGD Cheat Menu")]
[assembly: MelonGame("This I Dreamt", "Long Gone Days")]

namespace LongGoneDaysCheats
{
    public sealed class CheatMod : MelonMod
    {
        static MelonPreferences_Category settings;
        static MelonPreferences_Entry<KeyCode> key;
        static MelonPreferences_Entry<bool> ctrl, alt, shift;
        public static string ShortcutLabel
        { get { return (ctrl.Value ? "Ctrl+" : "") + (alt.Value ? "Alt+" : "") + (shift.Value ? "Shift+" : "") + key.Value; } }
        public static bool ShortcutPressed()
        {
            return Input.GetKeyDown(key.Value)
                && ctrl.Value == (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))
                && alt.Value == (Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt))
                && shift.Value == (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift));
        }
        public static void Bind(KeyCode value, bool control, bool alternate, bool shifted)
        {
            key.Value = value; ctrl.Value = control; alt.Value = alternate; shift.Value = shifted;
            settings.SaveToFile();
        }
        public override void OnInitializeMelon()
        {
            settings = MelonPreferences.CreateCategory("LongGoneDaysCheatMenu");
            key = settings.CreateEntry("ToggleKey", KeyCode.C);
            ctrl = settings.CreateEntry("RequireCtrl", true);
            alt = settings.CreateEntry("RequireAlt", false);
            shift = settings.CreateEntry("RequireShift", false);
            if (key.Value == KeyCode.None || key.Value == KeyCode.Escape) Bind(KeyCode.C, true, false, false);
            // MelonLoader discovers the HarmonyPatch classes in this assembly automatically.
            LoggerInstance.Msg("Standalone overlay ready. Shortcut: " + ShortcutLabel);
        }
        public override void OnLateInitializeMelon() { CheatMenu.Initialize(); }
        public override void OnSceneWasLoaded(int buildIndex, string sceneName) { CheatMenu.Initialize(); }
        public override void OnDeinitializeMelon() { CheatMenu.Shutdown(); }
    }

    [HarmonyPatch(typeof(InputController))]
    static class InputButtons
    {
        static IEnumerable<MethodBase> TargetMethods()
        {
            foreach (var name in new[] { "GetButtonDown", "GetButtonUp", "GetButtonPressed" }) yield return AccessTools.Method(typeof(InputController), name);
        }
        static bool Prefix(ref bool __result) { if (!CheatMenu.IsOpen) return true; __result = false; return false; }
    }
    [HarmonyPatch(typeof(InputController))]
    static class InputAxes
    {
        static IEnumerable<MethodBase> TargetMethods()
        {
            yield return AccessTools.Method(typeof(InputController), "HorizontalAxis");
            yield return AccessTools.Method(typeof(InputController), "VerticalAxis");
        }
        static bool Prefix(ref float __result) { if (!CheatMenu.IsOpen) return true; __result = 0; return false; }
    }
    [HarmonyPatch(typeof(Stats), "GetMaxStatAtLevel")]
    static class MaximumStats
    {
        static void Postfix(Stats __instance, string statId, ref int __result) { __result = CheatMenu.MaxStat(__result, __instance, statId); }
    }
    [HarmonyPatch(typeof(Stats), MethodType.Constructor, new Type[] { typeof(Actor) })]
    static class CopyStats
    {
        static void Postfix(Stats __instance, Actor actor) { CheatMenu.CopyBaseStats(__instance, actor); }
    }
    [HarmonyPatch(typeof(BattleActorAlly), MethodType.Constructor, new Type[] { typeof(Actor), typeof(ActorUI) })]
    static class BattleStats
    {
        static void Postfix(BattleActorAlly __instance, Actor actor) { CheatMenu.PrepareBattle(__instance, actor); }
    }
    [HarmonyPatch(typeof(SaveData), "InitializeData")]
    static class ResetOnLoad
    {
        static void Postfix() { CheatMenu.ResetExploration(); }
    }
    [HarmonyPatch(typeof(EntityAgent), "TryMoveToDirection")]
    static class NoclipMovement
    {
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            int replacements = 0;
            foreach (var instruction in instructions)
            {
                if (instruction.Calls(AccessTools.PropertyGetter(typeof(Tile), "Walkable")))
                {
                    // Preserve branch labels at the start of the replacement sequence.
                    instruction.opcode = OpCodes.Ldarg_0; instruction.operand = null;
                    yield return instruction;
                    yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(CheatMenu), "Walkable"));
                    replacements++;
                }
                else
                {
                    yield return instruction;
                    if (instruction.Calls(AccessTools.Method(typeof(EntityAgent), "CanMoveToTile")))
                    {
                        yield return new CodeInstruction(OpCodes.Ldarg_0);
                        yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(CheatMenu), "MovementCheck"));
                        replacements++;
                    }
                }
            }
            if (replacements != 2) throw new InvalidOperationException("Unrecognized movement code; noclip patch was not applied.");
        }
    }
}
