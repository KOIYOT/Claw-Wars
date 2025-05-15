using UnityEngine;
using TMPro;

public class EnemyCounterUI : MonoBehaviour
{
    public TMP_Text enemyCounterText;
    public float updateInterval = 0.5f;

    void Start()
    {
        InvokeRepeating(nameof(UpdateEnemyCount), 0f, updateInterval);
    }

    void UpdateEnemyCount()
    {
        int enemyCount = GameObject.FindGameObjectsWithTag("Enemy").Length;

        if (enemyCount > 0)
        {
            enemyCounterText.text = $"{enemyCount} rivals still standing.";
        }
        else
        {
            enemyCounterText.text = $"Zone conquered. Return to the hideout.";
        }
    }
}