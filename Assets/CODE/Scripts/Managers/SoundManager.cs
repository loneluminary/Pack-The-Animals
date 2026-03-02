using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class SoundManager : Singleton<SoundManager>
{
    public AudioClip InCorrectSound;
    public AudioClip CorrectSound;
    public AudioClip PopSound;

    private AudioSource _source;
    
    private void Awake()
    {
        _source = GetComponent<AudioSource>();
    }
    
    public void PlaySound(AudioClip clip) => _source?.PlayOneShot(clip);
}