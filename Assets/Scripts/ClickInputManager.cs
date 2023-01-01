using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;

public class ClickInputManager : MonoBehaviour
{
    const string slowVehicle = "slow";
    const string normalVehicle = "normal";
    const string fastVehicle = "fast";
    const int noIdleAnimation = -1;

    private PlayerInputActions playerInputActions;
    private NavMeshAgent navMeshAgent;
    private Animator animator;
    private GameObject rayTarget;
    
    void Awake()
    {
        playerInputActions = new PlayerInputActions();
    }

    void OnEnable()
    {
        playerInputActions.Player.ClickPenguin.Enable();
        playerInputActions.Player.ClickPenguin.performed += OnClickInput;
    }

    void OnDisable()
    {
        playerInputActions.Player.ClickPenguin.performed -= OnClickInput;
    }

    private void OnClickInput(InputAction.CallbackContext context)
    {
        RaycastHit raycastHit;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        bool raycastDidHitGO = Physics.Raycast(ray, out raycastHit);

        if (!raycastDidHitGO)
            return;

        if (raycastHit.transform.tag.Equals("pengAI"))
            HandleRayHitPenguin(raycastHit);
        else
            PlayCarFX(raycastHit);
    }

    private void HandleRayHitPenguin(RaycastHit raycastHit)
    {
        rayTarget = raycastHit.transform.gameObject;
        if (!rayTarget.GetComponent<PenguinMovement>().PenguinIsClickable())
            return;
        
        navMeshAgent = rayTarget.GetComponent<NavMeshAgent>();
        animator = rayTarget.GetComponent<Animator>();

        if (navMeshAgent && GameManager.Instance.GetState() != GameState.Paused)
            TogglePenguinMovementAndBehavior();
    }

    private void PlayCarFX(RaycastHit raycastHit)
    {
        if (raycastHit.transform.name.Contains(slowVehicle))
            AudioManager.Instance.Play(AudioManager.TruckHorn);
        else if (raycastHit.transform.name.Contains(normalVehicle))
            AudioManager.Instance.Play(AudioManager.CarHorn);
        else if (raycastHit.transform.name.Contains(fastVehicle))
            AudioManager.Instance.Play(AudioManager.CarHornHighPitch);
    }

    private void TogglePenguinMovementAndBehavior()
    {
        navMeshAgent.isStopped = !navMeshAgent.isStopped;

        if (navMeshAgent.isStopped)
            ShowStoppedPenguin();
        else
            ShowMovingPenguin();
    }

    private void ShowStoppedPenguin()
    {
        rayTarget.GetComponentInChildren<MovementRing>().ColorizeRingForState(ColoredMovementState.StoppedRed);
        AudioManager.Instance.Play(AudioManager.PenguinStopClick);
        animator.SetBool("isIdling", true);
        animator.SetInteger("idleAnimID", Random.Range(0,2));
    }

    private void ShowMovingPenguin()
    {
        rayTarget.GetComponentInChildren<MovementRing>().ColorizeRingForState(ColoredMovementState.MovingGreen);
        AudioManager.Instance.Play(AudioManager.PenguinStartClick);
        animator.SetBool("isIdling", false);
        animator.SetInteger("idleAnimID", noIdleAnimation);
    }
}
