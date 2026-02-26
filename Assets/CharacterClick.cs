using UnityEngine;

public class CharacterClick : MonoBehaviour
{
    private CharacterManager manager;
    private int index;

    public void Setup(CharacterManager mgr, int idx)
    {
        manager = mgr;
        index = idx;
    }

    void OnMouseDown()
    {
        manager.SelectCharacter(index);
    }
}
