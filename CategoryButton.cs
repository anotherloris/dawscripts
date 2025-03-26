// CategoryButton.cs
using UnityEngine;

public class CategoryButton : MonoBehaviour
{
    public DrumController drumController;

    private void OnMouseDown()
    {
        if (drumController != null)
        {
            drumController.OnCategoryButtonPressed();
        }
    }
}