using Robust.Shared.GameObjects;
using Robust.Shared.Serialization;

namespace Content.Shared._Starlight.Movement.Components;

/// <summary>
/// Component for handling shared EyeCursorTunnelVision functionality.
/// This provides configuration data for the tunnel vision effect.
/// </summary>
[RegisterComponent]
public sealed partial class SharedEyeCursorTunnelVisionComponent : Component
{
    /// <summary>
    /// The amount the view will be displaced when the cursor is positioned at/beyond the max offset distance.
    /// Measured in tiles.
    /// </summary>
    [DataField] public float MaxOffset = 3f;

    /// <summary>
    /// The speed which the camera adjusts to new positions. 0.5f seems like a good value, but can be changed if you want very slow/instant adjustments.
    /// </summary>
    [DataField] public float OffsetSpeed = 0.5f;

    /// <summary>
    /// The amount the PVS should increase to account for the max offset.
    /// Should be 1/10 of MaxOffset most of the time.
    /// </summary>
    [DataField] public float PvsIncrease = 0.3f;


    /// <summary>
    /// The amount of slow down percentage applied to a character after they use the tunnel vision effect.
    /// </summary>
    [DataField] public float TunnelVisionSlowdown = 0.5f;

    /// <summary>
    /// The maximum amount of zoom applied to camera when using the tunnel vision effect.
    /// It is being gradually increased to this value the closer the character is to the max offset.
    /// </summary>
    [DataField] public float TunnelVisionZoom = 1.5f;
}