using Content.Goobstation.Shared.Vehicles.Clowncar;
using Robust.Client.Animations;
using Robust.Client.GameObjects;

namespace Content.Goobstation.Client.Vehicles.Clowncar;

public sealed class ClowncarSystem : SharedClowncarSystem
{
    [Dependency] private readonly AnimationPlayerSystem _animationPlayer = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ClowncarComponent, AppearanceChangeEvent>(OnAppearanceChange);
        SubscribeLocalEvent<ClowncarComponent, AnimationCompletedEvent>(OnAnimationCompleted);
    }

    private void OnAppearanceChange(EntityUid uid, ClowncarComponent component, ref AppearanceChangeEvent args)
    {
        if (args.Sprite == null || !AppearanceSystem.TryGetData<bool>(uid, ClowncarVisuals.FireModeEnabled, out var fireModeEnabled, args.Component))
            return;

        if (!args.Sprite.LayerMapTryGet(ClowncarLayers.Base, out var baseLayerIdx))
            return;

        var state = args.Sprite.LayerGetState(baseLayerIdx);
    }

    private void OnAnimationCompleted(EntityUid uid, ClowncarComponent component, AnimationCompletedEvent args)
    {
        if (!TryComp<SpriteComponent>(uid, out var sprite))
            return;

        sprite.LayerSetAutoAnimated(ClowncarLayers.Base, true);
    }

    private void PlayAnimation(EntityUid uid, ClowncarLayers layer, string state, string finalState, float animationTime)
    {
        if (_animationPlayer.HasRunningAnimation(uid, state))
            return;

        var animation = new Animation()
        {
            Length = TimeSpan.FromSeconds(animationTime),
            AnimationTracks =
            {
                new AnimationTrackSpriteFlick
                {
                    LayerKey = layer,
                    KeyFrames =
                    {
                        new AnimationTrackSpriteFlick.KeyFrame(state, 0f),
                        new AnimationTrackSpriteFlick.KeyFrame(finalState, animationTime)
                    }
                }
            }
        };

        _animationPlayer.Play(uid, animation, state);
    }
}

internal enum ClowncarLayers : byte
{
   Base
}
