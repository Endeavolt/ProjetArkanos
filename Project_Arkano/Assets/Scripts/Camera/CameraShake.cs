using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraShake : MonoBehaviour
{
    [SerializeField] [Tooltip("Amount of shake")] [Range(0, 1)] private float shakeForce = 0.5f;
    [SerializeField] private float angleRotation = 10.0f;
    [SerializeField] private float maxOffset = 1.0f;

    private Vector3 cameraPosition;
    private Quaternion cameraRotation;

    public void Start()
    {
        Random.InitState(42);
    }
    public IEnumerator LaunchShakeEffect(float timeEffect, float shakeAmount)
    {
        float timer = 0;
        shakeForce = shakeAmount;
        cameraPosition = transform.position;
        cameraRotation = transform.rotation;
        while (timer < timeEffect)
        {
            ShakeCamera();
            timer += Time.deltaTime;
            yield return Time.deltaTime;
        }

        transform.position = cameraPosition;
        transform.rotation = cameraRotation;
    }

    private void ShakeCamera()
    {
        float angle = cameraRotation.z + angleRotation * shakeForce * Mathf.PerlinNoise(Random.value, Random.value) - (angleRotation * shakeForce) / 2;
        float offsetX = cameraPosition.x + maxOffset * shakeForce * Mathf.PerlinNoise(Random.value, Random.value);
        float offsetY = cameraPosition.y + maxOffset * shakeForce * Mathf.PerlinNoise(Random.value, Random.value);

        transform.rotation = Quaternion.Euler(0, 0, angle);
        transform.position = new Vector3(offsetX, offsetY, cameraPosition.z);
    }
}
