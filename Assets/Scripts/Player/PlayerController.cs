using UnityEngine.InputSystem;
using System.Collections;
using UnityEngine.UI; // Necesario para Sliders
using UnityEngine;
using System;

public class PlayerController : MonoBehaviour
{
    public static PlayerController Instance { get; private set; }

    public Animator Animator_;
    public Transform cameraTransform;
    public PlayerState currentState;
    public CharacterController CharacterController_;

    [Header("Movement")]
    public float Speed_ = 5f;
    public Vector2 MovementInput_;
    public float MovementSmoothness_ = 10f;
    public float RotationSmoothness_ = 10f;
    [HideInInspector] public Vector3 velocity;
    [HideInInspector] public Vector3 currentVelocity;

    // --- AÑADIDO: PROPIEDAD PARA LA VELOCIDAD HORIZONTAL CALCULADA POR LOS ESTADOS ---
    [HideInInspector] public Vector3 DesiredHorizontalVelocity;


    [Header("Sprint")]
    public bool SprintPressed_;
    public float SprintSpeed_ = 10f;

    [Header("Stamina")]
    public float Stamina_ = 5f;
    public float StaminaTop_ = 5f;
    public float StaminaRegenerationRate_ = 1f;
    public float StaminaDrainRate_ = 2f;
    public float StaminaCooldownDuration_ = 1f;
    private float StaminaCooldownTimer_ = 0f;
    public bool IsExhausted_ = false;

    [Header("Dash")]
    public bool DashPressed_;
    public float DashSpeed_ = 20f;
    public float DashStaminaDrain_ = 1f;
    public float DashDuration_ = 0.25f;
     // Opcional: Invulnerabilidad durante el dash
    public bool IsInvulnerableDuringDash = true;
    [HideInInspector] public bool isDashing = false; // Flag para indicar si está dasheando

    [Header("Jump")]
    public bool JumpPressed_;
    public float JumpHeight_ = 0.85f;
    public float JumpStaminaDrain_ = 1f; // Si el salto cuesta estamina
    public float Gravity_ = -9.81f;
    public bool IsGrounded_;

    [Header("Crouch")]
    public bool CrouchPressed_;
    public float CrouchSpeed_ = 2f;
    public float CrouchHeight_ = 0.5f;
    [HideInInspector] public float OriginalHeight_;
    // Opcional: Offset para el centro del CharacterController al agacharse
    [HideInInspector] public Vector3 OriginalCenter_;


    // --- AÑADIDO: PROPIEDADES PARA COMBATE ---
    [Header("Combat - Health")]
    public float MaxHealth = 100f;
    [HideInInspector] public float CurrentHealth;
    [Tooltip("Tiempo de invulnerabilidad después de recibir daño.")]
    public float HitInvulnerabilityDuration = 0.5f; // Invulnerabilidad después de ser golpeado
    private float hitInvulnerabilityTimer = 0f;


    [Header("Combat - Resistance")]
    public float MaxResistance = 100f;
    [HideInInspector] public float CurrentResistance;
    public float ResistanceRegenerationRate = 10f;
    public float ResistanceRegenCooldown = 2f; // Tiempo antes de que la resistencia empiece a regenerarse
    public float resistanceRegenTimer = 0f; // Ya estaba, pero lo mantengo
    [HideInInspector] public bool IsResistanceBroken = false;
    public float ResistanceBrokenDuration = 3f; // Duración del estado de resistencia rota
    private float resistanceBrokenTimer = 0f;


    [Header("Combat - Attacks")]
    // Puedes definir structs o clases para organizar mejor los datos de ataque si tienes muchos tipos
    // [System.Serializable]
    // public class AttackData { ... }
    // public AttackData lightAttack;
    // public AttackData heavyAttack;

    public float LightAttackDamage = 10f;
    public float LightAttackStaminaCost = 5f;
    public float LightAttackDuration = 0.5f; // Duración de la animación/estado de ataque ligero
    public float LightAttackResistanceDamage = 5f; // Daño a la resistencia del enemigo
    public float LightAttackMovementPenalty = 0.5f; // Multiplicador de velocidad mientras ataca (0-1)


    public float HeavyAttackDamage = 25f;
    public float HeavyAttackStaminaCost = 15f;
    public float HeavyAttackDuration = 1f; // Duración de la animación/estado de ataque pesado
    public float HeavyAttackResistanceDamage = 15f;
    public float HeavyAttackMovementPenalty = 0.2f;

    public float ChargedAttackMinChargeTime = 0.5f; // Tiempo mínimo para que se considere cargado
    public float ChargedAttackMaxChargeTime = 2f; // Tiempo máximo de carga
    public float ChargedAttackMinDamage = 30f; // Daño mínimo cargado
    public float ChargedAttackMaxDamage = 60f; // Daño máximo cargado
    public float ChargedAttackMinResistanceDamage = 20f; // Daño a resistencia mínimo
    public float ChargedAttackMaxResistanceDamage = 40f; // Daño a resistencia máximo
    public float ChargedAttackStaminaCost = 20f; // Costo fijo de estamina para un ataque cargado
    public float ChargedAttackDuration = 1.2f; // Duración de la animación/estado de ataque cargado
    public float CriticalHitChance = 0.1f; // Probabilidad de golpe crítico para ataques cargados
    public float CriticalHitMultiplier = 1.5f; // Multiplicador de daño para golpes críticos
    public float ChargedAttackMovementPenalty = 0.1f;

    // Propiedades para combos (esto es más complejo, lo abordaremos después)
    // public List<AttackData> ComboAttacks;
    [Header("Combat - Combo")]
    public float ComboInputWindow = 0.5f; // Tiempo después de un ataque para ingresar el siguiente combo
    private float comboTimer = 0f;
    private bool canPerformCombo = false;
    private int currentComboStep = 0; // Para rastrear en qué paso del combo estamos


    [Header("Combat - Blocking")]
    [HideInInspector] public bool BlockPressed_ = false; // Variable para la entrada de bloqueo
    public float BlockingResistanceCostPerHit = 10f; // Resistencia perdida al bloquear un golpe
    public float BlockDamageReduction = 0.5f; // Reducción de daño al bloquear (0.5 = 50%)
    public float BlockingMovementPenalty = 0.4f;


    [Header("Combat - Hit Reaction")]
    public float HitReactionDuration = 0.5f; // Duración de la animación de impacto
    [HideInInspector] private float hitReactionTimer = 0f;
    [HideInInspector] public bool isReactingToHit = false;


    [Header("Combat - Death")]
    [HideInInspector] public bool IsDead = false;
    // Aquí podrías tener una referencia a un script para activar el ragdoll
    public GameObject RagdollObject; // Referencia al objeto que contiene el ragdoll
    private Collider[] ragdollColliders;
    private Rigidbody[] ragdollRigidbodies;


    public InputSystem InputActions_;

     [Header("Attack Hitboxes")]
     // Podrías tener referencias a GameObjects o Scripts que representen las hitboxes
     public GameObject LightAttackHitbox;
     public GameObject HeavyAttackHitbox;
     public GameObject ChargedAttackHitbox;


    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        InputActions_ = new InputSystem();

        // Inicializar vida y resistencia al inicio
        CurrentHealth = MaxHealth;
        CurrentResistance = MaxResistance;

