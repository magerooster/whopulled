using System;
using Dalamud.Configuration;

namespace WhoPulled;

[Serializable]
public sealed class Configuration : IPluginConfiguration
{
    public int Version { get; set; } = 1;

    public bool TrackBRanks { get; set; } = true;
    public bool TrackARanks { get; set; } = true;
    public bool TrackSRanks { get; set; } = true;

    public void Save()
    {
        Plugin.PluginInterface.SavePluginConfig(this);
    }
}
