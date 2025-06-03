using UnityEngine.SceneManagement;
using UnityEngine;
using TMPro;

public class SaveSlotButton : MonoBehaviour
{
    [SerializeField] private int slotIndex = 0;
    [SerializeField] private TextMeshProUGUI label;
    private MainMenuController menu;

    private void Start()
    {
        RefreshDisplay();
        menu = FindFirstObjectByType<MainMenuController>();
    }

    public void RefreshDisplay()
    {
        if (SaveSystem.HasSaveData(slotIndex))
        {
            SaveData data = SaveSystem.LoadFromSlot(slotIndex);

            if (data.completedMissions <= 0)
                label.text = "Continue - No missions yet";
            else
                label.text = $"Continue - {data.completedMissions} mission{(data.completedMissions > 1 ? "s" : "")} completed";
        }
        else
        {
            label.text = "New Game";
        }
    }

    public void OnClickSlot()
    {
        if (SaveSystem.HasSaveData(slotIndex))
        {
            PlayerPrefs.SetInt("LastUsedSlot", slotIndex);
            PlayerPrefs.Save();

            SaveData loaded = SaveSystem.LoadFromSlot(slotIndex);
            Debug.Log($"Cargando partida: Nivel {loaded.level}");

            if (menu != null)
                menu.StartCoroutine(menu.FadeOutMusicAndLoadSceneAsync("Lobby"));
        }
        else
        {
            SaveData newData = new SaveData
            {
                level = 1,
                playTime = 0f,
                lastSaveDate = System.DateTime.Now.ToString(),
                walkSpeed = 2f,
                runSpeed = 5f,
                lightAttackDamage = 5f,
                heavyAttackDamage = 10f,
                maxStamina = 100f,
                completedMissions = 0
            };

            SaveSystem.SaveToSlot(slotIndex, newData);
            PlayerPrefs.SetInt("LastUsedSlot", slotIndex);
            PlayerPrefs.Save();

            Debug.Log($"Nueva partida creada en slot {slotIndex}");

            if (menu != null)
                menu.StartCoroutine(menu.FadeOutMusicAndLoadSceneAsync("Lobby"));
        }
    }
}
