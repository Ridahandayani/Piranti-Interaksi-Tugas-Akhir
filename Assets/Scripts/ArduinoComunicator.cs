using System;
using System.IO.Ports;
using System.Globalization;
using UnityEngine;

/// <summary>
/// MPU6050Controller menangani komunikasi serial dan mengontrol gerakan
/// maju/mundur (Translasi) dan belok kiri/kanan (Rotasi Yaw).
/// </summary>
public class MPU6050Controller : MonoBehaviour
{
    // --- Pengaturan Serial Port ---
    [Header("Serial Port Settings")]
    public string portName = "COM5";
    public int baudRate = 115200;

    // --- Pengaturan Target ---
    [Header("Target Object")]
    public Transform targetObject;

    // --- Pengaturan Gerakan ---
    [Header("Movement Settings")]
    [Tooltip("Kecepatan maksimum saat miring penuh ke depan (AccelX positif).")]
    public float maxForwardSpeed = 5.0f;
    [Tooltip("Sensitivitas belok (Yaw).")]
    [Range(0.1f, 2.0f)]
    public float turnSensitivity = 1.0f;
    [Tooltip("Input AccelX yang dibutuhkan untuk mulai bergerak.")]
    public float accelerationThreshold = 0.2f;

    [Header("Invert & Smoothing")]
    public bool invertY = false;
    [Range(0.01f, 1f)]
    public float smoothingFactor = 0.1f;

    [Header("Dead Zone")]
    [Tooltip("Toleransi pergerakan Yaw (derajat) agar objek dianggap diam dan tidak drift.")]
    public float yawDeadZone = 0.5f;
    [Tooltip("Toleransi AccelX (g) agar objek dianggap diam dan tidak maju/mundur karena noise.")]
    public float accelDeadZone = 0.05f; // Nilai AccelX harus mendekati 0.0, jadi dead zone lebih kecil

    // --- Debug Options ---
    [Header("Debug Options")]
    public bool enableDebugLog = true;
    public bool logRawData = false;
    public bool logParsedData = true;

    private SerialPort serialPort;
    private MPU6050Data currentData;
    private Vector3 smoothedRotation;
    private float calibrationOffsetY;    // Offset untuk Yaw
    private float calibrationOffsetAccelX = 0f; // Offset untuk AccelX (Maju/Mundur)
    private bool isCalibrated = false;

    // Properties untuk akses data yang aman
    public MPU6050Data CurrentData => currentData;
    public float Roll => currentData?.roll ?? 0f;
    public float Pitch => currentData?.pitch ?? 0f;
    public float Yaw => currentData?.yaw ?? 0f;

    // ======================================================================

    void Start()
    {
        currentData = new MPU6050Data();
        calibrationOffsetY = 0f;
        calibrationOffsetAccelX = 0f;
        smoothedRotation = transform.eulerAngles;

        if (targetObject == null)
            targetObject = transform;

        ConnectToArduino();
    }

    void ConnectToArduino()
    {
        try
        {
            serialPort = new SerialPort(portName, baudRate);
            serialPort.ReadTimeout = 50;
            serialPort.Open();
            Debug.Log($"[MPU6050] ✓ Connected to Arduino on {portName} at {baudRate} baud.");
        }
        catch (Exception e)
        {
            Debug.LogError($"[MPU6050] ✗ Failed to connect to {portName}. Error: {e.Message}");
        }
    }

    void Update()
    {
        if (serialPort != null && serialPort.IsOpen)
        {
            ReadArduinoData();
            ProcessMovement();
        }

        HandleInput();
    }

    void ReadArduinoData()
    {
        try
        {
            string data = serialPort.ReadLine().Trim();

            if (logRawData && enableDebugLog)
                Debug.Log("[MPU6050] Raw Data: " + data);

            if (data.StartsWith("{") && data.EndsWith("}"))
            {
                ParseCustomData(data);
            }
        }
        catch (TimeoutException) { }
        catch (Exception e)
        {
            Debug.LogWarning("[MPU6050] Serial Read Error: " + e.Message);
        }
    }

