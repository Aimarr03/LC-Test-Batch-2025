using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    PlayerInput input;
    PlayerMapController inputController;

    Rigidbody2D rigidBody;
    Collider2D playerCollider;
    Animator animator;
    PlayerUI playerUI;

    [SerializeField] SpriteRenderer spriteRenderer;

    [Header("Player Stats")]
    [SerializeField] private int maxHealth = 100;
    private int currentHealth = 0;
    private bool isDead = false;
    private bool immuneTakeDamage = false;

    //Movement Property
    [Header("Movement Property")]
    [SerializeField] private float movementSpeed = 5;
    Vector2 movementDirection = Vector2.zero;

    public Vector2 MovementDir => movementDirection;
    //Attack Property
    bool attackBuffer = false;
    float lastAttackPerformed = 0;
    int comboCounter = 0;
    int maxCombo = 3;

    //Crouch Property
    bool crouchPerformed = false;

    //Dash Performed
    bool dashAvailability = true;
    bool dashPerformed = false;
    [Header("Dash Property")]
    [SerializeField] float dashCooldown = 3;
    [SerializeField] float dashPower = 8f;
    [SerializeField] float dashDuration = 0.3f;

    [Header("Particle System")]
    [SerializeField] private ParticleSystem particleMoveEffect;

    string currentGroundTag = "";
    [Header("Attack Property")]
    public List<AttackSO> attackCombo;
    [SerializeField] LayerMask enemyLayer;
    [SerializeField] Transform attackStartingPos;

    AttackSO currentAttack;
    Coroutine attackMovement;

    public static event Action PlayerDead;
    [Header("Player Audio")]
    [SerializeField] private AudioClip attackSound;
    [SerializeField] private AudioClip heavyAttackSound;
    [SerializeField] private AudioClip hurtSound;
    [SerializeField] private AudioClip deadSound;
    [SerializeField] private AudioClip swordHitSound;
    [SerializeField] private AudioClip heavySwordHitSound;
    [SerializeField] private AudioClip dashSound;

    private void Awake()
    {
        inputController = new PlayerMapController();
        playerUI = FindFirstObjectByType<PlayerUI>();
    }
    private void Start()
    {
        inputController.GameplayTopDown.Disable();
        currentHealth = maxHealth;
        
        input = GetComponent<PlayerInput>();
        rigidBody = GetComponent<Rigidbody2D>();
        playerCollider = GetComponent<Collider2D>();
        animator = GetComponent<Animator>();

        inputController.GameplayTopDown.Dash.performed += ActionDash;
        inputController.GameplayTopDown.Attack.performed += ActionAttack;
        //inputController.GameplayTopDown.
    }

    private void Update()
    {
        if (isDead)
        {
            rigidBody.linearVelocity = Vector2.zero;
            return;
        }
        if (dashPerformed) return;
        MovementLogic();
        if (attackBuffer)
        {
            AnimatorStateInfo animatorState = animator.GetCurrentAnimatorStateInfo(0);
            if (animatorState.IsName("Knight Attack") && animatorState.normalizedTime > 0.75f)
            {
                HandleAttackExecution();
            }
        }
    }
    private void MovementLogic()
    {
        Vector2 currentVelocity = rigidBody.linearVelocity;
        Vector2 currentInput = inputController.GameplayTopDown.Move.ReadValue<Vector2>();

        currentVelocity = currentInput * movementSpeed;
        if (currentVelocity != Vector2.zero)
        {
            movementDirection = currentInput;
            spriteRenderer.flipX = movementDirection.x < 0;

        }
        animator.SetBool("Moving", currentInput != Vector2.zero);
        
        //Debug.Log("Name State: "+ animator.GetCurrentAnimatorStateInfo(0).IsName("Knight Attack"));
        //Debug.Log("Tag State: " + animator.GetCurrentAnimatorStateInfo(0).IsTag("Attack"));
        
        if (!animator.GetCurrentAnimatorStateInfo(0).IsName("Knight Attack"))
        {
            rigidBody.linearVelocity = currentVelocity;
        }
    }
    private void MovingPerformed(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            Debug.Log("Character begin moving");
        }
        else if (context.canceled)
        {
            Debug.Log("Character stop moving");
        }
        Vector2 currentVelocity = rigidBody.linearVelocity;
        float currentInput = context.ReadValue<float>();

        currentVelocity.x = currentInput * movementSpeed;
        rigidBody.linearVelocity = currentVelocity;
    }

    #region Action Logic
    private void ActionDash(InputAction.CallbackContext context)
    {
        if (dashAvailability)
        {
            Debug.Log("Dash Performed");
            StartCoroutine(DashPerformed());
        }
        else
        {
            playerUI.DashUnavailable();
        }
        
    }
    private void ActionAttack(InputAction.CallbackContext context)
    {
        if (dashPerformed) return;

        Invoke("HandleBufferAttack", 0.22f);
        if (Time.time - lastAttackPerformed > 0.3f && !attackBuffer)
        {
            HandleAttackExecution();
            return;
        }
        attackBuffer = true;
    }
    private void HandleAttackExecution()
    {
        CancelInvoke("EndCombo");
        if (attackMovement != null) StopCoroutine(attackMovement);

        Debug.Log($"Attack Performed : Combo {comboCounter}");

        currentAttack = attackCombo[comboCounter];
        rigidBody.linearVelocity = movementDirection * 0.1f;

        animator.runtimeAnimatorController = currentAttack.overrideController;
        animator.SetTrigger("Attack");
        attackMovement = StartCoroutine(AttackMovement());

        comboCounter++;
        lastAttackPerformed = Time.time;

        Invoke("EndCombo", 1.35f);
        
        if (comboCounter >= maxCombo)
        {
            AudioManager.Instance.PlayAudio(heavyAttackSound, true, 0.7f);
            CancelInvoke("EndCombo");
            EndCombo();
            Debug.Log("Attack Reach Max");
        }
        else AudioManager.Instance.PlayAudio(attackSound, true, 1.4f);
    }
    private void ActionDefend(InputAction.CallbackContext context)
    {
        Debug.Log("Defend!");
    }
    #endregion
    public void Attack()
    {
        Vector2 directionAttack = movementDirection.x > 0? Vector2.right: Vector2.left;
        Vector3 attackPos = attackStartingPos.position;
        attackPos.x = movementDirection.x > 0 ? 1 : -1;
        RaycastHit2D[] attackEnemy = Physics2D.BoxCastAll(attackStartingPos.position, new Vector2(1, 3), 0, directionAttack, currentAttack.range ,enemyLayer);
        bool hit = attackEnemy.Length > 0;
        if (hit)
        {
            if(comboCounter == 0) AudioManager.Instance.PlayAudio(heavySwordHitSound, true, 0.3f);
            else AudioManager.Instance.PlayAudio(swordHitSound, true, 0.3f);
        }
        foreach (RaycastHit2D enemy in attackEnemy)
        {
            Debug.Log("Enemy Attacked: " + enemy.collider.gameObject.name);
            EnemyController currentEnemy = enemy.collider.gameObject.GetComponent<EnemyController>();

            Vector2 direction = (Vector2)(currentEnemy.transform.position - transform.position).normalized;
            currentEnemy.TakeDamage(currentAttack.damage, direction);
        }
    }
    private void EndCombo()
    {
        Debug.Log("Invoke End Combo");
        comboCounter = 0;
    }
    private IEnumerator DashPerformed()
    {
        dashPerformed = true;
        dashAvailability = false;
        immuneTakeDamage = true;
        animator.SetTrigger("Dash");
        AudioManager.Instance.PlayAudio(dashSound, true, 1f);
        float timeDashPerformed = Time.time;
        float bufferGravity = rigidBody.gravityScale;
        playerUI.DashInvoke(dashCooldown);
        Invoke("EnableDash", dashCooldown);


        Vector2 dashDirection = movementDirection * dashPower;
        rigidBody.linearVelocity = dashDirection;
        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        
        yield return null;
        Debug.Log("Dash Direction: " + dashDirection);
        Debug.Log("Animator Normalized Time => " + animator.GetCurrentAnimatorStateInfo(0).normalizedTime);

        while(animator.GetCurrentAnimatorStateInfo(0).normalizedTime < 0.7f)
        {
            yield return null;
        }
        
        
        rigidBody.linearVelocity = Vector2.one * 0.5f;
        dashPerformed = false;
        immuneTakeDamage = false;
    }
    public void TakeDamage(int damage, Vector2 directionHit)
    {
        if(isDead) return;
        if (immuneTakeDamage) return;
        currentHealth = Mathf.Clamp(currentHealth - damage, 0, maxHealth);
        isDead = currentHealth <= 0;
        animator.SetTrigger("Hurt");
        playerUI.UpdateHealth(currentHealth, maxHealth);
        inputController.Gameplay.Disable();

        float percentageHealth = ((float)currentHealth) / ((float)maxHealth);
        if (percentageHealth < 0.3f)
        {
            //Invoke
            EffectManager.Instance.DisplayLowHealth(true);
        }
        if (isDead)
        {
            AudioManager.Instance.StopMusic();
            Debug.Log("Player Dies");
            EffectManager.Instance.FreezeHit();
            EffectManager.Instance.DisplayDead();
            animator.SetBool("Dead", true);
            animator.speed = 0.25f;
            AudioManager.Instance.PlayAudio("sfx-killed");
            AudioManager.Instance.PlayAudio(deadSound);
            PlayerDead?.Invoke();
            rigidBody.linearVelocity = Vector2.zero;
        }
        else
        {
            AudioManager.Instance.PlayAudio(hurtSound, true, 1f);
            rigidBody.linearVelocity = directionHit * 2f;
            EffectManager.Instance.DisplayHurt();
            EffectManager.Instance.FreezeHit(0.05f);
            Invoke("AfterHurt", 0.35f);
        }
        
    }
    private void AfterHurt()
    {
        rigidBody.linearVelocity = Vector2.zero;
        inputController.Gameplay.Enable();
    }
    private IEnumerator AttackMovement()
    {
        yield return new WaitUntil(() => animator.GetCurrentAnimatorStateInfo(0).normalizedTime > 0.25f);
        rigidBody.linearVelocity = movementDirection * currentAttack.moveSpeed;
        yield return new WaitForSeconds(0.15f);
        rigidBody.linearVelocity = movementDirection * 0.1f;
    }
    private void HandleBufferAttack()
    {
        attackBuffer = false;
    }
    private void EnableDash()
    {
        Debug.Log("Dash Available");
        dashAvailability = true;
    }
    public void ChangeEnabilityInput(bool value)
    {
        if(value) inputController.GameplayTopDown.Enable();
        else inputController.GameplayTopDown.Disable();
    }
}
