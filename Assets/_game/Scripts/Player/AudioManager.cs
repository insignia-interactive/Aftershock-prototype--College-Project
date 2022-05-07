using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class AudioManager : MonoBehaviour
{
    [SerializeField] private AudioSource footstepSource;

    [SerializeField] private AudioClip[] footsteps;
    [SerializeField] private AudioClip[] slide;

    public void PlayFootstep()
    {
        footstepSource.PlayOneShot(footsteps[Random.Range(0, footsteps.Length - 1)]);
    }

    public void PlaySlide()
    {
        footstepSource.PlayOneShot(slide[Random.Range(0, slide.Length - 1)]);
    }
}