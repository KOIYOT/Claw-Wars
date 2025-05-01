using UnityEngine;
using System;

public static class SaveSystem
{
    public static void SaveToSlot(int slotIndex, SaveData data)
    {
        string json = JsonUtility.ToJson(data);
        PlayerPrefs.SetString($"SaveSlot_{slotIndex}", json);
        PlayerPrefs.Save();
    }

    public static SaveData LoadFromSlot(int slotIndex)
    {
        string key = $"SaveSlot_{slotIndex}";
        if (!PlayerPrefs.HasKey(key)) return null;

        string json = PlayerPrefs.GetString(key);
        return JsonUtility.FromJson<SaveData>(json);
    }

    public static bool HasSaveData(int slotIndex)
    {
        return PlayerPrefs.HasKey($"SaveSlot_{slotIndex}");
    }

    public static void DeleteSlot(int slotIndex)
    {
        PlayerPrefs.DeleteKey($"SaveSlot_{slotIndex}");
    }
}