using UnityEngine.InputSystem;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Movimiento")]
    public float WalkSpeed_ = 2f;
    public float RunSpeed_ = 5f;
    public float RotationSpeed_ = 10f;

    [Header("Salto y Gravedad")]
    public float JumpHeight_ = 1.5f;
    public float Gravity_ = -9.81f;

    [Header("Stamina")]
    public float RunStaminaCostPerSecond_ = 2f;

    [Header("Referencias")]
    public Transform CameraTransform_;
    public Animator Animator_;

    private CharacterController Controller_;
    private Vector2 InputMovement_;
    private Vector3 Velocity_;
    private bool IsRunning_;
    private bool IsGrounded_;
    private bool CanRun_;
    private PlayerStamina Stamina_;

    private void Awake()
    {
        Controller_ = GetComponent<CharacterController>();
        Stamina_ = GetComponent<PlayerStamina>();

        // Aplicar multiplicador de velocidad desde SaveData
        int slot = PlayerPrefs.GetInt("LastUsedSlot", -1);
        float speedMult = 1f;
        if (slot >= 0 && SaveSystem.HasSaveData(slot))
        {
            SaveData data = SaveSystem.LoadFromSlot(slot);
            speedMult = data.Speed_;
        }

        WalkSpeed_ *= speedMult;
        RunSpeed_ *= speedMult;
    }

    public void OnInteract(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            Collider[] hits = Physics.OverlapSphere(transform.position, 2f);
            foreach (Collider hit in hits)
            {
                if (hit.TryGetComponent<ReturnInteractable>(out var returner))
                {
                    returner.ForceTriggerInteract();
                    return;
                }

                if (hit.TryGetComponent<MissionInteractable>(out var mission))
                {
                    mission.ForceTriggerInteract();
                    return;
                }
            }
            Debug.Log("No hay misión cerca para interactuar.");
        }
    }

    public void OnMove_(InputAction.CallbackContext context)
    {
        InputMovement_ = context.ReadValue<Vector2>();
    }

    public void OnRun_(InputAction.CallbackContext context)
    {
        IsRunning_ = context.ReadValueAsButton();
    }

    public void OnJump_(InputAction.CallbackContext context)
    {
        if (context.started && IsGrounded_ && Stamina_.UseStamina_(10f))
        {
            Velocity_.y = Mathf.Sqrt(JumpHeight_ * -2f * Gravity_);
        }
    }

    private void Update()
    {
        IsGrounded_ = Controller_.isGrounded;
        if (IsGrounded_ && Velocity_.y < 0)
        {
            Velocity_.y = -2f;
        }

        Vector3 direction = new Vector3(InputMovement_.x, 0, InputMovement_.y).normalized;

        if (direction.magnitude >= 0.1f)
        {
            float targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg + CameraTransform_.eulerAngles.y;
            float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref RotationSpeed_, 0.1f);
            transform.rotation = Quaternion.Euler(0, angle, 0);

            Vector3 moveDir = Quaternion.Euler(0, targetAngle, 0) * Vector3.forward;

            bool isMoving = InputMovement_.magnitude > 0.1f;

            if (IsRunning_ && isMoving)
            {
                if (Stamina_.CurrentStamina_ >= RunStaminaCostPerSecond_ * Time.deltaTime)
                {
                    Stamina_.UseStamina_(RunStaminaCostPerSecond_ * Time.deltaTime);
                    CanRun_ = true;
                }
                else
                {
                    CanRun_ = false;
                }
            }
            else
            {
                CanRun_ = false;
            }

            float speed = CanRun_ ? RunSpeed_ : WalkSpeed_;
            Controller_.Move(moveDir.normalized * speed * Time.deltaTime);
            Animator_.SetFloat("Speed", speed);
        }
        else
        {
            Animator_.SetFloat("Speed", 0);
        }

        Velocity_.y += Gravity_ * Time.deltaTime;
        Controller_.Move(Velocity_ * Time.deltaTime);
    }
}
// © 2025 KOIYOT. All rights reserved.
