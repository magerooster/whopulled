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
        this.Size = new System.Numerics.Vector2(460, 300);
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

        var trackSSRanks = this.configuration.TrackSSRanks;
        if (ImGui.Checkbox("SS ranks / minions", ref trackSSRanks))
        {
            this.configuration.TrackSSRanks = trackSSRanks;
            this.configuration.Save();
        }

        ImGui.Separator();
        ImGui.Text("Output format");
        var outputFormat = this.configuration.OutputFormat;
        if (ImGui.InputText("##output-format", ref outputFormat, 512))
        {
            this.configuration.OutputFormat = outputFormat;
            this.configuration.Save();
        }

        var defaultColorKey = (int)this.configuration.DefaultColorKey;
        if (ImGui.InputInt("Default color key", ref defaultColorKey))
        {
            this.configuration.DefaultColorKey = (ushort)Math.Clamp(defaultColorKey, 0, ushort.MaxValue);
            this.configuration.Save();
        }

        ImGui.TextWrapped("Placeholders: {Player}, {Target}, {Rank}. Leave the default color key at 0 for normal chat colors.");
        ImGui.TextWrapped("Colors: {color:500}colored text{/color}. Color keys are FFXIV UI color IDs; 500 is the standard Dalamud gold used by plugin messages.");
        ImGui.TextWrapped("Unknown pullers use Player = Unknown player. Color tags can be placed anywhere in the format.");
    }
}
