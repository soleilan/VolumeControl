using System.Collections.Generic;
using HarmonyLib;

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
}
