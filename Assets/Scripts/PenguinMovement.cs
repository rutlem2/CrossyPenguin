using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class PenguinMovement : MonoBehaviour
{
    const int noIdleAnimation = -1;
    const int NumParticles = 50;
    
    Rigidbody rigidPenguin;
    private int spawnPoint;
    
    [SerializeField] GameObject PenguinParticles;
    [SerializeField] Transform currentTarget;
    private Transform nextTarget;
    private NavMeshAgent navMeshAgent;
    private Animator animator;
    private MovementRing movementRing;

    private bool userCanClick;

    public enum CurrentPenguinAction
    {
        MovingInactively,
        Moving,
        Stopped,
        Hopping
    }

    private void Awake() 
    {
        rigidPenguin = GetComponent<Rigidbody>();
        navMeshAgent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        movementRing = GetComponentInChildren<MovementRing>();

        if (!currentTarget)
            currentTarget = gameObject.transform;
        else
            navMeshAgent.destination = currentTarget.position;
    }

    private void Start()
    {
        userCanClick = false;
    }

    public bool PenguinIsClickable()
    {
        return userCanClick;
    }

    public void SetPenguinUnclickable()
    {
        userCanClick = false;
    }

    public void ResumePathIfStopped()
    {
        if (navMeshAgent.isStopped)
        {
            animator.SetBool("isIdling", false);
            animator.SetInteger("idleAnimID", noIdleAnimation);
            navMeshAgent.isStopped = false;
        }
    }

    public int GetSpawnPoint()
    {
        return spawnPoint;
    }

    public void SetSpawnPoint(int sp)
    {
        spawnPoint = sp;
    }

    public void SetCurrentTarget(Transform newTarget)
    {
        if (!newTarget)
            throw new System.ArgumentNullException($"Invalid (or null) newTarget detected for {navMeshAgent}");
        currentTarget = newTarget;
        navMeshAgent.destination = newTarget.position;
    }

    public void SetNextTarget(Transform nextWaypoint)
    {
        nextTarget = nextWaypoint;
    }

    private IEnumerator OnCollisionEnter(Collision other) 
    {
        if (currentTarget.name == other.gameObject.name && currentTarget.tag == "Spawn")
        {
            PenguinSpawner.DespawnPenguin(gameObject);
            GameManager.score += 1;
            GameManager.Instance.UpdateScoreboardWithSound();
        }
        else if (currentTarget.name == other.gameObject.name) //waiting to cross
        {
            QueuePenguin();
            movementRing.ColorizeRingForState(ColoredMovementState.StoppedRed);

            if (other.gameObject.name == "stopClose")
                yield return StartCoroutine(RotateTowardStreet(new Vector3(0f, 180f, 0f), .8f));
            else
                yield return StartCoroutine(RotateTowardStreet(Vector3.zero, .8f));
            animator.SetBool("isIdling", true);

            while (navMeshAgent && navMeshAgent.isStopped)
            {
                yield return new WaitForSeconds(3.5f);
                animator.SetInteger("idleAnimID", Random.Range(0,2));
            }
        }
        else if (other.gameObject.tag == "Enemy")
        {
            GameManager.Instance.DecrementScore();
            Destroy(navMeshAgent); //removing navMeshAgent permits rigidbody physics
            PlayRemovePenguinFX();
            movementRing.ColorizeRingForState(ColoredMovementState.Disabled);

            yield return new WaitForSeconds(1.0f);
            PenguinSpawner.DespawnPenguin(gameObject);
        }
        else if (navMeshAgent && navMeshAgent.isOnOffMeshLink) //must check navMeshAgent exists in case penguin falls on hopZone on death
        {
            userCanClick = false;
        }
    }

    private void QueuePenguin()
    {
        navMeshAgent.isStopped = true;
        SetCurrentTarget(nextTarget);
        SetNextTarget(null);
        SetWalkingSpeed();
        userCanClick = true;
    }

    private void SetWalkingSpeed()
    {
        const float walkingSpeed = 2.5f;
        navMeshAgent.speed = walkingSpeed;
    }

    IEnumerator RotateTowardStreet(Vector3 dir, float duration)
    {
        Quaternion startRot = transform.rotation;
        Quaternion toRot = Quaternion.Euler(dir);
        float elapsedTime = 0.0f;
        while (elapsedTime < 1.0f)
        {
            transform.rotation = Quaternion.Slerp(startRot, toRot, elapsedTime);
            elapsedTime += Time.deltaTime / duration;
            yield return null;
        }
    }

    void PlayRemovePenguinFX()
    {
        rigidPenguin.constraints = RigidbodyConstraints.None;
        rigidPenguin.AddExplosionForce(250.0f, transform.position, 2.0f);
        AudioManager.Instance.Play(AudioManager.CarHit);

        GameObject deathParticles = Instantiate(PenguinParticles, transform.position, Quaternion.identity);
        deathParticles.GetComponent<ParticleSystem>().Emit(NumParticles);
    }
}
