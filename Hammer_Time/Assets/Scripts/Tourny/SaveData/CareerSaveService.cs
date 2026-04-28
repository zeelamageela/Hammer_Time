using System;
using System.IO;
using UnityEngine;

/// <summary>
/// Service responsible for saving and loading career data using JSON.
/// Handles file I/O, backup creation, and error recovery.
/// </summary>
public static class CareerSaveService
{
    private const string SAVE_FILE_NAME = "career_save.json";
    private const string BACKUP_FILE_NAME = "career_save_backup.json";
    
    private static string SavePath => Path.Combine(Application.persistentDataPath, SAVE_FILE_NAME);
    private static string BackupPath => Path.Combine(Application.persistentDataPath, BACKUP_FILE_NAME);
    
    // CRITICAL: Prevent concurrent saves from causing corruption
    private static bool _isSaving = false;
    private static object _saveLock = new object();
        
        /// <summary>
        /// Saves career data to JSON file with automatic backup
        /// </summary>
        public static bool SaveCareer(CareerSaveData data)
        {
            if (data == null)
            {
                Debug.LogError("[CareerSaveService] Cannot save null data");
                return false;
            }
            
            // CRITICAL: Prevent concurrent saves
            lock (_saveLock)
            {
                if (_isSaving)
                {
                    Debug.LogWarning("[CareerSaveService] Save already in progress - skipping concurrent save");
                    return false;
                }
                
                _isSaving = true;
            }
            
            try
            {
                // Create backup of existing save before overwriting
                CreateBackup();
                
                // Update save date
                data.saveDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                
                // Serialize to JSON with pretty printing
                string json = JsonUtility.ToJson(data, true);
                
                // Check if we have enough space to write (iOS specific)
                // Estimate: JSON size + 50% buffer for safety
                long estimatedSize = json.Length * 2;
                if (!CheckStorageSpace(estimatedSize))
                {
                    Debug.LogError("[CareerSaveService] Insufficient storage space to save");
                    return false;
                }
                
                // Write to file
                File.WriteAllText(SavePath, json);
                
                // Verify the file was written successfully (iOS specific check)
                if (!File.Exists(SavePath))
                {
                    Debug.LogError("[CareerSaveService] Save file was not created!");
                    return false;
                }
                
                // Verify file size is reasonable
                FileInfo fileInfo = new FileInfo(SavePath);
                if (fileInfo.Length == 0)
                {
                    Debug.LogError("[CareerSaveService] Save file is empty!");
                    return false;
                }
                
                Debug.Log($"[CareerSaveService] Career saved successfully to: {SavePath}");
                Debug.Log($"[CareerSaveService] Save size: {fileInfo.Length} bytes ({json.Length} characters)");
                return true;
            }
            catch (IOException ioEx)
            {
                Debug.LogError($"[CareerSaveService] IO error while saving: {ioEx.Message}");
                Debug.LogError("[CareerSaveService] This may indicate low storage space or permission issues on iOS");
                Debug.LogError($"[CareerSaveService] Stack trace: {ioEx.StackTrace}");
                return false;
            }
            catch (UnauthorizedAccessException authEx)
            {
                Debug.LogError($"[CareerSaveService] Permission denied while saving: {authEx.Message}");
                Debug.LogError("[CareerSaveService] Check iOS app permissions for file access");
                return false;
            }
            catch (Exception e)
            {
                Debug.LogError($"[CareerSaveService] Failed to save career: {e.Message}\n{e.StackTrace}");
                return false;
            }
            finally
            {
                lock (_saveLock)
                {
                    _isSaving = false;
                }
            }
        }
        
        /// <summary>
        /// Loads career data from JSON file with validation
        /// </summary>
        public static CareerSaveData LoadCareer()
        {
            if (!File.Exists(SavePath))
            {
                Debug.LogWarning($"[CareerSaveService] No save file found at: {SavePath}");
                return null;
            }
            
            try
            {
                string json = File.ReadAllText(SavePath);
                CareerSaveData data = JsonUtility.FromJson<CareerSaveData>(json);
                
                if (!ValidateSaveData(data))
                {
                    Debug.LogError("[CareerSaveService] Save data validation failed");
                    return TryLoadBackup();
                }
                
                Debug.Log($"[CareerSaveService] Career loaded successfully (Version {data.version}, Saved: {data.saveDate})");
                return data;
            }
            catch (Exception e)
            {
                Debug.LogError($"[CareerSaveService] Failed to load career: {e.Message}");
                return TryLoadBackup();
            }
        }
        
        /// <summary>
        /// Attempts to load from backup file
        /// </summary>
        private static CareerSaveData TryLoadBackup()
        {
            if (!File.Exists(BackupPath))
            {
                Debug.LogWarning("[CareerSaveService] No backup file available");
                return null;
            }
            
            try
            {
                Debug.Log("[CareerSaveService] Attempting to load from backup...");
                string json = File.ReadAllText(BackupPath);
                CareerSaveData data = JsonUtility.FromJson<CareerSaveData>(json);
                
                if (ValidateSaveData(data))
                {
                    Debug.Log("[CareerSaveService] Successfully loaded from backup");
                    return data;
                }
                
                Debug.LogError("[CareerSaveService] Backup data is also invalid");
                return null;
            }
            catch (Exception e)
            {
                Debug.LogError($"[CareerSaveService] Failed to load backup: {e.Message}");
                return null;
            }
        }
        
