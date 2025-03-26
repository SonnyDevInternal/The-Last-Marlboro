using static UnityEngine.JsonUtility;
using UnityEngine;
using System.IO;
using Unity.VisualScripting;
using UnityEngine.SceneManagement;

public struct PlayerSaveData
{
    public Vector3 position;
    public Quaternion rotation;
    public Vector3 velocity;
    public int sceneID;
    public int characterID;

    public float stamina;
    public bool isAlive;

    string GetSaveData()
    {
        return JsonUtility.ToJson(this);
    }

    bool LoadSaveData(string jsonSaveData)
    {
        try
        {
            JsonUtility.FromJsonOverwrite(jsonSaveData, this);
            return true;
        }
        catch (System.Exception)
        {
            return false;
        }
    }
}

public class Player : MonoBehaviour
{
    public enum PlayerMovement
    {
        None,
        Forward = 1 << 1,
        Backward = 1 << 2,
        Right = 1 << 3,
        Left = 1 << 4,
        Up = 1 << 5,
        Down = 1 << 6,
        FastForward = 1 << 7, //running

        All = Forward | Backward | Right | Left | Up | Down | FastForward,
    }

    public enum VelocityUpdate
    {
        None,
        Forward = 1 << 1,
        Right = 1 << 2,
        Up = 1 << 3,
        ForwardRight = Forward | Right,
    }

    public enum GameState
    {
        GameStarted,
        GameEnded
    }

    static PlayerSaveData saveData = new PlayerSaveData();

    protected Inventory playerInventory = null;

    [SerializeField]
    protected GameObject head = null;

    [SerializeField]
    protected GameObject leftHand = null;

    //[SerializeField]
    //protected 

    protected Rigidbody playerRigidBody = null;
    protected CapsuleCollider playerCapsuleCollider = null;

    private bool isGrounded = false;
    private bool isForcedSliding = false;

    protected float forceSlideTime = 2.0f;
    protected float forceSlideTimeCurrent = 0.0f;

    public float playerDrag = 3.0f;
    public float terminalVelocity = 30.0f;

    protected bool canMove = true;
    protected bool canJump = false;
    protected bool canRotate = true;
    protected bool canHeal = true;

    public bool isConsumingStamina = true;
    public bool isLocalPlayer = false;

    public bool isAlive = true;

    protected bool isMovementHooked = false;

    public float gravity = 9.8f;

    public float runningSpeed = 14.0f;
    public float movementSpeed = 6.0f;
    public float jumpHeight = 4.0f;

    [SerializeField]
    protected float currentStamina = 0.0f;

    public float maxStamina = 100.0f;

    public float regenStaminaAmount = 10.0f;
    public float degenStaminaAmount = 20.0f;

    protected float timeSinceWalking_Current = 0.0f;
    protected float timeSinceWalking = 2.0f;

    [SerializeField]
    private int currentHealth = 100;

    public int maxHealth = 100;
    public int maxHealAmount = 25;

    public int regenHealthAmount = 5;

    protected float timeSinceDamaged_Current = 0.0f;
    protected float timeSinceDamaged = 2.0f;

    public float interactDistance = 10.0f;

    public delegate void OnUpdatePlayer(Player _this);
    public delegate void OnDestroyPlayer(Player _this, bool ByScene);
    public delegate PlayerMovement OnMovePlayerHook(Player _this, PlayerMovement originalMovement);
    public delegate void OnChangedLivingState(Player _this, bool alive);

    protected OnUpdatePlayer onUpdatePlayer = null;
    protected OnDestroyPlayer onDestroyPlayer = null;
    protected OnMovePlayerHook onMovePlayerHook = null;
    protected OnChangedLivingState onChangedLivingState = null;

    public void BindOnUpdate(OnUpdatePlayer onUpdateValue) {this.onUpdatePlayer += onUpdateValue;}
    public void BindOnDestroy(OnDestroyPlayer onDestroyValue) {this.onDestroyPlayer += onDestroyValue;}
    public void BindOnChangeLivingState(OnChangedLivingState onChangedLivingState) {this.onChangedLivingState += onChangedLivingState;}
    public void HookOnMovePlayer(OnMovePlayerHook onMovePlayerHook) { this.isMovementHooked = true; this.onMovePlayerHook = onMovePlayerHook;}

