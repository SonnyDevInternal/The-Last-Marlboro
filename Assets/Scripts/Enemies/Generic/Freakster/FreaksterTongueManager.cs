using System.Collections.Generic;
using UnityEngine;

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

        this.body.transform.position = this.position;
        this.body.transform.rotation = this.rotation;
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

        this.position = joint.transform.position;
        this.rotation = joint.transform.rotation;
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
}

public class FreaksterTongueManager : MonoBehaviour
{
    [SerializeField]
    private Animator tongueAnimator = null;

    [SerializeField]
    private TongueData tongueData = new TongueData();

    private List<TonguePiece> tonguePieces = new List<TonguePiece>();

    private bool isTongueToggled = false;
    private bool isInitialized = false;

    private bool skipNextColliderEvent = false;

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
        /*
        for (int i = 0; i < tonguePieces.Count; i++)
        {
            tonguePieces[i].UnbindHandler(OnTonguePiece_ColliderEvent);
        }*/
    }

    private void LateUpdate()
    {
#if DEBUG
        if (DEBUG_ToggleTongue)
        {
            DEBUG_ToggleTongue = false;

            ActivateTongueRagdoll(!isTongueToggled);
        }
#endif

        skipNextColliderEvent = false;
    }

    private void Intialize()
    {
        if (isInitialized)
            return;

        isInitialized = true;

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
}
