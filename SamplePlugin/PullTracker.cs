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
    private readonly HashSet<ulong> announcedTargets = [];

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

        var now = DateTime.UtcNow;
        this.candidate = new Candidate(battleNpc!.GameObjectId, source.Name.ToString(), target.Name.ToString(), now, now.AddSeconds(10));
    }

    private void OnUpdate(IFramework _)
    {
        foreach (var battleNpc in this.objectTable.OfType<IBattleNpc>())
        {
            var isInCombat = battleNpc.StatusFlags.HasFlag(StatusFlags.InCombat);
            if (!this.combatStates.TryGetValue(battleNpc.GameObjectId, out var wasInCombat))
            {
                this.combatStates[battleNpc.GameObjectId] = isInCombat;
                continue;
            }

            this.combatStates[battleNpc.GameObjectId] = isInCombat;

            if (!isInCombat)
            {
                this.announcedTargets.Remove(battleNpc.GameObjectId);
            }

            if (wasInCombat || !isInCombat || !this.IsEnabledRank(battleNpc))
            {
                continue;
            }

            if (this.candidate is { } pending && pending.TargetId == battleNpc.GameObjectId &&
                !this.announcedTargets.Contains(battleNpc.GameObjectId) &&
                battleNpc.TargetObject is IPlayerCharacter target &&
                target.Name.ToString() == pending.PlayerName)
            {
                this.Announce(pending);
                continue;
            }

            this.AnnounceUnknown(battleNpc);
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

    }

    private void Announce(Candidate candidate)
    {
        this.chatGui.Print($"[Who Pulled] {candidate.PlayerName} pulled {candidate.TargetName}.");
        this.log.Information("{Player} pulled {Target}.", candidate.PlayerName, candidate.TargetName);
        this.announcedTargets.Add(candidate.TargetId);
        this.Reset();
    }

    private void AnnounceUnknown(IBattleNpc battleNpc)
    {
        var target = battleNpc.TargetObject is IPlayerCharacter player
            ? $" targeting {player.Name}"
            : string.Empty;

        this.chatGui.Print($"[Who Pulled] {battleNpc.Name} entered combat{target}, but the puller was not visible to your client.");
        this.log.Information("{Target} entered combat, but the puller was not visible to the client.", battleNpc.Name);
        this.announcedTargets.Add(battleNpc.GameObjectId);
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

    private sealed record Candidate(ulong TargetId, string PlayerName, string TargetName, DateTime CreatedAt, DateTime ExpiresAt);
}
