#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace BattleZoneMobile.Editor
{
    public static class BattleZoneMilestoneAssetGenerator
    {
        private const string AnimationFolder = "Assets/BattleZoneMobile/Animations";
        private const string ResourcesFolder = "Assets/BattleZoneMobile/Resources";
        private const string ControllerPath = ResourcesFolder + "/AC_PlayerHumanoid.controller";

        public static void Generate()
        {
            Directory.CreateDirectory(AnimationFolder);
            Directory.CreateDirectory(ResourcesFolder);

            AnimationClip idle = CreateClip("AN_Player_Idle", 1.2f);
            AnimationClip walk = CreateClip("AN_Player_Walk", 0.8f);
            AnimationClip run = CreateClip("AN_Player_Run", 0.55f);
            AnimationClip jump = CreateClip("AN_Player_Jump", 0.75f);
            AnimationClip crouch = CreateClip("AN_Player_Crouch", 0.8f);

            AnimatorController controller = AnimatorController.CreateAnimatorControllerAtPath(ControllerPath);
            controller.AddParameter("Speed", AnimatorControllerParameterType.Float);
            controller.AddParameter("Grounded", AnimatorControllerParameterType.Bool);
            controller.AddParameter("Crouching", AnimatorControllerParameterType.Bool);

            AnimatorStateMachine stateMachine = controller.layers[0].stateMachine;
            stateMachine.states = new ChildAnimatorState[0];

            AnimatorState idleState = stateMachine.AddState("Idle");
            AnimatorState walkState = stateMachine.AddState("Walk");
            AnimatorState runState = stateMachine.AddState("Run");
            AnimatorState jumpState = stateMachine.AddState("Jump");
            AnimatorState crouchState = stateMachine.AddState("Crouch");

            idleState.motion = idle;
            walkState.motion = walk;
            runState.motion = run;
            jumpState.motion = jump;
            crouchState.motion = crouch;
            stateMachine.defaultState = idleState;

            AddSpeedTransition(idleState, walkState, 0.15f, 5.5f);
            AddSpeedTransition(walkState, runState, 5.5f, 99f);
            AddSpeedTransition(runState, walkState, 0.15f, 5.5f);
            AddSpeedTransition(walkState, idleState, -1f, 0.15f);
            AddBoolTransition(idleState, jumpState, "Grounded", false);
            AddBoolTransition(walkState, jumpState, "Grounded", false);
            AddBoolTransition(runState, jumpState, "Grounded", false);
            AddBoolTransition(jumpState, idleState, "Grounded", true);
            AddBoolTransition(idleState, crouchState, "Crouching", true);
            AddBoolTransition(walkState, crouchState, "Crouching", true);
            AddBoolTransition(crouchState, idleState, "Crouching", false);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("BattleZone animation assets generated.");
            EditorApplication.Exit(0);
        }

        private static AnimationClip CreateClip(string clipName, float length)
        {
            string path = $"{AnimationFolder}/{clipName}.anim";
            AnimationClip clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(path);
            if (clip == null)
            {
                clip = new AnimationClip();
                AssetDatabase.CreateAsset(clip, path);
            }

            clip.frameRate = 30f;
            AnimationClipSettings settings = AnimationUtility.GetAnimationClipSettings(clip);
            settings.loopTime = clipName != "AN_Player_Jump";
            AnimationUtility.SetAnimationClipSettings(clip, settings);

            AnimationCurve placeholder = AnimationCurve.Linear(0f, 0f, length, 0f);
            clip.SetCurve("", typeof(Transform), "localPosition.x", placeholder);
            EditorUtility.SetDirty(clip);
            return clip;
        }

        private static void AddSpeedTransition(AnimatorState from, AnimatorState to, float minExclusive, float maxInclusive)
        {
            AnimatorStateTransition transition = from.AddTransition(to);
            transition.hasExitTime = false;
            transition.duration = 0.08f;

            if (minExclusive >= 0f)
            {
                transition.AddCondition(AnimatorConditionMode.Greater, minExclusive, "Speed");
            }

            if (maxInclusive < 99f)
            {
                transition.AddCondition(AnimatorConditionMode.Less, maxInclusive, "Speed");
            }
        }

        private static void AddBoolTransition(AnimatorState from, AnimatorState to, string parameter, bool value)
        {
            AnimatorStateTransition transition = from.AddTransition(to);
            transition.hasExitTime = false;
            transition.duration = 0.08f;
            transition.AddCondition(value ? AnimatorConditionMode.If : AnimatorConditionMode.IfNot, 0f, parameter);
        }
    }
}
#endif