        // Inicializar ragdoll (si existe)
        if (RagdollObject != null)
        {
            ragdollColliders = RagdollObject.GetComponentsInChildren<Collider>();
            ragdollRigidbodies = RagdollObject.GetComponentsInChildren<Rigidbody>();
            SetRagdollEnabled(false); // Deshabilitar ragdoll al inicio
        }
    }

    void OnEnable()
    {
        // Habilitar los Action Maps necesarios (CORREGIDO: usando Move y Combat)
        InputActions_.Move.Enable();
        InputActions_.Combat.Enable();
        // Si usas Camera o UI en este script, habilítalos también:
        // InputActions_.Camera.Enable();
        // InputActions_.UI.Enable();


        // Suscribir acciones de movimiento al Action Map "Move"
        InputActions_.Move.Move.performed += ctx => MovementInput_ = ctx.ReadValue<Vector2>();
        InputActions_.Move.Move.canceled += ctx => MovementInput_ = Vector2.zero;

        InputActions_.Move.Run.performed += ctx => SprintPressed_ = true;
        InputActions_.Move.Run.canceled += ctx => SprintPressed_ = false;

        InputActions_.Move.Dash.performed += ctx => DashPressed_ = true;
        InputActions_.Move.Dash.canceled += ctx => DashPressed_ = false;

        InputActions_.Move.Jump.performed += ctx => JumpPressed_ = true;

        InputActions_.Move.Crouch.performed += ctx => CrouchPressed_ = true;
        InputActions_.Move.Crouch.canceled += ctx => CrouchPressed_ = false;

        // --- AÑADIDO: EVENTOS PARA ENTRADA DE COMBATE ---
        // Suscribir acciones de combate al Action Map "Combat"
        InputActions_.Combat.LightAttack.performed += ctx => OnLightAttack();
        InputActions_.Combat.HeavyAttack.performed += ctx => OnHeavyAttack();
        InputActions_.Combat.HeavyAttack.canceled += ctx => OnHeavyAttackCanceled(); // Para ataque cargado
        InputActions_.Combat.Block.performed += ctx => OnBlockPressed();
        InputActions_.Combat.Block.canceled += ctx => OnBlockReleased();

        // Suscribir acciones de cámara o UI si es necesario
        // InputActions_.Camera...
        // InputActions_.UI...
    }

    void OnDisable()
    {
        // Deshabilitar los Action Maps (CORREGIDO: usando Move y Combat)
        InputActions_.Move.Disable();
        InputActions_.Combat.Disable();
        // InputActions_.Camera.Disable();
        // InputActions_.UI.Disable();


        // --- Desuscribirse de eventos de movimiento ---
        InputActions_.Move.Move.performed -= ctx => MovementInput_ = ctx.ReadValue<Vector2>();
        InputActions_.Move.Move.canceled -= ctx => MovementInput_ = Vector2.zero;

        InputActions_.Move.Run.performed -= ctx => SprintPressed_ = true;
        InputActions_.Move.Run.canceled -= ctx => SprintPressed_ = false;

        InputActions_.Move.Dash.performed -= ctx => DashPressed_ = true;
        InputActions_.Move.Dash.canceled -= ctx => DashPressed_ = false;

        InputActions_.Move.Jump.performed -= ctx => JumpPressed_ = true;

        InputActions_.Move.Crouch.performed -= ctx => CrouchPressed_ = true;
        InputActions_.Move.Crouch.canceled -= ctx => CrouchPressed_ = false;

        // --- Desuscribirse de eventos de combate ---
        InputActions_.Combat.LightAttack.performed -= ctx => OnLightAttack();
        InputActions_.Combat.HeavyAttack.performed -= ctx => OnHeavyAttack();
        InputActions_.Combat.HeavyAttack.canceled -= ctx => OnHeavyAttackCanceled();
        InputActions_.Combat.Block.performed -= ctx => OnBlockPressed();
        InputActions_.Combat.Block.canceled -= ctx => OnBlockReleased();

         // Desuscribirse de acciones de cámara o UI si es necesario
        // InputActions_.Camera...
        // InputActions_.UI...
    }

    void Start()
    {
        Animator_ = GetComponentInChildren<Animator>();
        CharacterController_ = GetComponent<CharacterController>();
        OriginalHeight_ = CharacterController_.height;
        OriginalCenter_ = CharacterController_.center; // Guardar centro original

        ChangeState(new IdleState(this));
        UpdateCombatUI(); // Llamada inicial para mostrar valores correctos

        // Deshabilitar hitboxes de ataque al inicio
        SetAttackHitboxesEnabled(false);
    }

    public void SetAnimation(string param, bool value)
    {
        if (Animator_ != null) Animator_.SetBool(param, value);
    }

    public void SetAnimationFloat(string param, float value)
    {
        if (Animator_ != null) Animator_.SetFloat(param, value);
    }

    void Update()
    {
        // --- AÑADIDO: Lógica de Actualización para Combate ---
        if (!IsDead)
        {
            // Actualizar UI de vida y resistencia
            UpdateCombatUI();

            // Lógica de invulnerabilidad después de ser golpeado
            if (hitInvulnerabilityTimer > 0)
            {
                hitInvulnerabilityTimer -= Time.deltaTime;
            }


            // Lógica de regeneración de resistencia
            // Solo regenera si no está en estado de resistencia rota, no está reaccionando a un golpe
            // y no está bloqueando. También si no está corriendo o dasheando (generalmente)
            if (!IsResistanceBroken && !isReactingToHit && !(currentState is BlockingState) && !(currentState is RunningState) && !(currentState is DashState) && CurrentResistance < MaxResistance)
            {
                if (resistanceRegenTimer > 0)
                {
                    resistanceRegenTimer -= Time.deltaTime;
                }
                else
                {
                    CurrentResistance += ResistanceRegenerationRate * Time.deltaTime;
                    CurrentResistance = Mathf.Clamp(CurrentResistance, 0, MaxResistance);
                }
            } else if (currentState is BlockingState) {
                 // Si está bloqueando, reiniciar el timer de regeneración si recibe daño a la resistencia
                 if (CurrentResistance < MaxResistance)
                 {
                     resistanceRegenTimer = ResistanceRegenCooldown;
                 }
            }


            // Lógica de estado de resistencia rota
            if (IsResistanceBroken)
            {
                resistanceBrokenTimer -= Time.deltaTime;
                if (resistanceBrokenTimer <= 0)
                {
                    IsResistanceBroken = false;
                    // Al salir del estado de resistencia rota, pasar a un estado de movimiento normal
                    if (currentState is ResistanceBrokenState)
                    {
                        ChangeState(MovementInput_.magnitude > 0 ? new WalkingState(this) : new IdleState(this));
                    }
                }
            }

            // Lógica de reacción a impacto
            if (isReactingToHit)
            {
                hitReactionTimer -= Time.deltaTime;
                if (hitReactionTimer <= 0)
                {
                    isReactingToHit = false;
                    // Volver al estado anterior o a Idle/Walking después de la reacción
                    if (currentState is HitReactionState)
                    {
                         ChangeState(MovementInput_.magnitude > 0 ? new WalkingState(this) : new IdleState(this));
                    }
                }
            }

            // Lógica de combos
            if (canPerformCombo)
            {
                comboTimer -= Time.deltaTime;
                if (comboTimer <= 0)
                {
                    canPerformCombo = false;
                    currentComboStep = 0; // Resetear combo si expira el tiempo
                    Debug.Log("Combo window closed. Combo reset."); // Placeholder
                }
            }


            currentState.UpdateState();

            // --- CORREGIDO: Lógica de Gravedad y Movimiento Combinado ---
            // No aplicar movimiento del CharacterController si estamos reaccionando a un golpe o en un estado de combate que lo deshabilite
            if (!isReactingToHit && !(currentState is ResistanceBrokenState) && !(currentState is DeadState))
            {
                 IsGrounded_ = CharacterController_.isGrounded;
                 if (IsGrounded_ && velocity.y < 0)
                     velocity.y = 0f;

                 velocity.y += Gravity_ * Time.deltaTime;

                 // Combinar la velocidad horizontal deseada con la velocidad vertical
                 Vector3 totalMovement = DesiredHorizontalVelocity + new Vector3(0, velocity.y, 0);


                 CharacterController_.Move(totalMovement * Time.deltaTime); // Mover una sola vez
            } else {
                 // Si estamos en un estado que inhabilita el movimiento, solo aplicar gravedad si el CharacterController está habilitado
                 if (CharacterController_.enabled)
                 {
                      IsGrounded_ = CharacterController_.isGrounded;
                      if (IsGrounded_ && velocity.y < 0)
                          velocity.y = 0f;

                      velocity.y += Gravity_ * Time.deltaTime;
                      // Asegurarse de que no haya movimiento horizontal en estados inhabilitados
                      DesiredHorizontalVelocity = Vector3.zero;
                      Vector3 totalMovement = DesiredHorizontalVelocity + new Vector3(0, velocity.y, 0);
                      CharacterController_.Move(totalMovement * Time.deltaTime);
                 }
            }


            // Lógica de estamina (CORREGIDO: permite regeneración en JumpingState)
            if (!(currentState is RunningState) && !(currentState is DashState) && !(currentState is AttackingState) && !(currentState is BlockingState) && !(currentState is HitReactionState) && !(currentState is ResistanceBrokenState) && !(currentState is DeadState))
            {
                 if (IsExhausted_)
                {
                    if (StaminaCooldownTimer_ > 0)
                    {
                        StaminaCooldownTimer_ -= Time.deltaTime;
                    }
                    else
                    {
                        Stamina_ += StaminaRegenerationRate_ * Time.deltaTime;
                        if (Stamina_ >= StaminaTop_)
                            IsExhausted_ = false;
                    }
                }
                else
                {
                    if (Stamina_ < StaminaTop_)
                    {
                        Stamina_ += StaminaRegenerationRate_ * Time.deltaTime;
                    }
                }
                Stamina_ = Mathf.Clamp(Stamina_, 0, StaminaTop_);
            }


        }
        // Si está muerto, no hacer nada de lo anterior (la lógica de muerte ya deshabilitó cosas)
        else
        {
            // Lógica que se ejecuta continuamente mientras está muerto (si es necesaria, por ejemplo, aplicar gravedad al ragdoll)
        }
    }

    // --- AÑADIDO: Métodos de combate ---

    // Método para habilitar/deshabilitar hitboxes de ataque
    public void SetAttackHitboxesEnabled(bool enabled, AttackType type = AttackType.None)
    {
        // Deshabilitar todas las hitboxes primero para evitar superposiciones si no se especifica un tipo
        if (LightAttackHitbox != null) LightAttackHitbox.SetActive(false);
        if (HeavyAttackHitbox != null) HeavyAttackHitbox.SetActive(false);
        if (ChargedAttackHitbox != null) ChargedAttackHitbox.SetActive(false);


        // Si se especifica un tipo y se habilita, asegurarse de habilitar solo esa
        if (enabled && type != AttackType.None)
        {
             if (type == AttackType.Light && LightAttackHitbox != null) LightAttackHitbox.SetActive(true);
             if (type == AttackType.Heavy && HeavyAttackHitbox != null) HeavyAttackHitbox.SetActive(true);
             if (type == AttackType.Charged && ChargedAttackHitbox != null) ChargedAttackHitbox.SetActive(true);
        }

        // Debug.Log($"Setting Hitboxes Enabled: {enabled} for type: {type}"); // Placeholder
    }


    // Enum para tipos de ataque
    public enum AttackType
    {
        None,
        Light,
        Heavy,
        Charged,
        Combo1, // Ejemplo para futuros combos
        Combo2
    }

    // Método para que los estados de ataque llamen al detectar un golpe
    // Recibe información sobre el ataque realizado para calcular daño, etc.
    public void HandleAttackHit(GameObject hitObject, AttackType attackType, float chargeTime = 0f)
    {
        // Aquí iría la lógica para infligir daño y daño a la resistencia al hitObject
        // Esto dependerá de cómo implementes el sistema de salud y resistencia en los enemigos.
        // Deberías buscar un componente en hitObject que maneje la salud/resistencia enemiga
        // Por ejemplo: hitObject.GetComponent<EnemyHealthSystem>()?.TakeDamage(calculatedDamage, calculatedResistanceDamage, IsResistanceBroken);

        float damage = 0f;
        float resistanceDamage = 0f;
        bool isCritical = false;

        switch (attackType)
        {
            case AttackType.Light:
                damage = LightAttackDamage;
                resistanceDamage = LightAttackResistanceDamage;
                break;
            case AttackType.Heavy:
                damage = HeavyAttackDamage;
                resistanceDamage = HeavyAttackResistanceDamage;
                break;
            case AttackType.Charged:
                // Calcular daño y daño a resistencia basado en el tiempo de carga
                float chargePercentage = Mathf.Clamp01((chargeTime - ChargedAttackMinChargeTime) / (ChargedAttackMaxChargeTime - ChargedAttackMinChargeTime));
                damage = Mathf.Lerp(ChargedAttackMinDamage, ChargedAttackMaxDamage, chargePercentage);
                resistanceDamage = Mathf.Lerp(ChargedAttackMinResistanceDamage, ChargedAttackMaxResistanceDamage, chargePercentage);

                // Aplicar golpe crítico si aplica
                isCritical = UnityEngine.Random.value < CriticalHitChance;
                if (isCritical)
                {
                    damage *= CriticalHitMultiplier;
                    resistanceDamage *= CriticalHitMultiplier; // Los críticos también rompen más resistencia
                    Debug.Log("Critical Hit!"); // Placeholder
                }
                break;
                // Añadir casos para combos si es necesario
        }

        // --- Lógica para aplicar daño y daño a resistencia al enemigo ---
        // Necesitas una forma de que el enemigo reciba esta información.
        // Por ejemplo, una interfaz ICanBeHit o un componente específico.
        // Ejemplo hipotético:
        // ICanBeHit enemy = hitObject.GetComponent<ICanBeHit>();
        // if (enemy != null)
        // {
        //     enemy.ReceiveHit(damage, resistanceDamage, isCritical, transform.position); // Pasar información del golpe
        // }
        // --- FIN Lógica para aplicar daño ---

        Debug.Log($"Hit detected on {hitObject.name} with {attackType} - Damage: {damage}, Resistance Damage: {resistanceDamage}"); // Placeholder
    }


    public void ChangeState(PlayerState newState)
    {
        if (currentState != null)
            currentState.ExitState();

        currentState = newState;
        currentState.EnterState();
    }

    public void Jump()
    {
        // Verificar estamina para saltar si es necesario
        // if (!IsExhausted_ && Stamina_ >= JumpStaminaDrain_)
        // {
             velocity.y = Mathf.Sqrt(JumpHeight_ * -2f * Gravity_);
             // Stamina_ -= JumpStaminaDrain_;
             // Stamina_ = Mathf.Max(Stamina_, 0);
             // if (Stamina_ <= 0) IsExhausted_ = true;
        // } else {
             // Si no hay estamina, no saltar o saltar más bajo
             // JumpPressed_ = false; // Resetear la entrada si no se puede saltar
        // }
         velocity.y = Mathf.Sqrt(JumpHeight_ * -2f * Gravity_); // Salto sin costo de estamina por ahora
    }

    public void StartStaminaCooldown()
    {
        StaminaCooldownTimer_ = StaminaCooldownDuration_;
    }

    public Vector3 GetDashDirection()
    {
        if (MovementInput_.magnitude > 0.1f)
        {
            Vector3 camForward = new Vector3(cameraTransform.forward.x, 0f, cameraTransform.forward.z).normalized;
            Vector3 camRight = new Vector3(cameraTransform.right.x, 0f, cameraTransform.right.z).normalized;
            return (camForward * MovementInput_.y + camRight * MovementInput_.x).normalized;
        }

        return transform.forward;
    }
    public void PlayDashAnimation(float duration)
    {
        StartCoroutine(DashAnimTimer(duration));
    }

    private IEnumerator DashAnimTimer(float time)
    {
        isDashing = true; // Activar flag de dasheando
        SetAnimation("isDashing", true);
        // Opcional: Activar invulnerabilidad si aplica
        // if (IsInvulnerableDuringDash) { /* Lógica para activar invulnerabilidad */ }
        yield return new WaitForSeconds(time);
        SetAnimation("isDashing", false);
        isDashing = false; // Desactivar flag de dasheando
        // Opcional: Desactivar invulnerabilidad
        // if (IsInvulnerableDuringDash) { /* Lógica para desactivar invulnerabilidad */ }
    }

    // --- AÑADIDO: Métodos para manejar la entrada de combate ---
    public void OnLightAttack()
    {
        // Restricciones para atacar: No muerto, no reaccionando, no dasheando, no rompiendo resistencia, no atacando ya, no bloqueando, no cargando heavy
        if (!IsDead && !isReactingToHit && !isDashing && !IsResistanceBroken && !(currentState is AttackingState) && !(currentState is HeavyAttackChargeState) && !(currentState is BlockingState))
        {
            // Lógica de combos: Si puede hacer combo y el input es rápido
            if (canPerformCombo && currentComboStep < 2) // Ejemplo: combo de hasta 3 golpes ligeros (pasos 1, 2, 3)
            {
                currentComboStep++;
                ChangeState(new LightAttackState(this, currentComboStep)); // Pasar el paso del combo
                canPerformCombo = false; // Se reinicia la ventana de combo al iniciar el siguiente ataque
            }
            // Si no puede hacer combo (primer ataque) o el combo se reseteó
            else if (!canPerformCombo || currentComboStep == 0)
            {
                 if (Stamina_ >= LightAttackStaminaCost) // Verificar costo de estamina
                 {
                     currentComboStep = 1; // Iniciar el primer paso del combo
                     ChangeState(new LightAttackState(this, currentComboStep));
                 }
                 else
                 {
                     Debug.Log("Not enough stamina for Light Attack!"); // Placeholder
                     // Podrías hacer un sonido o animación de intento fallido
                 }
            } else {
                 // Si presionó light attack pero no está en la ventana de combo o el combo terminó, reiniciar combo
                 currentComboStep = 0;
                 canPerformCombo = false;
                 // Podrías intentar realizar el primer golpe ligero si tiene estamina
                 if (Stamina_ >= LightAttackStaminaCost)
                 {
                     currentComboStep = 1;
                     ChangeState(new LightAttackState(this, currentComboStep));
                 } else {
                      Debug.Log("Not enough stamina for Light Attack!"); // Placeholder
                 }
            }
        } else {
            // Debug.Log("Cannot attack right now."); // Placeholder
        }
    }

    private float heavyAttackChargeTimer = 0f;
    public bool isHeavyCharging = false; // Flag para saber si está en el proceso de carga (desde que se presiona hasta que se suelta o lanza el ataque)


    public void OnHeavyAttack()
    {
        // Restricciones para atacar pesado: Similares al ataque ligero
        if (!IsDead && !isReactingToHit && !isDashing && !IsResistanceBroken && !(currentState is AttackingState) && !(currentState is HeavyAttackChargeState) && !(currentState is BlockingState))
        {
            // Si puede hacer combo ligero, un heavy attack podría romper la secuencia de combo ligero
            if (canPerformCombo)
            {
                canPerformCombo = false;
                currentComboStep = 0; // Resetear combo ligero
                 Debug.Log("Light combo interrupted by Heavy Attack attempt."); // Placeholder
            }

            // Verificar estamina inicial para empezar a cargar
            // Puedes tener un costo inicial bajo para empezar a cargar, y el costo principal al lanzar.
            // O todo el costo al lanzar si alcanzó el tiempo mínimo.
             if (Stamina_ >= 5f) // Ejemplo: un costo mínimo para iniciar la carga
             {
                 isHeavyCharging = true; // Activar el flag de carga
                 heavyAttackChargeTimer = 0f;
                 ChangeState(new HeavyAttackChargeState(this)); // Nuevo estado para cargar ataque
             } else {
                  Debug.Log("Not enough stamina to start Heavy Attack charge!"); // Placeholder
             }
        }
    }

    public void OnHeavyAttackCanceled()
    {
        // Este método se llama cuando se suelta el botón de Heavy Attack.
        // La lógica principal para decidir si lanzar el ataque cargado o cancelarlo
        // está en el UpdateState() del estado HeavyAttackChargeState.
        // Este método principalmente desactiva el flag de carga.
        if (isHeavyCharging)
        {
             isHeavyCharging = false; // Desactivar el flag de carga
            // El estado HeavyAttackChargeState detectará que isHeavyCharging es false y actuará.
        }
    }

    public void OnBlockPressed()
    {
         // Restricciones para bloquear: No muerto, no reaccionando, no dasheando, no rompiendo resistencia, no atacando (ligero o cargando heavy)
         if (!IsDead && !isReactingToHit && !isDashing && !IsResistanceBroken && !(currentState is AttackingState) && !(currentState is HeavyAttackChargeState))
         {
             BlockPressed_ = true; // Actualizar la propiedad
             // Solo cambiar a estado de bloqueo si no estamos ya bloqueando
             if (!(currentState is BlockingState))
             {
                 ChangeState(new BlockingState(this)); // Nuevo estado para bloquear
             }
         }
    }

    private void OnBlockReleased()
    {
         BlockPressed_ = false; // Actualizar la propiedad
         // El estado BlockingState se encargará de la transición cuando se suelta el botón
    }


    // --- AÑADIDO: Métodos relacionados con combate (Recibir daño, etc.) ---

    // Método para que otros objetos llamen al infligir daño al jugador
    public void TakeDamage(float damageAmount)
    {
        // No recibir daño si ya estás muerto, invulnerable después de golpe, dasheando (si aplica invulnerabilidad), o ya reaccionando (para evitar doble reacción)
        if (IsDead || hitInvulnerabilityTimer > 0 || (isDashing && IsInvulnerableDuringDash) || isReactingToHit) return;

        float finalDamage = damageAmount;
        bool wasBlocked = false;

        // Si está bloqueando, reducir el daño
        if (currentState is BlockingState && CurrentResistance > 0) // Solo bloquear si tiene resistencia
        {
            finalDamage *= (1f - BlockDamageReduction);
            // Consumir resistencia al bloquear
            CurrentResistance -= BlockingResistanceCostPerHit;
            resistanceRegenTimer = ResistanceRegenCooldown; // Reiniciar timer de regeneración de resistencia
            CurrentResistance = Mathf.Max(CurrentResistance, 0); // Asegurarse de que no sea negativo
            wasBlocked = true;
            Debug.Log($"Blocked attack. Resistance reduced by {BlockingResistanceCostPerHit}. Current Resistance: {CurrentResistance}"); // Placeholder


            if (CurrentResistance <= 0)
            {
                BreakResistance(); // Romper resistencia si llega a cero
            }

            // Si el daño después de la reducción es muy bajo, considerar que fue bloqueado completamente
             if (finalDamage <= 0.1f)
             {
                 Debug.Log("Attack fully blocked!"); // Placeholder
                 // Puedes añadir lógica para sonido o efecto visual de bloqueo exitoso aquí
                 return; // No recibir daño de vida
             }
        }

        // Si la resistencia está rota, recibir daño adicional
        if (IsResistanceBroken)
        {
            finalDamage *= 2f; // Ejemplo: doble daño cuando la resistencia está rota
            Debug.Log("Received extra damage due to broken resistance!"); // Placeholder
        }


        CurrentHealth -= finalDamage;
        CurrentHealth = Mathf.Max(CurrentHealth, 0); // Asegurarse de que no sea negativo

        // Reiniciar el timer de regeneración de resistencia al recibir daño (incluso si no bloqueó)
        resistanceRegenTimer = ResistanceRegenCooldown;

        Debug.Log($"Player took {finalDamage} damage. Current Health: {CurrentHealth}"); // Placeholder

        if (CurrentHealth <= 0 && !IsDead)
        {
            Die(); // Llamar al método de muerte
        }
        else if (!isReactingToHit && !IsResistanceBroken) // Reaccionar al golpe solo si no estamos ya reaccionando o en estado de resistencia rota
        {
             StartHitReaction();
        }
        // Si estaba bloqueando y recibió daño a la vida (bloqueo parcial), aún así puede reaccionar si está en estado de resistencia rota
        else if (wasBlocked && !isReactingToHit && !IsResistanceBroken)
        {
             StartHitReaction();
        }
        // Iniciar el timer de invulnerabilidad después de recibir el golpe
        hitInvulnerabilityTimer = HitInvulnerabilityDuration;
    }

    // Método para romper la resistencia del jugador
    public void BreakResistance()
    {
        IsResistanceBroken = true;
        CurrentResistance = 0; // La resistencia se va a 0
        resistanceBrokenTimer = ResistanceBrokenDuration;
        Debug.Log("Player Resistance Broken!"); // Placeholder

        // Si está bloqueando al romperse la resistencia, salir del estado de bloqueo
        if (currentState is BlockingState)
        {
             ChangeState(new ResistanceBrokenState(this)); // Cambiar a estado de resistencia rota
        }
        // Si está en otro estado (excepto muerte, hit reaction), cambiar a estado de resistencia rota
        else if (!(currentState is DeadState) && !(currentState is HitReactionState))
        {
             ChangeState(new ResistanceBrokenState(this));
        }

        // Podrías reproducir una animación de aturdimiento o tambaleo aquí
    }

    // Método para iniciar la reacción a un golpe
    public void StartHitReaction()
    {
        isReactingToHit = true;
        hitReactionTimer = HitReactionDuration;
        Debug.Log("Player hit reaction started."); // Placeholder

        // Si ya estaba en un estado que no permite movimiento (como carga, ataque, bloqueo),
        // podría quedarse en ese estado y solo reproducir la animación de impacto.
        // O forzar el cambio al estado de reacción para asegurar la interrupción.
        // Por ahora, forzamos el cambio si no está muerto o resistencia rota.
         if (!(currentState is DeadState) && !(currentState is ResistanceBrokenState))
         {
             ChangeState(new HitReactionState(this)); // Cambiar a estado de reacción a impacto
         }

        // Reproducir animación de impacto aquí
        // player.SetAnimation("isHit", true); // Esta animación se activa en EnterState del HitReactionState
    }


    // Método para manejar la muerte del jugador
    private void Die()
    {
        IsDead = true;
        Debug.Log("Player has died!"); // Placeholder

        // Deshabilitar entrada de todos los Action Maps
        InputActions_.Disable();
        // Puedes deshabilitar Action Maps específicos si es más granular
        // InputActions_.Move.Disable();
        // InputActions_.Combat.Disable();
        // InputActions_.Camera.Disable();
        // InputActions_.UI.Disable();


        // Deshabilitar CharacterController
        CharacterController_.enabled = false;
        // Deshabilitar animador
        if (Animator_ != null) Animator_.enabled = false;

        if (RagdollObject != null)
        {
            SetRagdollEnabled(true); // Activar ragdoll
            // Opcional: Aplicar una fuerza de impacto al ragdoll para que salga disparado
            // Example: ApplyExplosionForceToRagdoll(explosionForce, explosionPosition, explosionRadius);
        } else {
             // Si no hay ragdoll, simplemente deshabilitar el script o el GameObject si es necesario
             // this.enabled = false;
             // gameObject.SetActive(false); // Ocultar el jugador
        }


        // Cambiar a un estado de muerte
        ChangeState(new DeadState(this));

        // Aquí podrías activar una pantalla de Game Over, reiniciar nivel, etc.
        // Por ejemplo: Invoke("ShowGameOverScreen", 3f);
    }

    // Método para habilitar/deshabilitar el ragdoll
    public void SetRagdollEnabled(bool enabled)
    {
        if (ragdollColliders == null || ragdollRigidbodies == null)
        {
             Debug.LogWarning("Ragdoll components not assigned in PlayerController.");
             return;
        }

        foreach (var col in ragdollColliders)
        {
            col.enabled = enabled;
        }
        foreach (var rb in ragdollRigidbodies)
        {
            rb.isKinematic = !enabled; // Cinematics si está deshabilitado, no cinemático si está habilitado
             // Resetear velocidades si se deshabilita el ragdoll para evitar movimiento residual
             if (!enabled)
             {
                 rb.linearVelocity = Vector3.zero;
                 rb.angularVelocity = Vector3.zero;
             }
        }

        // Deshabilitar el collider principal y el rigidbody principal del "cuerpo vivo" si existen
        Collider mainCollider = GetComponent<Collider>();
        if (mainCollider != null) mainCollider.enabled = !enabled;
        Rigidbody mainRigidbody = GetComponent<Rigidbody>(); // Asumiendo que puedes tener un Rigidbody en el objeto principal
        if (mainRigidbody != null) mainRigidbody.isKinematic = enabled; // Rigidbody principal debe ser cinemático cuando el ragdoll está activo
    }


    // Método para actualizar las barras de UI de vida y resistencia
    private void UpdateCombatUI() { }

     // --- AÑADIDO: Métodos para el sistema de combos ---
     public void StartComboWindow()
     {
         canPerformCombo = true;
         comboTimer = ComboInputWindow;
         Debug.Log($"Combo window open for {ComboInputWindow}s. Current combo step: {currentComboStep}"); // Placeholder
     }

     public void ResetCombo()
     {
         canPerformCombo = false;
         comboTimer = 0f;
         currentComboStep = 0;
         Debug.Log("Combo reset."); // Placeholder
     }
}

