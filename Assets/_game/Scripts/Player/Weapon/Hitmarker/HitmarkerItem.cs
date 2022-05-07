using UnityEngine;
using Photon.Realtime;
using UnityEngine.UI;

public class HitmarkerItem : MonoBehaviour
{
    public float destroyTime = 0.2f;
    
    public AudioSource _audioSource;
    public Image _image;

    public Color headshotColor = Color.red;
    public float pitch = 1f;
    
    public void Initialize(bool isHeadshot)
    {
        if (isHeadshot)
        {
            _image.color = headshotColor;
            _audioSource.pitch = pitch;
        }
        
        _audioSource.Play();
        
        Destroy(gameObject, destroyTime);
    }
}
