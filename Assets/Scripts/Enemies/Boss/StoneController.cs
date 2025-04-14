using UnityEngine;

public class StoneController : MonoBehaviour
{
    [SerializeField]
    private GameObject hitParticle = null;

    private Rigidbody selfRigid = null;

    private Material selfMaterial = null;

    [SerializeField]
    private int Damage = 10;

    [SerializeField]
    private float DespawnTime = 10.0f;

    private float DespawnTimeCurrent = 0.0f;

    private bool isActive = false;
    private bool intialized = false;

    void Start()
    {
        Intialize();
    }

    private void Intialize()
    {
        if (intialized)
            return;

        intialized = true;

        selfMaterial = GetComponent<MeshRenderer>().material;
        selfRigid = GetComponent<Rigidbody>();
    }


    private void SpawnParticles()
    {
        var instatiated = Instantiate(hitParticle, transform.position, Quaternion.Euler(Vector3.zero));

        instatiated.GetComponent<BaseGunParticles>().StartParticle(Vector3.up, selfMaterial, 3.0f);
    }

    private void FinishHit()
    {
        SpawnParticles();

        Destroy(gameObject);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!isActive)
            return;


        if (1 << other.gameObject.layer == EnemyBase.GetPlayerLayerMask())
        {
            other.gameObject.GetComponent<Player>().DamagePlayer((int)((float)Damage * (selfRigid.linearVelocity.magnitude / 100)));

            FinishHit();
        }
    }

    private void Update()
    {
        if (DespawnTimeCurrent >= DespawnTime)
        {
            FinishHit();
        }
        else
            DespawnTimeCurrent += Time.deltaTime;
    }

    public void Activate()
    {
        Intialize();

        transform.parent = null;

        selfRigid.isKinematic = false;
        isActive = true;
    }
}
