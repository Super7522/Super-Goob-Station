// SPDX-FileCopyrightText: 2023 deltanedas <39013340+deltanedas@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 deltanedas <@deltanedas:kde.org>
// SPDX-FileCopyrightText: 2023 metalgearsloth <comedian_vs_clown@hotmail.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

using Content.Shared.Damage.Systems;
using Robust.Shared.GameStates;

namespace Content.Shared.Damage.Components;

/// <summary>
/// Multiplies the entity's <see cref="StaminaComponent.StaminaDamage"/> by the <see cref="Modifier"/>.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState, Access(typeof(SharedStaminaSystem))]
public sealed partial class StaminaModifierComponent : Component
{
    /// <summary>
    /// What to multiply max stamina by.
    /// When added this scales max stamina, but not stamina damags to give you an extra boost of survability.
    /// If you have too much damage when the modifier is removed, you suffer "withdrawl" and instantly stamcrit.
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite), DataField("modifier"), AutoNetworkedField]
    public float Modifier = 2f;
}