using UnityEngine.SceneManagement;
using System.Collections;
using UnityEngine.UI;
using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    public float MaxHealth_ = 100f;
    public float CurrentHealth_ = 100f;

    private Animator Animator_;
    public string GameOverSceneName = "GameOver";
    public Image FadePanel_;
    public float FadeDuration_ = 1f;

    private bool isDying = false;

    private void Awake()
    {
        CurrentHealth_ = MaxHealth_;
        Animator_ = GetComponentInChildren<Animator>();
    }

    public void TakeDamage_(float amount)
    {
        if (isDying) return;

        var block = GetComponent<PlayerBlock>();
        var resist = GetComponent<PlayerResistance>();

        if (block != null && block.IsBlocking_ && resist != null && resist.CanBlock_)
        {
            resist.AbsorbHit_(amount);
            Debug.Log("Golpe bloqueado. Resistencia actual: " + resist.CurrentResistance_);
            return;
        }

        CurrentHealth_ -= amount;
        Debug.Log("Daño recibido: " + amount + " | Vida actual: " + CurrentHealth_);

        if (CurrentHealth_ <= 0f)
        {
            CurrentHealth_ = 0f;
            Die_();
        }
    }
    private void Die_()
    {
        if (isDying) return;
        isDying = true;

        Debug.Log("El jugador ha muerto.");

        var move = GetComponent<PlayerMovement>();
        if (move != null) move.enabled = false;

        var combat = GetComponent<PlayerCombat>();
        if (combat != null) combat.enabled = false;

        var controller = GetComponent<CharacterController>();
        if (controller != null) controller.enabled = false;

        Animator_?.SetTrigger("Die");

        int lastSlot = PlayerPrefs.GetInt("LastUsedSlot", -1);
        if (lastSlot != -1)
        {
            SaveSystem.DeleteSlot(lastSlot);
            Debug.Log("Partida eliminada del slot: " + lastSlot);
        }

        if (FadePanel_ != null)
            StartCoroutine(FadeAndLoadScene_(GameOverSceneName));
        else
            SceneManager.LoadScene(GameOverSceneName);
    }

    private IEnumerator FadeAndLoadScene_(string sceneName)
    {
        Color original = FadePanel_.color;
        float time = 0f;

        while (time < FadeDuration_)
        {
            time += Time.deltaTime;
            float alpha = Mathf.Lerp(0f, 1f, time / FadeDuration_);
            FadePanel_.color = new Color(original.r, original.g, original.b, alpha);
            yield return null;
        }

        SceneManager.LoadScene(sceneName);
    }
}