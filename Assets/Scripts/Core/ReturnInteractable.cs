using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class ReturnInteractable : MonoBehaviour
{
    public float InteractionRadius_ = 2f;
    public string ReturnTextMessage_ = "Back to home";
    public TextMeshProUGUI ReturnText_;
    public Image FadeOverlay_;
    public float FadeDuration_ = 1f;
    public string LobbySceneName_ = "Lobby";

    private bool CanInteract_ = false;
    private Transform PlayerTransform_;
    private PlayerInput PlayerInput_;
    private InputAction InteractAction_;
    private Coroutine FadeCoroutine_;

    void Start()
    {
        GameObject PlayerObject_ = GameObject.FindGameObjectWithTag("Player");
        if (PlayerObject_ != null)
        {
            PlayerTransform_ = PlayerObject_.transform;
            PlayerInput_ = PlayerObject_.GetComponent<PlayerInput>();
            if (PlayerInput_ != null)
            {
                InteractAction_ = PlayerInput_.currentActionMap.FindAction("Interact");
                if (InteractAction_ == null)
                    Debug.LogWarning("Action 'Interact' not found in PlayerInput.");
            }
            else
            {
                Debug.LogWarning("PlayerInput not found on Player.");
            }
        }
        else
        {
            Debug.LogWarning("No Player found with tag.");
        }

        if (ReturnText_ != null)
        {
            ReturnText_.text = ReturnTextMessage_;
            SetTextAlpha_(ReturnText_, 0f);
            ReturnText_.gameObject.SetActive(false);
        }

        if (FadeOverlay_ != null)
        {
            SetImageAlpha_(FadeOverlay_, 0f);
            FadeOverlay_.gameObject.SetActive(false);
        }
    }

    void Update()
    {
        if (CanInteract_)
        {
            if (InteractAction_ != null && InteractAction_.WasPressedThisFrame())
            {
                TryReturnHome_();
            }
        }
    }

    void OnTriggerEnter(Collider Other_)
    {
        if (Other_.CompareTag("Player"))
        {
            CanInteract_ = true;
            if (ReturnText_ != null)
            {
                ReturnText_.gameObject.SetActive(true);
                StartCoroutine(FadeText_(1f, 0.25f));
            }
        }
    }

    void OnTriggerExit(Collider Other_)
    {
        if (Other_.CompareTag("Player"))
        {
            CanInteract_ = false;
            if (ReturnText_ != null)
            {
                StartCoroutine(FadeText_(0f, 0.25f));
            }
        }
    }

    void TryReturnHome_()
    {
        int EnemyCount_ = GameObject.FindGameObjectsWithTag("Enemy").Length;
        if (EnemyCount_ > 0)
        {
            Debug.Log("Defeat all rivals before returning!");
            return;
        }
        
        if (FadeCoroutine_ == null)
        {
            FadeCoroutine_ = StartCoroutine(FadeAndLoadScene_(LobbySceneName_, FadeDuration_));
        }
    }

    IEnumerator FadeText_(float TargetAlpha_, float Duration_)
    {
        if (ReturnText_ == null) yield break;

        float StartAlpha_ = ReturnText_.alpha;
        float Time_ = 0f;

        while (Time_ < Duration_)
        {
            Time_ += Time.deltaTime;
            float NormalizedTime_ = Mathf.Clamp01(Time_ / Duration_);
            float NewAlpha_ = Mathf.Lerp(StartAlpha_, TargetAlpha_, NormalizedTime_);
            SetTextAlpha_(ReturnText_, NewAlpha_);
            yield return null;
        }

        SetTextAlpha_(ReturnText_, TargetAlpha_);
        if (TargetAlpha_ == 0f)
            ReturnText_.gameObject.SetActive(false);
    }

    void SetTextAlpha_(TextMeshProUGUI TmpText_, float Alpha_)
    {
        if (TmpText_ != null)
        {
            Color C_ = TmpText_.color;
            C_.a = Alpha_;
            TmpText_.color = C_;
        }
    }

    IEnumerator FadeAndLoadScene_(string Scene_, float Duration_)
    {
        if (FadeOverlay_ == null)
        {
            SceneManager.LoadScene(Scene_);
            yield break;
        }

        FadeOverlay_.gameObject.SetActive(true);
        float StartAlpha_ = FadeOverlay_.color.a;
        float TargetAlpha_ = 1f;
        float Time_ = 0f;

        while (Time_ < Duration_)
        {
            Time_ += Time.deltaTime;
            float NormalizedTime_ = Mathf.Clamp01(Time_ / Duration_);
            float NewAlpha_ = Mathf.Lerp(StartAlpha_, TargetAlpha_, NormalizedTime_);
            SetImageAlpha_(FadeOverlay_, NewAlpha_);
            yield return null;
        }

        SetImageAlpha_(FadeOverlay_, TargetAlpha_);

        // Marcar que regresó exitosamente  
        PlayerPrefs.SetInt("ReturnedFromMission", 1);
        PlayerPrefs.Save();

        SceneManager.LoadScene(Scene_);

    }

    void SetImageAlpha_(Image Img_, float Alpha_)
    {
        if (Img_ != null)
        {
            Color C_ = Img_.color;
            C_.a = Alpha_;
            Img_.color = C_;
        }
    }
    
    public void ForceTriggerInteract()
    {
        TryReturnHome_();
    }
}