// --- DEFINICIÓN DE LOS ESTADOS DEL JUGADOR ---

public abstract class PlayerState
{
    protected PlayerController player;

    public PlayerState(PlayerController player)
    {
        this.player = player;
    }

    public abstract void EnterState();
    public abstract void UpdateState();
    public abstract void ExitState();

    // Métodos de ayuda que los estados pueden usar
    protected Vector3 GetCameraRelativeMovement()
    {
        Vector3 camForward = new Vector3(
            player.cameraTransform.forward.x,
            0f,
            player.cameraTransform.forward.z
        ).normalized;

        Vector3 camRight = new Vector3(
            player.cameraTransform.right.x,
            0f,
            player.cameraTransform.right.z
        ).normalized;

        Vector3 move = camForward * player.MovementInput_.y + camRight * player.MovementInput_.x;
        return move;
    }

    protected void RotatePlayerTowards(Vector3 direction)
    {
        if (direction.sqrMagnitude > 0.001f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            //targetRotation *= Quaternion.Euler(0, 180, 0);

            player.transform.rotation = Quaternion.Slerp(
                player.transform.rotation,
                targetRotation,
                player.RotationSmoothness_ * Time.deltaTime
            );
        }
    }

     // --- MODIFICADO: Método para calcular y almacenar la velocidad horizontal con interpolación ---
     protected void CalculateInterpolatedHorizontalVelocity(Vector3 desiredDirection, float speed)
     {
         Vector3 desiredVelocity = desiredDirection.normalized * speed;
         player.currentVelocity = Vector3.Lerp(
             player.currentVelocity,
             desiredVelocity,
             player.MovementSmoothness_ * Time.deltaTime
         );
         // Almacenar solo el componente horizontal para que se aplique en el Update principal
         Vector3 horizontalMove = new Vector3(player.currentVelocity.x, 0, player.currentVelocity.z);
         // Asegurarse de que el CharacterController está habilitado antes de intentar mover
         if (player.CharacterController_.enabled)
         {
             player.CharacterController_.Move(horizontalMove * Time.deltaTime); // Esta línea debe eliminarse o modificarse
         }
     }

