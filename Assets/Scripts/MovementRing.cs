using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ColoredMovementState
{
    MovingGreen,
    StoppedRed,
    BlueForInactive,
    Disabled
}

public class MovementRing : MonoBehaviour
{
    private ParticleSystem thisRing;
    private Color movementGreenStart;
    private Color movementGreenEnd;
    private Color stoppedRed;
    private Color stoppedRedStart;
    private Color inactiveBlueStart;
    private Color whiteEnd;

    void Start()
    {
        thisRing = GetComponent<ParticleSystem>();
        movementGreenStart = new Color(23f/255f, 253f/255f, 0, 255f);
        movementGreenEnd = new Color(129f/255f, 255f/255f, 130f/255f, 18f);
        stoppedRedStart = new Color(255f/255f, 30f/255f, 18f/255f, 255f);
        inactiveBlueStart = new Color(18f/255f, 255f/255f, 193f/255f, 255f);
        whiteEnd = new Color(255f/255f, 255f/255f, 255f/255f, 18f);
    }

    public void ColorizeRingForState(ColoredMovementState state)
    {
        Gradient grad = new Gradient();
        
        switch (state)
        {
            case ColoredMovementState.MovingGreen:
                grad.SetKeys( new GradientColorKey[] { new GradientColorKey(movementGreenStart, 0f), new GradientColorKey(movementGreenEnd, 1.0f) },
                new GradientAlphaKey[] { new GradientAlphaKey(1.0f, 0.0f), new GradientAlphaKey(0.0f, 1.0f) });
            break;

            case ColoredMovementState.StoppedRed:
                grad.SetKeys( new GradientColorKey[] { new GradientColorKey(stoppedRedStart, 0f), new GradientColorKey(whiteEnd, 1.0f) }, 
                new GradientAlphaKey[] { new GradientAlphaKey(1.0f, 0.0f), new GradientAlphaKey(0.0f, 1.0f) });
            break;

            case ColoredMovementState.BlueForInactive:
                grad.SetKeys( new GradientColorKey[] { new GradientColorKey(inactiveBlueStart, 0f), new GradientColorKey(whiteEnd, 1.0f) }, 
                new GradientAlphaKey[] { new GradientAlphaKey(1.0f, 0.0f), new GradientAlphaKey(0.0f, 1.0f) });
            break;

            case ColoredMovementState.Disabled:
                thisRing.gameObject.SetActive(false);
            break;
            
            default:
                throw new ArgumentOutOfRangeException(nameof(state), state, null);
        }

        var col = thisRing.colorOverLifetime;
        col.color = grad;
    }
}
