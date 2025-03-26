// SampleManager.cs
using UnityEngine;

public class SampleManager : MonoBehaviour
{
    [System.Serializable]
    public class DrumCategory
    {
        public string categoryName;
        public AudioClip[] samples;
        [Range(0f, 1f)]
        public float categoryVolume = 1f;
    }

    public DrumCategory[] categories;

    private static SampleManager instance;
    public static SampleManager Instance
    {
        get
        {
            if (instance == null)
                instance = FindObjectOfType<SampleManager>();
            return instance;
        }
    }

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else if (instance != this)
            Destroy(gameObject);
    }

    public int GetCategoryCount()
    {
        return categories.Length;
    }

    public DrumCategory GetCategory(int index)
    {
        if (index >= 0 && index < categories.Length)
            return categories[index];
        return null;
    }

    public float GetCategoryVolume(int categoryIndex)
    {
        if (categoryIndex >= 0 && categoryIndex < categories.Length)
        {
            return categories[categoryIndex].categoryVolume;
        }
        return 1f;
    }
}