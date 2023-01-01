using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TransitionHandler : MonoBehaviour
{
    [SerializeField] Animator crossfadeBlackView;
    [SerializeField] Animator crossfadeLoading;
    private const float transitionTimeSlide = 2.5f;
    private const float transitionTimeLoading = 0.8f;
    private const string fadeIn = "fadeIn";
    private const string fadeOut = "fadeOut";

    public void TransitionToGame()
    {
        StartCoroutine(PlaySlideTransition());
        StartCoroutine(PlayLoadingTransition());
    }

    private IEnumerator PlaySlideTransition()
    {
        crossfadeBlackView.SetBool(fadeIn, true);

        yield return new WaitForSeconds(transitionTimeSlide);

        crossfadeBlackView.SetBool(fadeIn, false);
        crossfadeBlackView.SetBool(fadeOut, true);

        yield return new WaitForSeconds(transitionTimeSlide);
        
        crossfadeBlackView.SetBool(fadeOut, false);
    }

    private IEnumerator PlayLoadingTransition()
    {
        yield return new WaitForSeconds(transitionTimeLoading);

        crossfadeLoading.SetBool(fadeIn, true);

        yield return new WaitForSeconds(1.5f);

        crossfadeLoading.SetBool(fadeIn, false);
        crossfadeLoading.SetBool(fadeOut, true);

        yield return new WaitForSeconds(transitionTimeLoading);

        crossfadeLoading.SetBool(fadeOut, false);
    }
}
