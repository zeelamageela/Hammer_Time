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
    private const string FAILED_LOAD_FILE_NAME = "career_save_failed_load.json";
    private const string REPLACED_FILE_NAME = "career_save_replaced.json";

    /// <summary>Test seam: when set, saves go here instead of persistentDataPath.</summary>
    public static string DirectoryOverride;

    private static string SaveDir => string.IsNullOrEmpty(DirectoryOverride) ? Application.persistentDataPath : DirectoryOverride;
    private static string SavePath => Path.Combine(SaveDir, SAVE_FILE_NAME);
    private static string BackupPath => Path.Combine(SaveDir, BACKUP_FILE_NAME);
    
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
            
            // Never write a save our own loader would reject (e.g. a pause/quit save fired
            // before a career exists) - it would also get rotated over the good backup.
            if (!ValidateSaveData(data))
            {
                Debug.LogError("[CareerSaveService] Refusing to save invalid career data");
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
                
                // Write to a temp file first so a crash/kill mid-write (iOS backgrounding)
                // can never leave a truncated career_save.json
                string tempPath = SavePath + ".tmp";
                File.WriteAllText(tempPath, json);
                
                // Verify the file was written successfully (iOS specific check)
                if (!File.Exists(tempPath))
                {
                    Debug.LogError("[CareerSaveService] Save file was not created!");
                    return false;
                }
                
                // Verify file size is reasonable
                FileInfo fileInfo = new FileInfo(tempPath);
                if (fileInfo.Length == 0)
                {
                    Debug.LogError("[CareerSaveService] Save file is empty!");
                    return false;
                }

                // Swap the verified temp file into place
                if (File.Exists(SavePath))
                    File.Replace(tempPath, SavePath, null);
                else
                    File.Move(tempPath, SavePath);
                
                GameLog.Info($"[CareerSaveService] Career saved successfully to: {SavePath}");
                GameLog.Info($"[CareerSaveService] Save size: {fileInfo.Length} bytes ({json.Length} characters)");
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

                // A failed save must not leave its temp file behind
                try { if (File.Exists(SavePath + ".tmp")) File.Delete(SavePath + ".tmp"); } catch { }
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
                    PreserveFailedLoad();
                    return TryLoadBackup();
                }
                
                GameLog.Info($"[CareerSaveService] Career loaded successfully (Version {data.version}, Saved: {data.saveDate})");
                return data;
            }
            catch (Exception e)
            {
                Debug.LogError($"[CareerSaveService] Failed to load career: {e.Message}");
                PreserveFailedLoad();
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
                GameLog.Info("[CareerSaveService] Attempting to load from backup...");
                string json = File.ReadAllText(BackupPath);
                CareerSaveData data = JsonUtility.FromJson<CareerSaveData>(json);
                
                if (ValidateSaveData(data))
                {
                    GameLog.Info("[CareerSaveService] Successfully loaded from backup");
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
                    // Don't rotate a corrupt/invalid file over the last good backup
                    if (!IsValidSaveFile(SavePath))
                    {
                        Debug.LogWarning("[CareerSaveService] Current save is invalid - keeping existing backup");
                        return;
                    }

                    File.Copy(SavePath, BackupPath, true);
                    GameLog.Info("[CareerSaveService] Backup created");
                }
                catch (Exception e)
                {
                    Debug.LogWarning($"[CareerSaveService] Failed to create backup: {e.Message}");
                }
            }
        }
        
        /// <summary>
        /// Copies the current save aside so a failed load can't lose it to the next autosave.
        /// </summary>
        public static void PreserveFailedLoad()
        {
            try
            {
                if (File.Exists(SavePath))
                {
                    File.Copy(SavePath, Path.Combine(SaveDir, FAILED_LOAD_FILE_NAME), true);
                    Debug.LogWarning("[CareerSaveService] Unloadable save preserved as " + FAILED_LOAD_FILE_NAME);
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[CareerSaveService] Could not preserve failed save: {e.Message}");
            }
        }

        private static bool IsValidSaveFile(string path)
        {
            try
            {
                return ValidateSaveData(JsonUtility.FromJson<CareerSaveData>(File.ReadAllText(path)));
            }
            catch
            {
                return false;
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
        /// Keeps a copy of the current career as career_save_replaced.json before a new career overwrites it
        /// (one previous career is kept). Prefers the main save, falls back to the backup if the main one is unusable.
        /// Returns true if a copy was kept; false if there was nothing valid to keep.
        /// </summary>
        public static bool ArchiveSave()
        {
            try
            {
                string source = null;
                if (File.Exists(SavePath) && IsValidSaveFile(SavePath))
                    source = SavePath;
                else if (File.Exists(BackupPath) && IsValidSaveFile(BackupPath))
                    source = BackupPath;

                if (source == null)
                    return false;

                File.Copy(source, Path.Combine(SaveDir, REPLACED_FILE_NAME), true);
                GameLog.Info($"[CareerSaveService] Previous career kept as {REPLACED_FILE_NAME}");
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"[CareerSaveService] Failed to archive previous career: {e.Message}");
                return false;
            }
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
                    GameLog.Info("[CareerSaveService] Save file deleted");
                }
                
                if (File.Exists(BackupPath))
                {
                    File.Delete(BackupPath);
                    GameLog.Info("[CareerSaveService] Backup file deleted");
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
                GameLog.Info($"[CareerSaveService] Save exported to: {targetPath}");
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
                string testFile = Path.Combine(SaveDir, ".storage_test");
                
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
                GameLog.Info($"[CareerSaveService] Save imported from: {sourcePath}");
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"[CareerSaveService] Import failed: {e.Message}");
                return false;
            }
        }
    }