        /// <summary>
        /// Validates save data integrity with comprehensive checks
        /// </summary>
        private static bool ValidateSaveData(CareerSaveData data)
        {
            if (data == null)
            {
                Debug.LogError("[CareerSaveService] Data is null");
                return false;
            }
            
            if (string.IsNullOrEmpty(data.playerName))
            {
                Debug.LogError("[CareerSaveService] Invalid save: missing player name");
                return false;
            }
            
            if (data.teams == null || data.teams.Count == 0)
            {
                Debug.LogError("[CareerSaveService] Invalid save: no teams data");
                return false;
            }
            
            if (data.version < 1)
            {
                Debug.LogError($"[CareerSaveService] Invalid save version: {data.version}");
                return false;
            }
            
            // CRITICAL: Validate game state consistency if game in progress
            if (data.currentGameState != null && data.currentGameState.gameInProgress)
            {
                if (data.currentGameState.endScores == null)
                {
                    Debug.LogWarning("[CareerSaveService] Game in progress but endScores is null - clearing gameInProgress flag");
                    data.currentGameState.gameInProgress = false;
                }
                
                if (data.currentGameState.ends <= 0 || data.currentGameState.rocks <= 0)
                {
                    Debug.LogWarning($"[CareerSaveService] Invalid game settings: ends={data.currentGameState.ends}, rocks={data.currentGameState.rocks} - clearing gameInProgress");
                    data.currentGameState.gameInProgress = false;
                }
            }
            
            return true;
        }
        
        /// <summary>
        /// Creates a backup of the current save file
        /// </summary>
        private static void CreateBackup()
        {
            if (File.Exists(SavePath))
            {
                try
                {
                    File.Copy(SavePath, BackupPath, true);
                    Debug.Log("[CareerSaveService] Backup created");
                }
                catch (Exception e)
                {
                    Debug.LogWarning($"[CareerSaveService] Failed to create backup: {e.Message}");
                }
            }
        }
        
        /// <summary>
        /// Checks if a save file exists
        /// </summary>
        public static bool SaveExists()
        {
            return File.Exists(SavePath);
        }
        
        /// <summary>
        /// Deletes the save file and backup
        /// </summary>
        public static void DeleteSave()
        {
            try
            {
                if (File.Exists(SavePath))
                {
                    File.Delete(SavePath);
                    Debug.Log("[CareerSaveService] Save file deleted");
                }
                
                if (File.Exists(BackupPath))
                {
                    File.Delete(BackupPath);
                    Debug.Log("[CareerSaveService] Backup file deleted");
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[CareerSaveService] Failed to delete save: {e.Message}");
            }
        }
        
        /// <summary>
        /// Gets save file info for display
        /// </summary>
        public static string GetSaveInfo()
        {
            if (!SaveExists())
            {
                return "No save file found";
            }
            
            try
            {
                FileInfo fileInfo = new FileInfo(SavePath);
                return $"Last saved: {fileInfo.LastWriteTime:yyyy-MM-dd HH:mm:ss}\nSize: {fileInfo.Length / 1024}KB";
            }
            catch
            {
                return "Save file info unavailable";
            }
        }
        
        /// <summary>
        /// Exports save to a custom location (useful for debugging or sharing)
        /// </summary>
        public static bool ExportSave(string targetPath)
        {
            if (!SaveExists())
            {
                Debug.LogError("[CareerSaveService] No save to export");
                return false;
            }
            
            try
            {
                File.Copy(SavePath, targetPath, true);
                Debug.Log($"[CareerSaveService] Save exported to: {targetPath}");
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"[CareerSaveService] Export failed: {e.Message}");
                return false;
            }
        }
        
        /// <summary>
        /// Checks if sufficient storage space is available (approximate)
        /// </summary>
        private static bool CheckStorageSpace(long requiredBytes)
        {
            try
            {
                // On iOS, we can't directly check free space, but we can try to detect issues
                // by checking if the save directory is writable
                string testFile = Path.Combine(Application.persistentDataPath, ".storage_test");
                
                try
                {
                    File.WriteAllText(testFile, "test");
                    File.Delete(testFile);
                    return true;
                }
                catch
                {
                    return false;
                }
            }
            catch
            {
                // If we can't test, assume we have space
                return true;
            }
        }
        
        /// <summary>
        /// Imports a save from a custom location
        /// </summary>
        public static bool ImportSave(string sourcePath)
        {
            if (!File.Exists(sourcePath))
            {
                Debug.LogError($"[CareerSaveService] Source file not found: {sourcePath}");
                return false;
            }
            
            try
            {
                // Validate before importing
                string json = File.ReadAllText(sourcePath);
                CareerSaveData testData = JsonUtility.FromJson<CareerSaveData>(json);
                
                if (!ValidateSaveData(testData))
                {
                    Debug.LogError("[CareerSaveService] Import failed: invalid save data");
                    return false;
                }
                
                // Create backup before importing
                CreateBackup();
                
                File.Copy(sourcePath, SavePath, true);
                Debug.Log($"[CareerSaveService] Save imported from: {sourcePath}");
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"[CareerSaveService] Import failed: {e.Message}");
                return false;
            }
        }
    }
