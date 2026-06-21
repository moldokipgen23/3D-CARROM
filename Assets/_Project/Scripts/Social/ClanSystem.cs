using UnityEngine;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;

public class ClanSystem : MonoBehaviour, IService
{
    [Header("Clan Settings")]
    public int maxClanSize = 50;
    public int minClanNameLength = 3;
    public int maxClanNameLength = 20;

    private Clan _currentClan;
    private List<ClanMember> _members = new List<ClanMember>();
    private List<ClanInvite> _pendingInvites = new List<ClanInvite>();

    public event Action<Clan> OnClanCreated;
    public event Action<Clan> OnClanJoined;
    public event Action OnClanLeft;
    public event Action<ClanMember> OnMemberJoined;
    public event Action<ClanMember> OnMemberLeft;
    public event Action<ClanInvite> OnInviteReceived;

    public Clan CurrentClan => _currentClan;
    public List<ClanMember> Members => _members;

    public async Task<bool> CreateClan(string name, string description)
    {
        try
        {
            if (name.Length < minClanNameLength || name.Length > maxClanNameLength)
            {
                Debug.LogWarning($"Clan name must be between {minClanNameLength} and {maxClanNameLength} characters");
                return false;
            }

            Debug.Log($"Creating clan: {name}");
            await Task.Delay(100);

            _currentClan = new Clan
            {
                id = Guid.NewGuid().ToString(),
                name = name,
                description = description,
                leaderId = "local_player",
                memberCount = 1,
                level = 1,
                createdAt = DateTime.UtcNow
            };

            _members.Clear();
            _members.Add(new ClanMember
            {
                playerId = "local_player",
                playerName = "Player",
                role = ClanRole.Leader,
                joinedAt = DateTime.UtcNow,
                contribution = 0
            });

            OnClanCreated?.Invoke(_currentClan);
            Debug.Log($"Clan created: {name}");
            return true;
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to create clan: {ex.Message}");
            return false;
        }
    }

    public async Task<bool> JoinClan(string clanId)
    {
        try
        {
            Debug.Log($"Joining clan: {clanId}");
            await Task.Delay(100);

            _currentClan = new Clan
            {
                id = clanId,
                name = "Joined Clan",
                memberCount = 10,
                level = 1
            };

            OnClanJoined?.Invoke(_currentClan);
            Debug.Log($"Joined clan: {clanId}");
            return true;
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to join clan: {ex.Message}");
            return false;
        }
    }

    public async Task<bool> LeaveClan()
    {
        try
        {
            Debug.Log("Leaving clan");
            await Task.Delay(100);

            _currentClan = null;
            _members.Clear();
            OnClanLeft?.Invoke();
            Debug.Log("Left clan");
            return true;
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to leave clan: {ex.Message}");
            return false;
        }
    }

    public async Task<bool> SendInvite(string targetPlayerId)
    {
        try
        {
            if (_currentClan == null)
            {
                Debug.LogWarning("Not in a clan");
                return false;
            }

            Debug.Log($"Sending clan invite to: {targetPlayerId}");
            await Task.Delay(100);

            ClanInvite invite = new ClanInvite
            {
                id = Guid.NewGuid().ToString(),
                clanId = _currentClan.id,
                clanName = _currentClan.name,
                senderId = "local_player",
                senderName = "Player",
                targetId = targetPlayerId,
                createdAt = DateTime.UtcNow
            };

            _pendingInvites.Add(invite);
            Debug.Log("Clan invite sent");
            return true;
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to send invite: {ex.Message}");
            return false;
        }
    }

    public async Task<bool> AcceptInvite(string inviteId)
    {
        try
        {
            Debug.Log($"Accepting clan invite: {inviteId}");
            await Task.Delay(100);

            ClanInvite invite = _pendingInvites.Find(i => i.id == inviteId);
            if (invite != null)
            {
                _pendingInvites.Remove(invite);
                await JoinClan(invite.clanId);
            }

            return true;
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to accept invite: {ex.Message}");
            return false;
        }
    }

    public async Task<bool> DeclineInvite(string inviteId)
    {
        try
        {
            Debug.Log($"Declining clan invite: {inviteId}");
            await Task.Delay(100);

            _pendingInvites.RemoveAll(i => i.id == inviteId);
            return true;
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to decline invite: {ex.Message}");
            return false;
        }
    }

    public List<ClanInvite> GetPendingInvites()
    {
        return new List<ClanInvite>(_pendingInvites);
    }

    public async Task<bool> KickMember(string playerId)
    {
        try
        {
            if (_currentClan == null) return false;

            Debug.Log($"Kicking member: {playerId}");
            await Task.Delay(100);

            ClanMember member = _members.Find(m => m.playerId == playerId);
            if (member != null && member.role != ClanRole.Leader)
            {
                _members.Remove(member);
                _currentClan.memberCount = _members.Count;
                OnMemberLeft?.Invoke(member);
                return true;
            }

            return false;
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to kick member: {ex.Message}");
            return false;
        }
    }
}

[Serializable]
public class Clan
{
    public string id;
    public string name;
    public string description;
    public string leaderId;
    public int memberCount;
    public int level;
    public int experience;
    public DateTime createdAt;
}

[Serializable]
public class ClanMember
{
    public string playerId;
    public string playerName;
    public ClanRole role;
    public DateTime joinedAt;
    public int contribution;
    public Sprite avatar;
}

public enum ClanRole
{
    Leader,
    Officer,
    Member
}

[Serializable]
public class ClanInvite
{
    public string id;
    public string clanId;
    public string clanName;
    public string senderId;
    public string senderName;
    public string targetId;
    public DateTime createdAt;
}
