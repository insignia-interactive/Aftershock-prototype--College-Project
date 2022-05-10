using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class AudioManager : MonoBehaviour
{
    private PlayerController _playerController;
    
    [SerializeField] private AudioSource footstepSource;

    [SerializeField] private AudioClip[] footsteps;
    [SerializeField] private AudioClip[] slide;
    
    private void Start()
    {
        _playerController = GetComponent<PlayerController>();
    }

    public void PlayFootstep()
    {
        if(_playerController.isGrounded || _playerController.isWallRunning) footstepSource.PlayOneShot(footsteps[Random.Range(0, footsteps.Length - 1)]);
    }

    public void PlaySlide()
    {
        if(_playerController.isGrounded || _playerController.isWallRunning) footstepSource.PlayOneShot(slide[Random.Range(0, slide.Length - 1)]);
    }
}