using UnityEngine;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;

public class RoomService : IService
{
    private NakamaService _nakamaService;

    public event Action<string> OnRoomJoined;
    public event Action OnRoomLeft;

    public void Initialize()
    {
        _nakamaService = ServiceLocator.Get<NakamaService>();
    }

    public async Task<string> CreateRoom()
    {
        try
        {
            Debug.Log("Creating private room...");
            await Task.Delay(100);
            string roomCode = GenerateRoomCode();
            Debug.Log($"Private room created with code: {roomCode}");
            return roomCode;
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to create room: {ex.Message}");
            throw;
        }
    }

    public async Task JoinRoom(string roomCode)
    {
        try
        {
            Debug.Log($"Joining room with code: {roomCode}");
            await Task.Delay(100);
            OnRoomJoined?.Invoke(roomCode);
            Debug.Log($"Joined room: {roomCode}");
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to join room: {ex.Message}");
            throw;
        }
    }

    public async Task LeaveRoom()
    {
        try
        {
            Debug.Log("Leaving room...");
            await Task.Delay(100);
            OnRoomLeft?.Invoke();
            Debug.Log("Left room");
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to leave room: {ex.Message}");
            throw;
        }
    }

    private string GenerateRoomCode()
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        System.Random random = new System.Random();
        char[] result = new char[6];
        for (int i = 0; i < 6; i++)
        {
            result[i] = chars[random.Next(chars.Length)];
        }
        return new string(result);
    }
}
