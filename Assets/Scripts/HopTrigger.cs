using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class HopTrigger : MonoBehaviour
{
    Animator animator;
    NavMeshAgent agent;
    OffMeshLink link;
    MovementRing movementRing;

    private void Awake() 
    {
        movementRing = GetComponentInChildren<MovementRing>();
        animator = GetComponent<Animator>();
        agent = GetComponent<NavMeshAgent>();
        agent.autoTraverseOffMeshLink = false;
    }

    private IEnumerator OnCollisionEnter(Collision other) 
    {
        if (other.gameObject.tag != "HopZoneArea")
            yield break;
            
        while (true) 
        {
            if (agent && agent.isOnOffMeshLink)
            {
                SetPenguinFinishingPath();
                
                animator.SetBool("onPlatform", true);
                AudioManager.Instance.Play(AudioManager.PengJump);
                yield return StartCoroutine(Hop(agent, 3f, 0.7f));
                agent.CompleteOffMeshLink();
                animator.SetBool("onPlatform", false);

                yield break;
            }
            yield return null;
        }
    }

    private void SetPenguinFinishingPath()
    {
        PenguinMovement penguinMovement = gameObject.GetComponent<PenguinMovement>();

        movementRing.ColorizeRingForState(ColoredMovementState.BlueForInactive);
        penguinMovement.SetPenguinUnclickable();
        penguinMovement.ResumePathIfStopped();
    }

    IEnumerator Hop(NavMeshAgent agent, float height, float duration)
    {
        OffMeshLinkData data = agent.currentOffMeshLinkData;
        Vector3 startPos = agent.transform.position;
        Vector3 endPos = data.endPos;
        float elapsedTime = 0.0f;
        while (elapsedTime < 1.0f)
        {
            float yOffset = height * (elapsedTime - Mathf.Pow(elapsedTime, 2.0f));
            agent.transform.position = Vector3.Lerp(startPos, endPos, elapsedTime) + (yOffset * Vector3.up);
            elapsedTime += Time.deltaTime / duration;
            yield return null;
        }
    }
}