    public void UnbindOnDestroy(OnDestroyPlayer onDestroyValue) { this.onDestroyPlayer -= onDestroyValue; }
    public void UnbindOnUpdate(OnUpdatePlayer onUpdateValue) { this.onUpdatePlayer -= onUpdateValue; }
    public void UnbindOnChangeLivingState(OnChangedLivingState onChangedLivingState) { this.onChangedLivingState -= onChangedLivingState; }

    public void UnhookOnMovePlayer() { this.isMovementHooked = false; this.onMovePlayerHook = null; }

    private void Start()
    {
        playerRigidBody = GetComponent<Rigidbody>();
        playerCapsuleCollider = GetComponent<CapsuleCollider>();

        playerInventory = GetComponent<Inventory>();

        Initialize();
    }

    private void Destroy()
    {
        if (onDestroyPlayer != null)
            onDestroyPlayer(this, false);
    }


    private void Update()
    {
        isGrounded = Physics.BoxCast(
        transform.position,
        new Vector3(playerCapsuleCollider.radius, 0.05f, playerCapsuleCollider.radius),
        -transform.up,
        Quaternion.identity,
        (playerCapsuleCollider.height / 2.0f) + 0.05f,
        ~0,
        QueryTriggerInteraction.Ignore);


        if (timeSinceWalking_Current >= timeSinceWalking)
        {
            if (currentStamina < maxStamina)
            {
                currentStamina += (regenStaminaAmount * Time.deltaTime);

                if (currentStamina > maxStamina)
                    currentStamina = maxStamina;
            }
        }
        else
            timeSinceWalking_Current += Time.deltaTime;

        if (timeSinceDamaged_Current >= timeSinceDamaged)
        {
            if(currentHealth < maxHealAmount)
            {
                currentHealth += (int)((float)regenHealthAmount * Time.deltaTime);

                if(currentHealth > maxHealAmount)
                    currentHealth = maxHealAmount;
            }
        }
        else
            timeSinceDamaged_Current += Time.deltaTime;

        if (isForcedSliding)
        {

            if (forceSlideTimeCurrent >= forceSlideTime)
            {
                forceSlideTimeCurrent = 0.0f;
                isForcedSliding = false;

                canMove = true;
            }
            else
                forceSlideTimeCurrent += Time.deltaTime;
        }

        this.UpdatePlayerGravity();
    }


    public void DamagePlayer(int DamageValue)
    {
        currentHealth -= DamageValue;

        if(currentHealth <= 0.0f)
        {
            onChangedLivingState?.Invoke(this, false);
        }
    }

    public void HealPlayer(int healValue, bool healOverMax = false)
    {
        currentHealth += healValue;

        if(!healOverMax && currentHealth > maxHealAmount)
            currentHealth = maxHealAmount;
    }

    public void SetHealth(int value)
    {
        currentHealth = value;
    }

