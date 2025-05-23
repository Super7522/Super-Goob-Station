// SPDX-FileCopyrightText: 2022 Kara <lunarautomaton6@gmail.com>
// SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Leon Friedrich <60421075+ElectroJr@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Nemanja <98561806+EmoGarbage404@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Pieter-Jan Briers <pieterjan.briers@gmail.com>
// SPDX-FileCopyrightText: 2023 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Baa <9057997+Baa14453@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Centronias <me@centronias.com>
// SPDX-FileCopyrightText: 2024 Sirionaut <148076704+Sirionaut@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Tayrtahn <tayrtahn@gmail.com>
// SPDX-FileCopyrightText: 2024 sirionaut <sirionaut@gmail.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aviu00 <93730715+Aviu00@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 SX-7 <92227810+SX-7@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Server.Actions;
using Content.Server.Animals.Components;
using Content.Server.Popups;
using Content.Shared.Actions.Events;
using Content.Shared.Mobs.Systems;
using Content.Shared.Nutrition.Components;
using Content.Shared.Nutrition.EntitySystems;
using Content.Shared.Storage;
using Robust.Server.Audio;
using Robust.Shared.Player;
using Robust.Shared.Random;
using Robust.Shared.Timing;

namespace Content.Server.Animals.Systems;

/// <summary>
///     Gives the ability to lay eggs/other things;
///     produces endlessly if the owner does not have a HungerComponent.
/// </summary>
public sealed class EggLayerSystem : EntitySystem
{
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly ActionsSystem _actions = default!;
    [Dependency] private readonly AudioSystem _audio = default!;
    [Dependency] private readonly HungerSystem _hunger = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly PopupSystem _popup = default!;
    [Dependency] private readonly MobStateSystem _mobState = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<EggLayerComponent, MapInitEvent>(OnMapInit);
        SubscribeLocalEvent<EggLayerComponent, EggLayInstantActionEvent>(OnEggLayAction);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);
        var query = EntityQueryEnumerator<EggLayerComponent>();
        var eligibleEggLayers = new List<Entity<EggLayerComponent>>(); // Goob - self-spawning
        while (query.MoveNext(out var uid, out var eggLayer))
        {
            // Goobstation - hard hunger requirement
            if (eggLayer.HungerRequired && !HasComp<HungerComponent>(uid))
                continue;

            // Players should be using the action.
            if (HasComp<ActorComponent>(uid))
                continue;

            if (_timing.CurTime < eggLayer.NextGrowth)
                continue;

            // Randomize next growth time for more organic egglaying.
            eggLayer.NextGrowth += TimeSpan.FromSeconds(_random.NextFloat(eggLayer.EggLayCooldownMin, eggLayer.EggLayCooldownMax));

            if (_mobState.IsDead(uid))
                continue;

            // Hungerlevel check/modification is done in TryLayEgg()
            // so it's used for player controlled chickens as well.
            eligibleEggLayers.Add((uid, eggLayer)); // Goob - self-spawning
        }
        // Goob - self-spawning start
        foreach (var ent in eligibleEggLayers)
        {
            TryLayEgg(ent.Owner, ent.Comp);
        }
        // Goob - self-spawning end
    }

    private void OnMapInit(EntityUid uid, EggLayerComponent component, MapInitEvent args)
    {
        _actions.AddAction(uid, ref component.Action, component.EggLayAction);
        component.NextGrowth = _timing.CurTime + TimeSpan.FromSeconds(_random.NextFloat(component.EggLayCooldownMin, component.EggLayCooldownMax));
    }

    private void OnEggLayAction(EntityUid uid, EggLayerComponent egglayer, EggLayInstantActionEvent args)
    {
        // Cooldown is handeled by ActionAnimalLayEgg in types.yml.
        args.Handled = TryLayEgg(uid, egglayer);
    }

    public bool TryLayEgg(EntityUid uid, EggLayerComponent? egglayer)
    {
        if (!Resolve(uid, ref egglayer))
            return false;

        if (_mobState.IsDead(uid))
            return false;

        // Goobstation - hard hunger requirement
        if (egglayer.HungerRequired && !HasComp<HungerComponent>(uid))
        {
            _popup.PopupEntity(Loc.GetString("action-popup-lay-egg-unable"), uid, uid);
            return false;
        }

        // Allow infinitely laying eggs if they can't get hungry.
        if (TryComp<HungerComponent>(uid, out var hunger))
        {
            if (_hunger.GetHunger(hunger) < egglayer.HungerUsage)
            {
                _popup.PopupEntity(Loc.GetString("action-popup-lay-egg-too-hungry"), uid, uid);
                return false;
            }

            _hunger.ModifyHunger(uid, -egglayer.HungerUsage, hunger);
        }

        foreach (var ent in EntitySpawnCollection.GetSpawns(egglayer.EggSpawn, _random))
        {
            Spawn(ent, Transform(uid).Coordinates);
        }

        // Sound + popups
        _audio.PlayPvs(egglayer.EggLaySound, uid);
        _popup.PopupEntity(Loc.GetString("action-popup-lay-egg-user"), uid, uid);
        _popup.PopupEntity(Loc.GetString("action-popup-lay-egg-others", ("entity", uid)), uid, Filter.PvsExcept(uid), true);

        return true;
    }
}