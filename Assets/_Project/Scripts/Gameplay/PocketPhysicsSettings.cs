using UnityEngine;

[CreateAssetMenu(fileName = "PocketPhysicsSettings", menuName = "Physics/PocketPhysicsSettings")]
public class PocketPhysicsSettings : ScriptableObject
{
    [Header("Pocket Physics")]
    public float pocketRadius = 0.05f;
    public float captureThreshold = 0.5f;
    public float pocketSpeedThreshold = 0.1f;

    public static PocketPhysicsSettings DefaultSettings
    {
        get
        {
            var settings = CreateInstance<PocketPhysicsSettings>();
            settings.pocketRadius = 0.05f;
            settings.captureThreshold = 0.5f;
            settings.pocketSpeedThreshold = 0.1f;
            return settings;
        }
    }
}
