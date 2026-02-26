using UnityEngine;
using System.Collections.Generic;
using System.Collections;

[CreateAssetMenu(menuName = "Character/Character Database")]
public class CharacterDatabase : ScriptableObject
{
    public Character[] character;

    // Per-character default preview scale (X,Y). If empty or shorter than character array, Vector2.one is used.
    public Vector2[] characterScales;

    // Add this: prefab references for each character
    public GameObject[] characterPrefabs;

    public int CharacterCount
    {
        get { return character != null ? character.Length : 0; }
    }

    public Character GetCharacter(int index)
    {
        if (character == null || index < 0 || index >= character.Length) return null;
        return character[index];
    }

    public Vector2 GetScale(int index)
    {
        if (characterScales != null && index >= 0 && index < characterScales.Length)
            return characterScales[index];
        return Vector2.one;
    }

    public void SetScale(int index, Vector2 scale)
    {
        if (index < 0) return;
        if (characterScales == null || characterScales.Length < CharacterCount)
        {
            var newArr = new Vector2[Mathf.Max(CharacterCount, index + 1)];
            if (characterScales != null)
            {
                for (int i = 0; i < characterScales.Length && i < newArr.Length; i++)
                    newArr[i] = characterScales[i];
            }
            for (int i = 0; i < newArr.Length; i++)
                if (newArr[i] == Vector2.zero) newArr[i] = Vector2.one;
            characterScales = newArr;
        }
        else if (index >= characterScales.Length)
        {
            var newArr = new Vector2[index + 1];
            for (int i = 0; i < characterScales.Length; i++) newArr[i] = characterScales[i];
            for (int i = characterScales.Length; i < newArr.Length; i++) newArr[i] = Vector2.one;
            characterScales = newArr;
        }

        characterScales[index] = scale;
    }

    // --- NEW: Get prefab for character ---
    public GameObject GetPrefab(int index)
    {
        if (characterPrefabs != null && index >= 0 && index < characterPrefabs.Length)
            return characterPrefabs[index];
        return null;
    }

    // --- NEW: Assign prefab for character ---
    public void SetPrefab(int index, GameObject prefab)
    {
        if (index < 0) return;
        if (characterPrefabs == null || characterPrefabs.Length < CharacterCount)
        {
            var newArr = new GameObject[Mathf.Max(CharacterCount, index + 1)];
            if (characterPrefabs != null)
            {
                for (int i = 0; i < characterPrefabs.Length && i < newArr.Length; i++)
                    newArr[i] = characterPrefabs[i];
            }
            characterPrefabs = newArr;
        }
        else if (index >= characterPrefabs.Length)
        {
            var newArr = new GameObject[index + 1];
            for (int i = 0; i < characterPrefabs.Length; i++) newArr[i] = characterPrefabs[i];
            characterPrefabs = newArr;
        }

        characterPrefabs[index] = prefab;
    }
}
