using System.Collections;
using UnityEngine;
using System.IO;
using UnityEngine.Video;

public class CutscenePlayer : MonoBehaviour
{
    public static string videoName = "";
    public static string nextSceneName = "";
    VideoPlayer videoPlayer;
    Coroutine LoadVideoCor;
    MeshRenderer meshRenderer;
    SpriteRenderer skipRenderer;
    bool skip = false;

    public static void LoadCutscene(string videoTitle, string nextScene)
    {
        videoName = videoTitle;
        nextSceneName = nextScene;
        DataShare.LoadSceneWithTransition("VideoPlayer");
    }
    IEnumerator ISkipCooldown()
    {
        skip = true;
        Color c = skipRenderer.color;
        c.a = 0;
        float progress = 0;
        while(progress<1)
        {
            progress += Time.deltaTime*5;
            yield return 0;
            c.a = Mathf.Lerp(0,1,progress);
            skipRenderer.color = c;
        }
        c.a = 1;
        skipRenderer.color = c;
        ///print("Skip?");
        yield return new WaitForSeconds(2f);
        ///print("Skip expired");
        progress = 0;
        while(progress<1)
        {
            progress += Time.deltaTime*5;
            yield return 0;
            c.a = Mathf.Lerp(1,0,progress);
            skipRenderer.color = c;
        }
        c.a = 0;
        skipRenderer.color = c;
        skip = false;
    }
    IEnumerator ILoadVideo(string path)
    {
        if(!File.Exists(path + videoName + ".mp4"))
        {
            LoadNextScene();
            yield break;
        }

        meshRenderer.enabled = false;

        // Load video
        videoPlayer.source = VideoSource.Url;
        videoPlayer.url = path + videoName + ".mp4";
        videoPlayer.isLooping = false;

        print("Video path:\n"+path+videoName);

        // Set audio source
        var audioSource = videoPlayer.GetComponent<AudioSource>();
        videoPlayer.audioOutputMode = VideoAudioOutputMode.AudioSource;
        videoPlayer.controlledAudioTrackCount = 1;
        videoPlayer.EnableAudioTrack(0, true);
        videoPlayer.SetTargetAudioSource(0, audioSource);

        videoPlayer.Prepare();

        while (!videoPlayer.isPrepared)
        {
            yield return 0;
        }
        meshRenderer.enabled = true;
        yield return new WaitForSeconds(0.1f);
        // Wait while video plays
        while (videoPlayer.isPlaying)
        {
            yield return 0;
        }
        LoadNextScene();
    }
    void LoadNextScene()
    {
        videoName = "";
        DataShare.LoadSceneWithTransition(nextSceneName);
        nextSceneName = "";
    }
    void FixedUpdate()
    {
        if(videoPlayer.isPlaying)
        {
            if(MGInput.GetButtonDown(MGInput.controls.Player.Jump))
            {
                if(skip)
                {
                    videoPlayer.Stop();
                    ///print("Skipped");
                }
                else StartCoroutine(ISkipCooldown());
            }
        }
    }
    public void LoadVideo(string VideoName,string nextSceneName)
    {
        VideoClip curVideo = videoPlayer.clip;
        // Stop Video if no string entered.
        if(VideoName == "")
        {
            if(curVideo!= null)
            {
                videoPlayer.Stop();
                StopCoroutine(LoadVideoCor);
            }
            print("Stopping video");
            return;
        }

        // Abort if currently playing video has same name.
        if(curVideo != null && curVideo.name == VideoName)
        {
            if(!videoPlayer.isPlaying)
            {
                videoPlayer.Play();
            }
            return;
        }

        string path = Application.streamingAssetsPath+"/Video/";
        if(LoadVideoCor != null) StopCoroutine(LoadVideoCor);
        LoadVideoCor = StartCoroutine(ILoadVideo(path));
    }
    // Start is called before the first frame update
    void Start()
    {
        Destroy(GameObject.Find("PauseMenu"));
        videoPlayer = GetComponent<VideoPlayer>();
        meshRenderer = GetComponent<MeshRenderer>();
        skipRenderer = transform.GetChild(0).GetComponent<SpriteRenderer>();
        Color c = skipRenderer.color;
        c.a = 0;
        skipRenderer.color = c;

        LoadVideo(videoName,nextSceneName);
    }
}
