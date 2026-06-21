using UnityEngine;

[CreateAssetMenu(fileName = "CoinPhysicsSettings", menuName = "Physics/CoinPhysicsSettings")]
public class CoinPhysicsSettings : ScriptableObject
{
    [Header("Coin Physics")]
    [Range(0f, 1f)]
    public float dynamicFriction = 0.3f;
    [Range(0f, 1f)]
    public float staticFriction = 0.3f;
    [Range(0f, 1f)]
    public float bounciness = 0.1f;

    public static CoinPhysicsSettings DefaultSettings
    {
        get
        {
            var settings = CreateInstance<CoinPhysicsSettings>();
            settings.dynamicFriction = 0.3f;
            settings.staticFriction = 0.3f;
            settings.bounciness = 0.1f;
            return settings;
        }
    }

    public PhysicsMaterial CreatePhysicsMaterial()
    {
        var mat = new PhysicsMaterial($"CoinPhysics_{name}");
        mat.dynamicFriction = dynamicFriction;
        mat.staticFriction = staticFriction;
        mat.bounciness = bounciness;
        mat.frictionCombine = PhysicsMaterialCombine.Minimum;
        mat.bounceCombine = PhysicsMaterialCombine.Minimum;
        return mat;
    }
}
