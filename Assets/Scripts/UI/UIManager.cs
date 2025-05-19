using UnityEngine;

public class UIManager : MonoBehaviour
{
    [Header("UI Bars")]
    public UIBarGhostEffect HealthBar_;
    public UIBarGhostEffect StaminaBar_;
    public UIBarGhostEffect ResistanceBar_;

    [Header("Referencias Jugador")]
    public PlayerHealth PlayerHealth_;
    public PlayerStamina PlayerStamina_;
    public PlayerResistance PlayerResistance;

    private void Update()
    {
        // Actualiza las barras como porcentaje entre 0 y 1
        HealthBar_.UpdateInstant_(PlayerHealth_.CurrentHealth_ / PlayerHealth_.MaxHealth_);
        StaminaBar_.UpdateInstant_(PlayerStamina_.CurrentStamina_ / PlayerStamina_.MaxStamina_);
        ResistanceBar_.UpdateInstant_(PlayerResistance.CurrentResistance_ / PlayerResistance.MaxResistance_);
    }
}
// © 2025 KOIYOT. All rights reserved.