using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Diagnostics;
using System.IO;
// using Python.Runtime; // Assicurati di avere Python.NET installato e configurato
using UnityEditor.Scripting.Python;

public class MediaPipeRunner : MonoBehaviour
{
    private Process pythonProcess;
    public string pythonExePath = "python"; // Path to python.exe, use "python" if in PATH
    public string pythonScriptPath = "Assets/Scripts/hand_tracking.py";
    
    // Assicurati che lo script venga eseguito una sola volta
    private bool scriptStarted = false;

    void Start()
    {
        PythonRunner.EnsureInitialized(); // Assicurati che PythonRunner sia inizializzato
        StartPythonScript();
    }

    public void StartPythonScript()
    {
        if (scriptStarted)
            return;
            
        try
        {
            // Converti il percorso relativo in assoluto
            string fullPath = Path.GetFullPath(pythonScriptPath);
            
            // Verifica che il file esista
            if (!File.Exists(fullPath))
            {
                UnityEngine.Debug.LogError($"Python script not found at: {fullPath}");
                return;
            }
            
            UnityEngine.Debug.Log($"Starting Python script: {fullPath}");
            
            // Configura il processo Python
            pythonProcess = new Process();
            pythonProcess.StartInfo.FileName = pythonExePath;
            pythonProcess.StartInfo.Arguments = fullPath;
            pythonProcess.StartInfo.UseShellExecute = false;
            pythonProcess.StartInfo.CreateNoWindow = false; // Mostra la finestra Python per il debug
            
            // Avvia il processo
            pythonProcess.Start();
            scriptStarted = true;
            
            UnityEngine.Debug.Log("Python script started successfully");
        }
        catch (System.Exception e)
        {
            UnityEngine.Debug.LogError($"Error starting Python script: {e.Message}");
        }
    }

    void OnApplicationQuit()
    {
        StopPythonScript();
    }

    public void StopPythonScript()
    {
        if (pythonProcess != null && !pythonProcess.HasExited)
        {
            try
            {
                pythonProcess.Kill();
                pythonProcess.WaitForExit();
                pythonProcess.Dispose();
                UnityEngine.Debug.Log("Python script stopped");
            }
            catch (System.Exception e)
            {
                UnityEngine.Debug.LogError($"Error stopping Python script: {e.Message}");
            }
        }
        
        scriptStarted = false;
    }
}
