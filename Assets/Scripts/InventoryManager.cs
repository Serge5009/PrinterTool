using System.IO;
using System.Linq;
using UnityEngine;
using System;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance { get; private set; }

    public UserSaveData ActiveData { get; private set; }

    private string SaveFilePath => Application.persistentDataPath + "/user_inventory.json";

    public event Action OnInventoryChanged;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        LoadData();
    }

    private void LoadData()
    {
        if (File.Exists(SaveFilePath))
        {
            try
            {
                string json = File.ReadAllText(SaveFilePath);
                ActiveData = JsonUtility.FromJson<UserSaveData>(json);
                Debug.Log("[InventoryManager] Loaded existing inventory data.");
            }
            catch (Exception e)
            {
                Debug.LogError($"[InventoryManager] Failed to load data: {e.Message}");
                InitializeFreshData();
            }
        }
        else
        {
            InitializeFreshData();
        }
    }

    private void InitializeFreshData()
    {
        ActiveData = new UserSaveData();

        ActiveData.allInventories.Add(new InventoryList("Owned Spools"));
        ActiveData.allInventories.Add(new InventoryList("Wishlist"));
        ActiveData.allInventories.Add(new InventoryList("Archived/Empty"));

        SaveData();
        Debug.Log("[InventoryManager] Created new fresh inventory profile.");
    }

    public void SaveData()
    {
        string json = JsonUtility.ToJson(ActiveData, true);
        File.WriteAllText(SaveFilePath, json);

        OnInventoryChanged?.Invoke();
    }

    public void AddSpoolToList(string listName, FilamentProfileSO profile, float startingWeight = 1000f)
    {
        InventoryList targetList = ActiveData.allInventories.FirstOrDefault(l => l.listName == listName);

        if (targetList != null)
        {
            SpoolInstance newSpool = new SpoolInstance(profile.uniqueId, startingWeight);
            targetList.spools.Add(newSpool);

            SaveData();
            Debug.Log($"[InventoryManager] Added new {profile.itemName} spool to {listName}.");
        }
        else
        {
            Debug.LogError($"[InventoryManager] List '{listName}' does not exist!");
        }
    }

    public FilamentProfileSO GetProfileForSpool(SpoolInstance spool)
    {
        return AppManager.Instance.masterDatabase.allFilaments
            .FirstOrDefault(f => f.uniqueId == spool.catalogItemId);
    }
}