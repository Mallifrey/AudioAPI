using System;

using LabApi.Loader.Features.Plugins;

namespace AudioAPI
{
    public class AudioPlugin : Plugin<AudioConfig>
    {
        public AudioPlugin()
            => Plugin = this;
        
        public static AudioPlugin Plugin { get; private set; }
        public static AudioConfig Config => (Plugin as Plugin<AudioConfig>).Config;
        
        public override string Name { get; } = "Audio API";
        public override string Author { get; } = "marchellcx";
        public override string Description { get; } = "Provides audio functionality for other plugins.";

        public override Version Version { get; } = new Version(1, 0, 0);
        public override Version RequiredApiVersion { get; } = null;

        public override void Enable() => AudioManager.Init();
        public override void Disable() { }
    }
}