using SMLHelper.V2.Json;
using SMLHelper.V2.Options;
using SMLHelper.V2.Options.Attributes;

namespace VolumeControl
{
    public class Mod
    {
        public const string GUID = "space.soleil.volumecontrol";
        public const string Name = "Volume Control";
        public const string Version = "1.0.0";

        [Menu(Mod.Name)]
        public class Options : ConfigFile
        {
            public Options() : base("config") { }

            [Slider("Creature Volume In Base", 0f, 1f, Format = "{0:0%}", DefaultValue = 1f, Id = "creatureBaseVolume", Step = 0.01f, Tooltip = "This only applies to the below creatures and it applies on top of their individual sliders.")]
            public float creatureBaseVolumePercent = 1f;

            [Slider("Reefback Volume", 0f, 1f, DefaultValue = 1f, Id = "reefbackVolume", Step = 0.01f, Format = "{0:0%}"), OnChange(nameof(ChangeSettings))]
            public float reefbackVolume = 1f;

            [Slider("Gasopod Volume", 0f, 1f, DefaultValue = 1f, Id = "gasopodVolume", Step = 0.01f, Format = "{0:0%}"), OnChange(nameof(ChangeSettings))]
            public float gasopodVolume = 1f;            
            
            [Slider("Stalker Volume", 0f, 1f, DefaultValue = 1f, Id = "stalkerVolume", Step = 0.01f, Format = "{0:0%}"), OnChange(nameof(ChangeSettings))]
            public float stalkerVolume = 1f;

            [Slider("Sandshark Volume", 0f, 1f, DefaultValue = 1f, Id = "sandsharkVolume", Step = 0.01f, Format = "{0:0%}"), OnChange(nameof(ChangeSettings))]
            public float sandsharkVolume = 1f;

            [Slider("Crabsnake Volume", 0f, 1f, DefaultValue = 1f, Id = "crabsnakeVolume", Step = 0.01f, Format = "{0:0%}"), OnChange(nameof(ChangeSettings))]
            public float crabsnakeVolume = 1f;
            
            [Slider("Ghost Ray Volume", 0f, 1f, DefaultValue = 1f, Id = "ghostrayVolume", Step = 0.01f, Format = "{0:0%}"), OnChange(nameof(ChangeSettings))]
            public float ghostrayVolume = 1f;


            public void ChangeSettings(SliderChangedEventArgs e)
            {
                //I don't think this is neccessary, but I like it so it stays
                if (MainGameController.instance != null)
                {
                    Main.UpdateVolume();
                }
            }
        }
    }
}
