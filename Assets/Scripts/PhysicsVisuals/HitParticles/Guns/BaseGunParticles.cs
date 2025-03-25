using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct BaseGunParticlesSettings
{
    public float maxCubeLifeTime;
}

public class BaseGunParticles : MonoBehaviour
{
    internal struct Particle
    {
        internal Rigidbody body;
        internal MeshRenderer renderer;

        internal Particle(Rigidbody body)
        {
            this.body = body;
            this.renderer = body.GetComponent<MeshRenderer>();
        }
    }

    private List<Particle> particles = new List<Particle>();

    [SerializeField]
    private BaseGunParticlesSettings settings = new BaseGunParticlesSettings();

    private float currentCubeLifeTime = 0.0f;

    private bool hasStarted = false;

    private void Update()
    {
        if(hasStarted)
        {
            if(currentCubeLifeTime >= settings.maxCubeLifeTime)
            {
                hasStarted = false;

                Destroy(gameObject);
            }
            else
                currentCubeLifeTime += Time.deltaTime;
        }
    }

    private void FlingInDirection(Vector3 direction, Material particleMat)
    {
        for (int i = 0; i < particles.Count; i++) 
        {
            particles[i].body.linearVelocity = direction;
            particles[i].renderer.material = particleMat;
        }
    }

    public void StartParticle(Vector3 direction, Material particleMat, float force)
    {
        var rigids = GetComponentsInChildren<Rigidbody>();

        for (int i = 0; i < rigids.Length; i++)
        {
            particles.Add(new Particle(rigids[i]));
        }

        FlingInDirection(direction * force, particleMat);

        hasStarted = true;
    }
}
