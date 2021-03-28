using EntityStates;
using RoR2;
using RoR2.Skills;
using System;
using UnityEngine;

namespace TTGL_Survivor.Modules
{
    internal static class Skills
    {
        internal static void CreateSkillFamilies(GameObject targetPrefab)
        {
            foreach (GenericSkill obj in targetPrefab.GetComponentsInChildren<GenericSkill>())
            {
                TTGL_SurvivorPlugin.DestroyImmediate(obj);
            }

            SkillLocator skillLocator = targetPrefab.GetComponent<SkillLocator>();

            skillLocator.primary = targetPrefab.AddComponent<GenericSkill>();
            SkillFamily primaryFamily = ScriptableObject.CreateInstance<SkillFamily>();
            primaryFamily.variants = new SkillFamily.Variant[0];
            skillLocator.primary._skillFamily = primaryFamily;
            ContentPacks.skillFamilies.Add(primaryFamily);

            skillLocator.secondary = targetPrefab.AddComponent<GenericSkill>();
            SkillFamily secondaryFamily = ScriptableObject.CreateInstance<SkillFamily>();
            secondaryFamily.variants = new SkillFamily.Variant[0];
            skillLocator.secondary._skillFamily = secondaryFamily;
            ContentPacks.skillFamilies.Add(secondaryFamily);

            skillLocator.utility = targetPrefab.AddComponent<GenericSkill>();
            SkillFamily utilityFamily = ScriptableObject.CreateInstance<SkillFamily>();
            utilityFamily.variants = new SkillFamily.Variant[0];
            skillLocator.utility._skillFamily = utilityFamily;
            ContentPacks.skillFamilies.Add(utilityFamily);

            skillLocator.special = targetPrefab.AddComponent<GenericSkill>();
            SkillFamily specialFamily = ScriptableObject.CreateInstance<SkillFamily>();
            specialFamily.variants = new SkillFamily.Variant[0];
            skillLocator.special._skillFamily = specialFamily;
            ContentPacks.skillFamilies.Add(specialFamily);
        }
        // this could all be a lot cleaner but at least it's simple and easy to work with
        internal static void AddPrimarySkill(GameObject targetPrefab, SkillDef skillDef)
        {
            SkillLocator skillLocator = targetPrefab.GetComponent<SkillLocator>();

            SkillFamily skillFamily = skillLocator.primary.skillFamily;

            Array.Resize(ref skillFamily.variants, skillFamily.variants.Length + 1);
            skillFamily.variants[skillFamily.variants.Length - 1] = new SkillFamily.Variant
            {
                skillDef = skillDef,
                unlockableName = "",
                viewableNode = new ViewablesCatalog.Node(skillDef.skillNameToken, false, null)
            };
        }

        internal static void AddSecondarySkill(GameObject targetPrefab, SkillDef skillDef)
        {
            SkillLocator skillLocator = targetPrefab.GetComponent<SkillLocator>();

            SkillFamily skillFamily = skillLocator.secondary.skillFamily;

            Array.Resize(ref skillFamily.variants, skillFamily.variants.Length + 1);
            skillFamily.variants[skillFamily.variants.Length - 1] = new SkillFamily.Variant
            {
                skillDef = skillDef,
                unlockableName = "",
                viewableNode = new ViewablesCatalog.Node(skillDef.skillNameToken, false, null)
            };
        }

        internal static void AddUtilitySkill(GameObject targetPrefab, SkillDef skillDef)
        {
            SkillLocator skillLocator = targetPrefab.GetComponent<SkillLocator>();

            SkillFamily skillFamily = skillLocator.utility.skillFamily;

            Array.Resize(ref skillFamily.variants, skillFamily.variants.Length + 1);
            skillFamily.variants[skillFamily.variants.Length - 1] = new SkillFamily.Variant
            {
                skillDef = skillDef,
                unlockableName = "",
                viewableNode = new ViewablesCatalog.Node(skillDef.skillNameToken, false, null)
            };
        }

        internal static void AddSpecialSkill(GameObject targetPrefab, SkillDef skillDef)
        {
            SkillLocator skillLocator = targetPrefab.GetComponent<SkillLocator>();

            SkillFamily skillFamily = skillLocator.special.skillFamily;

            Array.Resize(ref skillFamily.variants, skillFamily.variants.Length + 1);
            skillFamily.variants[skillFamily.variants.Length - 1] = new SkillFamily.Variant
            {
                skillDef = skillDef,
                unlockableName = "",
                viewableNode = new ViewablesCatalog.Node(skillDef.skillNameToken, false, null)
            };
        }
    }
}

internal class SkillDefInfo
{
    public string skillName;
    public string skillNameToken;
    public string skillDescriptionToken;
    public Sprite skillIcon;

    public SerializableEntityStateType activationState;
    public string activationStateMachineName;
    public int baseMaxStock;
    public float baseRechargeInterval;
    public bool beginSkillCooldownOnSkillEnd;
    public bool canceledFromSprinting;
    public bool forceSprintDuringState;
    public bool fullRestockOnAssign;
    public InterruptPriority interruptPriority;
    public bool isBullets;
    public bool isCombatSkill;
    public bool mustKeyPress;
    public bool noSprint;
    public int rechargeStock;
    public int requiredStock;
    public float shootDelay;
    public int stockToConsume;

    public string[] keywordTokens;
}