     // --- MODIFICADO: Método para calcular y almacenar la velocidad horizontal con aceleración ---
     protected void CalculateAcceleratedHorizontalVelocity(Vector3 desiredDirection, float targetSpeed, float accelerationRate)
     {
         Vector3 desiredVelocity = desiredDirection.normalized * targetSpeed;
         player.currentVelocity = Vector3.Lerp(player.currentVelocity, desiredVelocity, accelerationRate * Time.deltaTime);
         // Solo aplicar el movimiento horizontal
         Vector3 horizontalMove = new Vector3(player.currentVelocity.x, 0, player.currentVelocity.z);
         // Asegurarse de que el CharacterController está habilitado antes de intentar mover
         if (player.CharacterController_.enabled)
         {
             player.CharacterController_.Move(horizontalMove * Time.deltaTime); // Esta línea debe eliminarse o modificarse
         }
     }
}

// --- TUS ESTADOS DE MOVIMIENTO EXISTENTES (con comprobaciones de estados de combate y Action Map corregido) ---

public class IdleState : PlayerState
{
    private float decelerationRate = 5f;

    public IdleState(PlayerController player) : base(player) { }

    public override void EnterState()
    {
        player.SetAnimation("isIdle", true);
        player.SetAnimation("isWalking", false);
        player.SetAnimation("isRunning", false);
        player.SetAnimation("isCrouching", false);
        // Asegurarse de que las animaciones de combate estén desactivadas al entrar a Idle
        player.SetAnimation("isAttacking", false); // Nueva animación genérica de ataque
        player.SetAnimation("isBlocking", false); // Nueva animación de bloqueo
        player.SetAnimation("isHit", false); // Nueva animación de impacto
        player.SetAnimation("isDead", false); // Nueva animación de muerte
        player.SetAnimation("isResistanceBroken", false); // Nueva animación de resistencia rota
        player.SetAnimation("isChargingHeavyAttack", false); // Nueva animación de carga de ataque
         player.SetAnimation("isJumping", false); // Asegurarse de que la animación de salto se desactive si se llega a Idle en el aire (aunque no debería pasar)
        player.SetAnimation("isDashing", false);


        // Resetear cualquier flag de combate que deba estar desactivado en Idle
        // player.isReactingToHit = false; // Se maneja por timer en Update
        // player.IsResistanceBroken = false; // Se maneja por timer en Update
         player.ResetCombo(); // Resetear combo al volver a Idle
         player.SetAttackHitboxesEnabled(false); // Asegurarse de que las hitboxes estén desactivadas
         player.BlockPressed_ = false; // Asegurarse de que el bloqueo se desactiva

         // Al entrar a Idle, la velocidad horizontal deseada es cero
        player.DesiredHorizontalVelocity = Vector3.zero;
    }

    public override void UpdateState()
    {
        // --- Restricciones: No cambiar de estado si está reaccionando a un golpe, resistencia rota, muerto o dasheando ---
        if (player.isReactingToHit || player.IsResistanceBroken || player.IsDead || player.isDashing)
        {
             // Si el personaje está en un estado que restringe el movimiento, asegurarse de que la velocidad horizontal deseada sea cero
             player.DesiredHorizontalVelocity = Vector3.zero;
             return;
        }
        // --- FIN Restricciones ---

        // Si hay input de ataque ligero, cambiar a estado de ataque ligero
        if (player.InputActions_.Combat.LightAttack.WasPerformedThisFrame()) // Usar el Action Map "Combat"
        {
            player.OnLightAttack(); // La lógica dentro de OnLightAttack manejará el cambio de estado
            return;
        }
         // Si hay input de ataque pesado, cambiar a estado de carga de ataque pesado
        if (player.InputActions_.Combat.HeavyAttack.WasPerformedThisFrame()) // Usar el Action Map "Combat"
        {
             player.OnHeavyAttack();
             return;
        }
         // Si hay input de bloqueo, cambiar a estado de bloqueo
        if (player.InputActions_.Combat.Block.IsPressed()) // Usar el Action Map "Combat"
        {
             player.OnBlockPressed(); // La lógica dentro de OnBlockPressed manejará el cambio de estado
             return;
        }


        if (player.DashPressed_) // La propiedad DashPressed_ se actualiza desde el Action Map "Move"
        {
            player.DashPressed_ = false; // Consumir el input de dash
            if (!player.IsExhausted_ && player.Stamina_ >= player.DashStaminaDrain_)
            {
                player.ChangeState(new DashState(player));
                return;
            } else {
                 Debug.Log("Not enough stamina for Dash!"); // Placeholder
            }
        }

        if (player.JumpPressed_ && player.IsGrounded_) // La propiedad JumpPressed_ se actualiza desde el Action Map "Move"
        {
            player.JumpPressed_ = false; // Consumir el input de salto
            // Verificar estamina si el salto la consume
            // if (!player.IsExhausted_ && player.Stamina_ >= player.JumpStaminaDrain_)
            // {
                 player.ChangeState(new JumpingState(player, false));
            // } else {
                 // Debug.Log("Not enough stamina for Jump!"); // Placeholder
            // }
             player.ChangeState(new JumpingState(player, false)); // Salto sin estamina por ahora
            return;
        }

        if (player.CrouchPressed_) // La propiedad CrouchPressed_ se actualiza desde el Action Map "Move"
        {
            player.ChangeState(new CrouchingState(player));
            return;
        }

        if (player.MovementInput_.magnitude > 0f) // La propiedad MovementInput_ se actualiza desde el Action Map "Move"
        {
            player.ChangeState(new WalkingState(player));
            return;
        }

        // Aplicar desaceleración horizontal y almacenar en DesiredHorizontalVelocity
        if (player.currentVelocity.magnitude > 0.1f)
        {
            player.currentVelocity = Vector3.Lerp(player.currentVelocity, Vector3.zero, decelerationRate * Time.deltaTime);
            player.DesiredHorizontalVelocity = new Vector3(player.currentVelocity.x, 0, player.currentVelocity.z);
        }
        else
        {
            player.currentVelocity = Vector3.zero;
            player.DesiredHorizontalVelocity = Vector3.zero;
        }
         // Lógica de Rotación para Idle (ajustada para rotar con input ligero)...
        if (player.MovementInput_.magnitude > 0.01f) // Umbral pequeño
        {
             Vector3 moveDirection = GetCameraRelativeMovement();
             Vector3 flatMoveDirection = new Vector3(moveDirection.x, 0f, moveDirection.z).normalized;
             RotatePlayerTowards(flatMoveDirection);
        }
    }

    public override void ExitState() { }
}

public class WalkingState : PlayerState
{
    private float accelerationRate = 5f;

    public WalkingState(PlayerController player) : base(player) { }

    public override void EnterState()
    {
        player.SetAnimation("isWalking", true);
        player.SetAnimation("isIdle", false);
        player.SetAnimation("isRunning", false);
        player.SetAnimation("isCrouching", false);
         // Asegurarse de que las animaciones de combate estén desactivadas al entrar a Walking
        player.SetAnimation("isAttacking", false);
        player.SetAnimation("isBlocking", false);
        player.SetAnimation("isHit", false);
        player.SetAnimation("isDead", false);
        player.SetAnimation("isResistanceBroken", false);
        player.SetAnimation("isChargingHeavyAttack", false);
         player.SetAnimation("isJumping", false);
        player.SetAnimation("isDashing", false);


        player.ResetCombo(); // Resetear combo al volver a Walking
        player.SetAttackHitboxesEnabled(false); // Asegurarse de que las hitboxes estén desactivadas
        player.BlockPressed_ = false; // Asegurarse de que el bloqueo se desactiva
    }

    public override void UpdateState()
    {
         // --- Restricciones: No cambiar de estado si está reaccionando a un golpe, resistencia rota, muerto o dasheando ---
        if (player.isReactingToHit || player.IsResistanceBroken || player.IsDead || player.isDashing)
        {
             player.DesiredHorizontalVelocity = Vector3.zero; // Asegurarse de que la velocidad horizontal deseada sea cero
             return;
        }
         // --- FIN Restricciones ---

         // Si hay input de ataque ligero, cambiar a estado de ataque ligero
        if (player.InputActions_.Combat.LightAttack.WasPerformedThisFrame()) // Usar el Action Map "Combat"
        {
            player.OnLightAttack();
            return;
        }
         // Si hay input de ataque pesado, cambiar a estado de carga de ataque pesado
        if (player.InputActions_.Combat.HeavyAttack.WasPerformedThisFrame()) // Usar el Action Map "Combat"
        {
             player.OnHeavyAttack();
             return;
        }
         // Si hay input de bloqueo, cambiar a estado de bloqueo
        if (player.InputActions_.Combat.Block.IsPressed()) // Usar el Action Map "Combat"
        {
             player.OnBlockPressed();
             return;
        }


        if (player.DashPressed_) // La propiedad DashPressed_ se actualiza desde el Action Map "Move"
        {
            player.DashPressed_ = false;
            if (!player.IsExhausted_ && player.Stamina_ >= player.DashStaminaDrain_)
            {
                player.ChangeState(new DashState(player));
                return;
            } else {
                 Debug.Log("Not enough stamina for Dash!");
            }
        }

        if (player.JumpPressed_ && player.IsGrounded_) // La propiedad JumpPressed_ se actualiza desde el Action Map "Move"
        {
            player.JumpPressed_ = false;
            // Verificar estamina si el salto la consume
            // if (!player.IsExhausted_ && player.Stamina_ >= player.JumpStaminaDrain_)
            // {
                 player.ChangeState(new JumpingState(player, false));
            // } else {
                 // Debug.Log("Not enough stamina for Jump!");
            // }
             player.ChangeState(new JumpingState(player, false)); // Salto sin estamina por ahora
            return;
        }

        if (player.CrouchPressed_) // La propiedad CrouchPressed_ se actualiza desde el Action Map "Move"
        {
            player.ChangeState(new CrouchingState(player));
            return;
        }

        if (player.MovementInput_.magnitude == 0f) // La propiedad MovementInput_ se actualiza desde el Action Map "Move"
        {
            player.ChangeState(new IdleState(player));
            return;
        }

        if (player.SprintPressed_ && !player.IsExhausted_) // La propiedad SprintPressed_ se actualiza desde el Action Map "Move"
        {
            player.ChangeState(new RunningState(player));
            return;
        }

        Vector3 moveDirection = GetCameraRelativeMovement();
        // Calcular la velocidad horizontal con aceleración y almacenar en DesiredHorizontalVelocity
        CalculateAcceleratedHorizontalVelocity(moveDirection, player.Speed_, accelerationRate);
        // Rotar hacia la dirección horizontal del movimiento
         Vector3 flatMoveDirection = new Vector3(moveDirection.x, 0f, moveDirection.z).normalized;
         if (flatMoveDirection.sqrMagnitude > 0.001f)
         {
             RotatePlayerTowards(flatMoveDirection);
         }
    }

