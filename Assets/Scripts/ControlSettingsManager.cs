using UnityEngine;
using UnityEngine.InputSystem;

public class ControlSettingsManager : MonoBehaviour
{
    public static ControlSettingsManager Instance { get; private set; }

    public Key moveUp = Key.W;
    public Key moveDown = Key.S;
    public Key moveLeft = Key.A;
    public Key moveRight = Key.D;
    public Key rotateLeft = Key.Q;
    public Key rotateRight = Key.E;
    public Key openVojnici = Key.V;
    public Key openToranj = Key.C;

    const string Prefix = "opt_key_";

    void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        Load();
    }

    void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    void Load()
    {
        moveUp = LoadKey("moveUp", moveUp);
        moveDown = LoadKey("moveDown", moveDown);
        moveLeft = LoadKey("moveLeft", moveLeft);
        moveRight = LoadKey("moveRight", moveRight);
        rotateLeft = LoadKey("rotateLeft", rotateLeft);
        rotateRight = LoadKey("rotateRight", rotateRight);
        openVojnici = LoadKey("openVojnici", openVojnici);
        openToranj = LoadKey("openToranj", openToranj);
    }

    static Key LoadKey(string id, Key fallback)
    {
        string saved = PlayerPrefs.GetString(Prefix + id, fallback.ToString());
        if (System.Enum.TryParse(saved, out Key key))
            return key;
        return fallback;
    }

    public void SetKey(string id, Key key)
    {
        switch (id)
        {
            case "moveUp": moveUp = key; break;
            case "moveDown": moveDown = key; break;
            case "moveLeft": moveLeft = key; break;
            case "moveRight": moveRight = key; break;
            case "rotateLeft": rotateLeft = key; break;
            case "rotateRight": rotateRight = key; break;
            case "openVojnici": openVojnici = key; break;
            case "openToranj": openToranj = key; break;
            default: return;
        }
        PlayerPrefs.SetString(Prefix + id, key.ToString());
    }

    public string GetKeyLabel(string id)
    {
        return KeyDisplayName(GetKey(id));
    }

    public Key GetKey(string id)
    {
        switch (id)
        {
            case "moveUp": return moveUp;
            case "moveDown": return moveDown;
            case "moveLeft": return moveLeft;
            case "moveRight": return moveRight;
            case "rotateLeft": return rotateLeft;
            case "rotateRight": return rotateRight;
            case "openVojnici": return openVojnici;
            case "openToranj": return openToranj;
            default: return Key.None;
        }
    }

    public static string KeyDisplayName(Key key)
    {
        if (key == Key.None)
            return "—";
        string name = key.ToString();
        if (name.Length == 1)
            return name.ToUpper();
        return name;
    }

    public bool IsDown(Key key)
    {
        return Keyboard.current != null && Keyboard.current[key].isPressed;
    }

    public bool WasPressed(Key key)
    {
        return Keyboard.current != null && Keyboard.current[key].wasPressedThisFrame;
    }
}
