using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DashSkill : Skill
{
    [Header("Dash")]
    [SerializeField] private UI_SkillTreeSlot dashUnlockButton;
    public bool dashUnlocked { get; private set; }

    [Header("Clone on dash")]
    [SerializeField] private UI_SkillTreeSlot cloneOnDashUnlockButton;
    public bool cloneDashUnlocked { get; private set; }

    [Header("Clone on arrival")]
    [SerializeField] private UI_SkillTreeSlot cloneOnArrivalUnlockButton;
    public bool cloneArrivalUnlocked { get; private set; }

    public override void UseSkill()
    {
        base.UseSkill();
    }

    protected override void Start()
    {
        base.Start();

        dashUnlockButton.GetComponent<Button>().onClick.AddListener(() => { dashUnlockButton.UnlockSkillSlot(); UnlockDash(); });

        cloneOnDashUnlockButton.GetComponent<Button>().onClick.AddListener(() => { cloneOnDashUnlockButton.UnlockSkillSlot(); UnlockCloneOnDash(); });

        cloneOnArrivalUnlockButton.GetComponent<Button>().onClick.AddListener(() => { cloneOnArrivalUnlockButton.UnlockSkillSlot(); UnlockCloneOnArrival(); });

    }

    #region Booleans
    private void UnlockDash()
    {

        if (dashUnlockButton.unlocked)
            dashUnlocked = true;
    }
    private void UnlockCloneOnDash() 
    {
        if (cloneOnDashUnlockButton.unlocked) 
            cloneDashUnlocked = true; 
    }

    private void UnlockCloneOnArrival()
    {
        if (cloneOnArrivalUnlockButton.unlocked)
            cloneArrivalUnlocked = true;
    }
    #endregion

    // Clone
    public void CloneOnDash()
    {
        if (cloneDashUnlocked)
        {
            SkillManager.instance.clone.CreateClone(player.transform, Vector3.zero);
        }
    }

    // Arrival
    public void CloneOnArrival()
    {
        if (cloneArrivalUnlocked)
        {
            SkillManager.instance.clone.CreateClone(player.transform, Vector3.zero);
        }
    }

}
