using UnityEngine;

public class AimIndicator : MonoBehaviour
{
    [Header("Aim Indicator Settings")]
    public LineRenderer lineRenderer;
    public float lineWidth = 0.02f;
    public Color aimLineColor = Color.yellow;
    public Color powerLineColor = Color.green;

    [Header("References")]
    public StrikerController strikerController;

    private void Awake()
    {
        if (lineRenderer == null)
        {
            lineRenderer = gameObject.AddComponent<LineRenderer>();
        }

        SetupLineRenderer();
    }

    private void Start()
    {
        if (strikerController != null)
        {
            strikerController.OnShotFired += OnShotFired;
        }
    }

    private void SetupLineRenderer()
    {
        lineRenderer.startWidth = lineWidth;
        lineRenderer.endWidth = lineWidth;
        lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        lineRenderer.startColor = aimLineColor;
        lineRenderer.endColor = powerLineColor;
        lineRenderer.positionCount = 2;
        lineRenderer.enabled = false;
    }

    private void Update()
    {
        UpdateAimIndicator();
    }

    private void UpdateAimIndicator()
    {
        if (strikerController != null && strikerController.IsAiming)
        {
            Vector3 startPoint = strikerController.transform.position;
            Vector3 endPoint = startPoint + strikerController.ShotDirection * 2f;

            lineRenderer.SetPosition(0, startPoint);
            lineRenderer.SetPosition(1, endPoint);
            lineRenderer.enabled = true;
        }
        else
        {
            lineRenderer.enabled = false;
        }
    }

    private void OnShotFired(Vector2 direction, float power)
    {
        lineRenderer.enabled = false;
        Debug.Log($"AimIndicator: Shot fired - Power: {power:F1}");
    }
}