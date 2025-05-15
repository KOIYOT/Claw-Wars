using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using System.Collections;
using UnityEngine.UI;
using UnityEngine;
using TMPro;

public class ReturnInteractable : MonoBehaviour
{
    public float InteractionTextFadeDuration_ = 0.25f;
    public string InteractionTextString_ = "Back to home";
    public TextMeshProUGUI InteractTextMeshPro_;

    [Header("Scene Fade Settings")]
    public Image SceneFadeOverlayImage_;
    public float SceneFadeDuration_ = 1.0f;

    private bool CanInteract_ = false;
    private Coroutine FadeCoroutine_;
    private Coroutine SceneFadeCoroutine_;
    private Coroutine FeedbackCoroutine_;
    private Transform PlayerTransform_;
    private PlayerInput PlayerInput_;
    private InputAction InteractAction_;

    void Start()
    {
        GameObject PlayerObject_ = GameObject.FindGameObjectWithTag("Player");
        if (PlayerObject_ != null)
        {
            PlayerTransform_ = PlayerObject_.transform;
            PlayerInput_ = PlayerObject_.GetComponent<PlayerInput>();
            InteractAction_ = PlayerInput_?.currentActionMap?.FindAction("Interact");
        }

        if (InteractTextMeshPro_ != null)
        {
            InteractTextMeshPro_.text = "";
            SetTextAlpha_(InteractTextMeshPro_, 0f);
            InteractTextMeshPro_.gameObject.SetActive(false);
        }

        if (SceneFadeOverlayImage_ != null)
        {
            SetImageAlpha_(SceneFadeOverlayImage_, 0f);
            SceneFadeOverlayImage_.gameObject.SetActive(false);
        }
    }

    void Update()
    {
        if (PlayerTransform_ == null || !CanInteract_) return;

        if (InteractAction_ != null && InteractAction_.WasPressedThisFrame())
        {
            int enemyCount = GameObject.FindGameObjectsWithTag("Enemy").Length;
            if (enemyCount > 0)
            {
                ShowFeedback("Defeat all rivals before returning!");
                return;
            }

            if (SceneFadeCoroutine_ == null)
            {
                SceneFadeCoroutine_ = StartCoroutine(SaveAndFadeOut_("Lobby", SceneFadeDuration_));
            }
        }
    }

    void OnTriggerEnter(Collider Other_)
    {
        if (Other_.CompareTag("Player"))
        {
            CanInteract_ = true;

            int enemyCount = GameObject.FindGameObjectsWithTag("Enemy").Length;
            if (enemyCount <= 0 && InteractTextMeshPro_ != null)
            {
                if (FadeCoroutine_ != null) StopCoroutine(FadeCoroutine_);
                InteractTextMeshPro_.text = InteractionTextString_;
                InteractTextMeshPro_.gameObject.SetActive(true);
                FadeCoroutine_ = StartCoroutine(FadeText_(1f, InteractionTextFadeDuration_));
            }
        }
    }

    void OnTriggerExit(Collider Other_)
    {
        if (Other_.CompareTag("Player"))
        {
            CanInteract_ = false;

            if (FadeCoroutine_ != null) StopCoroutine(FadeCoroutine_);
            FadeCoroutine_ = StartCoroutine(FadeText_(0f, InteractionTextFadeDuration_));

            if (SceneFadeCoroutine_ != null)
            {
                StopCoroutine(SceneFadeCoroutine_);
                SceneFadeCoroutine_ = null;
                if (SceneFadeOverlayImage_ != null)
                {
                    SetImageAlpha_(SceneFadeOverlayImage_, 0f);
                    SceneFadeOverlayImage_.gameObject.SetActive(false);
                }
            }
        }
    }

    IEnumerator FadeText_(float TargetAlpha_, float Duration_)
    {
        if (InteractTextMeshPro_ == null) yield break;

        float StartAlpha_ = InteractTextMeshPro_.alpha;
        float Time_ = 0f;

        while (Time_ < Duration_)
        {
            Time_ += Time.deltaTime;
            float NewAlpha_ = Mathf.Lerp(StartAlpha_, TargetAlpha_, Time_ / Duration_);
            SetTextAlpha_(InteractTextMeshPro_, NewAlpha_);
            yield return null;
        }

        SetTextAlpha_(InteractTextMeshPro_, TargetAlpha_);
        if (TargetAlpha_ == 0f)
            InteractTextMeshPro_.gameObject.SetActive(false);

        FadeCoroutine_ = null;
    }

    void SetTextAlpha_(TextMeshProUGUI TmpText_, float Alpha_)
    {
        if (TmpText_ != null)
        {
            Color c = TmpText_.color;
            c.a = Alpha_;
            TmpText_.color = c;
        }
    }

    IEnumerator SaveAndFadeOut_(string SceneName_, float Duration_)
    {
        int slot = PlayerPrefs.GetInt("LastUsedSlot", -1);
        if (slot >= 0)
        {
            SaveData data = SaveSystem.LoadFromSlot(slot);
            data.level += 1;
            data.lastSaveDate = System.DateTime.Now.ToString();
            SaveSystem.SaveToSlot(slot, data);
        }

        if (SceneFadeOverlayImage_ != null)
        {
            SceneFadeOverlayImage_.gameObject.SetActive(true);
            float Time_ = 0f;
            float StartAlpha_ = SceneFadeOverlayImage_.color.a;

            while (Time_ < Duration_)
            {
                Time_ += Time.deltaTime;
                float NewAlpha_ = Mathf.Lerp(StartAlpha_, 1f, Time_ / Duration_);
                SetImageAlpha_(SceneFadeOverlayImage_, NewAlpha_);
                yield return null;
            }

            SetImageAlpha_(SceneFadeOverlayImage_, 1f);
        }

        SceneManager.LoadScene(SceneName_);
    }

    void SetImageAlpha_(Image Img_, float Alpha_)
    {
        if (Img_ != null)
        {
            Color c = Img_.color;
            c.a = Alpha_;
            Img_.color = c;
        }
    }

    void ShowFeedback(string message)
    {
        if (InteractTextMeshPro_ == null) return;

        if (FeedbackCoroutine_ != null) StopCoroutine(FeedbackCoroutine_);
        FeedbackCoroutine_ = StartCoroutine(ShowFeedbackRoutine(message));
    }

    IEnumerator ShowFeedbackRoutine(string message)
    {
        InteractTextMeshPro_.text = message;
        InteractTextMeshPro_.gameObject.SetActive(true);
        SetTextAlpha_(InteractTextMeshPro_, 1f);
        yield return new WaitForSeconds(2f);

        int enemyCount = GameObject.FindGameObjectsWithTag("Enemy").Length;
        if (enemyCount <= 0)
        {
            InteractTextMeshPro_.text = InteractionTextString_;
        }
        else
        {
            InteractTextMeshPro_.text = "";
            InteractTextMeshPro_.gameObject.SetActive(false);
        }

        FeedbackCoroutine_ = null;
    }
}