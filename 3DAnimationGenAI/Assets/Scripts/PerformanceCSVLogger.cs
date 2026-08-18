using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Unity.Profiling;
using UnityEngine;

public class FinalPerformanceLogger : MonoBehaviour
{
    // Collections for storing raw session data
    private List<float> allFrameTimesMs = new List<float>();
    private List<float> allLatenciesMs = new List<float>();
    
    // Memory profiling hooks
    private ProfilerRecorder gcMemoryRecorder;
    private ProfilerRecorder systemMemoryRecorder;
    
    private float peakGcMemoryMb = 0f;
    private float peakSystemMemoryMb = 0f;

    void OnEnable()
    {
        // Keep tracking active across scene changes
        DontDestroyOnLoad(gameObject);

        // Initialize low-overhead memory trackers
        gcMemoryRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Memory, "GC Managed Memory");
        systemMemoryRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Memory, "System Used Memory");
    }

    void Update()
    {
        // 1. Record frame time
        float currentFrameTimeMs = Time.unscaledDeltaTime * 1000f;
        allFrameTimesMs.Add(currentFrameTimeMs);

        // 2. Record Engine Latency (Main Thread + Render Present Wait)
        FrameTiming[] timings = new FrameTiming[1];
        FrameTimingManager.CaptureFrameTimings();
        uint numTimings = FrameTimingManager.GetLatestTimings(1, timings);
        
        float latencyMs = (numTimings > 0) 
            ? (float)(timings[0].cpuFrameTime + timings[0].cpuMainThreadPresentWaitTime) 
            : currentFrameTimeMs;
        allLatenciesMs.Add(latencyMs);

        // 3. Track absolute peak memory usage during this session
        if (gcMemoryRecorder.Valid)
        {
            float currentGc = gcMemoryRecorder.LastValue / (1024f * 1024f);
            if (currentGc > peakGcMemoryMb) peakGcMemoryMb = currentGc;
        }
        if (systemMemoryRecorder.Valid)
        {
            float currentSys = systemMemoryRecorder.LastValue / (1024f * 1024f);
            if (currentSys > peakSystemMemoryMb) peakSystemMemoryMb = currentSys;
        }
    }

    void OnDisable()
    {
        // Clean up native profiling handles immediately
        gcMemoryRecorder.Dispose();
        systemMemoryRecorder.Dispose();

        if (allFrameTimesMs.Count == 0) return;

        // --- CALCULATE FINAL METRICS ---
        float totalTimeMs = 0f;
        for (int i = 0; i < allFrameTimesMs.Count; i++) totalTimeMs += allFrameTimesMs[i];
        float avgFrameTimeMs = totalTimeMs / allFrameTimesMs.Count;
        float finalAvgFps = 1000f / avgFrameTimeMs;

        List<float> sortedFrameTimes = new List<float>(allFrameTimesMs);
        sortedFrameTimes.Sort();

        float maxFrameTimeMs = sortedFrameTimes[sortedFrameTimes.Count - 1];
        float finalLowestFps = 1000f / maxFrameTimeMs;

        int onePercentCount = Mathf.Max(1, Mathf.RoundToInt(sortedFrameTimes.Count * 0.01f));
        int startIndex = sortedFrameTimes.Count - onePercentCount;
        float worstFrameTimesSum = 0f;
        for (int i = startIndex; i < sortedFrameTimes.Count; i++)
        {
            worstFrameTimesSum += sortedFrameTimes[i];
        }
        float avg1PercentWorstFrameTimeMs = worstFrameTimesSum / onePercentCount;
        float final1PercentLowFps = 1000f / avg1PercentWorstFrameTimeMs;

        float totalLatencyMs = 0f;
        for (int i = allLatenciesMs.Count - 1; i >= 0; i--) totalLatencyMs += allLatenciesMs[i];
        float finalAvgLatencyMs = totalLatencyMs / allLatenciesMs.Count;

        // --- WRITE FINAL SUMMARY (APPEND MODE) ---
        string csvPath = Path.Combine(Application.persistentDataPath, "Final_Session_Performance_Summary.csv");
        
        try
        {
            // Check if the file already exists before opening the stream
            bool fileExists = File.Exists(csvPath);

            // Setting the second argument to 'true' enables APPEND mode
            using (StreamWriter writer = new StreamWriter(csvPath, true, Encoding.UTF8))
            {
                // Only write headers if this is a brand new file
                if (!fileExists)
                {
                    writer.WriteLine("Timestamp,Total_Frames,Avg_FPS,1Percent_Low_FPS,Absolute_Lowest_FPS,Avg_Latency_ms,Peak_GC_Memory_MB,Peak_System_Memory_MB");
                }
                
                // Append the new session row seamlessly at the bottom
                writer.WriteLine(string.Format("{0},{1},{2:F1},{3:F1},{4:F1},{5:F2},{6:F2},{7:F2}",
                    DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), // Useful for tracking separate runs
                    allFrameTimesMs.Count,
                    finalAvgFps,
                    final1PercentLowFps,
                    finalLowestFps,
                    finalAvgLatencyMs,
                    peakGcMemoryMb,
                    peakSystemMemoryMb
                ));
            }
            Debug.Log($"[Performance] Performance metrics appended successfully to: {csvPath}");
        }
        catch (Exception e)
        {
            Debug.LogError($"[Performance] Failed to log session metrics: {e.Message}");
        }
    }
}