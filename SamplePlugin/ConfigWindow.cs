using System;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Windowing;

namespace WhoPulled;

internal sealed class ConfigWindow : Window, IDisposable
{
    private readonly Configuration configuration;

    internal ConfigWindow(Configuration configuration)
        : base("Who Pulled Configuration")
    {
        this.configuration = configuration;
        this.Size = new System.Numerics.Vector2(280, 150);
    }

    public void Dispose()
    {
    }

    public override void Draw()
    {
        var trackBRanks = this.configuration.TrackBRanks;
        if (ImGui.Checkbox("B ranks", ref trackBRanks))
        {
            this.configuration.TrackBRanks = trackBRanks;
            this.configuration.Save();
        }

        var trackARanks = this.configuration.TrackARanks;
        if (ImGui.Checkbox("A ranks", ref trackARanks))
        {
            this.configuration.TrackARanks = trackARanks;
            this.configuration.Save();
        }

        var trackSRanks = this.configuration.TrackSRanks;
        if (ImGui.Checkbox("S ranks", ref trackSRanks))
        {
            this.configuration.TrackSRanks = trackSRanks;
            this.configuration.Save();
        }
    }
}
