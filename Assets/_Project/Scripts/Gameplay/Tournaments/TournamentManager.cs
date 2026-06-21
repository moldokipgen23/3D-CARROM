using UnityEngine;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;

public class TournamentManager : MonoBehaviour
{
    [Header("Tournament Settings")]
    public int maxParticipants = 64;
    public float registrationTimeHours = 24f;
    public float matchTimeoutMinutes = 10f;

    [Header("References")]
    public LeaderboardService leaderboardService;

    private Tournament _currentTournament;
    private List<TournamentMatch> _matches = new List<TournamentMatch>();

    public event Action<Tournament> OnTournamentCreated;
    public event Action<TournamentMatch> OnMatchReady;
    public event Action<Tournament> OnTournamentCompleted;

    private void Start()
    {
        if (leaderboardService == null)
        {
            leaderboardService = ServiceLocator.Get<LeaderboardService>();
        }
    }

    public async Task<Tournament> CreateTournament(string name, TournamentType type)
    {
        try
        {
            Debug.Log($"Creating tournament: {name}");

            _currentTournament = new Tournament
            {
                id = Guid.NewGuid().ToString(),
                name = name,
                type = type,
                maxParticipants = maxParticipants,
                currentParticipants = 0,
                status = TournamentStatus.Registration,
                startTime = DateTime.UtcNow.AddHours(registrationTimeHours)
            };

            OnTournamentCreated?.Invoke(_currentTournament);
            Debug.Log($"Tournament created: {name} (ID: {_currentTournament.id})");

            return _currentTournament;
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to create tournament: {ex.Message}");
            return null;
        }
    }

    public async Task<bool> JoinTournament(string tournamentId)
    {
        try
        {
            Debug.Log($"Joining tournament: {tournamentId}");
            await Task.Delay(100);

            if (_currentTournament != null && _currentTournament.status == TournamentStatus.Registration)
            {
                _currentTournament.currentParticipants++;
                Debug.Log($"Joined tournament. Participants: {_currentTournament.currentParticipants}");
                return true;
            }

            return false;
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to join tournament: {ex.Message}");
            return false;
        }
    }

    public async Task StartTournament()
    {
        if (_currentTournament == null) return;

        try
        {
            Debug.Log("Starting tournament...");
            _currentTournament.status = TournamentStatus.InProgress;
            GenerateBracket();
            await Task.Delay(100);
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to start tournament: {ex.Message}");
        }
    }

    private void GenerateBracket()
    {
        _matches.Clear();
        int totalMatches = _currentTournament.currentParticipants / 2;

        for (int i = 0; i < totalMatches; i++)
        {
            TournamentMatch match = new TournamentMatch
            {
                id = Guid.NewGuid().ToString(),
                round = 1,
                player1Id = $"player_{i * 2}",
                player2Id = $"player_{i * 2 + 1}",
                status = MatchStatus.Pending
            };
            _matches.Add(match);
        }

        Debug.Log($"Generated {totalMatches} matches for round 1");
        OnMatchReady?.Invoke(_matches[0]);
    }

    public async Task ReportMatchResult(string matchId, string winnerId)
    {
        try
        {
            Debug.Log($"Reporting match result: {matchId}, winner: {winnerId}");
            await Task.Delay(100);

            TournamentMatch match = _matches.Find(m => m.id == matchId);
            if (match != null)
            {
                match.winnerId = winnerId;
                match.status = MatchStatus.Completed;
            }

            CheckTournamentProgress();
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to report match result: {ex.Message}");
        }
    }

    private void CheckTournamentProgress()
    {
        bool allCompleted = true;
        foreach (TournamentMatch match in _matches)
        {
            if (match.status != MatchStatus.Completed)
            {
                allCompleted = false;
                break;
            }
        }

        if (allCompleted)
        {
            if (_matches.Count > 1)
            {
                GenerateNextRound();
            }
            else
            {
                CompleteTournament();
            }
        }
    }

    private void GenerateNextRound()
    {
        int nextRound = _matches[0].round + 1;
        List<TournamentMatch> completedMatches = _matches.FindAll(m => m.status == MatchStatus.Completed);

        _matches.Clear();

        for (int i = 0; i < completedMatches.Count; i += 2)
        {
            if (i + 1 < completedMatches.Count)
            {
                TournamentMatch match = new TournamentMatch
                {
                    id = Guid.NewGuid().ToString(),
                    round = nextRound,
                    player1Id = completedMatches[i].winnerId,
                    player2Id = completedMatches[i + 1].winnerId,
                    status = MatchStatus.Pending
                };
                _matches.Add(match);
            }
        }

        Debug.Log($"Generated round {nextRound} with {_matches.Count} matches");
        if (_matches.Count > 0)
        {
            OnMatchReady?.Invoke(_matches[0]);
        }
    }

    private void CompleteTournament()
    {
        if (_matches.Count == 1 && _matches[0].winnerId != null)
        {
            _currentTournament.status = TournamentStatus.Completed;
            _currentTournament.winnerId = _matches[0].winnerId;
            Debug.Log($"Tournament completed! Winner: {_matches[0].winnerId}");
            OnTournamentCompleted?.Invoke(_currentTournament);
        }
    }

    public Tournament GetCurrentTournament() => _currentTournament;
    public List<TournamentMatch> GetMatches() => _matches;
}

[Serializable]
public class Tournament
{
    public string id;
    public string name;
    public TournamentType type;
    public int maxParticipants;
    public int currentParticipants;
    public TournamentStatus status;
    public DateTime startTime;
    public string winnerId;
}

public enum TournamentType
{
    SingleElimination,
    DoubleElimination,
    RoundRobin,
    Swiss
}

public enum TournamentStatus
{
    Registration,
    InProgress,
    Completed,
    Cancelled
}

[Serializable]
public class TournamentMatch
{
    public string id;
    public int round;
    public string player1Id;
    public string player2Id;
    public string winnerId;
    public MatchStatus status;
}

public enum MatchStatus
{
    Pending,
    InProgress,
    Completed
}
