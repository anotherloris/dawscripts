using System;
using System.Collections.Generic;
using UnityEngine;

public class DynamicRowManager : MonoBehaviour
{
    public static DynamicRowManager Instance;

    [Header("Row Settings")]
    [Tooltip("Total number of rows to have initially.")]
    public int defaultNumRows = 2;

    [Header("Column Settings")]
    [Tooltip("Total number of timeline columns per row (including the one already in the prefab).")]
    public int defaultNumColumnsPerRow = 1;

    [Header("Prefabs & References")]
    [Tooltip("The first row (manually placed in the scene).")]
    public RowController firstRow;
    [Tooltip("Row prefab (with RowController) used for creating additional rows.")]
    public GameObject rowPrefab;
    [Tooltip("Timeline column prefab used for creating columns.")]
    public GameObject columnPrefab;
    [Tooltip("Parent container for all rows.")]
    public Transform rowsParent;

    // List to store all rows.
    private List<RowController> rows = new List<RowController>();

    // Dictionary mapping each RowController to its list of active timeline columns.
    private Dictionary<RowController, List<ColumnController>> rowColumns = new Dictionary<RowController, List<ColumnController>>();

    // Event invoked whenever rows or columns are updated.
    public event Action OnRowsAndColumnsUpdated;

    // Flag to indicate that initialization is complete.
    public bool IsInitialized { get; private set; } = false;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    private void Start()
    {
        if (firstRow == null)
        {
            Debug.LogError("[DynamicRowManager] First row is not assigned!");
            return;
        }

        // Add the first row and initialize its column list.
        rows.Add(firstRow);
        rowColumns.Add(firstRow, new List<ColumnController>());
        InitializeRowColumns(firstRow);

        // Ensure the first row has the required number of columns.
        int extra = defaultNumColumnsPerRow - GetColumnCount(firstRow);
        for (int j = 0; j < extra; j++)
            AddColumnToRow(firstRow);

        // Spawn additional rows until we reach defaultNumRows.
        while (rows.Count < defaultNumRows)
            AddNewRow();

        // Mark initialization complete.
        IsInitialized = true;
        // Fire the event so subscribers know everything is ready.
        OnRowsAndColumnsUpdated?.Invoke();
    }

    // Initializes the column list for a given row by scanning its timelineContainer.
    private void InitializeRowColumns(RowController row)
    {
        if (row.timelineContainer == null)
        {
            Debug.LogError("[DynamicRowManager] Timeline container is not assigned in " + row.gameObject.name);
            return;
        }

        int childCount = row.timelineContainer.childCount;
        for (int i = 0; i < childCount; i++)
        {
            GameObject colObj = row.timelineContainer.GetChild(i).gameObject;
            ColumnController colCtrl = colObj.GetComponent<ColumnController>();
            if (colCtrl != null)
            {
                colCtrl.SetupColumn();
                rowColumns[row].Add(colCtrl);
                // For the last column, update row's columnSpawnPoint.
                if (i == childCount - 1 && colCtrl.nextColumnSpawnPoint != null)
                    row.columnSpawnPoint = colCtrl.nextColumnSpawnPoint;
            }
            else
            {
                Debug.LogWarning("[DynamicRowManager] Column " + colObj.name + " in row " + row.gameObject.name + " is missing a ColumnController.");
            }
        }
    }

    // Adds a new row at the spawn point of the last row.
    public void AddNewRow()
    {
        RowController lastRow = rows[rows.Count - 1];
        if (lastRow.rowSpawnPoint == null)
        {
            Debug.LogError("[DynamicRowManager] Last row's spawn point is not assigned in " + lastRow.gameObject.name);
            return;
        }
        Vector3 spawnPos = lastRow.rowSpawnPoint.position;
        GameObject newRowObj = Instantiate(rowPrefab, spawnPos, Quaternion.identity, rowsParent);
        RowController newRow = newRowObj.GetComponent<RowController>();
        if (newRow == null)
        {
            Debug.LogError("[DynamicRowManager] Instantiated row is missing RowController component!");
            return;
        }
        rows.Add(newRow);
        rowColumns.Add(newRow, new List<ColumnController>());

        // Initialize columns for the new row.
        InitializeRowColumns(newRow);
        int extra = defaultNumColumnsPerRow - GetColumnCount(newRow);
        for (int j = 0; j < extra; j++)
            AddColumnToRow(newRow);

        // Notify subscribers
        OnRowsAndColumnsUpdated?.Invoke();
    }

    // Creates and adds a new column to the specified row.
    public void AddColumnToRow(RowController row)
    {
        if (columnPrefab == null)
        {
            Debug.LogError("[DynamicRowManager] Column prefab is not assigned.");
            return;
        }
        if (row == null)
        {
            Debug.LogError("[DynamicRowManager] Row is null when adding a column.");
            return;
        }
        if (row.columnSpawnPoint == null)
        {
            Debug.LogError("[DynamicRowManager] Row '" + row.name + "' does not have a columnSpawnPoint assigned.");
            return;
        }
        Vector3 spawnPos = row.columnSpawnPoint.position;
        GameObject newColumnObj = Instantiate(columnPrefab, spawnPos, Quaternion.identity, row.timelineContainer);
        Debug.Log("[DynamicRowManager] Instantiated new column at: " + spawnPos + " in row " + row.gameObject.name);

        ColumnController colCtrl = newColumnObj.GetComponent<ColumnController>();
        if (colCtrl != null)
        {
            colCtrl.SetupColumn();
            rowColumns[row].Add(colCtrl);
            // Update the row's spawn point for further columns.
            if (colCtrl.nextColumnSpawnPoint != null)
            {
                row.columnSpawnPoint = colCtrl.nextColumnSpawnPoint;
                Debug.Log("[DynamicRowManager] Updated " + row.gameObject.name + " columnSpawnPoint to: " + row.columnSpawnPoint.position);
            }
            else
            {
                Debug.LogWarning("[DynamicRowManager] New column " + newColumnObj.name + " is missing nextColumnSpawnPoint.");
            }
        }
        else
        {
            Debug.LogError("[DynamicRowManager] New column " + newColumnObj.name + " does not have a ColumnController component.");
        }
        // Fire the update event.
        OnRowsAndColumnsUpdated?.Invoke();

        // Force AudioManager to update the track length.
        if (AudioManager.Instance != null)
            AudioManager.Instance.UpdateTrackLength();
    }

    // Returns the number of columns for the specified row.
    public int GetColumnCount(RowController row)
    {
        if (rowColumns.ContainsKey(row))
            return rowColumns[row].Count;
        return 0;
    }

    // Optional: Add one column to every row.
    public void AddColumnToAllRows()
    {
        foreach (RowController row in rows)
            AddColumnToRow(row);
    }
}