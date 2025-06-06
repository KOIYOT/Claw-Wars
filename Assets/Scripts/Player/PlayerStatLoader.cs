using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

[System.Serializable]
public class PlayerStatsData
{
    public float maxHealth;
    public float maxStamina;
    public float walkSpeed;
    public float runSpeed;
    public float strength;
    public float resistance;
}

public class PlayerStatLoader : MonoBehaviour
{
    public string statsUrl = "http://127.0.0.1:5000/stats";

    private void Start()
    {
        StartCoroutine(LoadStatsWithDelay());
    }

    IEnumerator LoadStatsWithDelay()
    {
        // Espera a que todos los otros scripts terminen sus Start()
        yield return new WaitForSeconds(0.1f);
        yield return LoadStats();
    }

    IEnumerator LoadStats()
    {
        UnityWebRequest www = UnityWebRequest.Get(statsUrl);
        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("❌ Error al obtener stats desde la API: " + www.error);
        }
        else
        {
            PlayerStatsData stats = JsonUtility.FromJson<PlayerStatsData>(www.downloadHandler.text);
            ApplyStats(stats);
        }
    }

    void ApplyStats(PlayerStatsData stats)
    {
        var health = GetComponent<PlayerHealth>();
        if (health)
        {
            health.MaxHealth_ = stats.maxHealth;
            Debug.Log($"✅ MaxHealth cargado desde API: {health.MaxHealth_}");
        }

        var stamina = GetComponent<PlayerStamina>();
        if (stamina)
        {
            stamina.MaxStamina_ = stats.maxStamina;
            Debug.Log($"✅ MaxStamina cargada desde API: {stamina.MaxStamina_}");
        }

        var move = GetComponent<PlayerMovement>();
        if (move)
        {
            move.WalkSpeed_ = stats.walkSpeed;
            move.RunSpeed_ = stats.runSpeed;
            Debug.Log($"✅ WalkSpeed: {move.WalkSpeed_} | RunSpeed: {move.RunSpeed_} cargados desde API");
        }

        var combat = GetComponent<PlayerCombat>();
        if (combat)
        {
            var field = typeof(PlayerCombat).GetField("Strength_", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (field != null)
            {
                field.SetValue(combat, stats.strength);
                float strength = (float)field.GetValue(combat);
                Debug.Log($"✅ Strength (daño base) cargado desde API: {strength}");
            }
        }

        var resist = GetComponent<PlayerResistance>();
        if (resist)
        {
            resist.MaxResistance_ = stats.resistance;
            Debug.Log($"✅ Resistance máxima cargada desde API: {resist.MaxResistance_}");
        }

        Debug.Log("✅ Todos los stats fueron aplicados desde la API");
    }
}
