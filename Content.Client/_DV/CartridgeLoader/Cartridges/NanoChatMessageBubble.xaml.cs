// SPDX-FileCopyrightText: 2024 Milon <milonpl.git@proton.me>
// SPDX-FileCopyrightText: 2024 Skubman <ba.fallaria@gmail.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <aiden@djkraz.com>
// SPDX-FileCopyrightText: 2025 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2025 deltanedas <39013340+deltanedas@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 deltanedas <@deltanedas:kde.org>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared._DV.CartridgeLoader.Cartridges;
using Robust.Client.AutoGenerated;
using Robust.Client.Graphics;
using Robust.Client.UserInterface.Controls;
using Robust.Client.UserInterface.XAML;

namespace Content.Client._DV.CartridgeLoader.Cartridges;

[GenerateTypedNameReferences]
public sealed partial class NanoChatMessageBubble : BoxContainer
{
    public static readonly Color OwnMessageColor = Color.FromHex("#173717d9"); // Dark green
    public static readonly Color OtherMessageColor = Color.FromHex("#252525d9"); // Dark gray
    public static readonly Color BorderColor = Color.FromHex("#40404066"); // Subtle border
    public static readonly Color TextColor = Color.FromHex("#dcdcdc"); // Slightly softened white
    public static readonly Color ErrorColor = Color.FromHex("#cc3333"); // Red

    public NanoChatMessageBubble()
    {
        RobustXamlLoader.Load(this);
    }

    public void SetMessage(NanoChatMessage message, bool isOwnMessage)
    {
        if (MessagePanel.PanelOverride is not StyleBoxFlat)
            return;

        // Configure message appearance
        var style = (StyleBoxFlat)MessagePanel.PanelOverride;
        style.BackgroundColor = isOwnMessage ? OwnMessageColor : OtherMessageColor;
        style.BorderColor = BorderColor;

        // Set message content
        MessageText.Text = message.Content;
        MessageText.Modulate = TextColor;

        // Show delivery failed text if needed (only for own messages)
        DeliveryFailedLabel.Visible = isOwnMessage && message.DeliveryFailed;
        if (DeliveryFailedLabel.Visible)
            DeliveryFailedLabel.Modulate = ErrorColor;

        // For own messages: FlexSpace -> MessagePanel -> RightSpacer
        // For other messages: LeftSpacer -> MessagePanel -> FlexSpace
        MessageContainer.RemoveAllChildren();

        // fuuuuuck
        MessageBox.Parent?.RemoveChild(MessageBox);

        if (isOwnMessage)
        {
            MessageContainer.AddChild(FlexSpace);
            MessageContainer.AddChild(MessageBox);
            MessageContainer.AddChild(RightSpacer);
        }
        else
        {
            MessageContainer.AddChild(LeftSpacer);
            MessageContainer.AddChild(MessageBox);
            MessageContainer.AddChild(FlexSpace);
        }
    }
}