    public override void ExitState() { }
}

public class RunningState : PlayerState
{
     private float accelerationRate = 7f;

    public RunningState(PlayerController player) : base(player) { }

    public override void EnterState()
    {
        player.SetAnimation("isRunning", true);
        player.SetAnimation("isIdle", false);
        player.SetAnimation("isWalking", false);
        player.SetAnimation("isCrouching", false);
         // Asegurarse de que las animaciones de combate estén desactivadas al entrar a Running
        player.SetAnimation("isAttacking", false);
        player.SetAnimation("isBlocking", false);
        player.SetAnimation("isHit", false);
        player.SetAnimation("isDead", false);
        player.SetAnimation("isResistanceBroken", false);
        player.SetAnimation("isChargingHeavyAttack", false);
        player.SetAnimation("isJumping", false);
        player.SetAnimation("isDashing", false);


        player.ResetCombo(); // Resetear combo al volver a Running
        player.SetAttackHitboxesEnabled(false); // Asegurarse de que las hitboxes estén desactivadas
        player.BlockPressed_ = false; // Asegurarse de que el bloqueo se desactiva
    }

    public override void UpdateState()
    {
         // --- Restricciones: No cambiar de estado si está reaccionando a un golpe, resistencia rota, muerto o dasheando ---
        if (player.isReactingToHit || player.IsResistanceBroken || player.IsDead || player.isDashing)
        {
             player.DesiredHorizontalVelocity = Vector3.zero; // Asegurarse de que la velocidad horizontal deseada sea cero
             return;
        }
         // --- FIN Restricciones ---

         // Si hay input de ataque ligero, cambiar a estado de ataque ligero (puede que quieras restringir esto al correr)
        // if (player.InputActions_.Combat.LightAttack.WasPerformedThisFrame())
        // {
        //     player.OnLightAttack();
        //     return;
        // }
         // Si hay input de ataque pesado, cambiar a estado de carga de ataque pesado (probablemente restringido al correr)
        // if (player.InputActions_.Combat.HeavyAttack.WasPerformedThisFrame())
        // {
        //      player.OnHeavyAttack();
        //      return;
        // }
         // Si hay input de bloqueo, cambiar a estado de bloqueo (puede que quieras permitirlo al correr)
        if (player.InputActions_.Combat.Block.IsPressed()) // Usar el Action Map "Combat"
        {
             player.OnBlockPressed();
             return;
        }


        if (player.DashPressed_) // La propiedad DashPressed_ se actualiza desde el Action Map "Move"
        {
            player.DashPressed_ = false;
            if (!player.IsExhausted_ && player.Stamina_ >= player.DashStaminaDrain_)
            {
                player.ChangeState(new DashState(player));
                return;
            } else {
                 Debug.Log("Not enough stamina for Dash!");
            }
        }

        if (player.JumpPressed_ && player.IsGrounded_) // La propiedad JumpPressed_ se actualiza desde el Action Map "Move"
        {
            player.JumpPressed_ = false;
            // Verificar estamina si el salto la consume
            // if (!player.IsExhausted_ && player.Stamina_ >= player.JumpStaminaDrain_)
            // {
                 player.ChangeState(new JumpingState(player, true)); // Pasar true para indicar que salta desde correr
            // } else {
                 // Debug.Log("Not enough stamina for Jump!");
            // }
             player.ChangeState(new JumpingState(player, true)); // Salto sin estamina por ahora
            return;
        }

        if (player.CrouchPressed_) // La propiedad CrouchPressed_ se actualiza desde el Action Map "Move"
        {
            player.ChangeState(new CrouchingState(player));
            return;
        }

        // Si no está presionado el sprint o está agotado, volver a caminar
        if (!player.SprintPressed_ || player.IsExhausted_) // La propiedad SprintPressed_ se actualiza desde el Action Map "Move"
        {
            player.ChangeState(new WalkingState(player));
            return;
        }

        Vector3 moveDirection = GetCameraRelativeMovement();
        // Calcular la velocidad horizontal con aceleración y almacenar en DesiredHorizontalVelocity
        CalculateAcceleratedHorizontalVelocity(moveDirection, player.SprintSpeed_, accelerationRate);
        // Rotar hacia la dirección horizontal del movimiento
        Vector3 flatMoveDirection = new Vector3(moveDirection.x, 0f, moveDirection.z).normalized;
         if (flatMoveDirection.sqrMagnitude > 0.001f)
         {
             RotatePlayerTowards(flatMoveDirection);
         }

        // Consumir estamina al correr
        player.Stamina_ -= player.StaminaDrainRate_ * Time.deltaTime;
        if (player.Stamina_ <= 0f)
        {
            player.Stamina_ = 0;
            player.IsExhausted_ = true;
            player.StartStaminaCooldown();
            player.ChangeState(new WalkingState(player)); // Cambiar a caminar al agotarse
            return;
        }
        player.Stamina_ = Mathf.Clamp(player.Stamina_, 0, player.StaminaTop_);
    }

    public override void ExitState() { }
}

public class DashState : PlayerState
{
    // --- MODIFICADO: Usa la duración del dash de PlayerController ---
    private float dashDuration;
    private float timer = 0f;
    private Vector3 dashDirection;
    private float decelerationRate = 10f; // Desaceleración al final del dash

    public DashState(PlayerController player) : base(player) { }

    public override void EnterState()
    {
        dashDuration = player.DashDuration_; // Usar la duración de PlayerController
        timer = dashDuration;
        dashDirection = player.GetDashDirection();

        player.PlayDashAnimation(dashDuration); // inicia animación (coroutine)
        player.SetAnimation("isIdle", false);
        player.SetAnimation("isWalking", false);
        player.SetAnimation("isRunning", false);
        player.SetAnimation("isCrouching", false);
        // Asegurarse de que las animaciones de combate estén desactivadas al dashear
        player.SetAnimation("isAttacking", false);
        player.SetAnimation("isBlocking", false);
        player.SetAnimation("isHit", false);
        player.SetAnimation("isDead", false);
        player.SetAnimation("isResistanceBroken", false);
        player.SetAnimation("isChargingHeavyAttack", false);
        player.SetAnimation("isJumping", false);


        // Asegurarse de que las hitboxes de ataque estén desactivadas al dashear
        player.SetAttackHitboxesEnabled(false);

        player.Stamina_ -= player.DashStaminaDrain_;
        player.Stamina_ = Mathf.Max(player.Stamina_, 0);
         if (player.Stamina_ <= 0) player.IsExhausted_ = true; // Agotar si la estamina llega a cero

        // Opcional: Deshabilitar la detección de colisiones del CharacterController por un momento para pasar a través de enemigos
        // if (player.CharacterController_.enabled) player.CharacterController_.enabled = false; // Considera esto con cuidado

        player.ResetCombo(); // Resetear combo al dashear
         player.BlockPressed_ = false; // Asegurarse de que el bloqueo se desactiva

         // --- MODIFICADO: Almacenar la velocidad horizontal del dash en DesiredHorizontalVelocity ---
         player.currentVelocity = dashDirection * player.DashSpeed_;
         player.DesiredHorizontalVelocity = player.currentVelocity;
    }

    public override void UpdateState()
    {
         // --- Restricciones: No cambiar de estado si está reaccionando a un golpe o muerto (Dash te hace invulnerable a daño directo, pero no a estados) ---
        if (player.isReactingToHit || player.IsDead)
        {
             player.DesiredHorizontalVelocity = Vector3.zero; // Asegurarse de que la velocidad horizontal deseada sea cero
             return;
        }
         // --- FIN Restricciones ---

        timer -= Time.deltaTime;

        if (timer > 0f)
        {
            // Movimiento desacelerado orgánico
            float t = 1f - (timer / dashDuration);
            float curve = Mathf.SmoothStep(1f, 0f, t);
            // --- MODIFICADO: Calcular velocidad horizontal del dash y almacenar en DesiredHorizontalVelocity ---
            player.currentVelocity = dashDirection * player.DashSpeed_ * curve;
            player.DesiredHorizontalVelocity = player.currentVelocity; // Almacenar la velocidad horizontal calculada

            // Rotación fluida hacia la dirección del dash
            if (dashDirection.sqrMagnitude > 0.01f)
            {
                Quaternion targetRot = Quaternion.LookRotation(dashDirection);
                player.transform.rotation = Quaternion.Slerp(
                    player.transform.rotation,
                    targetRot,
                    Time.deltaTime * 10f
                );
            }

            return;
        }

        // Después del dash, cambio de estado
        // Opcional: Habilitar la detección de colisiones de nuevo
        // if (!player.CharacterController_.enabled) player.CharacterController_.enabled = true; // Considera esto con cuidado

        // --- MODIFICADO: Al terminar el dash, resetear DesiredHorizontalVelocity ---
        player.DesiredHorizontalVelocity = Vector3.zero; // Resetear la velocidad horizontal al final del dash

        if (player.IsGrounded_) // Solo cambiar a estados de movimiento normal si está en el suelo
        {
            if (player.SprintPressed_ && player.MovementInput_.magnitude > 0f && !player.IsExhausted_) // La propiedad SprintPressed_ y MovementInput_ se actualizan desde el Action Map "Move"
                player.ChangeState(new RunningState(player));
            else if (player.MovementInput_.magnitude > 0f) // La propiedad MovementInput_ se actualiza desde el Action Map "Move"
                player.ChangeState(new WalkingState(player));
            else
                player.ChangeState(new IdleState(player));
        } else {
             // Si termina el dash en el aire, ir al estado de salto/caída
             player.ChangeState(new JumpingState(player, false)); // O un estado específico para terminar dash en aire
        }
    }

    public override void ExitState()
    {
        // Asegurarse de que el CharacterController esté habilitado al salir
        // if (!player.CharacterController_.enabled)
        // {
        //     player.CharacterController_.enabled = true;
        // }
    }
}

public class JumpingState : PlayerState
{
    private bool JumpFromRunning_;
    private float airControlFactor = 0.5f; // Cuánto control tiene el jugador en el aire

    public JumpingState(PlayerController player, bool jumpFromRunning) : base(player)
    {
        this.JumpFromRunning_ = jumpFromRunning;
    }

    public override void EnterState()
    {
        player.SetAnimation("isJumping", true); // Activar animación de salto
        player.SetAnimation("isIdle", false);
        player.SetAnimation("isWalking", false);
        player.SetAnimation("isRunning", false);
        player.SetAnimation("isCrouching", false);
        // Asegurarse de que las animaciones de combate estén desactivadas al saltar
        player.SetAnimation("isAttacking", false);
        player.SetAnimation("isBlocking", false);
        player.SetAnimation("isHit", false);
        player.SetAnimation("isDead", false);
        player.SetAnimation("isResistanceBroken", false);
        player.SetAnimation("isChargingHeavyAttack", false);
        player.SetAnimation("isDashing", false);


        player.SetAttackHitboxesEnabled(false); // Asegurarse de que las hitboxes estén desactivadas en el aire

        player.Jump(); // Aplicar la fuerza de salto
        player.JumpPressed_ = false; // Consumir el input de salto (propiedad actualizada desde Action Map "Move")

        // Consumir estamina si el salto la requiere
        // if (!player.IsExhausted_ && player.Stamina_ >= player.JumpStaminaDrain_)
        // {
        //      player.Stamina_ -= player.JumpStaminaDrain_;
        //      player.Stamina_ = Mathf.Max(player.Stamina_, 0);
        //      if (player.Stamina_ <= 0) player.IsExhausted_ = true;
        // } else if (player.Stamina_ < player.JumpStaminaDrain_ && player.JumpStaminaDrain_ > 0)
        // {
        //      // Si no hay suficiente estamina para un salto completo, podría hacer un salto más bajo
        //      // o simplemente no saltar (la lógica de Jump() debería manejar esto si se llama condicionalmente)
        // }

        player.ResetCombo(); // Resetear combo al saltar
         player.BlockPressed_ = false; // Asegurarse de que el bloqueo se desactiva

         // --- MODIFICADO: Al entrar al estado de salto, la velocidad horizontal inicial es la velocidad actual ---
         player.DesiredHorizontalVelocity = new Vector3(player.currentVelocity.x, 0, player.currentVelocity.z);
    }

