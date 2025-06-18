using System.Numerics;

namespace Content.Shared._Starlight.Movement.Components;

/// <summary>
/// This is used for...
/// </summary>
[RegisterComponent]
public sealed partial class SharedEyeCursorTunnelVisionOffsetComponent : Component
{
    /// <summary>
    /// The location the offset will attempt to pan towards; based on the cursor's position in the game window.
    /// </summary>
    public Vector2 TargetPosition = Vector2.Zero;

    /// <summary>
    /// The current positional offset being applied. Used to enable gradual panning.
    /// </summary>
    public Vector2 CurrentPosition = Vector2.Zero;
}