using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class Menu : MonoBehaviour
{
    [Header("UI / Menu")]
    public GameObject menuPanel;            // panel that contains the menu; shown on start
    public Button nextButton;
    public Button backButton;
    public Button startButton;
    public Text characterNameText;         // optional text showing selected character name

    [Header("Character selection")]
    public GameObject[] characterPrefabs;  // set of character prefabs to cycle through
    public Transform previewParent;        // empty GameObject used as parent for the preview instance
    public Transform playerSpawnPoint;     // where the real player will be instantiated when game starts (optional)

    int currentIndex = 0;
    GameObject currentPreview;

    void Start()
    {
        // Ensure menu is visible at start
        if (menuPanel != null) menuPanel.SetActive(true);

        // Hook up UI callbacks if buttons assigned
        if (nextButton != null) nextButton.onClick.AddListener(NextCharacter);
        if (backButton != null) backButton.onClick.AddListener(PrevCharacter);
        if (startButton != null) startButton.onClick.AddListener(StartGame);

        UpdatePreview();
    }

    public void NextCharacter()
    {
        if (characterPrefabs == null || characterPrefabs.Length == 0) return;
        currentIndex = (currentIndex + 1) % characterPrefabs.Length;
        UpdatePreview();
    }

    public void PrevCharacter()
    {
        if (characterPrefabs == null || characterPrefabs.Length == 0) return;
        currentIndex = (currentIndex - 1 + characterPrefabs.Length) % characterPrefabs.Length;
        UpdatePreview();
    }

    void UpdatePreview()
    {
        // remove old preview
        if (currentPreview != null)
            Destroy(currentPreview);

        if (characterPrefabs == null || characterPrefabs.Length == 0 || previewParent == null)
        {
            if (characterNameText != null) characterNameText.text = "";
            return;
        }

        var prefab = characterPrefabs[currentIndex];
        // instantiate preview as child of previewParent
        currentPreview = Instantiate(prefab, previewParent);
        currentPreview.transform.localPosition = Vector3.zero;
        currentPreview.transform.localRotation = Quaternion.identity;
        currentPreview.transform.localScale = Vector3.one;

        // Disable gameplay-affecting components for the preview but keep Animator so preview animates
        DisableRuntimeComponents(currentPreview);

        if (characterNameText != null)
            characterNameText.text = prefab.name;
    }

    // Called by Start button: hides menu and spawns the selected character into the scene
    public void StartGame()
    {
        if (menuPanel != null) menuPanel.SetActive(false);

        if (characterPrefabs == null || characterPrefabs.Length == 0) return;

        var prefab = characterPrefabs[currentIndex];
        Transform spawn = playerSpawnPoint != null ? playerSpawnPoint : null;

        GameObject player = spawn != null
            ? Instantiate(prefab, spawn.position, spawn.rotation)
            : Instantiate(prefab);

        // Optionally tag the spawned player
        player.tag = "Player";
    }

    void DisableRuntimeComponents(GameObject root)
    {
        // Disable MonoBehaviours (scripts) so preview doesn't run gameplay logic
        var monos = root.GetComponentsInChildren<MonoBehaviour>(true);
        foreach (var mb in monos)
        {
            // keep disabled if it's an Editor-only script, safe to disable everything for preview
            mb.enabled = false;
        }

        // Keep animators enabled so the preview can play animations
        var animators = root.GetComponentsInChildren<Animator>(true);
        foreach (var a in animators) a.enabled = true;

        // Make rigidbodies kinematic and zero velocity so physics doesn't move the preview
        var rbs = root.GetComponentsInChildren<Rigidbody>(true);
        foreach (var rb in rbs)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.isKinematic = true;
        }
        var rbs2 = root.GetComponentsInChildren<Rigidbody2D>(true);
        foreach (var rb2 in rbs2)
        {
            rb2.linearVelocity = Vector2.zero;
            rb2.angularVelocity = 0f;
            rb2.bodyType = RigidbodyType2D.Kinematic;
        }

        // Disable colliders so preview won't interact with scene
        var cols = root.GetComponentsInChildren<Collider>(true);
        foreach (var c in cols) c.enabled = false;
        var cols2 = root.GetComponentsInChildren<Collider2D>(true);
        foreach (var c2 in cols2) c2.enabled = false;

        // Optional: set the preview layer so UI cameras / lighting can be configured separately
        // int previewLayer = LayerMask.NameToLayer("UI");
        // if (previewLayer >= 0) SetLayerRecursively(root.transform, previewLayer);
    }

    void SetLayerRecursively(Transform t, int layer)
    {
        t.gameObject.layer = layer;
        for (int i = 0; i < t.childCount; i++) SetLayerRecursively(t.GetChild(i), layer);
    }
}