    public override void UpdateState()
    {
         // --- Restricciones: No cambiar de estado si está reaccionando a un golpe, resistencia rota, muerto o dasheando ---
        if (player.isReactingToHit || player.IsResistanceBroken || player.IsDead || player.isDashing)
        {
             player.DesiredHorizontalVelocity = Vector3.zero; // Asegurarse de que la velocidad horizontal deseada sea cero
             // Si se interrumpe en el aire, intentar volver a un estado aéreo seguro (como este mismo)
             // O manejar la interrupción de forma específica (por ejemplo, caer inmediatamente)
             if (player.IsDead) return; // Si muere en el aire, permanece muerto
             // Si está reaccionando o resistencia rota en el aire, podría simplemente caer
             if (player.isReactingToHit || player.IsResistanceBroken)
             {
                 // No hacer nada, simplemente dejar que la gravedad actúe hasta que aterrice o el estado termine
             }
             return;
        }
         // --- FIN Restricciones ---

        // En el aire, no procesamos la entrada de salto hasta que aterrice
        player.JumpPressed_ = false;

        Vector3 moveDirection = GetCameraRelativeMovement();
        float targetSpeed = JumpFromRunning_ ? player.SprintSpeed_ : player.Speed_;

        // Aplicar control en el aire (movimiento horizontal reducido)
        Vector3 airMove = moveDirection * targetSpeed * airControlFactor;
        // --- MODIFICADO: Calcular velocidad horizontal en aire y almacenar en DesiredHorizontalVelocity ---
        player.currentVelocity = Vector3.Lerp(player.currentVelocity, airMove, airControlFactor * Time.deltaTime); // Interpolar velocidad en el aire
        player.DesiredHorizontalVelocity = new Vector3(player.currentVelocity.x, 0, player.currentVelocity.z); // Almacenar la velocidad horizontal

        RotatePlayerTowards(moveDirection);


        if (player.IsGrounded_) // Verificar si ha aterrizado
        {
            player.SetAnimation("isJumping", false); // Desactivar animación de salto
            player.currentVelocity = Vector3.zero; // Detener el movimiento horizontal al aterrizar
            player.DesiredHorizontalVelocity = Vector3.zero; // Asegurarse de que la velocidad horizontal sea cero al aterrizar


             // Transición al estado apropiado al aterrizar
            if (player.CrouchPressed_) // La propiedad CrouchPressed_ se actualiza desde el Action Map "Move"
            {
                player.ChangeState(new CrouchingState(player));
            }
            else if (player.SprintPressed_ && player.MovementInput_.magnitude > 0f && !player.IsExhausted_) // Propiedades actualizadas desde Action Map "Move"
            {
                 player.ChangeState(new RunningState(player));
            }
            else if (player.MovementInput_.magnitude > 0f) // La propiedad MovementInput_ se actualiza desde el Action Map "Move"
            {
                player.ChangeState(new WalkingState(player));
            }
            else
            {
                player.ChangeState(new IdleState(player));
            }
        }
    }

    public override void ExitState()
    {
         // Asegurarse de que la animación de salto esté desactivada al salir del estado
        player.SetAnimation("isJumping", false);
    }
}

public class CrouchingState : PlayerState
{
    private float accelerationRate = 4f;
    private float decelerationRate = 6f;

    public CrouchingState(PlayerController player) : base(player) { }

    public override void EnterState()
    {
        player.SetAnimation("isCrouching", true);
        player.SetAnimation("isIdle", false);
        player.SetAnimation("isWalking", false);
        player.SetAnimation("isRunning", false);
         // Asegurarse de que las animaciones de combate estén desactivadas al entrar a Crouching
        player.SetAnimation("isAttacking", false);
        player.SetAnimation("isBlocking", false);
        player.SetAnimation("isHit", false);
        player.SetAnimation("isDead", false);
        player.SetAnimation("isResistanceBroken", false);
        player.SetAnimation("isChargingHeavyAttack", false);
        player.SetAnimation("isJumping", false);
        player.SetAnimation("isDashing", false);


        // Ajustar la altura y el centro del CharacterController
        player.CharacterController_.height = player.CrouchHeight_;
        // player.CharacterController_.center = new Vector3(player.OriginalCenter_.x, player.CrouchHeight_ / 2f, player.OriginalCenter_.z);


        player.ResetCombo(); // Resetear combo al agacharse
        player.SetAttackHitboxesEnabled(false); // Asegurarse de que las hitboxes estén desactivadas
        player.BlockPressed_ = false; // Asegurarse de que el bloqueo se desactiva

         // --- MODIFICADO: Al entrar a agacharse, la velocidad horizontal deseada es la velocidad actual, limitada ---
         Vector3 currentHorizontalVelocity = new Vector3(player.currentVelocity.x, 0, player.currentVelocity.z);
         player.DesiredHorizontalVelocity = Vector3.ClampMagnitude(currentHorizontalVelocity, player.CrouchSpeed_);
         player.currentVelocity = player.DesiredHorizontalVelocity; // Sincronizar currentVelocity
    }

    public override void UpdateState()
    {
         // --- Restricciones: No cambiar de estado si está reaccionando a un golpe, resistencia rota, muerto o dasheando ---
        if (player.isReactingToHit || player.IsResistanceBroken || player.IsDead || player.isDashing)
        {
             player.DesiredHorizontalVelocity = Vector3.zero; // Asegurarse de que la velocidad horizontal deseada sea cero
             return;
        }
         // --- FIN Restricciones ---

         // Si hay input de ataque ligero, cambiar a estado de ataque ligero (puede que quieras tener ataques agachado)
        // if (player.InputActions_.Combat.LightAttack.WasPerformedThisFrame()) // Usar el Action Map "Combat"
        // {
        //     player.OnLightAttack();
        //     return;
        // }
         // Si hay input de ataque pesado, cambiar a estado de carga de ataque pesado (probablemente restringido)
        // if (player.InputActions_.Combat.HeavyAttack.WasPerformedThisFrame()) // Usar el Action Map "Combat"
        // {
        //      player.OnHeavyAttack();
        //      return;
        // }
         // Si hay input de bloqueo, cambiar a estado de bloqueo (puede que quieras permitir bloquear agachado)
        if (player.InputActions_.Combat.Block.IsPressed()) // Usar el Action Map "Combat"
        {
             player.OnBlockPressed();
             return;
        }


        Vector3 moveDirection = GetCameraRelativeMovement();

        // --- MODIFICADO: Calcular la velocidad horizontal con aceleración y almacenar en DesiredHorizontalVelocity ---
        CalculateAcceleratedHorizontalVelocity(moveDirection, player.CrouchSpeed_, accelerationRate);
        // Rotar hacia la dirección horizontal del movimiento
        Vector3 flatMoveDirection = new Vector3(moveDirection.x, 0f, moveDirection.z).normalized;
         if (flatMoveDirection.sqrMagnitude > 0.001f)
         {
             RotatePlayerTowards(flatMoveDirection);
         }

        if (player.JumpPressed_ && player.IsGrounded_) // La propiedad JumpPressed_ se actualiza desde el Action Map "Move"
        {
            player.JumpPressed_ = false;
            // Restablecer la altura y centro antes de saltar
            player.CharacterController_.height = player.OriginalHeight_;
            // player.CharacterController_.center = player.OriginalCenter_;
            player.ChangeState(new JumpingState(player, false));
            return;
        }

        if (!player.CrouchPressed_) // Si se suelta el botón de agachar (propiedad actualizada desde Action Map "Move")
        {
            // Aquí podrías añadir una verificación para asegurarte de que no hay obstáculos encima antes de levantarse
            // RaycastHit hit;
            // if (!Physics.Raycast(player.transform.position + Vector3.up * player.CharacterController_.radius, Vector3.up, out hit, player.OriginalHeight_ - player.CharacterController_.height))
            // {
                 // Restablecer la altura y centro
                 player.CharacterController_.height = player.OriginalHeight_;
                 // player.CharacterController_.center = player.OriginalCenter_;

                 if (player.MovementInput_.magnitude > 0) // La propiedad MovementInput_ se actualiza desde el Action Map "Move"
                     player.ChangeState(new WalkingState(player));
                 else
                     player.ChangeState(new IdleState(player));
            // }
            // else
            // {
                // Si hay algo encima, no se puede levantar y se queda agachado
                // player.CrouchPressed_ = true; // Forzar que se quede agachado hasta que no haya obstáculo
                // Debug.Log("Cannot stand up, obstacle above."); // Placeholder
            // }
        }
    }

    public override void ExitState()
    {
        // Aplicar desaceleración al salir del estado agachado
        CalculateAcceleratedHorizontalVelocity(Vector3.zero, 0, decelerationRate); // Usar el método modificado
        // Restablecer la altura y centro si no se hizo ya al cambiar de estado
        // player.CharacterController_.height = player.OriginalHeight_;
        // player.CharacterController_.center = player.OriginalCenter_;
         player.DesiredHorizontalVelocity = Vector3.zero; // Asegurarse de que la velocidad horizontal sea cero al salir
    }
}


// --- AÑADIDO: NUEVOS ESTADOS DE COMBATE ---

public class AttackingState : PlayerState
{
    protected float attackDuration; // Duración de la animación/estado de ataque actual
    protected float attackTimer = 0f;
    protected PlayerController.AttackType attackType; // Tipo de ataque para manejar la lógica
    protected int comboStep = 0; // Para los combos ligeros

    public AttackingState(PlayerController player, float duration, PlayerController.AttackType type, int comboStep = 0) : base(player)
    {
        this.attackDuration = duration;
        this.attackType = type;
        this.comboStep = comboStep;
    }

    public override void EnterState()
    {
        player.SetAnimation("isAttacking", true); // Activar animación genérica de ataque
        // Asegurarse de que las animaciones de movimiento y otros estados estén desactivadas
        player.SetAnimation("isIdle", false);
        player.SetAnimation("isWalking", false);
        player.SetAnimation("isRunning", false);
        player.SetAnimation("isCrouching", false);
        player.SetAnimation("isBlocking", false);
        player.SetAnimation("isHit", false);
        player.SetAnimation("isDead", false);
        player.SetAnimation("isResistanceBroken", false);
        player.SetAnimation("isChargingHeavyAttack", false);
        player.SetAnimation("isJumping", false);
        player.SetAnimation("isDashing", false);


        attackTimer = attackDuration;

        // Deshabilitar o reducir el movimiento durante el ataque y asegurarse de que la velocidad horizontal deseada sea cero inicialmente
        player.currentVelocity = Vector3.zero;
        player.DesiredHorizontalVelocity = Vector3.zero; // Detener el movimiento horizontal al entrar al estado de ataque
        // player.MovementInput_ = Vector2.zero; // Ignorar input de movimiento durante el ataque

        // Habilitar la hitbox correspondiente al tipo de ataque
        player.SetAttackHitboxesEnabled(true, attackType);


        // Consumir estamina/recursos si no se hizo ya al iniciar la carga/ataque
        // Esto puede depender del tipo de ataque y si ya se consumió estamina al cargar
        // La estamina para LightAttack se consume en su EnterState específico.
        // La estamina para ChargedAttack se consume en su EnterState específico.

        // Reiniciar el timer de regeneración de estamina al atacar
        player.StartStaminaCooldown();
    }