    void ParseCustomData(string jsonData)
    {
        // Mendefinisikan CultureInfo Invariant untuk parsing angka yang menggunakan titik (dot)
        CultureInfo invariantCulture = CultureInfo.InvariantCulture;

        try
        {
            // Pembersihan String KRITIS (menghilangkan CR dan Tanda Kutip Ganda)
            jsonData = jsonData.Replace("\r", "");
            jsonData = jsonData.Replace("\"", "");

            // Hapus kurung kurawal luar
            jsonData = jsonData.Substring(1, jsonData.Length - 2);

            string[] pairs = jsonData.Split(',');

            foreach (string pair in pairs)
            {
                string[] keyValue = pair.Split(':');
                if (keyValue.Length == 2)
                {
                    string key = keyValue[0].Trim();
                    string valueString = keyValue[1].Trim();

                    // Parsing dengan Culture Invariant
                    if (float.TryParse(valueString, NumberStyles.Any, invariantCulture, out float value))
                    {
                        switch (key)
                        {
                            case "accelX": currentData.accelX = value; break;
                            case "accelY": currentData.accelY = value; break;
                            case "accelZ": currentData.accelZ = value; break;
                            case "gyroX": currentData.gyroX = value; break;
                            case "gyroY": currentData.gyroY = value; break;
                            case "gyroZ": currentData.gyroZ = value; break;
                            case "roll": currentData.roll = value; break;
                            case "pitch": currentData.pitch = value; break;
                            case "yaw": currentData.yaw = value; break;
                        }
                    }
                }
            }

            if (logParsedData && enableDebugLog)
                Debug.Log("[MPU6050] Updated Data: " + currentData);
        }
        catch (Exception e)
        {
            Debug.LogWarning("[MPU6050] Parsing failed: " + e.Message);
        }
    }

    void ProcessMovement()
    {
        if (targetObject == null) return;

        // 1. KONTROL BELOK (ROTASI YAW)
        float targetYaw = (invertY ? -currentData.yaw : currentData.yaw);

        if (isCalibrated)
            targetYaw -= calibrationOffsetY;

        // --- Terapkan DEAD ZONE YAW (Mengatasi Drift Belok) ---
        float yawMagnitude = Mathf.Abs(targetYaw);
        if (yawMagnitude < yawDeadZone)
        {
            targetYaw = 0f;
        }
        // --- Akhir DEAD ZONE YAW ---

        targetYaw *= turnSensitivity;

        smoothedRotation.y = Mathf.Lerp(smoothedRotation.y, targetYaw, smoothingFactor);

        // Terapkan rotasi: Hanya sumbu Y yang digunakan untuk belok
        targetObject.rotation = Quaternion.Euler(0, smoothedRotation.y, 0);


        // 2. KONTROL MAJU/MUNDUR (TRANSLASI)
        float inputAccelX = currentData.accelX;

        // Terapkan offset akselerasi jika sudah dikalibrasi
        if (isCalibrated)
            inputAccelX -= calibrationOffsetAccelX;

        float forwardSpeed = 0f;

        // --- Terapkan DEAD ZONE ACCELX (Mengatasi Drift Maju/Mundur) ---
        float accelXMagnitude = Mathf.Abs(inputAccelX);

        if (accelXMagnitude > accelDeadZone)
        {
            // Hitung kecepatan maju/mundur jika di luar Dead Zone
            if (inputAccelX > accelerationThreshold) // Miring ke depan
            {
                float normalizedInput = Mathf.Clamp01((inputAccelX - accelerationThreshold) / (1.0f - accelerationThreshold));
                forwardSpeed = normalizedInput * maxForwardSpeed;
            }
            else if (inputAccelX < -accelerationThreshold) // Miring ke belakang
            {
                float normalizedInput = Mathf.Clamp01((-inputAccelX - accelerationThreshold) / (1.0f - accelerationThreshold));
                forwardSpeed = -normalizedInput * maxForwardSpeed * 0.5f;
            }
        }
        // --- Akhir DEAD ZONE ACCELX ---


        // Pindahkan objek sesuai orientasi (Yaw) saat ini
        Vector3 forwardVector = targetObject.forward;
        targetObject.position += forwardVector * forwardSpeed * Time.deltaTime;
    }

    // ======================================================================
    // Logika Kalibrasi dan Reset

    void HandleInput()
    {
        if (Input.GetKeyDown(KeyCode.C))
        {
            CalibrateRotation();
        }
        else if (Input.GetKeyDown(KeyCode.R))
        {
            ResetRotation();
        }
    }

    void CalibrateRotation()
    {
        // Ambil offset Yaw (Y)
        calibrationOffsetY = (invertY ? -currentData.yaw : currentData.yaw);

        // Ambil offset Akselerasi X saat sensor diam (mengatasi drift maju)
        calibrationOffsetAccelX = currentData.accelX;

        isCalibrated = true;

        if (enableDebugLog)
            Debug.Log($"[MPU6050] Calibrated. Yaw Offset: {calibrationOffsetY}, AccelX Offset: {calibrationOffsetAccelX}");
    }

    void ResetRotation()
    {
        calibrationOffsetY = 0f;
        calibrationOffsetAccelX = 0f; // Reset juga offset AccelX
        isCalibrated = false;
        smoothedRotation = Vector3.zero;

        if (targetObject != null)
        {
            targetObject.rotation = Quaternion.identity;
            // Hanya reset posisi X dan Z
            targetObject.position = new Vector3(0, targetObject.position.y, 0);
        }

        if (enableDebugLog)
            Debug.Log("[MPU6050] Rotation and Movement reset.");
    }

    void OnApplicationQuit()
    {
        if (serialPort != null && serialPort.IsOpen)
            serialPort.Close();
    }
}