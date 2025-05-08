using UnityEngine.UI; 
using UnityEngine;

public class PlayerUIController : MonoBehaviour
{
    [Header("UI References")]
    public Slider StaminaBar_;
    public Slider LostStaminaBar_;
    public Slider HealthBar;
    public Slider LostHealthBar;
    public Slider ResistanceBar;
    public Slider LostResistanceBar;


    [Tooltip("Velocidad de interpolación para las barras de pérdida (Stamina, Health, Resistance).")]
    public float LerpSpeed_ = 2.5f;

    // Referencia al PlayerController para obtener los valores de las estadísticas
    private PlayerController playerController;

    void Awake()
    {
        // Intentar obtener la referencia al PlayerController.
        // Puedes ajustarlo si tu PlayerController no está en el mismo GameObject
        // o si prefieres obtenerlo de otra forma (por ejemplo, FindObjectOfType).
        playerController = GetComponentInParent<PlayerController>(); // Busca en los padres
        if (playerController == null)
        {
            playerController = FindObjectOfType<PlayerController>(); // Busca en toda la escena como fallback
        }

        if (playerController == null)
        {
            Debug.LogError("PlayerUIController could not find PlayerController reference!");
            // Opcional: Desactivar este script o el GameObject si no se encuentra el PlayerController
            // gameObject.SetActive(false);
        }

        // Configurar los valores máximos de las barras al inicio
        if (playerController != null)
        {
            if (HealthBar != null) HealthBar.maxValue = playerController.MaxHealth;
            if (LostHealthBar != null)
            {
                LostHealthBar.maxValue = playerController.MaxHealth;
                LostHealthBar.value = playerController.CurrentHealth; // Sincronizar al inicio
            }

             if (ResistanceBar != null) ResistanceBar.maxValue = playerController.MaxResistance;
             if (LostResistanceBar != null)
             {
                 LostResistanceBar.maxValue = playerController.MaxResistance;
                 LostResistanceBar.value = playerController.CurrentResistance; // Sincronizar al inicio
             }

             if (StaminaBar_ != null) StaminaBar_.maxValue = playerController.StaminaTop_; // Asumiendo que StaminaTop_ es el máximo
             if (LostStaminaBar_ != null)
             {
                 LostStaminaBar_.maxValue = playerController.StaminaTop_;
                 LostStaminaBar_.value = playerController.Stamina_; // Sincronizar al inicio
             }
        }
    }

    void Update()
    {
        // Si no tenemos la referencia al PlayerController, no hacemos nada.
        if (playerController == null) return;

        // Actualizar la UI de estamina
        if (StaminaBar_ != null)
        {
            StaminaBar_.value = playerController.Stamina_;
        }
        if (LostStaminaBar_ != null)
        {
            // Interpolamos el valor del slider de pérdida hacia el valor actual de estamina
            LostStaminaBar_.value = Mathf.Lerp(LostStaminaBar_.value, playerController.Stamina_, Time.deltaTime * LerpSpeed_);
        }

        // Actualizar la UI de vida
        if (HealthBar != null)
        {
            HealthBar.value = playerController.CurrentHealth;
        }
         if (LostHealthBar != null)
         {
             // --- AÑADIDO: Interpolamos el valor del slider de pérdida de vida ---
             LostHealthBar.value = Mathf.Lerp(LostHealthBar.value, playerController.CurrentHealth, Time.deltaTime * LerpSpeed_);
         }

        // Actualizar la UI de resistencia
        if (ResistanceBar != null)
        {
            ResistanceBar.value = playerController.CurrentResistance;
        }
         if (LostResistanceBar != null)
         {
             // --- AÑADIDO: Interpolamos el valor del slider de pérdida de resistencia ---
             LostResistanceBar.value = Mathf.Lerp(LostResistanceBar.value, playerController.CurrentResistance, Time.deltaTime * LerpSpeed_);
         }
    }

    // Opcional: Un método público si necesitas activar/desactivar la UI desde otro script
    public void SetUIVisible(bool isVisible)
    {
        // Implementar lógica para mostrar u ocultar la UI (por ejemplo, desactivando el GameObject principal de la UI)
        gameObject.SetActive(isVisible); // Esto desactivaría el GameObject al que está adjunto este script
    }
}