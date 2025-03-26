// SampleButton.cs
using UnityEngine;
using TMPro;

public class SampleButton : MonoBehaviour
{
    public TextMeshPro numberText;
    private System.Action onPressed;

    public void Initialize(int number, System.Action callback)
    {
        numberText.text = number.ToString();
        onPressed = callback;
    }

    private void OnMouseDown()
    {
        onPressed?.Invoke();
    }
}