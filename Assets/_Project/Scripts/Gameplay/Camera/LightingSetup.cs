using UnityEngine;

public class LightingSetup : MonoBehaviour
{
    [Header("Main Light")]
    public Color mainLightColor = new Color(1f, 0.97f, 0.92f);
    public float mainLightIntensity = 1.4f;

    [Header("Ambient")]
    public Color ambientColor = new Color(0.35f, 0.35f, 0.4f);

    private void Awake()
    {
        SetupLighting();
    }

    private void SetupLighting()
    {
        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
        RenderSettings.ambientLight = ambientColor;
        RenderSettings.ambientIntensity = 0.6f;

        GameObject existingLight = GameObject.Find("MainLight");
        if (existingLight == null)
        {
            CreateMainLight();
        }

        CreateOverheadLight();
        CreateRimLights();
        CreatePocketGlow();
    }

    private void CreateMainLight()
    {
        GameObject lightObj = new GameObject("MainLight");
        lightObj.transform.rotation = Quaternion.Euler(55f, -25f, 0);

        Light light = lightObj.AddComponent<Light>();
        light.type = LightType.Directional;
        light.color = mainLightColor;
        light.intensity = mainLightIntensity;
        light.shadows = LightShadows.Soft;
        light.shadowStrength = 0.65f;
        light.shadowBias = 0.02f;
        light.shadowNormalBias = 0.4f;
    }

    private void CreateOverheadLight()
    {
        GameObject overhead = new GameObject("OverheadLight");
        overhead.transform.position = new Vector3(0, 3f, 0);
        overhead.transform.rotation = Quaternion.Euler(90f, 0, 0);

        Light light = overhead.AddComponent<Light>();
        light.type = LightType.Spot;
        light.color = new Color(1f, 0.95f, 0.88f);
        light.intensity = 2.0f;
        light.spotAngle = 65f;
        light.innerSpotAngle = 45f;
        light.range = 6f;
        light.shadows = LightShadows.Soft;
        light.shadowStrength = 0.5f;
    }

    private void CreateRimLights()
    {
        CreatePointLight("RimLight_1", new Vector3(-2.5f, 1.8f, -2.5f),
            new Color(0.5f, 0.6f, 0.9f), 0.5f, 5f);
        CreatePointLight("RimLight_2", new Vector3(2.5f, 1.8f, 2.5f),
            new Color(0.9f, 0.85f, 0.7f), 0.4f, 5f);
        CreatePointLight("RimLight_3", new Vector3(2.5f, 1.8f, -2.5f),
            new Color(0.7f, 0.8f, 1f), 0.3f, 4f);
        CreatePointLight("RimLight_4", new Vector3(-2.5f, 1.8f, 2.5f),
            new Color(1f, 0.9f, 0.8f), 0.3f, 4f);
    }

    private void CreatePocketGlow()
    {
        float boardHalf = 1.45f;
        Vector3[] pocketPositions = {
            new Vector3(-boardHalf, 0.15f, boardHalf),
            new Vector3(boardHalf, 0.15f, boardHalf),
            new Vector3(-boardHalf, 0.15f, -boardHalf),
            new Vector3(boardHalf, 0.15f, -boardHalf)
        };

        for (int i = 0; i < 4; i++)
        {
            CreatePointLight($"PocketGlow_{i}", pocketPositions[i],
                new Color(1f, 0.85f, 0.3f), 0.25f, 0.25f);
        }
    }

    private void CreatePointLight(string name, Vector3 position, Color color, float intensity, float range)
    {
        GameObject lightObj = new GameObject(name);
        lightObj.transform.position = position;

        Light light = lightObj.AddComponent<Light>();
        light.type = LightType.Point;
        light.color = color;
        light.intensity = intensity;
        light.range = range;
    }
}
