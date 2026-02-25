using Terraria;
using TrProtocol.Interfaces;

namespace TrProtocol.Models;

public struct PlayerControlData : IPackedSerializable
{
    public BitsByte packedValue;
    public bool ControlUp {
        get => packedValue[0];
        set => packedValue[0] = value;
    }
    public bool ControlDown {
        get => packedValue[1];
        set => packedValue[1] = value;
    }
    public bool ControlLeft {
        get => packedValue[2];
        set => packedValue[2] = value;
    }
    public bool ControlRight {
        get => packedValue[3];
        set => packedValue[3] = value;
    }
    public bool ControlJump {
        get => packedValue[4];
        set => packedValue[4] = value;
    }
    public bool IsUsingItem {
        get => packedValue[5];
        set => packedValue[5] = value;
    }
    public bool FaceDirection {
        get => packedValue[6];
        set => packedValue[6] = value;
    }
    
    public override string ToString() {
        var active = new List<string>();
        if (ControlUp) active.Add("Up");
        if (ControlDown) active.Add("Down");
        if (ControlLeft) active.Add("Left");
        if (ControlRight) active.Add("Right");
        if (ControlJump) active.Add("Jump");
        if (IsUsingItem) active.Add("UsingItem");
    
        string dir = FaceDirection ? "Right" : "Left";
        return $"{{Controls: [{string.Join("|", active)}], Face: {dir}}}";
    }
}
public struct PlayerMiscData1 : IPackedSerializable
{
    public BitsByte packedValue;

    public bool IsUsingPulley {
        get => packedValue[0];
        set => packedValue[0] = value;
    }

    public bool PulleyDirection {
        get => packedValue[1];
        set => packedValue[1] = value;
    }

    public bool HasVelocity {
        get => packedValue[2];
        set => packedValue[2] = value;
    }

    public bool IsVortexStealthActive {
        get => packedValue[3];
        set => packedValue[3] = value;
    }

    public bool GravityDirection {
        get => packedValue[4];
        set => packedValue[4] = value;
    }

    public bool IsShieldRaised {
        get => packedValue[5];
        set => packedValue[5] = value;
    }

    public bool IsGhosted {
        get => packedValue[6];
        set => packedValue[6] = value;
    }

    public bool IsMounting {
        get => packedValue[7];
        set => packedValue[7] = value;
    }
    
    public override string ToString() {
        var states = new List<string>();
        if (IsUsingPulley) states.Add(PulleyDirection ? "PulleyDown" : "PulleyUp");
        if (HasVelocity) states.Add("Moving");
        if (IsVortexStealthActive) states.Add("Stealth");
        if (GravityDirection) states.Add("ReverseGravity");
        if (IsShieldRaised) states.Add("ShieldUp");
        if (IsGhosted) states.Add("Ghost");
        if (IsMounting) states.Add("Mounted");

        return states.Count > 0 ? $"{string.Join(", ", states)}" : "Normal";
    }
}
public struct PlayerMiscData2 : IPackedSerializable
{
    public BitsByte packedValue;
    public bool TryHoveringUp {
        get => packedValue[0];
        set => packedValue[0] = value;
    }
    public bool IsVoidVaultEnabled {
        get => packedValue[1];
        set => packedValue[1] = value;
    }
    public bool IsSitting {
        get => packedValue[2];
        set => packedValue[2] = value;
    }
    public bool HasDownedDd2Event {
        get => packedValue[3];
        set => packedValue[3] = value;
    }
    public bool IsPettingAnimal {
        get => packedValue[4];
        set => packedValue[4] = value;
    }
    public bool IsPettedAnimalSmall {
        get => packedValue[5];
        set => packedValue[5] = value;
    }
    public bool CanReturnWithPotionOfReturn {
        get => packedValue[6];
        set => packedValue[6] = value;
    }
    public bool TryHoveringDown {
        get => packedValue[7];
        set => packedValue[7] = value;
    }
    
    public override string ToString() {
        var states = new List<string>();
        if (IsSitting) states.Add("Sitting");
        if (IsPettingAnimal) states.Add(IsPettedAnimalSmall ? "PettingSmall" : "PettingLarge");
        if (IsVoidVaultEnabled) states.Add("VoidVault");
        if (TryHoveringUp) states.Add("HoverUp");
        if (TryHoveringDown) states.Add("HoverDown");
        if (CanReturnWithPotionOfReturn) states.Add("CanReturn");
    
        return $"{(states.Count > 0 ? string.Join(", ", states) : "Idle")}";
    }
}
public struct PlayerMiscData3 : IPackedSerializable
{
    public BitsByte packedValue;
    public bool IsSleeping {
        get => packedValue[0];
        set => packedValue[0] = value;
    }
    public bool AutoReuseAllWeapons {
        get => packedValue[1];
        set => packedValue[1] = value;
    }
    public bool ControlDownHold {
        get => packedValue[2];
        set => packedValue[2] = value;
    }
    public bool IsOperatingAnotherEntity {
        get => packedValue[3];
        set => packedValue[3] = value;
    }
    public bool ControlUseTile {
        get => packedValue[4];
        set => packedValue[4] = value;
    }
    public bool HasNetCameraTarget {
        get => packedValue[5];
        set => packedValue[5] = value;
    }
    public bool LastItemUseAttemptSuccess {
        get => packedValue[6];
        set => packedValue[6] = value;
    }
    
    public override string ToString() {
        var states = new List<string>();
        if (IsSleeping) states.Add("Sleeping");
        if (AutoReuseAllWeapons) states.Add("AutoReuse");
        if (ControlUseTile) states.Add("UsingTile");
        if (IsOperatingAnotherEntity) states.Add("OperatingEntity");
        if (HasNetCameraTarget) states.Add("CamTargeted");
        if (LastItemUseAttemptSuccess) states.Add("UseSuccess");

        return $"{(states.Count > 0 ? string.Join(", ", states) : "None")}";
    }
}
