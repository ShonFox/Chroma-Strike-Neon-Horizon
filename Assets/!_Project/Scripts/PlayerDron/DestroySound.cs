using UnityEngine;

public class DestroySound : MonoBehaviour
{
    [SerializeField] private AudioSource _audioSource;

    public void PlaySound()
    {
        _audioSource.Play();
    }
}
