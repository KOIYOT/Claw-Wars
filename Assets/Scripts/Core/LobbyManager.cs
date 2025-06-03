using UnityEngine;

public class LobbyManager : MonoBehaviour
{
    public GameObject UpgradePanel_;

    void Start()
    {
        int slot = PlayerPrefs.GetInt("LastUsedSlot", -1);
        bool returned = PlayerPrefs.GetInt("ReturnedFromMission", 0) == 1;

        if (slot >= 0 && SaveSystem.HasSaveData(slot))
        {
            SaveData data = SaveSystem.LoadFromSlot(slot);

            if (returned && data.UpgradePoints_ > 0)
            {
                UpgradePanel_.SetActive(true);
                UpgradePanel_.GetComponent<AttributeUpgrade>().Init(data);
            }
            else
            {
                UpgradePanel_.SetActive(false);
            }
        }

        PlayerPrefs.DeleteKey("ReturnedFromMission");
        PlayerPrefs.Save();
    }
}