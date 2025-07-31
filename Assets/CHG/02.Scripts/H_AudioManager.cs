using UnityEngine;

public class H_AudioManager : MonoBehaviour
{
    public static H_AudioManager Instance;

    public AudioClip[] sfxClips;
    public float sfxVolume;
    public int channels;
    public enum Sfx { EnemyDead, ExpGet = 3}
    private AudioSource[] SfxPlayer;
    int channelIndex;
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else Destroy(gameObject);

        GameObject sfxObject = new GameObject("SfxPlayer");
        sfxObject.transform.parent = transform;
        SfxPlayer = new AudioSource[channels];

        for (int i = 0; i < SfxPlayer.Length; i++)
        {
            SfxPlayer[i] = sfxObject.AddComponent<AudioSource>();
            SfxPlayer[i].playOnAwake = false;
            SfxPlayer[i].bypassListenerEffects = true;
            SfxPlayer[i].volume = sfxVolume;
        }
    }

    public void SfxPlay(Sfx sfx)
    {
        for (int i = 0; i < SfxPlayer.Length; i++)
        {
            int loopIndex = (i + channelIndex) % SfxPlayer.Length;

            if (SfxPlayer[loopIndex].isPlaying)
                continue;

            int ran = 0;
            if (sfx == Sfx.EnemyDead)
                ran = Random.Range(0, 2);

            channelIndex = loopIndex;
            SfxPlayer[0].clip = sfxClips[(int)sfx + ran];
            SfxPlayer[0].Play();
            break;
        }
    }
}