    private void UpdatePlayerGravity()
    {
        if (gravity > 0.0f && !isGrounded)
        {
            var velocityCopy = this.playerRigidBody.linearVelocity;

            if (velocityCopy.y > -terminalVelocity)
            {
                velocityCopy.y -= (gravity * Time.deltaTime);

                if (velocityCopy.y < -terminalVelocity)
                    velocityCopy.y = -terminalVelocity;

                this.playerRigidBody.linearVelocity = velocityCopy;
            }
        }

        if (playerDrag > 0.0f && CanPerformDrag())
        {
            bool changedVelocity = false;

            Vector3 localVelocity = transform.InverseTransformDirection(this.playerRigidBody.linearVelocity);

            if (localVelocity.x > 0.0f)
            {
                localVelocity.x -= playerDrag * Time.deltaTime;
                if (localVelocity.x < 0.0f) localVelocity.x = 0.0f;
                changedVelocity = true;
            }
            else if (localVelocity.x < 0.0f)
            {
                localVelocity.x += playerDrag * Time.deltaTime;
                if (localVelocity.x > 0.0f) localVelocity.x = 0.0f;
                changedVelocity = true;
            }

            if (localVelocity.z > 0.0f)
            {
                localVelocity.z -= playerDrag * Time.deltaTime;
                if (localVelocity.z < 0.0f) localVelocity.z = 0.0f;
                changedVelocity = true;
            }
            else if (localVelocity.z < 0.0f)
            {
                localVelocity.z += playerDrag * Time.deltaTime;
                if (localVelocity.z > 0.0f) localVelocity.z = 0.0f;
                changedVelocity = true;
            }

            if (changedVelocity)
            {
                Vector3 transformedVelocity = transform.TransformDirection(localVelocity);

                Vector3 newVelocityCal = new Vector3(transformedVelocity.x, this.playerRigidBody.linearVelocity.y, transformedVelocity.z);

                this.playerRigidBody.linearVelocity = newVelocityCal;
            }
        }
    }

    public void UpdatePlayerPosition(Vector3 position, Vector3 velocity, VelocityUpdate update = VelocityUpdate.None)
    {
        this.transform.position = position;

        if(this.playerRigidBody)
        {
            this.playerRigidBody.position = position;

            if(update != VelocityUpdate.None)
            {
                var velocityCopy = this.playerRigidBody.linearVelocity;

                Vector3 targetVelocity = new Vector3();

                if ((update & VelocityUpdate.Forward) == VelocityUpdate.Forward)
                    targetVelocity.x = velocity.x;
                else
                    targetVelocity.x = velocityCopy.x;

                if ((update & VelocityUpdate.Up) == VelocityUpdate.Up)
                    targetVelocity.y = velocity.y;
                else
                    targetVelocity.y = velocityCopy.y;

                if ((update & VelocityUpdate.Right) == VelocityUpdate.Right)
                    targetVelocity.z = velocity.z;
                else
                    targetVelocity.z = velocityCopy.z;

                this.playerRigidBody.linearVelocity = targetVelocity;
            }
        }
    }


    public void UpdateRotation(Vector3 rotation)
    {
        this.transform.Rotate(rotation);
    }

    public void SetCanMove(bool canMove)
    {
        this.canMove = canMove;

        if (this.playerRigidBody)
        {
            if (canMove)
                this.playerRigidBody.isKinematic = false;
            else
                this.playerRigidBody.isKinematic = true;
        }
    }

    public void SetForceSlided(float seconds)
    {
        this.forceSlideTime = seconds;
        this.forceSlideTimeCurrent = 0.0f;

        this.isForcedSliding = true;

        this.canMove = false;
    }


    public void SetPosition(Vector3 position)
    {
        transform.position = position;

        if (!this.playerRigidBody.isKinematic)
            this.playerRigidBody.position = position;
    }

    public void OnRotateCharacter(Vector3 addValue)
    {
        this.transform.Rotate(addValue);
    }

