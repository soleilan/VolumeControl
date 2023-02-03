using BepInEx;
using BepInEx.Logging;
using FMOD.Studio;
using HarmonyLib;
using SMLHelper.V2.Handlers;
using System;
using System.Linq;
using System.Reflection;
using UnityEngine;
using VolumeControl.Patches;

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
        
        //It's not pretty and there's some overlapping, but I really don't want to affect the wrong things or degrade performance, even if I probably do
        //Basically just making sure its an object I want to change the volume of and its not already been changed
        public static void ChangeVolume(string name, EventInstance evt)
        {
            //Yeah, I know, stupid long if statement. Only use it twice, so I didn't feel like making a method for it. It works at least
            if (name == "Reefback(Clone)" || name == "ReefbackBaby(Clone)" || name == "Gasopod(Clone)" || name == "GasPod(Clone)" || name == "Stalker(Clone)" || name == "SandShark(Clone)" || name == "Mouth" || name == "Mouth(Clone)" || name == "CrabSnake(Clone)" || name == "GhostRayBlue(Clone)")
            {
                evt.getVolume(out float currentvolume);

                if (currentvolume != GetVolume(name))
                {
                    evt.setVolume(GetVolume(name));
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

        //This is what gets called when you enter/exit your base, or change the mod settings
        //Not the most elegant way, as I believe it also inadvertently affects the plants on the back of Reefbacks, including Peepers and Biters that get spawned along with it
        //But this is a lazy mod and I'm lazy, plus I hoped ChangeVolume would avoid affecting them
        public static void UpdateVolume()
        {
            foreach (Creature creature in CreatureStart_Patch.creatureList.ToList<Creature>())
            {
                if (creature != null)
                {
                    foreach (FMOD_CustomEmitter emitter in creature.GetAllComponentsInChildren<FMOD_CustomEmitter>().ToList<FMOD_CustomEmitter>())
                    {
                        ChangeVolume(emitter.name, emitter.evt);
                    }
                    foreach (FMOD_StudioEventEmitter emitter in creature.GetAllComponentsInChildren<FMOD_StudioEventEmitter>().ToList<FMOD_StudioEventEmitter>())
                    {
                        ChangeVolume(emitter.name, emitter.evt);
                    }
                }
                else
                {
                    CreatureStart_Patch.creatureList.Remove(creature);
                }
            }
            //Full list of all assets played by the creatures, and which emitters I affect by doing the above method:
            //The ones that have volume: 1 are offenders that reset their volume somehow, most likely through StartEvent

            //sandshark
            //[Info: Volume Control] Emitter check!SandShark(Clone) | attack(FMODAsset) | volume: 1 | SandShark(Clone)(FMOD_CustomEmitter)
            //[Info: Volume Control] Emitter check!SandShark(Clone) | idle(FMODAsset) | volume: 1 | SandShark(Clone)(FMOD_CustomLoopingEmitter)
            //[Info: Volume Control] Emitter check!SandShark(Clone) | move_sand(FMODAsset) | volume: 1 | SandShark(Clone)(FMOD_CustomLoopingEmitter)
            //[Info: Volume Control] Emitter check!SandShark(Clone) | idle(FMODAsset) | volume: 1 | SandShark(Clone)(FMOD_CustomLoopingEmitterWithCallback)
            //[Info: Volume Control] Emitter check!SandShark(Clone) | burrow(FMODAsset) | volume: 0 | SandShark(Clone)(FMOD_StudioEventEmitter)
            //[Info: Volume Control] Emitter check!SandShark(Clone) | bite(FMODAsset) | volume: 0 | SandShark(Clone)(FMOD_StudioEventEmitter)
            //[Info: Volume Control] Emitter check!SandShark(Clone) | alert(FMODAsset) | volume: 0 | SandShark(Clone)(FMOD_StudioEventEmitter)
            //[Info: Volume Control] Emitter check!SandShark(Clone) | death(FMODAsset) | volume: 0 | SandShark(Clone)(FMOD_StudioEventEmitter)
            //[Info: Volume Control] Emitter check!SandShark(Clone) | pain(FMODAsset) | volume: 0 | SandShark(Clone)(FMOD_StudioEventEmitter

            //stalker
            //[Info: Volume Control] Emitter check! Stalker(Clone) | charge(FMODAsset) | volume: 1 | Stalker(Clone)(FMOD_CustomEmitter)
            //[Info: Volume Control] Emitter check! Stalker(Clone) | roar(FMODAsset) | volume: 1 | Stalker(Clone)(FMOD_CustomLoopingEmitter)
            //[Info: Volume Control] Emitter check! Stalker(Clone) | roar(FMODAsset) | volume: 1 | Stalker(Clone)(FMOD_CustomLoopingEmitterWithCallback)
            //[Info: Volume Control] Emitter check! Stalker(Clone) | wound(FMODAsset) | volume: 0 | Stalker(Clone)(FMOD_StudioEventEmitter)
            //[Info: Volume Control] Emitter check! Stalker(Clone) | bite(FMODAsset) | volume: 0 | Stalker(Clone)(FMOD_StudioEventEmitter)

            //ghostrayblue
            //[Info: Volume Control] Emitter check! GhostRayBlue(Clone) | idle(FMODAsset) | volume: 0 | GhostRayBlue(Clone)(FMOD_CustomLoopingEmitter)

            //crabsnake
            //[Info: Volume Control] Emitter check! Mouth | idle_swim(FMODAsset) | volume: 1 | Mouth(FMOD_CustomLoopingEmitter)
            //[Info: Volume Control] Emitter check! CrabSnake(Clone) | attack_cine(FMODAsset) | volume: 0 | CrabSnake(Clone)(FMOD_StudioEventEmitter)
            //[Info: Volume Control] Emitter check! CrabSnake(Clone) | attack(FMODAsset) | volume: 0 | CrabSnake(Clone)(FMOD_StudioEventEmitter)
            //[Info: Volume Control] Emitter check! Mouth | alert(FMODAsset) | volume: 1 | Mouth(FMOD_StudioEventEmitter)

            //gasopod
            //[Info: Volume Control] Emitter check! Gasopod(Clone) | idle(FMODAsset) | volume: 0 | Gasopod(Clone)(FMOD_CustomLoopingEmitter)
            //[Info: Volume Control] Emitter check! Gasopod(Clone) | idle(FMODAsset) | volume: 0 | Gasopod(Clone)(FMOD_CustomLoopingEmitterWithCallback)

            //reefback
            //[Info: Volume Control] Emitter check! Reefback(Clone) | idle(FMODAsset) | volume: 0 | Reefback(Clone)(FMOD_CustomLoopingEmitter)
            //[Info: Volume Control] Emitter check! Reefback(Clone) | idle(FMODAsset) | volume: 0 | Reefback(Clone)(FMOD_CustomLoopingEmitterWithCallback)
            //[Info: Volume Control] Emitter check! Coral_reef_purple_mushrooms_01_04(Clone) | shroom_in(FMODAsset) | volume: 0 | Coral_reef_purple_mushrooms_01_04(Clone)(FMOD_StudioEventEmitter)
            //[Info: Volume Control] Emitter check! Coral_reef_purple_mushrooms_01_04(Clone) | shroom_out(FMODAsset) | volume: 0 | Coral_reef_purple_mushrooms_01_04(Clone)(FMOD_StudioEventEmitter)
            //[Info: Volume Control] Emitter check! Peeper(Clone) | chirp(FMODAsset) | volume: 0 | Peeper(Clone)(FMOD_StudioEventEmitter)

            //Simple stuff below for v.1.1.0
            //I got some errors: InvalidOperationException: Collection was modified; enumeration operation may not execute.
            //Most likely due to my decision to remove stuff from lists mid ForEach loop, but I really don't want players to end up with 400 entries in a long game session
            //Hopefully with .ToList there won't be any issues and it'll work smoothly!
            foreach (BaseFiltrationMachineGeometry filtrationMachine in BaseFiltrationMachineStart_Patch.filtrationMachineList.ToList<BaseFiltrationMachineGeometry>())
            {
                if(filtrationMachine!= null)
                {
                    filtrationMachine.workSound.evt.setVolume(SMLConfig.filtrationmachineVolume);
                }
                else
                {
                    BaseFiltrationMachineStart_Patch.filtrationMachineList.Remove(filtrationMachine);
                }
            }
            foreach(BaseNuclearReactorGeometry nuclearReactor in BaseNuclearReactorStart_Patch.nuclearReactorList.ToList<BaseNuclearReactorGeometry>())
            {
                if(nuclearReactor!= null)
                {
                    nuclearReactor.workSound.evt.setVolume(SMLConfig.nuclearreactorVolume);
                }
                else
                {
                    BaseNuclearReactorStart_Patch.nuclearReactorList.Remove(nuclearReactor);
                }
            }
            foreach(Charger charger in ChargerStart_Patch.chargerList.ToList<Charger>())
            {
                if(charger!= null)
                {
                    charger.soundCharge.evt.setVolume(SMLConfig.chargerVolume);
                }
                else
                {
                    ChargerStart_Patch.chargerList.Remove(charger);
                }
            }
            foreach(MapRoomFunctionality mapRoom in MapRoomFunctionalityStart_Patch.mapRoomList.ToList<MapRoomFunctionality>())
            {
                if(mapRoom!= null)
                {
                    mapRoom.ambientSound.evt.setVolume(SMLConfig.scannerroomVolume);
                }
                else
                {
                    MapRoomFunctionalityStart_Patch.mapRoomList.Remove(mapRoom);
                }
            }
        }
    }
}
