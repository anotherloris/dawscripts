// SampleMenu.cs
using UnityEngine;

public class SampleMenu : MonoBehaviour
{
    public GameObject sampleButtonPrefab;
    public Transform buttonContainer;

    private DrumController drumController;

    public void Initialize(DrumController controller, int categoryIndex)
    {
        drumController = controller;
        PopulateMenu(categoryIndex);
    }

    private void PopulateMenu(int categoryIndex)
    {
        var category = SampleManager.Instance.GetCategory(categoryIndex);
        if (category == null) return;

        for (int i = 0; i < category.samples.Length; i++)
        {
            GameObject buttonObj = Instantiate(sampleButtonPrefab, buttonContainer);
            SampleButton button = buttonObj.GetComponent<SampleButton>();

            AudioClip sample = category.samples[i];
            button.Initialize(i + 1, () => SelectSample(sample));
        }
    }

    private void SelectSample(AudioClip sample)
    {
        if (drumController != null && drumController.associatedDrumPad != null)
        {
            drumController.associatedDrumPad.SetDrumSound(sample);
        }
        Destroy(gameObject);
    }
}