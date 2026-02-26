using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class CharacterManager : MonoBehaviour
{
    [Header("Spawn Point UI Frame")]
    public Image spawnPointFrameUI;

    [Header("Database and UI")]
    public CharacterDatabase characterDB;
    public Text nameText;
    public Text selectionStatusText;
    public Transform characterContainer;
    public GameObject framePrefab;

    [Header("Spawn Point Images")]
    public Image spawnPointPreviewUI;          
    public List<Sprite> spawnPointImages = new List<Sprite>();

    [Header("Spawn Point Image Size")]
    public Vector2 spawnPointImageSize = new Vector2(300f, 300f);

    [Header("Selection State")]
    private int firstSelectedIndex = -1;
    private int secondSelectedIndex = -1;
    private int selectedOption = 0;
    private bool selectingFirst = true;

    [Header("Visual Settings")]
    public Vector2 targetSize = new Vector2(1f, 1f);
    public bool preserveAspect = true;
    public List<float> perCharacterScale = new List<float>();
    [Range(0.1f, 2f)] public float selectedScaleMultiplier = 1.5f;
    [Range(0.1f, 1f)] public float unselectedScaleMultiplier = 0.6f;

    [Header("Frame Settings")]
    public float frameWorldSize = 1.2f;
    public float frameExpandSpeed = 5f;
    public float frameIdleScale = 0.5f;

    [Header("Button Animation Settings")]
    [Range(0.1f, 1f)] public float buttonClickScale = 0.8f;
    [Range(0.01f, 0.5f)] public float buttonAnimationDuration = 0.1f;

    [Header("Camera Transition")]
    public float cameraMoveSpeed = 3f;
    public float cameraZoom = 4f;
    public float sceneChangeDelay = 1f;

    [Header("Spawn Point Settings")]
    public List<Transform> spawnPoints = new List<Transform>();
    private int selectedSpawnPointIndex = 0;
    public static int chosenSpawnPointIndex = 0;

    [Header("Player Prefab")]
    public GameObject playerPrefab; // default player prefab, optional

    private List<SpriteRenderer> characterRenderers = new List<SpriteRenderer>();
    private List<GameObject> frames = new List<GameObject>();
    private List<Coroutine> frameCoroutines = new List<Coroutine>();
    private Camera mainCam;
    private Dictionary<GameObject, Coroutine> buttonAnimationCoroutines = new Dictionary<GameObject, Coroutine>();

    // Track spawned players
    private GameObject player1Instance;
    private GameObject player2Instance;

    void Start()
    {
        mainCam = Camera.main;
        EnsurePerCharacterScaleList();
        SpawnAllCharacters();
        UpdateCharacterSelection(true);

        // Load saved spawn point
        if (PlayerPrefs.HasKey("SpawnPointIndex"))
        {
            selectedSpawnPointIndex = PlayerPrefs.GetInt("SpawnPointIndex");
            chosenSpawnPointIndex = selectedSpawnPointIndex;
            Debug.Log("Loaded saved spawn point index: " + selectedSpawnPointIndex);
        }

        if (selectionStatusText != null)
            selectionStatusText.text = "Select Character 1";

        UpdateSpawnPointSelection();
    }

    // -------------------
    // SPAWN POINT CONTROLS
    // -------------------
    public void NextSpawnPoint()
    {
        if (spawnPoints.Count == 0) return;
        selectedSpawnPointIndex = (selectedSpawnPointIndex + 1) % spawnPoints.Count;
        UpdateSpawnPointSelection();
    }

    public void PreviousSpawnPoint()
    {
        if (spawnPoints.Count == 0) return;
        selectedSpawnPointIndex--;
        if (selectedSpawnPointIndex < 0) selectedSpawnPointIndex = spawnPoints.Count - 1;
        UpdateSpawnPointSelection();
    }

    private void UpdateSpawnPointSelection()
    {
        if (spawnPoints.Count == 0) return;

        chosenSpawnPointIndex = selectedSpawnPointIndex;
        PlayerPrefs.SetInt("SpawnPointIndex", chosenSpawnPointIndex);
        PlayerPrefs.Save();
        Debug.Log("Spawn point saved: " + chosenSpawnPointIndex);

        // Update UI
        if (spawnPointFrameUI != null && spawnPointPreviewUI != null)
        {
            RectTransform frameRT = spawnPointFrameUI.rectTransform;
            RectTransform imgRT = spawnPointPreviewUI.rectTransform;
            frameRT.sizeDelta = imgRT.sizeDelta + new Vector2(40f, 40f);
            frameRT.anchoredPosition = imgRT.anchoredPosition;
        }

        if (spawnPointPreviewUI != null &&
            spawnPointImages.Count > 0 &&
            selectedSpawnPointIndex < spawnPointImages.Count)
        {
            Sprite s = spawnPointImages[selectedSpawnPointIndex];
            if (s != null)
            {
                spawnPointPreviewUI.sprite = s;
                spawnPointPreviewUI.enabled = true;
                spawnPointPreviewUI.color = Color.white;
                spawnPointPreviewUI.type = Image.Type.Simple;
                spawnPointPreviewUI.preserveAspect = false;

                RectTransform rt = spawnPointPreviewUI.rectTransform;
                rt.sizeDelta = spawnPointImageSize;

                var arf = spawnPointPreviewUI.GetComponent<UnityEngine.UI.AspectRatioFitter>();
                if (arf != null)
                {
                    arf.aspectRatio = s.rect.width / s.rect.height;
                    arf.aspectMode = AspectRatioFitter.AspectMode.EnvelopeParent;
                }
                Debug.Log("Showing spawn image: " + s.name);
            }
        }

        // Move players if spawned
        Transform spawn = spawnPoints[selectedSpawnPointIndex];
        if (player1Instance != null)
            player1Instance.transform.position = spawn.position + Vector3.left * 1.5f;
        if (player2Instance != null)
            player2Instance.transform.position = spawn.position + Vector3.right * 1.5f;
    }

    // -------------------
    // CHARACTER NAVIGATION
    // -------------------
    public void NextOption()
    {
        if (characterDB == null || characterDB.CharacterCount == 0) return;
        selectedOption = (selectedOption + 1) % characterDB.CharacterCount;
        UpdateCharacterSelection(false);
    }

    public void BackOption()
    {
        if (characterDB == null || characterDB.CharacterCount == 0) return;
        selectedOption--;
        if (selectedOption < 0) selectedOption = characterDB.CharacterCount - 1;
        UpdateCharacterSelection(false);
    }

    // -------------------
    // CHARACTER SPAWNING
    // -------------------
    private void EnsurePerCharacterScaleList()
    {
        if (characterDB == null) return;
        int count = characterDB.CharacterCount;
        while (perCharacterScale.Count < count)
            perCharacterScale.Add(1f);
    }

    private void SpawnAllCharacters()
    {
        if (characterDB == null || characterContainer == null) return;

        foreach (Transform child in characterContainer)
            Destroy(child.gameObject);

        characterRenderers.Clear();
        frames.Clear();
        frameCoroutines.Clear();

        float spacing = targetSize.x * 1.6f;

        for (int i = 0; i < characterDB.CharacterCount; i++)
        {
            Character character = characterDB.GetCharacter(i);

            GameObject charObj = new GameObject(character.characterName);
            charObj.transform.SetParent(characterContainer, false);
            charObj.transform.localPosition = new Vector3(i * spacing, 0f, 0f);

            SpriteRenderer sr = charObj.AddComponent<SpriteRenderer>();
            sr.sprite = character.characterSprite;
            characterRenderers.Add(sr);

            GameObject frame = Instantiate(framePrefab, characterContainer);
            frame.transform.localPosition = charObj.transform.localPosition;
            SetFrameWorldSize(frame, false);
            frames.Add(frame);
            frameCoroutines.Add(null);
        }
    }

    private void UpdateCharacterSelection(bool instant = false)
    {
        Character selectedCharacter = characterDB.GetCharacter(selectedOption);
        if (nameText != null) nameText.text = selectedCharacter.characterName;

        for (int i = 0; i < characterRenderers.Count; i++)
        {
            SpriteRenderer sr = characterRenderers[i];
            if (sr == null || sr.sprite == null) continue;

            float scale = Mathf.Min(targetSize.x / sr.sprite.bounds.size.x, targetSize.y / sr.sprite.bounds.size.y) * perCharacterScale[i];
            bool isSelected = (i == selectedOption);
            float finalScale = scale * (isSelected ? selectedScaleMultiplier : unselectedScaleMultiplier);
            sr.transform.localScale = Vector3.one * finalScale;

            if (frames[i] != null)
            {
                frames[i].transform.localPosition = sr.transform.localPosition;

                if (frameCoroutines[i] != null)
                {
                    StopCoroutine(frameCoroutines[i]);
                    frameCoroutines[i] = null;
                }

                if (instant) SetFrameWorldSize(frames[i], isSelected);
                else frameCoroutines[i] = StartCoroutine(AnimateFrameExpand(frames[i], isSelected));
            }
        }
    }

    // -------------------
    // CHARACTER SELECTION
    // -------------------
    public void SelectCharacter(int index)
    {
        selectedOption = index;
        UpdateCharacterSelection(true);
        SelectButtonPressed();
    }

    public void SelectButtonPressed()
    {
        if (selectingFirst)
        {
            firstSelectedIndex = selectedOption;
            selectingFirst = false;
            if (selectionStatusText != null)
                selectionStatusText.text = "Character 1 selected! Now select Character 2.";
        }
        else
        {
            if (selectedOption == firstSelectedIndex)
            {
                if (selectionStatusText != null)
                    selectionStatusText.text = "You already picked that character!";
                return;
            }

            secondSelectedIndex = selectedOption;
            SelectedCharacterHolder.firstSelectedPrefab = characterDB.GetPrefab(firstSelectedIndex);
            SelectedCharacterHolder.secondSelectedPrefab = characterDB.GetPrefab(secondSelectedIndex);

            if (selectionStatusText != null)
                selectionStatusText.text = "Character 2 selected! You can now press Play.";

            SpawnPlayers();
        }
    }

    private void SpawnPlayers()
    {
        if (player1Instance != null) Destroy(player1Instance);
        if (player2Instance != null) Destroy(player2Instance);

        Transform spawn = spawnPoints[selectedSpawnPointIndex];

        // Player 1
        if (SelectedCharacterHolder.firstSelectedPrefab != null)
        {
            player1Instance = Instantiate(SelectedCharacterHolder.firstSelectedPrefab,
                spawn.position + Vector3.left * 1.5f, Quaternion.identity);
            player1Instance.name = "Player_1";
        }

        // Player 2
        if (SelectedCharacterHolder.secondSelectedPrefab != null)
        {
            player2Instance = Instantiate(SelectedCharacterHolder.secondSelectedPrefab,
                spawn.position + Vector3.right * 1.5f, Quaternion.identity);
            player2Instance.name = "Player_2";
        }
    }

    // -------------------
    // PLAY BUTTON
    // -------------------
    public void PlayButtonPressed(int sceneID)
    {
        if (firstSelectedIndex == -1 || secondSelectedIndex == -1)
        {
            if (selectionStatusText != null)
                selectionStatusText.text = "Please select two characters first!";
            return;
        }

        if (player1Instance == null || player2Instance == null)
            SpawnPlayers();

        UpdateSpawnPointSelection();
        StartCoroutine(TransitionCameraAndChange(sceneID));
    }

    private void SetFrameWorldSize(GameObject frame, bool expand)
    {
        SpriteRenderer fr = frame.GetComponent<SpriteRenderer>();
        if (fr == null || fr.sprite == null) return;

        float scale = Mathf.Min(frameWorldSize / fr.sprite.bounds.size.x, frameWorldSize / fr.sprite.bounds.size.y);
        frame.transform.localScale = Vector3.one * (scale * (expand ? 1f : frameIdleScale));
    }

    private IEnumerator AnimateFrameExpand(GameObject frame, bool expand)
    {
        SpriteRenderer fr = frame.GetComponent<SpriteRenderer>();
        if (fr == null || fr.sprite == null) yield break;

        float baseScale = Mathf.Min(frameWorldSize / fr.sprite.bounds.size.x, frameWorldSize / fr.sprite.bounds.size.y);
        float startScale = frame.transform.localScale.x;
        float targetScale = expand ? baseScale : baseScale * frameIdleScale;

        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime * frameExpandSpeed;
            frame.transform.localScale = Vector3.one * Mathf.Lerp(startScale, targetScale, t);
            yield return null;
        }
        frame.transform.localScale = Vector3.one * targetScale;
    }

    private IEnumerator TransitionCameraAndChange(int sceneID)
    {
        if (mainCam == null)
        {
            SceneManager.LoadScene(sceneID);
            yield break;
        }

        SpriteRenderer targetChar = characterRenderers[selectedOption];
        Vector3 startPos = mainCam.transform.position;
        Vector3 endPos = new Vector3(targetChar.transform.position.x, targetChar.transform.position.y, startPos.z);
        float startSize = mainCam.orthographicSize;
        float t = 0f;

        while (t < 1f)
        {
            t += Time.deltaTime * cameraMoveSpeed;
            mainCam.transform.position = Vector3.Lerp(startPos, endPos, t);
            mainCam.orthographicSize = Mathf.Lerp(startSize, cameraZoom, t);
            yield return null;
        }

        yield return new WaitForSeconds(sceneChangeDelay);

        DontDestroyOnLoad(player1Instance);
        DontDestroyOnLoad(player2Instance);

        SceneManager.LoadScene(sceneID);
    }
}
