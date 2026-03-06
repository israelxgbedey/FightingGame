using UnityEngine;

public class ObjectNamer : MonoBehaviour
{
    [Header("Naming Settings")]
    [SerializeField] private string objectName = "New Object";
    [SerializeField] private bool applyOnStart = true;
    [SerializeField] private bool applyOnAwake = false;
    [SerializeField] private bool addNumberSuffix = false;
    [SerializeField] private bool logToConsole = true;
    
    private void Awake()
    {
        if (applyOnAwake)
        {
            ApplyName();
        }
    }
    
    void Start()
    {
        if (applyOnStart && !applyOnAwake)
        {
            ApplyName();
        }
    }
    
    [ContextMenu("Apply Name")]
    public void ApplyName()
    {
        string newName = objectName;
        
        if (addNumberSuffix)
        {
            // Find all objects with similar names and add a unique number
            int count = 1;
            string baseName = objectName;
            GameObject[] allObjects = FindObjectsOfType<GameObject>();
            
            foreach (GameObject obj in allObjects)
            {
                if (obj.name.StartsWith(baseName) && obj != gameObject)
                {
                    count++;
                }
            }
            
            newName = $"{baseName} ({count})";
        }
        
        gameObject.name = newName;
        
        if (logToConsole)
        {
            Debug.Log($"Object '{gameObject.name}' renamed to: {newName}");
        }
    }
    
    // Reset the object name to default when component is added
    private void Reset()
    {
        objectName = gameObject.name;
    }
}