using System;
using UnityEngine;

/// <summary>
/// Struktur data yang menampung semua pembacaan sensor MPU-6050.
/// Harus disimpan dalam file MPU6050Data.cs dan TIDAK diwariskan dari MonoBehaviour.
/// </summary>
[System.Serializable]
public class MPU6050Data
{
    public float accelX, accelY, accelZ;
    public float gyroX, gyroY, gyroZ;
    public float roll, pitch, yaw;

    public override string ToString()
    {
        return $"Accel: ({accelX:F3}, {accelY:F3}, {accelZ:F3}), " +
               $"Gyro: ({gyroX:F3}, {gyroY:F3}, {gyroZ:F3}), " +
               $"Rot: R={roll:F1}° P={pitch:F1}° Y={yaw:F1}°";
    }
}