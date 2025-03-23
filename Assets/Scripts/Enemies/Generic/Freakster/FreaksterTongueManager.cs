using System.Collections.Generic;
using UnityEngine;

enum ETongueAttack
{
    Slash
}

internal struct TonguePiece
{
    public Rigidbody body;
    public ConfigurableJoint joint;
    //public ColliderHandler handler;
    public Vector3 position;
    public Quaternion rotation;
    public int index;

    public void SetXDriveData(TongueData.TongueDrive Drive)
    {
        if (joint)
            joint.xDrive = SetJointDrive(Drive.Spring, Drive.Damper);
    }

    public void SetYDriveData(TongueData.TongueDrive Drive)
    {
        if (joint)
            joint.yDrive = SetJointDrive(Drive.Spring, Drive.Damper);
    }

    public void SetZDriveData(TongueData.TongueDrive Drive)
    {
        if (joint)
            joint.zDrive = SetJointDrive(Drive.Spring, Drive.Damper);
    }

    private JointDrive SetJointDrive(float spring, float damping)
    {
        JointDrive drive = new JointDrive
        {
            positionSpring = spring,
            positionDamper = damping,
            maximumForce = Mathf.Infinity
        };
        return drive;
    }

    //Call when you turn off Piece
    public void ResetPiece()
    {
        this.body.isKinematic = true;

        this.body.transform.localPosition = this.position;
        this.body.transform.localRotation = this.rotation;
    }

    /*
    public void BindHandler(OnTriggerColliderEvent onTriggerColliderEvent)
    {
        if(this.handler)
            this.handler.BindOnTriggerColliderEvent(onTriggerColliderEvent);
    }

    public void UnbindHandler(OnTriggerColliderEvent onTriggerColliderEvent)
    {
        if (this.handler)
            this.handler.UnbindOnTriggerColliderEvent(onTriggerColliderEvent);
    }
    */

    internal TonguePiece(ConfigurableJoint joint, int index)
    {
        this.index = index;
        this.joint = joint;

        joint.xMotion = ConfigurableJointMotion.Limited;
        joint.yMotion = ConfigurableJointMotion.Limited;
        joint.zMotion = ConfigurableJointMotion.Limited;

        this.body = joint.GetComponent<Rigidbody>();
        //this.handler = joint.GetComponent<ColliderHandler>();

        this.position = joint.transform.localPosition;
        this.rotation = joint.transform.localRotation;
    }
}

[System.Serializable]
public struct TongueData
{
    [System.Serializable]
    public struct TongueDrive
    {
        public int Spring;
        public int Damper;
        public int MaximumForce;
    }

    public TongueDrive X_Drive;
    public TongueDrive Y_Drive;
    public TongueDrive Z_Drive;

    public int tongueDamage;

    //For Checking if hit Player
    public float raycastDistance;
}

public class FreaksterTongueManager : MonoBehaviour
{
    [SerializeField]
    private Animator tongueAnimator = null;

    private FreaksterTongueAnimationHandler tongueAnimHandler = null;

    [SerializeField]
    private TongueData tongueData = new TongueData();

    private List<TonguePiece> tonguePieces = new List<TonguePiece>();

    private Vector3 targetHitPosition = Vector3.zero;

    private bool isTongueToggled = false;
    private bool isInitialized = false;

    private bool isAttacking = false;

#if DEBUG
    [SerializeField]
    private bool DEBUG_ToggleTongue = false;
#endif

    void Start()
    {
        Intialize();
    }

    private void OnDestroy()
    {
        if(tongueAnimator)
            tongueAnimator.enabled = false;
        /*
        for (int i = 0; i < tonguePieces.Count; i++)
        {
            tonguePieces[i].UnbindHandler(OnTonguePiece_ColliderEvent);
        }*/

        tongueAnimHandler?.UnbindOnAnimationHandlerCalled(OnTongueAnimEvent);
    }

#if DEBUG
    private void LateUpdate()
    {

        if (DEBUG_ToggleTongue)
        {
            DEBUG_ToggleTongue = false;

            ActivateTongueRagdoll(!isTongueToggled);
        }
    }
#endif

#if DEBUG
    private void OnDrawGizmosSelected()
    {
        if(tongueAnimator)
        {
            var currentObj = gameObject;

            var pos = currentObj.transform.position + (Vector3.down * 0.4f);

            var pos1 = (currentObj.transform.forward * tongueData.raycastDistance);

            Debug.DrawRay(pos, pos1, Color.yellow);
        }
    }
#endif