    public void OnMoveCharacter(PlayerMovement movingDir)
    {
        if (movingDir == PlayerMovement.None)
            return;

        VelocityUpdate velocityState = VelocityUpdate.ForwardRight;

        Vector3 currentPosition = transform.position;

        Vector3 nextPosition = currentPosition;

        float speedBoost = 0.0f;

        if ((movingDir & PlayerMovement.FastForward) == PlayerMovement.FastForward)
        {
            if(isConsumingStamina)
            {
                timeSinceWalking_Current = 0.0f;

                if (currentStamina > 0.0f)
                {
                    currentStamina -= (degenStaminaAmount * Time.deltaTime);

                    if (currentStamina < 0.0f)
                        currentStamina = 0.0f;

                    speedBoost = runningSpeed - movementSpeed;
                }
            }
            else
                speedBoost = runningSpeed - movementSpeed;
        }

        if ((movingDir & PlayerMovement.Forward) == PlayerMovement.Forward)
        {
            Vector3 fw = transform.forward;

            nextPosition.x += (fw.x * (movementSpeed + speedBoost));
            nextPosition.z += (fw.z * (movementSpeed + speedBoost));
        }

        if ((movingDir & PlayerMovement.Backward) == PlayerMovement.Backward)
        {
            

            Vector3 fw = transform.forward;

            nextPosition.x -= (fw.x * movementSpeed);
            nextPosition.z -= (fw.z * movementSpeed);
        }

        if ((movingDir & PlayerMovement.Right) == PlayerMovement.Right)
        {
            

            Vector3 r = transform.right;

            nextPosition.x += (r.x * movementSpeed);
            nextPosition.z += (r.z * movementSpeed);
        }

        if ((movingDir & PlayerMovement.Left) == PlayerMovement.Left)
        {
            

            Vector3 r = transform.right;

            nextPosition.x -= (r.x * movementSpeed);
            nextPosition.z -= (r.z * movementSpeed);
        }

        if ((movingDir & PlayerMovement.Up) == PlayerMovement.Up)
        {
            if (isGrounded && canJump)
            {
                Vector3 up = transform.up;

                nextPosition.y += (up.y * jumpHeight);

                velocityState |= VelocityUpdate.Up;
            }
        }

        if ((movingDir & PlayerMovement.Down) == PlayerMovement.Down)
        {
            if (!isGrounded)
            {
                Vector3 up = transform.up;

                nextPosition.y -= (up.y * jumpHeight);

                velocityState |= VelocityUpdate.Up;
            }
        }

        this.UpdatePlayerPosition(currentPosition, (nextPosition - currentPosition), velocityState);
    }

    public void OnClientMove(Player.PlayerMovement movement)
    {
        if (!canMove)
            return;

        if (isMovementHooked)
            OnMoveCharacter(onMovePlayerHook(this, movement));
        else
            OnMoveCharacter(movement);
    }

    public void OnClientRotate(Vector3 addValue)
    {
        if (!canMove)
            return;

        if(canRotate)
            OnRotateCharacter(addValue);
    }

    public void TryInteract(Vector3 lookPos, Vector3 lookDirection)
    {
#if DEBUG
        Debug.DrawRay(lookPos, lookDirection * interactDistance, Color.red, 10.0f);
#endif
        if(Physics.Raycast(lookPos, lookDirection, out RaycastHit hit, interactDistance, 1 << Item.GetInteractableLayer()))
        {
            var interactable = hit.transform.GetComponent<Interactable>();

            if(interactable)
            {
                interactable.Interact(this);
            }
        }
    }

    public void TryUseActiveItem()
    {
        if (playerInventory.HasActiveItem())
        {
            var item = playerInventory.GetActiveItem();

            item.UseItem();
        }
    }

    public void ActivateSlot(int Index)
    {
        playerInventory.ToggleItemOnIndex(Index);
    }

    protected bool CanPerformDrag()
    {
        return !isForcedSliding;
    }


    private void Initialize()
    {
        IntializePlayer();
    }

    protected virtual void IntializePlayer()
    {

    }

    public GameObject GetPlayerHead()
    {
        return head;
    }


    public bool IsMovementHooked()
    {
        return isMovementHooked;
    }

    public bool IsGrounded()
    {
        return isGrounded;
    }

    public float GetStamina()
    {
        return currentStamina;
    }

    public float GetMaxStamina()
    {
        return maxStamina;
    }

    public int GetHealth()
    {
        return currentHealth;
    }

    public int GetMaxHealth()
    {
        return maxHealth;
    }

    public GameObject GetInteractionHolder()
    {
        return leftHand;
    }

    public Inventory GetInventory()
    {
        return playerInventory;
    }

    public void SetStamina(float stamina)
    {
        currentStamina = stamina;
    }

    void SavePlayerData()
    {
        saveData.isAlive = isAlive;
        saveData.position = transform.position;
        saveData.stamina = currentStamina;
        saveData.rotation = transform.rotation;
        saveData.velocity = playerRigidBody.linearVelocity;

        saveData.sceneID = SceneManager.GetActiveScene().buildIndex;
    }
}
