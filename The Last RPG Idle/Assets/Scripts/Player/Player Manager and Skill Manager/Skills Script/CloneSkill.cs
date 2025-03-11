using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CloneSkill : Skill
{

    [Header("Clone info")]
    [SerializeField] private float attackMultiplier;
    [SerializeField] private GameObject clonePrefab;
    [SerializeField] private float _cloneDuration;
    [Space]

    [Header("Clone Attack")]
    [SerializeField] private UI_SkillTreeSlot cloneAttackUnlockButton;
    [SerializeField] private float cloneAttackMultiplier;
    [SerializeField] private bool canAttack;

    [Header("Aggressive Clone")]
    [SerializeField] private UI_SkillTreeSlot aggressiveCloneUnlockButton;
    [SerializeField] private float aggressiveCloneMultiplier;
    public bool canApplyOnHitEffect { get; private set; }


    [Header("Multiple clone")]
    [SerializeField] private UI_SkillTreeSlot multipleUnlockButton;
    [SerializeField] private float multiCloneAttackMultiplier;
    [SerializeField] private bool canDuplicateClone;
    [SerializeField] private float chanceToDuplicate;

    [Header("Crystal instead of clone")]
    [SerializeField] private UI_SkillTreeSlot crystalInsteadUnlockButton;
    public bool crystalInsteadOfClone;
    protected override void Start()
    {
        base.Start();
        cloneAttackUnlockButton.GetComponent<Button>().onClick.AddListener(() =>
        {
            cloneAttackUnlockButton.UnlockSkillSlot();
            UnlockCloneAttack();
        });

        aggressiveCloneUnlockButton.GetComponent <Button>().onClick.AddListener(() =>
        {
            aggressiveCloneUnlockButton.UnlockSkillSlot();
            UnlockAggressiveClone();
        });

        multipleUnlockButton.GetComponent<Button>().onClick.AddListener(() =>
        {
            multipleUnlockButton.UnlockSkillSlot();
            UnlockMultiClone();
        });

        crystalInsteadUnlockButton.GetComponent<Button>().onClick.AddListener(() =>
        {
            crystalInsteadUnlockButton.UnlockSkillSlot();
            UnlockCrystalInstead();
        }); 
    }
    public void CreateClone(Transform _clonePosition, Vector3 _offset)
    {

        if (crystalInsteadOfClone)
        {
            SkillManager.instance.crystal.CreateCrystal();
            //SkillManager.instance.crystal.CurrentCrystalChooseRandomTarget();
            return;
        }

        GameObject newClone = Instantiate(clonePrefab);
        newClone.GetComponent<CloneSkillController>().SetupClone(_clonePosition, _cloneDuration, canAttack, _offset, FindClosestEnemy(newClone.transform), canDuplicateClone, chanceToDuplicate, player, attackMultiplier);
    }


    public void CreateCloneWithDelay(Transform _enemyTransform)
    {
    
        StartCoroutine(CloneDelayCorourine(_enemyTransform, new Vector3(2 * player.facingDir, 0)));
        
    }

    private IEnumerator CloneDelayCorourine(Transform _transform, Vector3 _offset)
    {
        yield return new WaitForSeconds(.4f);
            CreateClone(_transform.transform, _offset);
    }

    #region Unlock Clone Region
    private void UnlockCloneAttack()
    {
        if (cloneAttackUnlockButton.unlocked)
        {
            canAttack = true;
            attackMultiplier = cloneAttackMultiplier;
        }
    }

    private void UnlockAggressiveClone() { 
        if (aggressiveCloneUnlockButton.unlocked)
        {
            canApplyOnHitEffect = true;
            attackMultiplier = aggressiveCloneMultiplier;
        }
    }
    private void UnlockMultiClone()
    {
        if (multipleUnlockButton.unlocked) 
        {
            canDuplicateClone = true;
            attackMultiplier = multiCloneAttackMultiplier;
        }
    }

    private void UnlockCrystalInstead() {
        if (crystalInsteadUnlockButton.unlocked) { 
            crystalInsteadOfClone = true;
        }
    }

    #endregion

}
