using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using UnityEngine;

namespace VolumeControl
{
    public static class Patches
    {
        //Creature lists for ease of use
        public static List<Reefback> reefbackList = new List<Reefback>();
        public static List<GasoPod> gasopodList = new List<GasoPod>();
        public static List<Stalker> stalkerList = new List<Stalker>();
        public static List<SandShark> sandsharkList = new List<SandShark>();
        public static List<CrabSnake> crabsnakeList = new List<CrabSnake>();
        public static List<GhostRay> ghostrayList = new List<GhostRay>();

        //Base module lists for ease of use
        public static List<MapRoomFunctionality> mapRoomList = new List<MapRoomFunctionality>();
        public static List<Charger> chargerList = new List<Charger>();
        public static List<BaseNuclearReactorGeometry> nuclearReactorList = new List<BaseNuclearReactorGeometry>();
        public static List<BaseFiltrationMachineGeometry> filtrationMachineList = new List<BaseFiltrationMachineGeometry>();

        public static bool playerInBase;

        public static bool CheckIfValidCreature(Creature creature)
        {
            if (creature.GetComponent<Reefback>() || creature.GetComponent<GasoPod>() || creature.GetComponent<Stalker>() || creature.GetComponent<SandShark>() || creature.GetComponent<CrabSnake>() || creature.GetComponent<GhostRay>())
                return true;

            return false;
        }

        public static void AddCreatureToList(Creature creature)
        {
            switch (creature)
            {
                case Reefback reefie:
                    reefbackList.Add(reefie);
                    break;
                case GasoPod gassy:
                    gasopodList.Add(gassy);
                    break;
                case Stalker stalky:
                    stalkerList.Add(stalky);
                    break;
                case SandShark sharky:
                    sandsharkList.Add(sharky);
                    break;
                case CrabSnake snaky:
                    crabsnakeList.Add(snaky);
                    break;
                case GhostRay ghosty:
                    ghostrayList.Add(ghosty);
                    break;
            }
        }

        [HarmonyPatch(typeof(Creature), nameof(Creature.Start))]
        public static class CreatureStart_Patch
        {
            [HarmonyPostfix]
            public static void Postfix(Creature __instance)
            {
                if (CheckIfValidCreature(__instance))
                {
                    AddCreatureToList(__instance);
                    Main.ChangeCreatureVolume(__instance);
                }
            }
        }

        //My first transpiler woo
        //PlayEnvSound is called for a lot of creatures and creature related stuff, so this is going to inject VolumeControl.Main.GetVolume(base.name) into it
        //Thankfully it already has an internal volume float with a default value of 1f, so this shouldn't affect anything that isn't included in this mod
        [HarmonyPatch]
        public static class PlayEnvSound_Patch
        {
            [HarmonyTranspiler]
            [HarmonyPatch(typeof(Utils))]
            [HarmonyPatch("PlayEnvSound", new Type[] { typeof(FMOD_StudioEventEmitter), typeof(Vector3), typeof(float) })]
            public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                var matcher = new CodeMatcher(instructions).MatchForward(false, new CodeMatch(OpCodes.Ldc_R4, 1f));

                if (!matcher.IsValid)
                {
                    Main.logger.LogError("PlayEnvSound_Patch match failed!");
                    return instructions;
                }

                matcher.Advance(3);
                matcher.InsertAndAdvance(new CodeInstruction(OpCodes.Callvirt, typeof(UnityEngine.Object).GetMethod("get_name")));
                matcher.InsertAndAdvance(new CodeInstruction(OpCodes.Call, typeof(Main).GetMethod(nameof(Main.GetVolume))));
                matcher.InsertAndAdvance(new CodeInstruction(OpCodes.Stloc_1, null));
                matcher.Insert(new CodeInstruction(OpCodes.Ldarg_0, null));

                return matcher.InstructionEnumeration();
            }
        }

        //Some creatures, especially annoying as hell Sandsharks, insist on calling StartEvent multiple times based on creature actions
        //This can sometime release and make a new EventInstance for the emitter I think, so I'm forced to patch this in to make sure the new EventInstance also changes volume
        [HarmonyPatch(typeof(FMOD_StudioEventEmitter), nameof(FMOD_StudioEventEmitter.StartEvent))]
        public static class StudioEventEmitterStartEvent_Patch
        {
            [HarmonyPostfix]
            public static void Postfix(FMOD_StudioEventEmitter __instance)
            {
                if (__instance.GetComponentInParent<Reefback>() || __instance.GetComponentInParent<GasoPod>() || __instance.GetComponentInParent<Stalker>() || __instance.GetComponentInParent<SandShark>() || __instance.GetComponentInParent<CrabSnake>() || __instance.GetComponentInParent<GhostRay>())
                {
                    __instance.evt.getVolume(out float curvol);
                    if (curvol != Main.GetVolume(__instance.name))
                    {
                        __instance.evt.setVolume(Main.GetVolume(__instance.name));
                    }
                }
            }
        }

        //Another inspiration from my predecessor
        //Being able to easily tell when the player enters and exits the base is nice and this seemed like a simple way to do it
        [HarmonyPatch(typeof(Player), "UpdateIsUnderwater")]
        public static class PlayerUpdateIsUnderwater_Patch
        {
            [HarmonyPostfix]
            private static void Postfix(Player __instance)
            {
                if (playerInBase != __instance.IsInBase())
                {
                    playerInBase = __instance.IsInBase();
                    Main.UpdateBaseModulesVolume();
                    Main.UpdateCreaturesVolume();
                }
            }
        }

        //v1.1.0 additions below, extending to base stuff, but thankfully ultra insanely easier than dealing with the horrific creature mess, these all mostly have one emitter and they behave
        [HarmonyPatch(typeof(BaseFiltrationMachineGeometry), nameof(BaseFiltrationMachineGeometry.Start))]
        public static class BaseFiltrationMachineStart_Patch
        {
            [HarmonyPostfix]
            private static void Postfix(BaseFiltrationMachineGeometry __instance)
            {
                filtrationMachineList.Add(__instance);
                Main.UpdateBaseModulesVolume();
            }
        }

        [HarmonyPatch(typeof(BaseNuclearReactorGeometry), nameof(BaseNuclearReactorGeometry.Start))]
        public static class BaseNuclearReactorStart_Patch
        {
            [HarmonyPostfix]
            private static void Postfix(BaseNuclearReactorGeometry __instance)
            {
                nuclearReactorList.Add(__instance);
                Main.UpdateBaseModulesVolume();
            }
        }

        //The only one that calls StartEvent, so I'm better off patching this to prevent EventInstance desync
        //Turns out, no, that's an awful idea because it destroys framerates, this will do
        [HarmonyPatch(typeof(Charger), nameof(Charger.Start))]
        public static class ChargerStart_Patch
        {
            [HarmonyPostfix]
            private static void Postfix(Charger __instance)
            {
                if (__instance.GetComponent<BatteryCharger>() || __instance.GetComponent<PowerCellCharger>())
                {
                    chargerList.Add(__instance);
                    Main.UpdateBaseModulesVolume();
                }

            }
        }

        [HarmonyPatch(typeof(MapRoomFunctionality), nameof(MapRoomFunctionality.Start))]
        public static class MapRoomFunctionalityStart_Patch
        {
            [HarmonyPostfix]
            private static void Postfix(MapRoomFunctionality __instance)
            {
                mapRoomList.Add(__instance);
                Main.UpdateBaseModulesVolume();
            }
        }
    }
}
