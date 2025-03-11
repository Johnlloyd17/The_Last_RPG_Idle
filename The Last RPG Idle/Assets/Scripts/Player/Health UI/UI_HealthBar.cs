using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class UI_HealthBar : MonoBehaviour
{
    private Entity entity;
    private CharacterStats myStats;
    private RectTransform myTransform;
    private Slider slider;

    private void Start()
    {
        CheckComponents();
        entity.onFlipped += FlipUI;
        myStats.onHealthChanged += UpdateHealthUI;
        UpdateHealthUI();
    }

    private void UpdateHealthUI()
    {
        slider.maxValue = myStats.GetMaxHealValue();
        slider.value = myStats.currentHealth;
    }

    #region Checking Components and GameObjects
    private void CheckComponents()
    {
        myTransform = GetComponent<RectTransform>();
        entity = GetComponentInParent<Entity>();
        slider = GetComponentInChildren<Slider>();
        myStats = GetComponentInParent<CharacterStats>();

        DefensiveStatement();
    }

    private void DefensiveStatement()
    {
        if (entity == null) Debug.Log("Entity is not found.");
        if (myTransform == null) Debug.Log("Transform is not found.");
        if (slider == null) Debug.Log("Slider is not found.");
        if (myStats == null) Debug.Log("Character Stats is not found.");
    }
    #endregion

    private void FlipUI() => myTransform.Rotate(0, -180, 0);
    private void OnDisable()
    {
        entity.onFlipped -= FlipUI;
        myStats.onHealthChanged -= UpdateHealthUI;
    }
}
