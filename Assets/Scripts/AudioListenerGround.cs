using UnityEngine;

public class AudioListenerGround : MonoBehaviour
{
    public float groundHeight = 10f;

    static AudioListenerGround _instance;
    bool _ready;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Bootstrap()
    {
        if (_instance != null)
            return;

        GameObject go = new GameObject("GroundAudioListener");
        _instance = go.AddComponent<AudioListenerGround>();
    }

    void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        _instance = this;
    }

    void OnDestroy()
    {
        if (_instance == this)
            _instance = null;
    }

    void Start()
    {
        SetupListener();
    }

    void SetupListener()
    {
        Camera cam = Camera.main;
        if (cam == null)
        {
            Invoke(nameof(SetupListener), 0.15f);
            return;
        }

        AudioListener onCam = cam.GetComponent<AudioListener>();
        if (onCam != null)
            Destroy(onCam);

        if (GetComponent<AudioListener>() == null)
            gameObject.AddComponent<AudioListener>();

        _ready = true;
    }

    void LateUpdate()
    {
        if (!_ready)
        {
            SetupListener();
            return;
        }

        Camera cam = Camera.main;
        if (cam == null)
            return;

        Vector3 p = cam.transform.position;
        transform.position = new Vector3(p.x, groundHeight, p.z);
    }
}
