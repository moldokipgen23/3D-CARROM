using UnityEngine;
using System;
using System.IO;

public class PerformanceMonitor : MonoBehaviour
{
    [Header("Monitoring Settings")]
    public bool enableMonitoring = true;
    public float sampleInterval = 1f;
    public int maxSamples = 300;

    private float _fps;
    private float _deltaTime;
    private float _memoryUsage;
    private int _sampleCount;

    private float _fpsAccumulator;
    private int _fpsFrames;
    private float _lastSampleTime;

    public static PerformanceMonitor Instance { get; private set; }

    public float CurrentFPS => _fps;
    public float CurrentMemoryMB => _memoryUsage;
    public int SampleCount => _sampleCount;

    public event Action<float> OnFPSUpdated;
    public event Action<float> OnMemoryWarning;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Update()
    {
        if (!enableMonitoring) return;

        _deltaTime += (Time.unscaledDeltaTime - _deltaTime) * 0.1f;
        _fpsAccumulator += Time.unscaledDeltaTime;
        _fpsFrames++;

        if (Time.time - _lastSampleTime >= sampleInterval)
        {
            RecordSample();
        }
    }

    private void RecordSample()
    {
        _fps = _fpsFrames / _fpsAccumulator;
        _memoryUsage = UnityEngine.Profiling.Profiler.GetTotalAllocatedMemoryLong() / (1024f * 1024f);
        _sampleCount++;

        OnFPSUpdated?.Invoke(_fps);

        if (_memoryUsage > 500f)
        {
            OnMemoryWarning?.Invoke(_memoryUsage);
            Debug.LogWarning($"High memory usage: {_memoryUsage:F1} MB");
        }

        _fpsAccumulator = 0f;
        _fpsFrames = 0;
        _lastSampleTime = Time.time;
    }

    public string GetPerformanceReport()
    {
        return $"FPS: {_fps:F1} | Memory: {_memoryUsage:F1} MB | Samples: {_sampleCount}";
    }

    public float GetAverageFPS()
    {
        return _fps;
    }

    public void LogPerformance()
    {
        Debug.Log($"[Performance] {GetPerformanceReport()}");
    }

    public void ExportReport(string filename)
    {
        try
        {
            string path = Path.Combine(Application.persistentDataPath, filename);
            string report = $"Performance Report\n" +
                           $"Date: {DateTime.Now}\n" +
                           $"Platform: {Application.platform}\n" +
                           $"Device: {SystemInfo.deviceModel}\n" +
                           $"OS: {SystemInfo.operatingSystem}\n" +
                           $"FPS: {_fps:F1}\n" +
                           $"Memory: {_memoryUsage:F1} MB\n" +
                           $"Samples: {_sampleCount}\n";

            File.WriteAllText(path, report);
            Debug.Log($"Performance report exported to: {path}");
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to export report: {ex.Message}");
        }
    }
}
