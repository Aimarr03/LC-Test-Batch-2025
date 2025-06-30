using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    PlayerInput input;
    PlayerMapController inputController;

    Rigidbody2D rigidBody;
    Collider2D playerCollider;

    [SerializeField] SpriteRenderer spriteRenderer;

    //Movement Property
    [Header("Movement Property")]
    [SerializeField] private float movementSpeed = 5;
    float xDirection = 0;

    //Jump Property
    [Header("Jump Property")]
    [SerializeField] private float jumpForce = 3f;
    
    //Attack Property
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

    string currentGroundTag = "";
    private void Awake()
    {
        inputController = new PlayerMapController();
    }
    private void Start()
    {
        inputController.Gameplay.Enable();
        input = GetComponent<PlayerInput>();
        rigidBody = GetComponent<Rigidbody2D>();
        playerCollider = GetComponent<Collider2D>();
        
        xDirection = 1f;

        inputController.Gameplay.Jump.performed += ActionJump;
        inputController.Gameplay.Dash.performed += ActionDash;
        inputController.Gameplay.Attack.performed += ActionAttack;
    }

    private void Update()
    {
        if (dashPerformed) return;
        MovementLogic();
    }
    private void MovementLogic()
    {
        Vector2 currentVelocity = rigidBody.linearVelocity;
        float currentInput = inputController.Gameplay.Move.ReadValue<float>();

        currentVelocity.x = currentInput * movementSpeed;
        if (currentVelocity.x != 0)
        {
            xDirection = currentInput;
            spriteRenderer.flipX = xDirection < 0;
        }
        rigidBody.linearVelocity = currentVelocity;
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
    private void ActionJump(InputAction.CallbackContext context)
    {
        if (dashPerformed) return;
        
        if (context.performed)
        {
            
            if (crouchPerformed)
            {
                Debug.Log("Character Off");
                playerCollider.enabled = false;
                rigidBody.linearVelocityY = -2f;
                Invoke("EnableCollider", 0.4f);
            }
            else
            {
                Debug.Log("Character Jump");
                rigidBody.linearVelocityY = 0.5f;
                rigidBody.AddForceY(jumpForce, ForceMode2D.Impulse);
            }
        }
    }
    private void ActionDash(InputAction.CallbackContext context)
    {
        if (dashAvailability)
        {
            Debug.Log("Dash Performed");
            StartCoroutine(DashPerformed());
        }
        
    }
    private void ActionAttack(InputAction.CallbackContext context)
    {
        if (dashPerformed) return;
        
        if (Time.time - lastAttackPerformed > 0.2f)
        {
            CancelInvoke("EndCombo");
            Debug.Log($"Attack Performed : Combo {comboCounter}");
            
            comboCounter++;
            lastAttackPerformed = Time.time;
            Invoke("EndCombo", 1.35f);
            if(comboCounter >= maxCombo)
            {
                CancelInvoke("EndCombo");
                EndCombo();
                Debug.Log("Attack Reach Max");
            }
        }
    }
    private void CrouchAction(InputAction.CallbackContext context)
    {
        if (context.canceled)
        {
            crouchPerformed = false;
        }
        else
        {
            crouchPerformed = true;
        }
    }
    #endregion
    private void EndCombo()
    {
        Debug.Log("Invoke End Combo");
        comboCounter = 0;
    }

    private IEnumerator DashPerformed()
    {
        dashPerformed = true;
        dashAvailability = false;

        float timeDashPerformed = Time.time;
        float bufferGravity = rigidBody.gravityScale;
        float dashDirection = xDirection * dashPower;

        rigidBody.gravityScale = 0.5f;
        while(Time.time - timeDashPerformed < dashDuration)
        {
            rigidBody.linearVelocityX = dashDirection;
            
            float yVelocity = rigidBody.linearVelocityY;
            rigidBody.linearVelocityY = Mathf.Min(yVelocity, 0.5f);

            yield return null;
        }
        dashPerformed = false;
        rigidBody.gravityScale = bufferGravity;
        Invoke("EnableDash", dashCooldown);
    }
    private void EnableDash()
    {
        Debug.Log("Dash Available");
        dashAvailability = true;
    }
    private void EnableCollider()
    {
        playerCollider.enabled = true;   
    }
}
