using BepInEx;
using BepInEx.Logging;
using FMOD.Studio;
using HarmonyLib;
using SMLHelper.V2.Handlers;
using System;
using System.Reflection;
using VolumeControl.Patches;

namespace VolumeControl
{
    [BepInPlugin(Mod.GUID, Mod.Name, Mod.Version)]
    public class Main : BaseUnityPlugin
    {
        public static Mod.Options SMLConfig { get; } = OptionsPanelHandler.RegisterModOptions<Mod.Options>();
        public static ManualLogSource logger = new ManualLogSource(Mod.Name);

        private const string helloworld = "Hi, if you're decompiling this, please don't be mad at me. " +
            "I'm a complete amateur with immense anxiety issues, just writing this took a week of no sleep due to anxiety and impostor syndrome. " +
            "Towards the last day, I just.. couldn't work anymore due to anxiety issues. It was either abandon the project or release it in this state, so here it is. " +
            "If you think you could do X or Y better/more elegantly/less atrociously, you're welcome to fork the mod, copy the mod, or just make your own! " +
            "I'm sure I made mistakes, broke coding rules and destroyed your game performance with my implementations. " +
            "The plan originally with the mod was to expand it for base stuff as well. Volume sliders for scanner room, filtration machine, nuclear reactor and so on, but well.. yeah" +
            "I 50% did this for personal use, and 50% as an exercise and to learn more modding. " +
            "It's 02:28 AM, I really just want to get this over with and forget about it at this point. ";

        private void Awake()
        {
            try
            {
                Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly(), Mod.GUID);
                logger.LogInfo(Mod.Name + " " + Mod.Version + " successfully patched!");
                logger = Logger;
            }
            catch (Exception e)
            {
                logger.LogError(Mod.Name + " " + e.InnerException.Message);
                logger.LogError(e.StackTrace);
            }
        }
        
        public static void ChangeVolume(string name, EventInstance evt)
        {
            if (name == "Reefback(Clone)" || name == "Gasopod(Clone)" || name == "GasPod(Clone)" || name == "Stalker(Clone)" || name == "SandShark(Clone)" || name == "CrabSnake(Clone)" || name == "GhostRayBlue(Clone)")
            {
                evt.getVolume(out float currentvolume);

                if (currentvolume != GetVolume(name))
                {
                    evt.setVolume(GetVolume(name));
                }
            }
        }

        public static float GetVolume(string name)
        {
            float volume = 1f;
            switch (name)
            {
                case "ReefbackBaby(Clone)":
                case "Reefback(Clone)":
                    volume = SMLConfig.reefbackVolume;
                    break;

                case "Gasopod(Clone)":
                case "GasPod(Clone)":
                    volume = SMLConfig.gasopodVolume;
                    break;

                case "Stalker(Clone)":
                    volume = SMLConfig.stalkerVolume;
                    break;

                case "SandShark(Clone)":
                    volume = SMLConfig.sandsharkVolume;
                    break;

                case "Mouth":
                case "Mouth(Clone)":
                case "CrabSnake(Clone)":
                    volume = SMLConfig.crabsnakeVolume;
                    break;

                case "GhostRayBlue(Clone)":
                    volume = SMLConfig.ghostrayVolume;
                    break;
            }

            if (PlayerUpdateIsUnderwater_Patch.playerInBase)
                volume *= SMLConfig.creatureBaseVolumePercent;

            return volume;
        }

        public static void UpdateVolume()
        {
            foreach (Creature creature in CreatureStart_Patch.creatureList)
            {
                if (creature != null)
                {
                    foreach (FMOD_CustomEmitter emitter in creature.GetAllComponentsInChildren<FMOD_CustomEmitter>())
                    {
                        ChangeVolume(emitter.name, emitter.evt);
                    }
                    foreach (FMOD_StudioEventEmitter emitter in creature.GetAllComponentsInChildren<FMOD_StudioEventEmitter>())
                    {
                        ChangeVolume(emitter.name, emitter.evt);
                    }
                }

            }
        }
    }
}
