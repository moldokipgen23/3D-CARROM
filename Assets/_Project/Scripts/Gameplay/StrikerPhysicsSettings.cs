using UnityEngine;

[CreateAssetMenu(fileName = "StrikerPhysicsSettings", menuName = "Physics/StrikerPhysicsSettings")]
public class StrikerPhysicsSettings : ScriptableObject
{
    [Header("Striker Physics")]
    [Range(0f, 1f)]
    public float dynamicFriction = 0.5f;
    [Range(0f, 1f)]
    public float staticFriction = 0.5f;
    [Range(0f, 1f)]
    public float bounciness = 0.1f;

    public static StrikerPhysicsSettings DefaultSettings
    {
        get
        {
            var settings = CreateInstance<StrikerPhysicsSettings>();
            settings.dynamicFriction = 0.5f;
            settings.staticFriction = 0.5f;
            settings.bounciness = 0.1f;
            return settings;
        }
    }

    public PhysicMaterial CreatePhysicMaterial()
    {
        var mat = new PhysicMaterial($"StrikerPhysics_{name}");
        mat.dynamicFriction = dynamicFriction;
        mat.staticFriction = staticFriction;
        mat.bounciness = bounciness;
        mat.frictionCombine = PhysicMaterialCombine.Minimum;
        mat.bounceCombine = PhysicMaterialCombine.Minimum;
        return mat;
    }
}
