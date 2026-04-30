using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rotate : MonoBehaviour
{
    // Минимальный и максимальный размер
    public float minScale = 1.5f;
    public float maxScale = 2.5f;

    // Минимальная и максимальная скорость вращения
    public float minRotationSpeed = 5.0f;
    public float maxRotationSpeed = 15.0f;

    // Случайная ось вращения
    private Vector3 rotationAxis;
    // Скорость вращения
    private float rotationSpeed;

    // Материал объекта
    private Material objectMaterial;

    void Start()
    {
        // Задаем случайный размер объекту
        float randomScale = Random.Range(minScale, maxScale);
        transform.localScale = new Vector3(randomScale, randomScale, randomScale);

        // Задаем случайную ось вращения
        rotationAxis = Random.onUnitSphere; // Случайная единичная ось

        // Задаем случайную скорость вращения
        rotationSpeed = Random.Range(minRotationSpeed, maxRotationSpeed);

        // Получаем материал объекта
        Renderer renderer = GetComponent<Renderer>();
        if (renderer != null)
        {
            objectMaterial = renderer.material;

            // Изменяем цвет материала (либо светлее, либо темнее)
            Color baseColor = objectMaterial.color;  // Исходный цвет
            float randomFactor = Random.Range(0.8f, 1.2f); // Случайный коэффициент для осветления/затемнения
            Color randomColor = baseColor * randomFactor;  // Измененный цвет
            objectMaterial.color = randomColor;  // Применяем цвет
        }
    }

    void Update()
    {
        // Вращение объекта вокруг случайной оси с случайной скоростью
        transform.Rotate(rotationAxis * rotationSpeed * Time.deltaTime);
    }
}
