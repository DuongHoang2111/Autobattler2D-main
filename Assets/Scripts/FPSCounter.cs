using UnityEngine;

public class FPSCounter : MonoBehaviour
{
    public float updateInterval = 0.5f; // Cập nhật FPS mỗi 0.5 giây
    private float accum = 0f; // Tổng thời gian của các khung hình
    private int frames = 0; // Số khung hình đếm được
    private float timeLeft; // Thời gian còn lại để cập nhật
    private string fpsText = "";

    void Start()
    {
        timeLeft = updateInterval;
    }

    void Update()
    {
        timeLeft -= Time.deltaTime;
        accum += Time.timeScale / Time.deltaTime; // Tính FPS dựa trên deltaTime
        frames++;

        if (timeLeft <= 0f)
        {
            float fps = accum / frames;
            fpsText = $"FPS: {fps:F2}"; // Hiển thị FPS với 2 chữ số thập phân
            timeLeft = updateInterval;
            accum = 0f;
            frames = 0;
        }
    }

    void OnGUI()
    {
        GUI.Label(new Rect(10, 10, 100, 20), fpsText); // Hiển thị FPS trên màn hình
    }
}