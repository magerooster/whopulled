using System;
using System.Collections.Generic;
using System.Linq;
using Dalamud.Game.ClientState.Objects.Enums;
using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Game.Chat;
using Dalamud.Plugin.Services;
using Lumina.Excel.Sheets;

namespace WhoPulled;

internal sealed class PullTracker : IDisposable
{
    private readonly IChatGui chatGui;
    private readonly IFramework framework;
    private readonly IObjectTable objectTable;
    private readonly IDataManager dataManager;
    private readonly Configuration configuration;
    private readonly IPluginLog log;
    private readonly Dictionary<ulong, bool> combatStates = [];

    private Candidate? candidate;

    internal PullTracker(
        IChatGui chatGui,
        IFramework framework,
        IObjectTable objectTable,
        IDataManager dataManager,
        Configuration configuration,
        IPluginLog log)
    {
        this.chatGui = chatGui;
        this.framework = framework;
        this.objectTable = objectTable;
        this.dataManager = dataManager;
        this.configuration = configuration;
        this.log = log;

        this.chatGui.LogMessage += this.OnLogMessage;
        this.framework.Update += this.OnUpdate;
    }

    public void Dispose()
    {
        this.chatGui.LogMessage -= this.OnLogMessage;
        this.framework.Update -= this.OnUpdate;
    }

    private void OnLogMessage(ILogMessage message)
    {
        if (this.candidate is not null || message.SourceEntity is not { IsPlayer: true } source ||
            message.TargetEntity is not { IsPlayer: false } target)
        {
            return;
        }

        if (!this.TryFindBattleNpc(target.Name.ToString(), out var battleNpc) ||
            !this.IsEnabledRank(battleNpc!))
        {
            return;
        }

        this.candidate = new Candidate(battleNpc!.GameObjectId, source.Name.ToString(), target.Name.ToString(), DateTime.UtcNow.AddSeconds(10));
    }

    private void OnUpdate(IFramework _)
    {
        foreach (var battleNpc in this.objectTable.OfType<IBattleNpc>())
        {
            var isInCombat = battleNpc.StatusFlags.HasFlag(StatusFlags.InCombat);
            var wasInCombat = this.combatStates.GetValueOrDefault(battleNpc.GameObjectId);
            this.combatStates[battleNpc.GameObjectId] = isInCombat;

            if (this.candidate is { } pending && pending.TargetId == battleNpc.GameObjectId &&
                !wasInCombat && isInCombat && battleNpc.TargetObject is IPlayerCharacter)
            {
                this.chatGui.Print($"[Who Pulled] {pending.PlayerName} pulled {pending.TargetName}.");
                this.log.Information("{Player} pulled {Target}.", pending.PlayerName, pending.TargetName);
                this.Reset();
            }
        }

        if (this.candidate is not { } candidate)
        {
            return;
        }

        if (DateTime.UtcNow > candidate.ExpiresAt)
        {
            this.Reset();
            return;
        }

        this.Reset();
    }

    private bool TryFindBattleNpc(string name, out IBattleNpc? battleNpc)
    {
        battleNpc = this.objectTable
            .OfType<IBattleNpc>()
            .FirstOrDefault(actor => actor.ObjectKind == ObjectKind.BattleNpc &&
                                     actor.CurrentDistance <= 100 &&
                                     actor.Name.ToString() == name);
        return battleNpc is not null;
    }

    private bool IsEnabledRank(IBattleNpc battleNpc)
    {
        if (!this.dataManager.GetExcelSheet<BNpcBase>().TryGetRow(battleNpc.BaseId, out var data))
        {
            return false;
        }

        return data.Rank switch
        {
            1 => this.configuration.TrackBRanks,
            2 => this.configuration.TrackARanks,
            3 => this.configuration.TrackSRanks,
            _ => false,
        };
    }

    private void Reset()
    {
        this.candidate = null;
    }

    private sealed record Candidate(ulong TargetId, string PlayerName, string TargetName, DateTime ExpiresAt);
}
