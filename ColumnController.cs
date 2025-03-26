using UnityEngine;

public class ColumnController : MonoBehaviour
{
    [Header("Spawn Point Settings")]
    [Tooltip("Child Transform that marks where the next timeline column should spawn.")]
    public Transform nextColumnSpawnPoint;

    [Header("Button Group Settings")]
    [Tooltip("Array of button-group parent objects. Order: 0 = 1 measure group (4 buttons), 1 = 2 measures (2 buttons), 2 = 4 measures (1 button).")]
    public GameObject[] buttonGroupParents;

    [Header("Recording Block Settings")]
    [Tooltip("Block length (measures); valid values: 1, 2, or 4. Determines which button group is activated.")]
    public int blockLength = 1;

    /// <summary>
    /// Configures this column by deactivating all button-group objects,
    /// then activating only the group corresponding to the blockLength.
    /// </summary>
    public void SetupColumn()
    {
        Debug.Log("[ColumnController] SetupColumn called on " + gameObject.name + " with blockLength: " + blockLength);
        if (buttonGroupParents == null || buttonGroupParents.Length == 0)
        {
            Debug.LogError("[ColumnController] Button group parents are not assigned.");
            return;
        }

        // Deactivate all groups.
        for (int i = 0; i < buttonGroupParents.Length; i++)
        {
            if (buttonGroupParents[i] != null)
            {
                buttonGroupParents[i].SetActive(false);
                Debug.Log("[ColumnController] Deactivated group: " + buttonGroupParents[i].name);
            }
            else
            {
                Debug.LogWarning("[ColumnController] Button group at index " + i + " is null.");
            }
        }

        // Determine which group to activate.
        int indexToActivate = 0;
        if (blockLength == 1)
            indexToActivate = 0;
        else if (blockLength == 2)
            indexToActivate = 1;
        else if (blockLength == 4)
            indexToActivate = 2;
        else
        {
            Debug.LogWarning("[ColumnController] Unrecognized blockLength (" + blockLength + "). Defaulting to 1 measure.");
            indexToActivate = 0;
        }

        if (indexToActivate < buttonGroupParents.Length && buttonGroupParents[indexToActivate] != null)
        {
            buttonGroupParents[indexToActivate].SetActive(true);
            Debug.Log("[ColumnController] Activated group: " + buttonGroupParents[indexToActivate].name);
        }
        else
        {
            Debug.LogError("[ColumnController] Could not activate button group at index " + indexToActivate);
        }
    }
}