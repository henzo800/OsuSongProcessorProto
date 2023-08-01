using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using OsuSongProcessorNS;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    public AudioClip currentSong;
    AudioSource audioSource;
    public GameObject BeatPrefab;
    public Canvas UICanvas;
    float BeatHitPoint;
    public float Velocity = 10;
    public float progress = 0;
    
    void Awake() {
        instance = this;
        audioSource = GetComponent<AudioSource>();
        BeatHitPoint = Camera.main.ScreenToWorldPoint(new Vector3((Screen.width/4),Screen.height/2,0)).x;
    }
    // Start is called before the first frame update
    void Start()
    {
        OsuSongProcessor osuSongProcessor = new OsuSongProcessor();
        string osuFilePath = Application.persistentDataPath + "/Songs/" + "2001413 Little V - Idol/Little V - Idol (skolodojko) [Kantan].osu";
        string songFilePath = Application.persistentDataPath + "/Songs/" + "2001413 Little V - Idol/audio.ogg";
        Song current = osuSongProcessor.ExtractSong(osuFilePath, songFilePath);
        currentSong = current.Music;
        audioSource.clip = currentSong;
        audioSource.Play();
        foreach(Beat b in current.beats){
            Vector3 targetPos = new Vector3(BeatHitPoint + b.time/100,0,10);
            GameObject beat = Instantiate(BeatPrefab, targetPos, Quaternion.identity, UICanvas.transform);
            beat.GetComponent<BeatController>().hitTime = b.time;
        }
        
    }

    // Update is called once per frame
    void Update()
    {
        progress = (audioSource.time * 1000);
    }

    int GetTime() {
        return (int)(audioSource.time * 1000);
    }
}