    public override void UpdateState()
    {
        // --- Restricciones: Si está reaccionando a un golpe, resistencia rota o muerto, interrumpir el ataque ---
        if (player.isReactingToHit || player.IsResistanceBroken || player.IsDead)
        {
            // Interrumpir el ataque y pasar al estado de interrupción
             player.ChangeState(player.isReactingToHit ? new HitReactionState(player) : (player.IsResistanceBroken ? new ResistanceBrokenState(player) : new DeadState(player)));
             return;
        }
         // --- FIN Restricciones ---

        attackTimer -= Time.deltaTime;

        // Aplicar penalización de movimiento si se permite movimiento durante el ataque
        float movePenalty = 0f; // Por defecto no se mueve
        if (attackType == PlayerController.AttackType.Light) movePenalty = player.LightAttackMovementPenalty;
        else if (attackType == PlayerController.AttackType.Heavy || attackType == PlayerController.AttackType.Charged) movePenalty = player.HeavyAttackMovementPenalty; // O ChargedAttackMovementPenalty
         // Si se permite movimiento, calcularlo y aplicar la velocidad horizontal deseada
         Vector3 moveDirection = GetCameraRelativeMovement(); // La propiedad MovementInput_ se actualiza desde el Action Map "Move"
         if (moveDirection.magnitude > 0.1f) // Solo moverse si hay input de movimiento
         {
              CalculateInterpolatedHorizontalVelocity(moveDirection, player.Speed_ * movePenalty); // Usar interpolación o aceleración con penalización
         } else {
              // Si no hay movimiento permitido o input, la velocidad horizontal deseada es cero
              player.DesiredHorizontalVelocity = Vector3.zero;
              player.currentVelocity = Vector3.Lerp(player.currentVelocity, Vector3.zero, 10f * Time.deltaTime); // Desacelerar currentVelocity
         }


        // Rotar hacia la dirección del input si hay, o mantener la rotación actual si no
         Vector3 lookDirection = player.MovementInput_.magnitude > 0 ? GetCameraRelativeMovement() : player.transform.forward; // La propiedad MovementInput_ se actualiza desde el Action Map "Move"
         RotatePlayerTowards(lookDirection);


        if (attackTimer <= 0)
        {
            // Ataque terminado
            // Deshabilitar la hitbox del ataque
            player.SetAttackHitboxesEnabled(false, attackType);

            // Lógica de combos: Si el ataque actual puede iniciar una ventana de combo
            // y si hay input del siguiente ataque dentro de esa ventana, pasar al siguiente estado de combo.
            // Esto es más complejo y requiere chequear el input y el paso del combo.
            // Por ahora, simplificamos y si era un ataque ligero que puede continuar el combo, abrimos la ventana.
             if (attackType == PlayerController.AttackType.Light && comboStep < 3) // Ejemplo para Light Attack Combo (hasta 3 golpes)
             {
                  player.StartComboWindow();
                  // Esperar el input del siguiente ataque ligero en la ventana de combo.
                  // No cambiamos de estado inmediatamente, el Update del PlayerController manejará el timer del combo.
             } else {
                  // Si el ataque no es parte de un combo, el combo ha terminado, o es un ataque pesado/cargado
                  player.ResetCombo();
                  // Transición al estado apropiado después del ataque (si no se está esperando input de combo)
                   player.ChangeState(player.MovementInput_.magnitude > 0 ? new WalkingState(player) : new IdleState(player)); // La propiedad MovementInput_ se actualiza desde el Action Map "Move"
             }
        }
         // Durante la ventana de combo (si está activa), si el timer del ataque termina,
         // el Update del PlayerController decidirá el siguiente estado si el timer de combo expira.
    }

    public override void ExitState()
    {
        player.SetAnimation("isAttacking", false); // Desactivar animación genérica de ataque
        // Asegurarse de que la hitbox de este ataque esté desactivada al salir del estado
        player.SetAttackHitboxesEnabled(false, attackType);

        // Si salimos del estado de ataque por alguna interrupción (hit, resistencia rota),
        // asegurarse de que el combo se resetea si no se completó.
         // La lógica de reset de combo al interrumpir por hit/resistencia rota está en los UpdateState de esos estados.
         // Si salimos de AttackingState a otro estado que NO es de ataque o carga (ej: Idle, Walking), resetear combo.
         if (!(player.currentState is AttackingState) && !(player.currentState is HeavyAttackChargeState))
         {
              player.ResetCombo();
         }
         player.DesiredHorizontalVelocity = Vector3.zero; // Asegurarse de que la velocidad horizontal deseada sea cero al salir del estado de ataque
    }
}

// Estado específico para ataque ligero (hereda de AttackingState)
public class LightAttackState : AttackingState
{
    public LightAttackState(PlayerController player, int currentComboStep) : base(player, player.LightAttackDuration, PlayerController.AttackType.Light, currentComboStep)
    {
        // La duración y el tipo de ataque se pasan a la clase base.
        // currentComboStep se pasa y se guarda en la propiedad 'comboStep' de la clase base.
    }

    public override void EnterState()
    {
        base.EnterState(); // Llama a la lógica EnterState de la clase base

        // Activar la animación específica del paso del combo ligero
        // player.SetAnimation($"isLightAttacking{comboStep}", true); // Ejemplo: isLightAttacking1, isLightAttacking2, etc.
         Debug.Log($"Performing Light Attack - Step {comboStep}"); // Placeholder

         // Consumir estamina al inicio del ataque ligero
         player.Stamina_ -= player.LightAttackStaminaCost;
         player.Stamina_ = Mathf.Max(player.Stamina_, 0);
         if (player.Stamina_ <= 0) player.IsExhausted_ = true;

         // Reiniciar el timer de regeneración de estamina
         player.StartStaminaCooldown();
    }

    public override void UpdateState()
    {
        base.UpdateState(); // Llama a la lógica UpdateState de la clase base
        // Lógica específica del ataque ligero si es necesaria (por ejemplo, detección de hit en un frame específico)
    }

    public override void ExitState()
    {
        base.ExitState(); // Llama a la lógica ExitState de la clase base
        // Desactivar animación específica de ataque ligero
        // player.SetAnimation($"isLightAttacking{comboStep}", false);
         Debug.Log($"Light Attack Step {comboStep} Finished"); // Placeholder
    }
}

// Estado para cargar ataque pesado
public class HeavyAttackChargeState : PlayerState
{
    private float chargeTimer = 0f;

    public HeavyAttackChargeState(PlayerController player) : base(player) { }

    public override void EnterState()
    {
         player.SetAnimation("isChargingHeavyAttack", true); // Activar animación de carga
         // Asegurarse de que otras animaciones estén desactivadas
         player.SetAnimation("isIdle", false);
         player.SetAnimation("isWalking", false);
         player.SetAnimation("isRunning", false);
         player.SetAnimation("isCrouching", false);
         player.SetAnimation("isAttacking", false);
         player.SetAnimation("isBlocking", false);
         player.SetAnimation("isHit", false);
         player.SetAnimation("isDead", false);
         player.SetAnimation("isResistanceBroken", false);
         player.SetAnimation("isJumping", false);
         player.SetAnimation("isDashing", false);


         chargeTimer = 0f;
         // Opcional: Consumir estamina al iniciar la carga o gradualmente
         // player.Stamina_ -= player.HeavyAttackStaminaCost; // Ejemplo de costo inicial

         // Deshabilitar movimiento y asegurarse de que la velocidad horizontal deseada sea cero
         player.currentVelocity = Vector3.zero;
         player.DesiredHorizontalVelocity = Vector3.zero;
         // player.MovementInput_ = Vector2.zero;
         player.ResetCombo(); // Resetear combo al cargar ataque pesado
         player.SetAttackHitboxesEnabled(false); // Asegurarse de que las hitboxes estén desactivadas
         player.BlockPressed_ = false; // Asegurarse de que el bloqueo se desactiva
    }

    public override void UpdateState()
    {
         // --- Restricciones: Si está reaccionando a un golpe, resistencia rota o muerto, cancelar la carga ---
        if (player.isReactingToHit || player.IsResistanceBroken || player.IsDead)
        {
            // Si algo de esto sucede, cancelar la carga y pasar al estado de interrupción
             player.OnHeavyAttackCanceled(); // Llama al método para limpiar el flag de carga
             player.ChangeState(player.isReactingToHit ? new HitReactionState(player) : (player.IsResistanceBroken ? new ResistanceBrokenState(player) : new DeadState(player)));
             return;
        }
         // --- FIN Restricciones ---

        // Si el botón de ataque pesado ya no está presionado, lanzar el ataque o cancelarlo
        if (!player.InputActions_.Combat.HeavyAttack.IsPressed()) // Verificar si el botón sigue presionado (usar Action Map "Combat")
        {
            player.OnHeavyAttackCanceled(); // Asegurarse de limpiar el flag

            // Decidir si lanzar ataque cargado o cancelado basado en chargeTimer y estamina
            if (chargeTimer >= player.ChargedAttackMinChargeTime && player.Stamina_ >= player.ChargedAttackStaminaCost)
            {
                player.ChangeState(new ChargedAttackState(player, chargeTimer)); // Pasar tiempo de carga al estado de ataque cargado
            }
            else
            {
                 // Cancelar la carga si no alcanzó el tiempo mínimo o no tiene estamina
                 Debug.Log("Heavy Attack Charge Canceled (Insufficient charge time or stamina)."); // Placeholder
                 player.ChangeState(player.MovementInput_.magnitude > 0 ? new WalkingState(player) : new IdleState(player)); // La propiedad MovementInput_ se actualiza desde el Action Map "Move"
            }
            player.DesiredHorizontalVelocity = Vector3.zero; // Asegurarse de que sea cero al salir de la carga
            return; // Salir del UpdateState para evitar más lógica de carga
        }

        chargeTimer += Time.deltaTime;
        // Limitar el tiempo de carga al máximo definido
        chargeTimer = Mathf.Clamp(chargeTimer, 0f, player.ChargedAttackMaxChargeTime);

        // Opcional: Retroalimentación visual o sonora mientras se carga (cambiar animación, efectos, etc.)
        // float chargeProgress = Mathf.Clamp01(chargeTimer / player.ChargedAttackMaxChargeTime);
        // player.SetAnimationFloat("ChargeProgress", chargeProgress); // Si tienes un parámetro float para la carga
        // Debug.Log($"Charging Heavy Attack: {chargeTimer:F2}s"); // Placeholder

        // No permitir movimiento, solo rotación hacia la cámara o input
         Vector3 lookDirection = player.MovementInput_.magnitude > 0 ? GetCameraRelativeMovement() : player.transform.forward; // La propiedad MovementInput_ se actualiza desde el Action Map "Move"
         RotatePlayerTowards(lookDirection);

        // Asegurarse de que la velocidad horizontal deseada sea cero durante la carga
         player.DesiredHorizontalVelocity = Vector3.zero;
    }

    public override void ExitState()
    {
        player.SetAnimation("isChargingHeavyAttack", false); // Desactivar animación de carga
        // player.SetAnimationFloat("ChargeProgress", 0f); // Resetear parámetro si lo usaste
         player.isHeavyCharging = false; // Asegurarse de que el flag de carga se desactiva
    }
}

// Estado para ejecutar el ataque pesado o cargado
public class ChargedAttackState : AttackingState
{
    private float finalChargeTime;

    public ChargedAttackState(PlayerController player, float chargeTime) : base(player, player.ChargedAttackDuration, PlayerController.AttackType.Charged) // Usar la duración del ataque cargado
    {
        this.finalChargeTime = chargeTime;
    }

    public override void EnterState()
    {
        base.EnterState(); // Llama a la lógica EnterState de la clase base
        // Aquí podrías activar una animación específica de ataque pesado o cargado
        // player.SetAnimation("isChargedAttacking", true); // O isHeavyAttacking

         // Calcular daño y daño a resistencia basado en el tiempo de carga
         float chargePercentage = Mathf.Clamp01((finalChargeTime - player.ChargedAttackMinChargeTime) / (player.ChargedAttackMaxChargeTime - player.ChargedAttackMinChargeTime));
         float damage = Mathf.Lerp(player.ChargedAttackMinDamage, player.ChargedAttackMaxDamage, chargePercentage);
         float resistanceDamage = Mathf.Lerp(player.ChargedAttackMinResistanceDamage, player.ChargedAttackMaxResistanceDamage, chargePercentage);

         // Aplicar golpe crítico si aplica (solo para ataques cargados)
         bool isCritical = UnityEngine.Random.value < player.CriticalHitChance;
         if (isCritical)
         {
             damage *= player.CriticalHitMultiplier;
             resistanceDamage *= player.CriticalHitMultiplier; // Los críticos también rompen más resistencia
             Debug.Log("Critical Hit!"); // Placeholder
             // Podrías activar un efecto visual o sonoro para crítico
         }

         Debug.Log($"Performing Charged Attack with {finalChargeTime:F2}s charge. Calculated Damage: {damage}, Resistance Damage: {resistanceDamage}"); // Placeholder


         // Consumir estamina (costo fijo para ataque cargado)
         player.Stamina_ -= player.ChargedAttackStaminaCost;
         player.Stamina_ = Mathf.Max(player.Stamina_, 0);
         if (player.Stamina_ <= 0) player.IsExhausted_ = true;

         // Reiniciar el timer de regeneración de estamina
         player.StartStaminaCooldown();

         // --- Lógica para activar la detección de impacto (hitbox) con los valores calculados ---
         // El PlayerController ya tiene el método HandleAttackHit que puede ser llamado
         // desde un script en la hitbox cuando se detecta una colisión.
         // Por ahora, la activación de la hitbox se hace en EnterState de AttackingState base.
         // La hitbox necesitará acceder a estos valores (damage, resistanceDamage, isCritical).
         // Una forma es que la hitbox tenga una referencia al PlayerController y le pida los valores actuales
         // cuando detecta un hit. O que el PlayerController le pase los valores al habilitarla.
    }

