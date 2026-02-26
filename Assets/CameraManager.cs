using UnityEngine;

/// <summary>
/// Simple camera manager: selects camera1 at start and provides a slot / switch for camera2.
/// - Assign camera1 and camera2 in inspector (camera2 optional).
/// - If camera1 is null, Camera.main will be used (or a new camera is created).
/// - If camera2 is null and createCamera2IfMissing is true, a second camera is created/copied from camera1.
/// - Use SwitchToCamera(1) / SwitchToCamera(2) or ToggleCamera() at runtime.
/// </summary>
public class CameraManager : MonoBehaviour
{
    [Tooltip("Primary camera to use at game start. If null, Camera.main will be used or created.")]
    public Camera camera1;

    [Tooltip("Secondary camera slot. Optional - will be created if missing when requested.")]
    public Camera camera2;

    [Tooltip("If true and camera2 is not assigned, create a copy of camera1 for camera2.")]
    public bool createCamera2IfMissing = true;

    [Tooltip("Optional transform to position the created camera2. If null, camera2 will copy camera1's transform.")]
    public Transform camera2Anchor;

    // active camera index: 1 or 2
    public int activeCameraIndex = 1;

    void Start()
    {
        EnsureCamera1();
        if (createCamera2IfMissing && camera2 == null)
            EnsureCamera2();

        // Make sure camera1 is active at start
        SwitchToCamera(1);
    }

    void EnsureCamera1()
    {
        if (camera1 != null) return;

        // try main camera
        camera1 = Camera.main;
        if (camera1 != null) return;

        // create a fallback camera
        var go = new GameObject("Camera1");
        camera1 = go.AddComponent<Camera>();
        camera1.transform.position = Vector3.zero;
        camera1.transform.rotation = Quaternion.identity;
        camera1.clearFlags = CameraClearFlags.Skybox;
    }

    void EnsureCamera2()
    {
        if (camera1 == null) EnsureCamera1();

        // create new GameObject and copy settings from camera1
        GameObject go = new GameObject("Camera2");
        camera2 = go.AddComponent<Camera>();
        camera2.CopyFrom(camera1);

        // position camera2
        if (camera2Anchor != null)
        {
            camera2.transform.SetPositionAndRotation(camera2Anchor.position, camera2Anchor.rotation);
        }
        else
        {
            // place slightly offset so preview in editor is visible
            camera2.transform.SetPositionAndRotation(camera1.transform.position + Vector3.right * 2f, camera1.transform.rotation);
        }

        // Avoid multiple AudioListeners active
        var al = go.GetComponent<AudioListener>();
        if (al != null) al.enabled = false;
        var mainAL = camera1.GetComponent<AudioListener>();
        if (mainAL == null)
        {
            // if camera1 lacks an AudioListener, try to add one to camera1
            camera1.gameObject.AddComponent<AudioListener>();
        }

        // start disabled
        camera2.enabled = false;
    }

    /// <summary>
    /// Switch active camera to index 1 or 2. Safe if camera2 is missing.
    /// </summary>
    public void SwitchToCamera(int index)
    {
        if (index != 1 && index != 2) return;

        if (index == 1)
        {
            if (camera1 == null) EnsureCamera1();
            if (camera1 != null) camera1.enabled = true;
            if (camera2 != null) camera2.enabled = false;
            activeCameraIndex = 1;
        }
        else // index == 2
        {
            if (camera2 == null && createCamera2IfMissing) EnsureCamera2();
            if (camera2 == null) return; // nothing to switch to

            if (camera1 != null) camera1.enabled = false;
            camera2.enabled = true;
            activeCameraIndex = 2;
        }

        // ensure only one AudioListener active
        EnsureSingleAudioListener();
    }

    /// <summary>
    /// Toggle between camera1 and camera2 (creates camera2 if allowed and missing).
    /// </summary>
    public void ToggleCamera()
    {
        if (activeCameraIndex == 1) SwitchToCamera(2);
        else SwitchToCamera(1);
    }

    Camera GetActiveCamera()
    {
        return (activeCameraIndex == 1) ? camera1 : camera2;
    }

    void EnsureSingleAudioListener()
    {
AudioListener[] listeners = Object.FindObjectsByType<AudioListener>(FindObjectsSortMode.None);
        bool found = false;
        foreach (var l in listeners)
        {
            if (!found && l.enabled)
            {
                found = true;
                continue;
            }
            // disable any extra listeners
            l.enabled = false;
        }
    }
}
