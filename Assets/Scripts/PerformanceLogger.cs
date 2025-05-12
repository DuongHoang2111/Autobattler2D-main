using System;
using System.Collections;
using System.IO;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.Profiling;

public class PerformanceLogger : MonoBehaviour
{
    public string logFolderPath = "D:\\Project\\AtUniversity\\Parallel_Computing_Project\\LogFile\\Parallel";
    public string logFileName = "performance_log_sU8.txt";

    public bool isFighting = false;

    private float minFps = float.MaxValue;
    private float maxFps = 0f;
    private float timer = 0f;
    private float logInterval = 10f;
    private float totalFps = 0f;
    private int frameCount = 0;

    private string logPath;

    private void Start()
    {
        // Tạo thư mục nếu chưa có
        Directory.CreateDirectory(logFolderPath);

        // Ghép đường dẫn file log
        logPath = Path.Combine(logFolderPath, logFileName);

        StartCoroutine(LogPerformanceRepeatedly());
    }

    private void Update()
    {
        if (!isFighting) return;

        float currentFps = 1f / Time.unscaledDeltaTime;
        if (currentFps > maxFps) maxFps = currentFps;
        if (currentFps < minFps) minFps = currentFps;

        totalFps += currentFps;
        frameCount++;

        timer += Time.unscaledDeltaTime;
    }

    IEnumerator LogPerformanceRepeatedly()
    {
        while (true)
        {
            yield return new WaitForSeconds(logInterval);

            if (!isFighting) continue;

            float cpuUsage = Profiler.GetTotalAllocatedMemoryLong() / (1024f * 1024f);
            float memoryUsage = System.GC.GetTotalMemory(false) / (1024f * 1024f);
            float avgFps = frameCount > 0 ? totalFps / frameCount : 0f;

            // Lấy VRAM usage
            float vramUsage = Profiler.GetAllocatedMemoryForGraphicsDriver() / (1024f * 1024f); // Chuyển sang MB

            using (StreamWriter writer = new StreamWriter(logPath, true))
            {
                writer.WriteLine("------ Performance Log ------");
                writer.WriteLine("Time: " + System.DateTime.Now);
                writer.WriteLine("Max FPS: " + maxFps);
                writer.WriteLine("Min FPS: " + minFps);
                writer.WriteLine("Average FPS: " + avgFps);
                writer.WriteLine("CPU Allocated Memory (MB): " + cpuUsage);
                writer.WriteLine("GC Memory (MB): " + memoryUsage);
                writer.WriteLine("VRAM Usage (MB): " + vramUsage);
                writer.WriteLine("-----------------------------\n");
            }

            // Reset các giá trị
            minFps = float.MaxValue;
            maxFps = 0f;
            totalFps = 0f;
            frameCount = 0;
            timer = 0f;
        }
    }
}
