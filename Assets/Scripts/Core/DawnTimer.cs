using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using System;

public class DawnTimer : MonoBehaviour
{
    [Header("Inicio de la aventura")]
    [Tooltip("Hora de salida (24 h)")]
    public int startHour = 23;
    [Tooltip("Minuto de salida")]
    public int startMinute = 30;

    [Header("Hora límite de regreso")]
    [Tooltip("Hora de amanecer (24 h), por ejemplo 6 = 6 AM, o 8 = 8 AM")]
    public int dawnHour = 6;
    [Tooltip("Minuto de amanecer")]
    public int dawnMinute = 0;

    [Header("Velocidad de tiempo")]
    [Tooltip("Cuántos segundos reales equivalen a 1 hora de juego (60 = 1 min real = 1 h juego)")]
    public float realSecondsPerGameHour = 60f;

    [Header("Alerta de finalización")]
    [Tooltip("Cantidad de horas de juego restantes para activar la alerta")]
    public float alertThresholdHours = 2f;
    [Tooltip("Magnitud de la vibración cuando hay poca batería de tiempo")]
    public float shakeMagnitude = 5f;

    [Header("UI")]
    [SerializeField] private TextMeshProUGUI clockLabel;

    private float elapsedRealSeconds = 0f;
    private float totalGameHours;
    private float startTimeHours;
    private Vector2 originalAnchoredPos;
    private bool alertActive = false;

    void Start()
    {
        // Guarda la posición original para el shake
        originalAnchoredPos = clockLabel.rectTransform.anchoredPosition;

        // Convierte las horas de inicio y amanecer en horas decimales
        startTimeHours = startHour + startMinute / 60f;
        float dawnTimeHours = dawnHour + dawnMinute / 60f;

        // Calcula cuántas horas de juego hay entre salida y amanecer
        totalGameHours = dawnTimeHours - startTimeHours;
        if (totalGameHours <= 0f) totalGameHours += 24f;

        UpdateClockLabel(startHour, startMinute);
    }

    void Update()
    {
        elapsedRealSeconds += Time.deltaTime;
        float gameHoursPassed = elapsedRealSeconds / realSecondsPerGameHour;
        float hoursLeft = totalGameHours - gameHoursPassed;

        if (gameHoursPassed < totalGameHours)
        {
            // Cálculo de hora actual de juego
            float currentTime = startTimeHours + gameHoursPassed;
            int hour24 = ((int)Mathf.Floor(currentTime)) % 24;
            int minutes = (int)((currentTime - Mathf.Floor(currentTime)) * 60f);
            UpdateClockLabel(hour24, minutes);

            // Verifica umbral de alerta
            if (hoursLeft <= alertThresholdHours)
            {
                if (!alertActive)
                {
                    alertActive = true;
                    clockLabel.color = Color.red;
                }

                // Aplica vibración suave
                Vector2 shakeOffset = UnityEngine.Random.insideUnitCircle * shakeMagnitude;
                clockLabel.rectTransform.anchoredPosition = originalAnchoredPos + shakeOffset;
            }
            else if (alertActive)
            {
                // Restaura estilo
                alertActive = false;
                clockLabel.color = Color.white;
                clockLabel.rectTransform.anchoredPosition = originalAnchoredPos;
            }
        }
        else
        {
            TimeUp();
        }
    }

    private void UpdateClockLabel(int hour24, int minutes)
    {
        int hour12 = hour24 % 12;
        if (hour12 == 0) hour12 = 12;
        string ampm = hour24 < 12 ? "AM" : "PM";
        clockLabel.text = $"{hour12:00}:{minutes:00} {ampm}";
    }

    private void TimeUp()
    {
        int slot = PlayerPrefs.GetInt("LastUsedSlot", -1);
        if (slot >= 0)
        {
            SaveSystem.DeleteSlot(slot);  // Mantiene tu lógica de borrado de guardado
            PlayerPrefs.DeleteKey("LastUsedSlot");
            PlayerPrefs.Save();
        }
        SceneManager.LoadScene("GameOver");
    }
}