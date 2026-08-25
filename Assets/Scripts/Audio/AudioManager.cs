using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("Music")]
    public AudioSource musicSource;

    [Header("SFX")]
    public AudioSource sfxSource;

    [Header("UI")]
    public AudioClip buttonClick;

    [Header("Roll")]
    public AudioClip spin;

    [Header("Unlock")]
    public AudioClip unlock;

    [Header("Combat")]
    public AudioClip attack;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void PlayButton()
    {
        Play(buttonClick);
    }

    public void PlayRollStart()
    {
        Play(spin);
    }

    public void PlayUnlock()
    {
        Play(unlock);
    }

    public void PlayAttack()
    {
        Play(attack);
    }

    private void Play(AudioClip clip)
    {
        if (clip == null)
            return;

        sfxSource.PlayOneShot(clip);
    }
}