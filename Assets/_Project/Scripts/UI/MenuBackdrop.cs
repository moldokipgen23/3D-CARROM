using System.Collections;
using UnityEngine;

/// <summary>
/// Builds the hero carrom board that sits behind the main menu UI. The board,
/// coins and lighting are generated at runtime so the menu needs no baked art.
/// </summary>
public class MenuBackdrop : MonoBehaviour
{
    private Camera cam;
    private Transform boardRoot;
    private float time;

    private void Start()
    {
        StartCoroutine(BuildRoutine());
    }

    private IEnumerator BuildRoutine()
    {
        GameObject board = new GameObject("MenuBackdropBoard");
        board.transform.SetParent(transform, false);
        board.AddComponent<BoardSetup>();
        boardRoot = board.transform;

        yield return null;

        GameObject coins = new GameObject("MenuBackdropCoins");
        coins.transform.SetParent(transform, false);
        coins.AddComponent<CoinSpawner>();

        SetupCamera();
        AddHeroLight();
        AddBackground();
    }

    private void SetupCamera()
    {
        cam = Camera.main;
        if (cam == null)
        {
            GameObject camObj = new GameObject("Main Camera");
            camObj.tag = "MainCamera";
            cam = camObj.AddComponent<Camera>();
            camObj.AddComponent<AudioListener>();
        }

        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = new Color(0.09f, 0.06f, 0.04f, 1f);
        cam.fieldOfView = 50f;
        cam.nearClipPlane = 0.1f;
        cam.farClipPlane = 60f;
        cam.allowHDR = true;
        cam.allowMSAA = true;

        // Board is deliberately cropped by the portrait frame for a hero shot.
        cam.transform.position = new Vector3(0f, 6.4f, -4.4f);
        cam.transform.LookAt(new Vector3(0f, 0f, 0.15f));
    }

    private void AddHeroLight()
    {
        GameObject keyObj = new GameObject("MenuKeyLight");
        keyObj.transform.SetParent(transform, false);
        keyObj.transform.position = new Vector3(-1.2f, 4f, -1.4f);
        keyObj.transform.rotation = Quaternion.Euler(62f, -22f, 0f);

        Light key = keyObj.AddComponent<Light>();
        key.type = LightType.Spot;
        key.color = new Color(1f, 0.93f, 0.80f);
        key.intensity = 3.4f;
        key.spotAngle = 78f;
        key.innerSpotAngle = 45f;
        key.range = 12f;
        key.shadows = LightShadows.Soft;
        key.shadowStrength = 0.55f;
    }

    private void AddBackground()
    {
        // Warm floor beneath the table so the frame never floats in pure black.
        GameObject floor = GameObject.CreatePrimitive(PrimitiveType.Cube);
        floor.name = "MenuFloor";
        floor.transform.SetParent(transform, false);
        floor.transform.position = new Vector3(0f, -0.7f, 0f);
        floor.transform.localScale = new Vector3(24f, 0.2f, 24f);

        Shader shader = Shader.Find("Standard");
        if (shader == null) shader = Shader.Find("Diffuse");
        Material mat = new Material(shader);
        mat.color = new Color(0.10f, 0.070f, 0.048f);
        mat.SetFloat("_Metallic", 0.0f);
        mat.SetFloat("_Glossiness", 0.25f);
        floor.GetComponent<MeshRenderer>().sharedMaterial = mat;
    }

    private void LateUpdate()
    {
        if (cam == null) return;

        // Very slow drift keeps the menu alive without distracting from the UI.
        time += Time.deltaTime;
        float sway = Mathf.Sin(time * 0.28f) * 0.035f;
        cam.transform.position = new Vector3(sway, 6.4f + Mathf.Sin(time * 0.21f) * 0.03f, -4.4f);
        cam.transform.LookAt(new Vector3(sway * 0.6f, 0f, 0.15f));
    }
}