    public override void UpdateState()
    {
        base.UpdateState(); // Llama a la lógica UpdateState de la clase base
        // Lógica específica del ataque cargado si es necesaria (por ejemplo, detección de hit en un frame específico)
    }

    public override void ExitState()
    {
        base.ExitState(); // Llama a la lógica ExitState de la clase base
        // Desactivar animación específica de ataque pesado o cargado
        // player.SetAnimation("isChargedAttacking", false); // O isHeavyAttacking
         Debug.Log("Charged Attack Finished"); // Placeholder
    }
}


// Estado para el bloqueo
public class BlockingState : PlayerState
{
    public BlockingState(PlayerController player) : base(player) { }

    public override void EnterState()
    {
         player.SetAnimation("isBlocking", true); // Activar animación de bloqueo
         // Asegurarse de que otras animaciones estén desactivadas
         player.SetAnimation("isIdle", false);
         player.SetAnimation("isWalking", false);
         player.SetAnimation("isRunning", false);
         player.SetAnimation("isCrouching", false);
         player.SetAnimation("isAttacking", false);
         player.SetAnimation("isHit", false);
         player.SetAnimation("isDead", false);
         player.SetAnimation("isResistanceBroken", false);
         player.SetAnimation("isChargingHeavyAttack", false);
         player.SetAnimation("isJumping", false);
         player.SetAnimation("isDashing", false);


         // Asegurarse de que las hitboxes de ataque estén desactivadas al bloquear
         player.SetAttackHitboxesEnabled(false);

        // Opcional: Reducir la velocidad de movimiento mientras bloquea
        // player.currentVelocity *= 0.5f; // Ejemplo: 50% de la velocidad actual
        // Deshabilitar ciertas acciones mientras bloquea (ataque, dash, salto)
         player.ResetCombo(); // Resetear combo al bloquear
         // --- MODIFICADO: Al entrar a bloquear, la velocidad horizontal deseada es la velocidad actual, reducida ---
         Vector3 currentHorizontalVelocity = new Vector3(player.currentVelocity.x, 0, player.currentVelocity.z);
         player.DesiredHorizontalVelocity = Vector3.ClampMagnitude(currentHorizontalVelocity, player.Speed_ * player.BlockingMovementPenalty);
         player.currentVelocity = player.DesiredHorizontalVelocity; // Sincronizar currentVelocity con la velocidad deseada inicial
    }

    public override void UpdateState()
    {
         // --- Restricciones: Si está reaccionando a un golpe, resistencia rota o muerto, salir del estado de bloqueo ---
        if (player.isReactingToHit || player.IsResistanceBroken || player.IsDead)
        {
             player.DesiredHorizontalVelocity = Vector3.zero; // Asegurarse de que la velocidad horizontal deseada sea cero
             // Si algo de esto sucede, salir del estado de bloqueo y pasar al estado de interrupción
             player.ChangeState(player.isReactingToHit ? new HitReactionState(player) : (player.IsResistanceBroken ? new ResistanceBrokenState(player) : new DeadState(player)));
             return;
        }
         // --- FIN Restricciones ---

        // Si el botón de bloqueo ya no está presionado, salir del estado
        if (!player.BlockPressed_) // Usar la propiedad actualizada por el Input System
        {
             player.ChangeState(player.MovementInput_.magnitude > 0 ? new WalkingState(player) : new IdleState(player)); // La propiedad MovementInput_ se actualiza desde el Action Map "Move"
             player.DesiredHorizontalVelocity = Vector3.zero; // Asegurarse de que sea cero al salir del bloqueo
             return;
        }

        // Puedes permitir un movimiento muy lento o ninguna rotación mientras bloqueas
         Vector3 moveDirection = GetCameraRelativeMovement(); // La propiedad MovementInput_ se actualiza desde el Action Map "Move"
         // --- MODIFICADO: Calcular la velocidad horizontal con interpolación y penalización, y almacenar en DesiredHorizontalVelocity ---
         CalculateInterpolatedHorizontalVelocity(moveDirection, player.Speed_ * player.BlockingMovementPenalty);

         // Rotar hacia la dirección del input si hay, o mantener la rotación actual si no
         Vector3 lookDirection = player.MovementInput_.magnitude > 0 ? GetCameraRelativeMovement() : player.transform.forward; // La propiedad MovementInput_ se actualiza desde el Action Map "Move"
         RotatePlayerTowards(lookDirection); // O RotatePlayerTowards(player.transform.forward); para no rotar

         // Si recibe un ataque y tiene resistencia, la lógica de TakeDamage() manejará la reducción de resistencia y el potencial BreakResistance()
    }

    public override void ExitState()
    {
        player.SetAnimation("isBlocking", false); // Desactivar animación de bloqueo
        // Asegurarse de que el flag BlockPressed_ se resetea si no lo hizo el Input System
        // player.BlockPressed_ = false; // Esto ya lo maneja el evento canceled del Input System
    }
}

// Estado para cuando la resistencia está rota
public class ResistanceBrokenState : PlayerState
{
    public ResistanceBrokenState(PlayerController player) : base(player) { }

    public override void EnterState()
    {
        // Activar animación de aturdimiento o tambaleo
        player.SetAnimation("isResistanceBroken", true); // Nueva animación

        player.SetAnimation("isIdle", false);
        player.SetAnimation("isWalking", false);
        player.SetAnimation("isRunning", false);
        player.SetAnimation("isCrouching", false);
        player.SetAnimation("isAttacking", false);
        player.SetAnimation("isBlocking", false);
        player.SetAnimation("isHit", false); // Asegurarse de que la animación de hit no se active si la resistencia se rompe sin recibir hit directo
        player.SetAnimation("isDead", false);
        player.SetAnimation("isChargingHeavyAttack", false);
        player.SetAnimation("isJumping", false);
        player.SetAnimation("isDashing", false);


        // Deshabilitar movimiento y acciones de combate
        player.currentVelocity = Vector3.zero;
        player.DesiredHorizontalVelocity = Vector3.zero; // Asegurarse de que la velocidad horizontal deseada sea cero
        player.MovementInput_ = Vector2.zero; // Resetear input de movimiento

        player.SetAttackHitboxesEnabled(false); // Asegurarse de que las hitboxes estén desactivadas
        player.BlockPressed_ = false; // Asegurarse de que el bloqueo se desactiva
        player.ResetCombo(); // Resetear combo
    }

    public override void UpdateState()
    {
        // --- Restricciones: Si está muerto, salir del estado de resistencia rota ---
        if (player.IsDead)
        {
            player.ChangeState(new DeadState(player));
            return;
        }
         // --- FIN Restricciones ---

        // Si se recibe un impacto que provoca reacción, pasar a HitReactionState (Tiene prioridad)
         if (player.isReactingToHit)
         {
             player.ChangeState(new HitReactionState(player));
             return;
         }


        // En este estado, el jugador está vulnerable y no puede hacer nada hasta que termine el timer
        // La lógica para salir de este estado (resistanceBrokenTimer) está en el Update del PlayerController.

        // Asegurarse de que la velocidad horizontal deseada sea cero durante este estado
         player.DesiredHorizontalVelocity = Vector3.zero;
    }

    public override void ExitState()
    {
         player.SetAnimation("isResistanceBroken", false); // Desactivar animación
         // Al salir de resistencia rota, reiniciar el timer de regeneración de resistencia
         player.resistanceRegenTimer = player.ResistanceRegenCooldown;
    }
}

// Estado para reaccionar a un golpe
public class HitReactionState : PlayerState
{
    public HitReactionState(PlayerController player) : base(player) { }

    public override void EnterState()
    {
         player.SetAnimation("isHit", true); // Activar animación de impacto

         // Asegurarse de que otras animaciones estén desactivadas
         player.SetAnimation("isIdle", false);
         player.SetAnimation("isWalking", false);
         player.SetAnimation("isRunning", false);
         player.SetAnimation("isCrouching", false);
         player.SetAnimation("isAttacking", false);
         player.SetAnimation("isBlocking", false);
         player.SetAnimation("isResistanceBroken", false);
         player.SetAnimation("isDead", false); // Asegurarse de que la animación de muerte no se active si solo es un hit
         player.SetAnimation("isChargingHeavyAttack", false);
         player.SetAnimation("isJumping", false);
         player.SetAnimation("isDashing", false);


        // Deshabilitar movimiento y acciones de combate durante la reacción
        player.currentVelocity = Vector3.zero;
        player.DesiredHorizontalVelocity = Vector3.zero; // Asegurarse de que la velocidad horizontal deseada sea cero
        player.MovementInput_ = Vector2.zero; // Resetear input de movimiento
        player.SetAttackHitboxesEnabled(false); // Asegurarse de que las hitboxes estén desactivadas
        player.BlockPressed_ = false; // Asegurarse de que el bloqueo se desactiva
        player.ResetCombo(); // Resetear combo
    }

    public override void UpdateState()
    {
         // --- Restricciones: Si está muerto, salir del estado de reacción ---
        if (player.IsDead)
        {
            player.ChangeState(new DeadState(player));
            return;
        }
         // --- FIN Restricciones ---


        // En este estado, el jugador está en la animación de reacción
        // La lógica para salir de este estado (hitReactionTimer) está en el Update del PlayerController.

        // Si se recibe otro impacto que provoca reacción ANTES de que termine el timer actual,
        // la lógica en TakeDamage() reiniciará el timer. El estado se mantendrá.

        // Asegurarse de que la velocidad horizontal deseada sea cero durante este estado
         player.DesiredHorizontalVelocity = Vector3.zero;
    }

    public override void ExitState()
    {
        player.SetAnimation("isHit", false); // Desactivar animación de impacto
        player.isReactingToHit = false; // Asegurarse de que el flag se desactiva
    }
}

// Estado de muerte
public class DeadState : PlayerState
{
    public DeadState(PlayerController player) : base(player) { }

    public override void EnterState()
    {
         player.SetAnimation("isDead", true); // Activar animación de muerte

         // Asegurarse de que todas las demás animaciones y flags estén desactivados
         player.SetAnimation("isIdle", false);
         player.SetAnimation("isWalking", false);
         player.SetAnimation("isRunning", false);
         player.SetAnimation("isCrouching", false);
         player.SetAnimation("isAttacking", false);
         player.SetAnimation("isBlocking", false);
         player.SetAnimation("isResistanceBroken", false);
         player.SetAnimation("isHit", false);
         player.SetAnimation("isChargingHeavyAttack", false);
         player.SetAnimation("isJumping", false);
         player.SetAnimation("isDashing", false);


        // En este estado, el jugador está muerto y no puede hacer nada
        // El CharacterController y el Animator (o ragdoll) ya fueron manejados en el método Die()
        player.currentVelocity = Vector3.zero;
        player.DesiredHorizontalVelocity = Vector3.zero; // Asegurarse de que la velocidad horizontal deseada sea cero
        player.MovementInput_ = Vector2.zero; // Resetear input de movimiento
        player.SetAttackHitboxesEnabled(false); // Asegurarse de que las hitboxes estén desactivadas
        player.BlockPressed_ = false; // Asegurarse de que el bloqueo se desactiva
        player.ResetCombo(); // Resetear combo
        player.isReactingToHit = false; // Asegurarse de que el flag se desactiva
        player.IsResistanceBroken = false; // Asegurarse de que el flag se desactiva

        // Aquí podrías iniciar alguna secuencia post-muerte (pantalla de game over, etc.)
    }

    public override void UpdateState()
    {
        // Asegurarse de que la velocidad horizontal deseada sea cero mientras está muerto
         player.DesiredHorizontalVelocity = Vector3.zero;
        // No hay lógica de actualización para un personaje muerto en este script.
        // La gravedad podría seguir actuando si el ragdoll la usa.
    }

    public override void ExitState()
    {
        // No hay salida "normal" de este estado (a menos que implementes respawn).
        // Si implementas respawn, aquí desactivarías la animación de muerte y el ragdoll,
        // y re-habilitarías el CharacterController, Animator, input, etc.
    }
}


// © 2025 KOIYOT. All rights reserved.