using UnityEngine;
using System;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class GameTestRunner : MonoBehaviour
{
    [Header("Test Settings")]
    public bool runTestsOnStart = false;
    public bool verboseOutput = true;

    private List<TestResult> _results = new List<TestResult>();
    private int _passed;
    private int _failed;
    private int _total;

    public event Action<TestResult> OnTestCompleted;
    public event Action<int, int, int> OnAllTestsCompleted;

    private void Start()
    {
        if (runTestsOnStart)
        {
            RunAllTests();
        }
    }

    public void RunAllTests()
    {
        _results.Clear();
        _passed = 0;
        _failed = 0;
        _total = 0;

        Debug.Log("=== Starting Game Test Suite ===");

        RunFoulDetectionTests();
        RunScoringTests();
        RunCurrencyTests();
        RunAITests();
        RunTurnManagerTests();

        Debug.Log($"=== Test Suite Complete: {_passed}/{_total} passed, {_failed} failed ===");
        OnAllTestsCompleted?.Invoke(_passed, _failed, _total);
    }

    private void RunFoulDetectionTests()
    {
        Debug.Log("--- Foul Detection Tests ---");

        Test("FoulDetector_Exists", () =>
        {
            FoulDetector detector = FindObjectOfType<FoulDetector>();
            return detector != null;
        });

        Test("FoulDetector_HasReferences", () =>
        {
            FoulDetector detector = FindObjectOfType<FoulDetector>();
            return detector != null && detector.turnManager != null && detector.scoreManager != null;
        });
    }

    private void RunScoringTests()
    {
        Debug.Log("--- Scoring Tests ---");

        Test("ScoreManager_SingletonExists", () =>
        {
            return ScoreManager.Instance != null;
        });

        Test("ScoreManager_InitialScoreZero", () =>
        {
            ScoreManager manager = ScoreManager.Instance;
            if (manager == null) return false;
            manager.ResetScores();
            return manager.GetPlayerScore(1) == 0 && manager.GetPlayerScore(2) == 0;
        });

        Test("ScoreManager_AddCoins", () =>
        {
            ScoreManager manager = ScoreManager.Instance;
            if (manager == null) return false;
            manager.ResetScores();
            manager.AddCoins(1, 0, 3);
            return manager.GetPlayerScore(1) == 3;
        });

        Test("ScoreManager_FoulPenalty", () =>
        {
            ScoreManager manager = ScoreManager.Instance;
            if (manager == null) return false;
            manager.ResetScores();
            manager.AddCoins(1, 0, 5);
            manager.ApplyFoulPenalty(1);
            return manager.GetPlayerScore(1) == 4;
        });
    }

    private void RunCurrencyTests()
    {
        Debug.Log("--- Currency Tests ---");

        Test("CurrencyService_Exists", () =>
        {
            CurrencyService service = ServiceLocator.Get<CurrencyService>();
            return service != null;
        });

        Test("CurrencyService_AddCurrency", () =>
        {
            CurrencyService service = ServiceLocator.Get<CurrencyService>();
            if (service == null) return false;
            int before = service.GetCurrency("coins");
            service.AddCurrency("coins", 100);
            return service.GetCurrency("coins") == before + 100;
        });

        Test("CurrencyService_SpendCurrency", () =>
        {
            CurrencyService service = ServiceLocator.Get<CurrencyService>();
            if (service == null) return false;
            service.AddCurrency("coins", 200);
            bool result = service.SpendCurrency("coins", 50);
            return result && service.GetCurrency("coins") >= 150;
        });

        Test("CurrencyService_InsufficientFunds", () =>
        {
            CurrencyService service = ServiceLocator.Get<CurrencyService>();
            if (service == null) return false;
            return !service.SpendCurrency("coins", 999999);
        });
    }

    private void RunAITests()
    {
        Debug.Log("--- AI Tests ---");

        Test("AIPlayer_Exists", () =>
        {
            AIPlayer ai = FindObjectOfType<AIPlayer>();
            return ai != null;
        });

        Test("AIDifficulty_SettingsExist", () =>
        {
            AINoiseParameters easy = AIDifficultySettings.GetNoiseParameters(AIDifficulty.Easy);
            AINoiseParameters master = AIDifficultySettings.GetNoiseParameters(AIDifficulty.Master);
            return easy.angleNoise > master.angleNoise;
        });

        Test("AIPlayer_HasReferences", () =>
        {
            AIPlayer ai = FindObjectOfType<AIPlayer>();
            return ai != null && ai.strikerController != null;
        });
    }

    private void RunTurnManagerTests()
    {
        Debug.Log("--- Turn Manager Tests ---");

        Test("TurnManager_Exists", () =>
        {
            TurnManager manager = FindObjectOfType<TurnManager>();
            return manager != null;
        });

        Test("TurnManager_InitialState", () =>
        {
            TurnManager manager = FindObjectOfType<TurnManager>();
            if (manager == null) return false;
            return manager.GetCurrentPlayer() == 1 || manager.GetCurrentPlayer() == 2;
        });

        Test("TurnManager_TurnCount", () =>
        {
            TurnManager manager = FindObjectOfType<TurnManager>();
            if (manager == null) return false;
            int count = manager.GetTurnCount();
            return count >= 0;
        });
    }

    private void Test(string testName, Func<bool> testFunc)
    {
        _total++;
        bool passed = false;
        string message = "";

        try
        {
            passed = testFunc();
            message = passed ? "PASSED" : "FAILED - Assertion failed";
        }
        catch (Exception ex)
        {
            passed = false;
            message = $"FAILED - Exception: {ex.Message}";
        }

        if (passed) _passed++;
        else _failed++;

        TestResult result = new TestResult
        {
            testName = testName,
            passed = passed,
            message = message
        };
        _results.Add(result);

        if (verboseOutput)
        {
            Debug.Log($"[{(passed ? "PASS" : "FAIL")}] {testName}: {message}");
        }

        OnTestCompleted?.Invoke(result);
    }

    public List<TestResult> GetResults() => _results;
    public int GetPassedCount() => _passed;
    public int GetFailedCount() => _failed;
    public int GetTotalCount() => _total;
}

[Serializable]
public class TestResult
{
    public string testName;
    public bool passed;
    public string message;
    public float duration;
}
