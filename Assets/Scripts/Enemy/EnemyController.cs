using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class EnemyController : MonoBehaviour
{
    [Header("Component")]
    Animator animator;
    Collider2D enemyCollider;
    Rigidbody2D enemyRigidBody;
    [SerializeField] SpriteRenderer spriteRenderer;

    [Header("Enemy General Property")]
    [SerializeField] private int maxHealth = 30;
    [SerializeField] private LayerMask playerLayer;
    private bool isDead = false;
    private int currentHealth = 0;


    [Header("Attack Property")]
    [SerializeField] private float attackSpeed = 1f;
    [SerializeField] private float attackRange = 3f;
    [SerializeField] private int normalDamage = 12;
    private EnemyState currentState;
    private Action currentAction;
    private PlayerController playerController;
    private float attackTrigger;

    [Header("Chase Property")]
    private Vector3 defaultPos;
    [SerializeField] private float movementSpeed = 2f;
    [SerializeField] private float aggroRange = 4f;
    [SerializeField] private float maxDistanceChase;

    [Header("UI")]
    [SerializeField] private Canvas canvas;
    [SerializeField] private Image HPBackground;
    [SerializeField] private Image HPBar;

    [Header("Audio")]
    [SerializeField] private AudioClip sfx_attack;
    [SerializeField] private AudioClip sfx_hurt;
    [SerializeField] private AudioClip sfx_dead;

    public static event Action Defeated;
    private void Awake()
    {
        currentState = EnemyState.Idle;
        currentHealth = maxHealth;
        enemyCollider = GetComponent<Collider2D>();
        enemyRigidBody = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        
        defaultPos = transform.position;
        currentAction = DetectingPlayer;
    }
    void Start()
    {
        canvas.gameObject.SetActive(false);
        PlayerController.PlayerDead += PlayerController_PlayerDead;
        EnemyController.Defeated += EnemyController_Defeated;
    }

    private void EnemyController_Defeated()
    {
        if (isDead) return;
        currentHealth += 12;
        maxHealth += 12;
        if(HPBar != null)HPBar.fillAmount = ((float)currentHealth) / (float)maxHealth;
        attackSpeed -= 0.12f;
        movementSpeed += 0.5f;
        normalDamage += 8;
    }

    private void OnDestroy()
    {
        EnemyController.Defeated += EnemyController_Defeated;
        PlayerController.PlayerDead -= PlayerController_PlayerDead;
    }
    // Update is called once per frame
    void Update()
    {
        if(isDead) return;
        currentAction?.Invoke();
    }
    private void PlayerController_PlayerDead()
    {
        canvas.gameObject.SetActive(false);
        currentAction = null;
        animator.SetBool("Moving", false);
        playerController = null;
    }
    private IEnumerator OnDetectPlayer()
    {
        float currentTime = Time.time;
        yield return null;

        while (Time.time - currentTime < 1f)
        {
            Vector2 direction = (Vector2)(playerController.transform.position - transform.position).normalized;
            if (direction.x != 0) spriteRenderer.flipX = direction.x < 0;
            yield return null;
        }

        yield return null;
        SetToChase();
    }
    private IEnumerator ResetToDefault()
    {
        yield return new WaitForSeconds(1f);
        Vector2 direction = (defaultPos - transform.position).normalized;
        spriteRenderer.flipX = direction.x < 0;
        enemyRigidBody.linearVelocity = direction * movementSpeed;
        currentAction = GoBackToDefaultPos;
    }
    private void DetectingPlayer()
    {
        Collider2D playerCollider = Physics2D.OverlapCircle(transform.position, aggroRange, playerLayer);
        if (playerCollider != null)
        {
            Debug.Log("Player Hit!");
            StartCoroutine(OnDetectPlayer());
            playerController = playerCollider.GetComponent<PlayerController>();
            currentAction = null;
        }   
    }
    private void GoBackToDefaultPos()
    {
        Debug.Log("Goign Back to Default Pos");
        Vector2 direction = (defaultPos - transform.position).normalized;
        float distance = Vector2.Distance(transform.position, defaultPos);
        if(distance < 0.5f)
        {
            enemyRigidBody.linearVelocity = Vector2.zero;
            currentAction = DetectingPlayer;
        }
    }
    private void ChasePlayer()
    {
        float distanceWithDefaultPos = Vector2.Distance(transform.position, defaultPos);
        if (distanceWithDefaultPos > maxDistanceChase)
        {
            Debug.Log("Distance with Default Pos: " + distanceWithDefaultPos);
            animator.SetBool("Moving", true);
            enemyRigidBody.linearVelocity = Vector2.zero;
            
            currentAction = null;
            playerController = null;
            StartCoroutine(ResetToDefault());
            return;
        }
        if (Vector2.Distance(transform.position, playerController.transform.position) > attackRange)
        {
            Vector2 direction = (Vector2)(playerController.transform.position - transform.position).normalized;
            Vector2 velocity = direction * movementSpeed;
            enemyRigidBody.linearVelocity = velocity;
            if (direction.x != 0) spriteRenderer.flipX = direction.x < 0;

        }
        else
        {
            Debug.Log("Enemy Focus On Attack");
            enemyRigidBody.linearVelocity = Vector2.zero;
            currentAction = FocusOnPlayer;
            TriggerAttack();
        }
    }
    //This script is focus on enemy when near player to do actions like hitting or get hit
    private void FocusOnPlayer()
    {
        float elapsedTime = Time.time - attackTrigger;
        //Debug.Log(elapsedTime);
        if (animator.GetCurrentAnimatorStateInfo(0).IsTag("Idle") && elapsedTime > attackSpeed)
        {
            //Do Action
            float distance = Vector2.Distance(playerController.transform.position, transform.position);
            if(distance < attackRange)
            {
                Debug.Log("Enemy Attack again!");
                TriggerAttack();
            }
            else if(distance < aggroRange)
            {
                Debug.Log("Player On Aggro!");
                currentAction = null;
                SetToChase();
            }
            else
            {
                currentAction = DetectingPlayer;
            }
        }
    }
    private void TriggerAttack()
    {
        enemyRigidBody.linearVelocity = Vector2.zero;
        Debug.Log("Player within Attack Range!");
        animator.SetBool("Moving", false);
        animator.SetTrigger("Attack");

        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        attackTrigger = Time.time + stateInfo.length;
    }
    private void SetToChase()
    {
        Debug.Log("Chasing Player!");
        currentState = EnemyState.Fighting;
        currentAction = ChasePlayer;
        animator.SetBool("Moving", true);
    }
    

    public void TakeDamage(int damage, Vector2 direction)
    {
        if(isDead) return;
        currentHealth = Mathf.Clamp(currentHealth - damage, 0, maxHealth);
        isDead = currentHealth <= 0;
        HPBar.fillAmount = ((float)currentHealth) / (float)maxHealth;
        canvas.gameObject.SetActive(!isDead);

        animator.SetTrigger("Hurt");
        StopAllCoroutines();

        
        
        spriteRenderer.flipX = direction.x > 0;

        if(playerController == null) playerController = FindFirstObjectByType<PlayerController>();

        if (!isDead)
        {
            AudioManager.Instance.PlayAudio(sfx_hurt, true, 0.8f);
            enemyRigidBody.linearVelocity = direction * 0.2f;
            Invoke(nameof(ResetVelocity), 0.2f);
            currentAction = FocusOnPlayer;
            attackTrigger = Time.time - (attackSpeed * 0.75f);
            animator.SetBool("Moving", false);
            Debug.Log(Time.time - attackTrigger);
            Debug.Log($"Enemy Take Damage {damage} from {direction}");
        }
        else
        {
            AudioManager.Instance.PlayAudio(sfx_dead);
            AudioManager.Instance.PlayAudio("sfx-killed");
            EffectManager.Instance.FreezeHit(0.12f);
            enemyRigidBody.linearVelocity = direction * 5.2f;
            Invoke(nameof(ResetVelocity), 0.15f);
            currentAction = null;
            playerController = null;
            animator.SetBool("Dead", true);
            Defeated?.Invoke();
        }        
    }
    private void ResetVelocity()
    {
        enemyRigidBody.linearVelocity = Vector2.zero;
        if (isDead)
        {
            enemyCollider.enabled = false;
        }
    }
    public void Attack()
    {
        Debug.Log($"Enemy {gameObject.name} Attack");
        float distance = Vector2.Distance(playerController.transform.position, transform.position);
        Vector2 direction = (playerController.transform.position - transform.position).normalized;
        if(distance < attackRange * 3)
        {
            AudioManager.Instance.PlayAudio(sfx_attack, true, 0.8f);
            playerController.TakeDamage(normalDamage, direction);
        }
    }
}
public enum EnemyState
{
    Idle,
    Fighting,
    Dead
}