    private void Intialize()
    {
        if (isInitialized)
            return;

        isInitialized = true;

        if(tongueAnimator)
        {
            tongueAnimHandler = tongueAnimator.GetComponent<FreaksterTongueAnimationHandler>();

            tongueAnimHandler.BindOnAnimationHandlerCalled(OnTongueAnimEvent);
        }

        var joints = GetComponentsInChildren<ConfigurableJoint>(true);

        //Ignore first bone since its root bone
        for (int i = 1; i < joints.Length; i++)
        {
            var piece = new TonguePiece(joints[i], i);

            piece.SetXDriveData(tongueData.X_Drive);
            piece.SetYDriveData(tongueData.Y_Drive);
            piece.SetZDriveData(tongueData.Z_Drive);

            //piece.BindHandler(OnTonguePiece_ColliderEvent);

            tonguePieces.Add(piece);
        }
    }

    /*
    private void OnTonguePiece_ColliderEvent(ColliderHandler handler, EColliderEvent Event)
    {
        switch (Event)
        {
            case EColliderEvent.OnColliderEnter:
                if(!isTongueToggled)
                {
                    if(!skipNextColliderEvent)
                    {
                        var colliding = handler.GetCollidingObject();

                        if (EnemyBase.GetPlayerLayerMask() == (1 << colliding.gameObject.layer))
                        {
                            skipNextColliderEvent = true;

                            var player = colliding.gameObject.GetComponent<Player>();

                            player.DamagePlayer(tongueData.tongueDamage);
                        }
                    }
                }
                break;

            default:
                break;
        }
    }
    */

    public void SlashAttack(Vector3 hitPosition)
    {
        this.targetHitPosition = hitPosition;

        this.ActivateTongueRagdoll(false);

        this.tongueAnimator.SetTrigger("SlashAttack");
    }

    private void OnTongueAnimEvent(AnimationHandler<EFreaksterAnimationEvent> _this, EFreaksterAnimationEvent Event)
    {
        isAttacking = false;

        switch (Event)
        {
            case EFreaksterAnimationEvent.SlashEnd:
                var playerMask = EnemyBase.GetPlayerLayerMask();

                var playerDir = (this.targetHitPosition - transform.position).normalized * tongueData.raycastDistance;

                Debug.DrawRay(transform.position, playerDir, Color.red, 2.0f);

                if (Physics.Raycast(transform.position, playerDir, out RaycastHit hitInfo, tongueData.raycastDistance, EnemyBase.GetPlayerLayerMask() | EnemyBase.GetObstacleLayerMask()))
                {
                    if ((1 << hitInfo.transform.gameObject.layer) == playerMask)
                        hitInfo.transform.GetComponent<Player>().DamagePlayer(this.tongueData.tongueDamage);
                }
                break;

            default:
                break;
        }

        this.ActivateTongueRagdoll(true);
    }

    private void SetRigidAndJointActive(bool active)
    {
        for (int i = 0; i < tonguePieces.Count; i++)
        {
            if(!tonguePieces[i].body)
                continue;

            if (active)
            {
                tonguePieces[i].SetXDriveData(tongueData.X_Drive);
                tonguePieces[i].SetYDriveData(tongueData.Y_Drive);
                tonguePieces[i].SetZDriveData(tongueData.Z_Drive);
            }
            else
                tonguePieces[i].ResetPiece();

            tonguePieces[i].body.isKinematic = !active;
        }
    }

    public void ActivateTongueRagdoll(bool ragdoll)
    {
        if (!tongueAnimator)
            return;

        if (!isInitialized)
            Intialize();

        isTongueToggled = ragdoll;

        if (ragdoll)
        {
            tongueAnimator.StopPlayback();
            tongueAnimator.enabled = false;
        }
        else
        {
            tongueAnimator.enabled = true;
        }

        SetRigidAndJointActive(ragdoll);
    }

    public void SetTongueDamage(int damage)
    {
        tongueData.tongueDamage = damage;
    }

    public bool IsAttacking()
    {
        return isAttacking;
    }
}
