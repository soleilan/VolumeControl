using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;

namespace VolumeControl.Patches
{

    //Got the idea from doing this from my predecessor, Deathtruth, and his Silent Base Ambience mod
    //Seems like a good idea still, having a list of the offenders that you can easily iterate through on leaving/entering base, or changing settings
    [HarmonyPatch(typeof(Creature), "Start")]
    public static class CreatureStart_Patch
    {
        public static List<Creature> creatureList = new List<Creature>();

        [HarmonyPostfix]
        private static void Postfix(Creature __instance)
        {
            if(__instance is Reefback || __instance is GasoPod || __instance is Stalker || __instance is SandShark || __instance is CrabSnake || __instance is GhostRay)
            {
                creatureList.Add(__instance);
                Main.UpdateVolume();
            }
        }
    }

    //Might be a good idea
    //v1.1.0 addition
    [HarmonyPatch(typeof(Creature),nameof(Creature.OnDestroy))]
    public static class CreatureOnDestroy_Patch
    {
        [HarmonyPrefix]
        private static void Prefix(Creature __instance)
        {
            if(CreatureStart_Patch.creatureList.Contains(__instance))
            {
                CreatureStart_Patch.creatureList.Remove(__instance);
            }
        }
    }

    //GasPods call PlayEnvWorld which passes no volume, and Stalkers, Sandsharks and Crabsnakes all have their "attack" and "pain" and "death" sounds played through this path as well
    //This is the lazy way I dealt with it
    [HarmonyPatch(typeof(FMOD_StudioEventEmitter),nameof(FMOD_StudioEventEmitter.PlayOneShotNoWorld))]
    public static class StudioEventEmitterPlayOneShotNoWorld_Patch
    {
        [HarmonyPrefix]
        public static void Prefix(FMOD_StudioEventEmitter __instance, ref float volume)
        {
            if(__instance.asset.name != "pod_burst")
            {
                if (__instance.name == "Reefback(Clone)" || __instance.name == "Gasopod(Clone)" || __instance.name == "GasPod(Clone)" || __instance.name == "Stalker(Clone)" || __instance.name == "SandShark(Clone)" || __instance.name == "CrabSnake(Clone)" || __instance.name == "GhostRayBlue(Clone)")
                    volume = Main.GetVolume(__instance.name);
            }
        }
    }

    //Some creatures, especially annoying as hell Sandsharks, insist on calling StartEvent multiple times based on creature actions
    //This can sometime release and make a new EventInstance for the emitter, so I'm forced to patch this in to make sure the new EventInstance also changes volume
    [HarmonyPatch(typeof(FMOD_StudioEventEmitter),nameof(FMOD_StudioEventEmitter.StartEvent))]
    public static class StudioEventEmitterStartEvent_Patch
    {
        [HarmonyPostfix]
        public static void Postfix(FMOD_StudioEventEmitter __instance)
        {
            Main.ChangeVolume(__instance.name, __instance.evt);
        }
    }

    //I'll be fully honest, I'm not sure if this is needed, but I'll leave it in for now
    //If there's any performance issues, this'll be the first suspect
    [HarmonyPatch(typeof(FMOD_CustomEmitter),nameof(FMOD_CustomEmitter.OnPlay))]
    public static class CustomEmitterOnPlay_Patch
    {
        [HarmonyPostfix]
        public static void Postfix(FMOD_CustomEmitter __instance)
        {
            Main.ChangeVolume(__instance.name, __instance.evt);
        }
    }

    //Another inspiration from my predecessor
    //Being able to easily tell when the player enters and exits the base is nice and this seemed like a simple way to do it
    [HarmonyPatch(typeof(Player),"UpdateIsUnderwater")]
    public static class PlayerUpdateIsUnderwater_Patch
    {
        public static bool playerInBase;

        [HarmonyPostfix]
        private static void Postfix(Player __instance)
        {
            if(playerInBase != __instance.IsInBase())
            {
                playerInBase = __instance.IsInBase();
                Main.UpdateVolume();
            }
        }
    }

    //v1.1.0 additions below, extending to base stuff, but thankfully ultra insanely easier than dealing with the horrific creature mess, these all mostly have one emitter and they behave
    [HarmonyPatch(typeof(BaseFiltrationMachineGeometry), nameof(BaseFiltrationMachineGeometry.Start))]
    public static class BaseFiltrationMachineStart_Patch
    {
        public static List<BaseFiltrationMachineGeometry> filtrationMachineList = new List<BaseFiltrationMachineGeometry>();

        [HarmonyPostfix]
        private static void Postfix(BaseFiltrationMachineGeometry __instance)
        {
            filtrationMachineList.Add(__instance);
            Main.UpdateVolume();
        }
    }

    [HarmonyPatch(typeof(BaseNuclearReactorGeometry), nameof(BaseNuclearReactorGeometry.Start))]
    public static class BaseNuclearReactorStart_Patch
    {
        public static List<BaseNuclearReactorGeometry> nuclearReactorList = new List<BaseNuclearReactorGeometry>();

        [HarmonyPostfix]
        private static void Postfix(BaseNuclearReactorGeometry __instance)
        {
            nuclearReactorList.Add(__instance);
            Main.UpdateVolume();
        }
    }

    //The only one that calls StartEvent, so I'm better off patching this to prevent EventInstance desync
    [HarmonyPatch(typeof(Charger), nameof(Charger.ToggleChargeSound))]
    public static class ChargerStart_Patch
    {
        public static List<Charger> chargerList = new List<Charger>();

        [HarmonyPostfix]
        private static void Postfix(Charger __instance)
        {
            chargerList.Add(__instance);
            Main.UpdateVolume();
        }
    }

    [HarmonyPatch(typeof(MapRoomFunctionality), nameof(MapRoomFunctionality.Start))]
    public static class MapRoomFunctionalityStart_Patch
    {
        public static List<MapRoomFunctionality> mapRoomList = new List<MapRoomFunctionality>();

        [HarmonyPostfix]
        private static void Postfix(MapRoomFunctionality __instance)
        {
            mapRoomList.Add(__instance);
            Main.UpdateVolume();
        }
    }
}
