using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using SMLHelper.V2.Handlers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace VolumeControl
{
    [BepInPlugin(Mod.GUID, Mod.Name, Mod.Version)]
    public class Main : BaseUnityPlugin
    {
        public static Mod.Options SMLConfig { get; } = OptionsPanelHandler.RegisterModOptions<Mod.Options>();
        public static ManualLogSource logger = new ManualLogSource(Mod.Name);

        private void Awake()
        {
            try
            {
                logger = Logger;
                Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly(), Mod.GUID);
                logger.LogInfo(Mod.Name + " " + Mod.Version + " successfully patched!");
            }
            catch (Exception e)
            {
                logger.LogError(Mod.Name + " " + e.InnerException.Message);
                logger.LogError(e.StackTrace);
            }
        }

        public static void ChangeCreatureVolume(Creature creature)
        {
            if (creature != null)
            {
                switch (creature)
                {
                    //These only have CustomLoopingEmitters for their sounds, so we'll only touch those.
                    case Reefback _:
                    case GasoPod _:
                    case GhostRay _:
                        ChangeCustomEmittersOfCreature(creature);
                        break;

                    //These are the bastards. They use multiple types of emitters, they reset event instances, they even have seperate game objects that hold their sounds. Bastards.
                    case Stalker _:
                    case SandShark _:
                    case CrabSnake _:
                        ChangeCustomEmittersOfCreature(creature);
                        ChangeStudioEmittersOfCreature(creature);
                        break;

                }
            }
        }

        public static void ChangeCustomEmittersOfCreature(Creature creature)
        {
            foreach (FMOD_CustomEmitter emitter in creature.GetAllComponentsInChildren<FMOD_CustomEmitter>())
            {
                if (!emitter.evt.isValid())
                    emitter.CacheEventInstance();

                if (emitter.name == creature.name || emitter.name == "Mouth")
                {
                    emitter.evt.getVolume(out float currentvol);

                    if (currentvol != GetVolume(creature.name))
                    {
                        emitter.evt.setVolume(GetVolume(creature.name));
                    }
                }
            }
        }

        public static void ChangeStudioEmittersOfCreature(Creature creature)
        {
            foreach (FMOD_StudioEventEmitter emitter in creature.GetAllComponentsInChildren<FMOD_StudioEventEmitter>())
            {
                if (!emitter.evt.isValid())
                    emitter.CacheEventInstance();

                if (emitter.name == creature.name)
                {
                    emitter.evt.getVolume(out float currentvol);

                    if (currentvol != GetVolume(creature.name))
                    {
                        emitter.evt.setVolume(GetVolume(creature.name));
                    }
                }
            }
        }

        //Simple float method to grab respective volumes
        //I'll leave a list of all the assets that are played by creatures at the bottom of this page, but the short story is that CrabSnakes play some sound through a Mouth object
        //and GasoPods play a pod_release sound via GasPods, plus Reefback babies
        //Reefbacks also have some plants and peepers that get spawned alongside them, on their back, so I'm hoping I've stopped those from being affected
        public static float GetVolume(string name)
        {
            float volume = 1f;
            switch (name)
            {
                case "ReefbackBaby(Clone)":
                case "Reefback(Clone)":
                    volume = SMLConfig.reefbackVolume;
                    break;

                case "GasPod(Clone)":
                case "Gasopod(Clone)":
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

            if (Patches.playerInBase)
                volume *= SMLConfig.creatureBaseVolumePercent;

            return volume;
        }

        //Moved to more lists, and I didn't want to write out a foreach loop for each list, so...
        //Forgive me
        public static void UpdateCreaturesVolume()
        {
            List<Creature> creatures = new List<Creature>();
            foreach (var creature in creatures.Concat(Patches.reefbackList).Concat(Patches.gasopodList).Concat(Patches.stalkerList).Concat(Patches.sandsharkList).Concat(Patches.crabsnakeList).Concat(Patches.ghostrayList))
            {
                if (creature != null && creature.enabled)
                {
                    Main.ChangeCreatureVolume(creature);
                }
            }
        }

        //This is lazy and I don't like it, but it works
        public static void UpdateBaseModulesVolume()
        {
            foreach (BaseFiltrationMachineGeometry filtrationMachine in Patches.filtrationMachineList.ToList<BaseFiltrationMachineGeometry>())
            {
                if (filtrationMachine != null)
                {
                    filtrationMachine.workSound.evt.getVolume(out float curvol);
                    if (curvol != SMLConfig.filtrationmachineVolume)
                    {
                        filtrationMachine.workSound.evt.setVolume(SMLConfig.filtrationmachineVolume);
                    }
                }
                else
                {
                    Patches.filtrationMachineList.Remove(filtrationMachine);
                }
            }
            foreach (BaseNuclearReactorGeometry nuclearReactor in Patches.nuclearReactorList.ToList<BaseNuclearReactorGeometry>())
            {
                if (nuclearReactor != null)
                {
                    nuclearReactor.workSound.evt.getVolume(out float curvol);
                    if (curvol != SMLConfig.nuclearreactorVolume)
                    {
                        nuclearReactor.workSound.evt.setVolume(SMLConfig.nuclearreactorVolume);
                    }
                }
                else
                {
                    Patches.nuclearReactorList.Remove(nuclearReactor);
                }
            }
            foreach (Charger charger in Patches.chargerList.ToList<Charger>())
            {
                if (charger != null)
                {
                    charger.soundCharge.evt.getVolume(out float curvol);
                    if (curvol != SMLConfig.chargerVolume)
                    {
                        charger.soundCharge.evt.setVolume(SMLConfig.chargerVolume);
                    }
                }
                else
                {
                    Patches.chargerList.Remove(charger);
                }
            }
            foreach (MapRoomFunctionality mapRoom in Patches.mapRoomList.ToList<MapRoomFunctionality>())
            {
                if (mapRoom != null)
                {
                    mapRoom.ambientSound.evt.getVolume(out float curvol);
                    if (curvol != SMLConfig.scannerroomVolume)
                    {
                        mapRoom.ambientSound.evt.setVolume(SMLConfig.scannerroomVolume);
                    }
                }
                else
                {
                    Patches.mapRoomList.Remove(mapRoom);
                }
            }
        }
    }
}
