using UnityEngine;

public class AimIndicator : MonoBehaviour
{
    [Header("Aim Settings")]
    public float maxLineLength = 1.5f;
    public float lineWidth = 0.012f;
    public int dotCount = 30;

    [Header("Colors")]
    public Color lowPowerColor = new Color(0.2f, 0.9f, 0.3f);
    public Color highPowerColor = new Color(1f, 0.2f, 0.15f);

    [Header("References")]
    public StrikerController strikerController;

    private GameObject[] dots;
    private GameObject powerBar;
    private Material dotMaterial;

    private void Awake()
    {
        CreateDots();
        CreatePowerBar();
    }

    private void CreateDots()
    {
        Shader shader = Shader.Find("Standard");
        if (shader == null) shader = Shader.Find("Universal Render Pipeline/Lit");

        dotMaterial = new Material(shader);
        dotMaterial.SetFloat("_Metallic", 0f);
        dotMaterial.SetFloat("_Glossiness", 0.3f);
        dotMaterial.color = lowPowerColor;

        dots = new GameObject[dotCount];
        for (int i = 0; i < dotCount; i++)
        {
            GameObject dot = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            dot.name = $"AimDot_{i}";
            dot.transform.parent = transform;
            dot.transform.localScale = new Vector3(lineWidth, 0.001f, lineWidth);
            dot.GetComponent<MeshRenderer>().sharedMaterial = dotMaterial;
            Destroy(dot.GetComponent<CapsuleCollider>());
            dot.SetActive(false);
            dots[i] = dot;
        }
    }

    private void CreatePowerBar()
    {
        powerBar = GameObject.CreatePrimitive(PrimitiveType.Cube);
        powerBar.name = "PowerBar";
        powerBar.transform.parent = transform;
        powerBar.transform.localScale = new Vector3(0.003f, 0.002f, 0.3f);
        Destroy(powerBar.GetComponent<BoxCollider>());

        Material barMat = new Material(Shader.Find("Standard") ?? Shader.Find("Universal Render Pipeline/Lit"));
        barMat.color = new Color(0.3f, 0.3f, 0.3f, 0.5f);
        barMat.SetFloat("_Metallic", 0f);
        barMat.SetFloat("_Glossiness", 0.2f);
        powerBar.GetComponent<MeshRenderer>().sharedMaterial = barMat;
        powerBar.SetActive(false);
    }

    private void Update()
    {
        UpdateAimVisual();
    }

    private void UpdateAimVisual()
    {
        if (strikerController != null && strikerController.IsAiming && strikerController.ShotDirection != Vector3.zero)
        {
            float powerPercent = strikerController.ShotDirection.magnitude > 0 ? 0.5f : 0f;

            Vector3 startPos = strikerController.transform.position + new Vector3(0, 0.02f, 0);
            Vector3 direction = new Vector3(strikerController.ShotDirection.x, 0, strikerController.ShotDirection.z).normalized;

            float activeLength = maxLineLength * powerPercent;
            float spacing = activeLength / dotCount;

            for (int i = 0; i < dotCount; i++)
            {
                float dist = (i + 1) * spacing;
                if (dist <= maxLineLength * 0.8f)
                {
                    dots[i].SetActive(true);
                    dots[i].transform.position = startPos + direction * dist;
                    dots[i].transform.localScale = new Vector3(
                        lineWidth * (1f - (float)i / dotCount * 0.5f),
                        0.001f,
                        lineWidth * (1f - (float)i / dotCount * 0.5f)
                    );

                    float t = (float)i / dotCount;
                    Color dotColor = Color.Lerp(lowPowerColor, highPowerColor, t);
                    dots[i].GetComponent<MeshRenderer>().sharedMaterial.color = dotColor;
                }
                else
                {
                    dots[i].SetActive(false);
                }
            }

            powerBar.SetActive(true);
            powerBar.transform.position = startPos + direction * (maxLineLength * 0.85f + 0.05f);
            powerBar.transform.rotation = Quaternion.LookRotation(direction);
            powerBar.transform.localScale = new Vector3(0.003f, 0.002f, 0.3f * powerPercent);

            Material barMat = powerBar.GetComponent<MeshRenderer>().sharedMaterial;
            barMat.color = Color.Lerp(lowPowerColor, highPowerColor, powerPercent);
        }
        else
        {
            Hide();
        }
    }

    public void Hide()
    {
        if (dots != null)
        {
            for (int i = 0; i < dots.Length; i++)
            {
                if (dots[i] != null) dots[i].SetActive(false);
            }
        }
        if (powerBar != null) powerBar.SetActive(false);
    }

    private void OnDestroy()
    {
        if (dots != null)
        {
            for (int i = 0; i < dots.Length; i++)
            {
                if (dots[i] != null) Destroy(dots[i]);
            }
        }
        if (powerBar != null) Destroy(powerBar);
    }
}
