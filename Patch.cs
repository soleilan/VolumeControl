using System.Collections.Generic;
using HarmonyLib;

namespace VolumeControl.Patches
{
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

    [HarmonyPatch(typeof(FMOD_StudioEventEmitter),nameof(FMOD_StudioEventEmitter.StartEvent))]
    public static class StudioEventEmitterStartEvent_Patch
    {
        [HarmonyPostfix]
        public static void Postfix(FMOD_StudioEventEmitter __instance)
        {
            Main.ChangeVolume(__instance.name, __instance.evt);
        }
    }

    [HarmonyPatch(typeof(FMOD_CustomEmitter),nameof(FMOD_CustomEmitter.OnPlay))]
    public static class CustomEmitterOnPlay_Patch
    {
        [HarmonyPostfix]
        public static void Postfix(FMOD_CustomEmitter __instance)
        {
            Main.ChangeVolume(__instance.name, __instance.evt);
        }
    }

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
}
