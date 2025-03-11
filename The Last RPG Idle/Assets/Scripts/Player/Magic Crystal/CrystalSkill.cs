using System.Collections;
using System.Collections.Generic;
using UnityEditor.Search;
using UnityEngine;
using UnityEngine.UI;

public class CrystalSkill : Skill
{
    [SerializeField] private float crystalDuration;
    [SerializeField] private GameObject crystalPrefab;
    private GameObject currentCrystal;

    [Header("Crystal Mirage")]
    [SerializeField] private UI_SkillTreeSlot unlockCloneInsteadButton;
    public bool cloneInsteadOfCrystal { get; private set; }

    // This is for the skill tree
    [Header("Crystal Simple")]
    [SerializeField] private UI_SkillTreeSlot unlockCrystalButtton;
    public bool crystalUnlocked { get; private set; }

    [Header("Explosive crystal")]
    [SerializeField] private UI_SkillTreeSlot unlockExplosiveButton;
    public bool canExplode { get; private set; }



    [Header("Moving crystal")]
    [SerializeField] private UI_SkillTreeSlot unlockMovingCrystalButton;
    [SerializeField] private bool canMoveToEnemy;
    [SerializeField] private float moveSpeed;

    [Header("Multi Stacking Crystal")]
    [SerializeField] private UI_SkillTreeSlot unlockMultiStackButton;
    [SerializeField] private bool canUseMultiStacks;
    [SerializeField] private int amountOfStacks;
    [SerializeField] private float multiStackCooldown;
    [SerializeField] private float useTimeWindow; // reset ability
    [SerializeField] private List<GameObject> crystalLeft = new List<GameObject>();

    protected override void Start()
    {
        base.Start();

        unlockCloneInsteadButton.GetComponent<Button>().onClick.AddListener(() => { unlockCloneInsteadButton.UnlockSkillSlot(); UnlockCrystalMirage(); });
        unlockCrystalButtton.GetComponent<Button>().onClick.AddListener(() => { unlockCrystalButtton.UnlockSkillSlot(); UnlockCrystal(); });
        unlockExplosiveButton.GetComponent<Button>().onClick.AddListener(() => { unlockExplosiveButton.UnlockSkillSlot(); UnlockExplosiveCrystal(); });
        unlockMovingCrystalButton.GetComponent<Button>().onClick.AddListener(() => { unlockMovingCrystalButton.UnlockSkillSlot(); UnlockMovingCrystal(); });
        unlockMultiStackButton.GetComponent<Button>().onClick.AddListener(() => { unlockMultiStackButton.UnlockSkillSlot(); UnlockMultiStack(); });
    }

    #region Skill Tree - Crystal Skill
    private void UnlockCrystal() { 
        if (unlockCrystalButtton.unlocked)
            crystalUnlocked = true;
    }
    private void UnlockCrystalMirage()
    {
        if (unlockCloneInsteadButton.unlocked)
            cloneInsteadOfCrystal = true;
    }

    private void UnlockExplosiveCrystal()
    {
        if (unlockExplosiveButton.unlocked)
            canExplode = true;
    }

    private void UnlockMovingCrystal()
    {
        if (unlockMovingCrystalButton.unlocked)
            canMoveToEnemy = true;
    }

    private void UnlockMultiStack()
    {
        if (unlockMultiStackButton.unlocked)
            canUseMultiStacks = true;
    }
    #endregion



    public override void UseSkill()
    {
        base.UseSkill();
        if (CanUseMultiCrystal())
            return;

        if (currentCrystal == null)
        {
            CreateCrystal();
        }
        else
        {

            if (canMoveToEnemy)
                return;

            Vector2 playerPos = player.transform.position;
            player.transform.position = currentCrystal.transform.position;
            currentCrystal.transform.position = playerPos;

            if (cloneInsteadOfCrystal)
            {
                SkillManager.instance.clone.CreateClone(currentCrystal.transform, Vector3.zero);
                Destroy(currentCrystal);
            }
            else 
            { 
                currentCrystal.GetComponent<CrystalSkillController>()?.FinishCrystal();
            }
        }
    }

    public void CreateCrystal()
    {
        currentCrystal = Instantiate(crystalPrefab, player.transform.position, Quaternion.identity);

        CrystalSkillController currentCrystalScript = currentCrystal.GetComponent<CrystalSkillController>();
        currentCrystalScript.SetupCrystal(crystalDuration, canExplode, canMoveToEnemy, moveSpeed, FindClosestEnemy(currentCrystal.transform), player);
    }
    public void CurrentCrystalChooseRandomTarget() {
        currentCrystal.GetComponent<CrystalSkillController>().ChooseRandomEnemy();
    }

    private void RefillCrystal()
    {
        int amountToAdd = amountOfStacks - crystalLeft.Count;

        for (int i = 0; i < amountToAdd; i++)
        {
            crystalLeft.Add(crystalPrefab);
        }
    }

    private bool CanUseMultiCrystal()
    {

        if (canUseMultiStacks)
        {
            if (crystalLeft.Count > 0)
            {
                if (crystalLeft.Count == amountOfStacks)
                {
                    Invoke("ResetAbility", useTimeWindow);
                }

                cooldown = 0;

                GameObject crystalSpawn = crystalLeft[crystalLeft.Count - 1];
                GameObject newCrystal = Instantiate(crystalSpawn, player.transform.position, Quaternion.identity);

                crystalLeft.Remove(crystalSpawn);

                newCrystal.GetComponent<CrystalSkillController>().
                    SetupCrystal(crystalDuration,
                                    canExplode,
                                    canMoveToEnemy,
                                    moveSpeed,
                                    FindClosestEnemy(newCrystal.transform), player);

                if (crystalLeft.Count <= 0)
                {
                    cooldown = multiStackCooldown;
                    RefillCrystal();

                }
                return true;

            }

        }
        return false;
    }
    private void ResetAbility()
    {

        if (cooldownTimer > 0)
            return;

        cooldownTimer = multiStackCooldown;
        RefillCrystal();
    }


}
