using System.ComponentModel;

namespace AudioAPI
{
    public class AudioConfig
    {
        [Description("Directory that contains all audio files.")]
        public string Directory { get; set; } = string.Empty;

        [Description("How many audio handlers to preconstruct for poolable audio.")]
        public int AudioPoolSize { get; set; } = 10;
    }
}