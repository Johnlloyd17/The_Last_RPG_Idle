using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class DodgeSkill : Skill
{
    [Header("Dodge")]
    [SerializeField] private UI_SkillTreeSlot dodgeUnlockButton;
    [SerializeField] private int evasionAmount;
    public bool dodgeUnlocked { get; private set; }


    [Header("Dodge with Mirage")]
    [SerializeField] private UI_SkillTreeSlot dodgeUnlockMirageButton;
    public bool dodgeMirageUnlocked { get; private set; }


    protected override void Start()
    {
        base.Start();
        dodgeUnlockButton.GetComponent<Button>().onClick.AddListener(() =>
        {
            dodgeUnlockButton.UnlockSkillSlot();
            UnlockDodge();
        });

        dodgeUnlockMirageButton.GetComponent<Button>().onClick.AddListener(() =>
        {
            dodgeUnlockMirageButton.UnlockSkillSlot();
            UnlockDodgeWithMirage();
        });
    }

    public override void UseSkill()
    {
        base.UseSkill();
    }

    private void UnlockDodge()
    {
        if (dodgeUnlockButton.unlocked)
        {
            player.stats.evasion.AddModifier(evasionAmount);
            Inventory.instance.UpdateStatsUI();
            dodgeUnlocked = true;
        }
    }
    private void UnlockDodgeWithMirage()
    {
        if (dodgeUnlockMirageButton.unlocked)
            dodgeMirageUnlocked = true;
    }


    public void CanDodgeWithMirage() { 
        if (dodgeMirageUnlocked)
        {
            SkillManager.instance.clone.CreateClone(player.transform, new Vector3(1.5f * player.facingDir,0));
        }
    }
}
