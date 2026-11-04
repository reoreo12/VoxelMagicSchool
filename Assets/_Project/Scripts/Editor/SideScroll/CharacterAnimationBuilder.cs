// CharacterAnimationConfig 값으로 Idle/Walk/Run 반복 클립과 Animator Controller를 만든다 (있으면 같은 에셋을 갱신).
// Controller에는 상태만 있고 전이·파라미터는 없다. 어떤 상태를 재생할지는 코드의 상태 머신이 정한다.
using System;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using VoxelMagic.Core;

namespace VoxelMagic.EditorTools
{
    public static class CharacterAnimationBuilder
    {
        private const string Folder = "Assets/_Project/Animations/SideScroll";
        private const string ControllerPath = Folder + "/SideScrollCharacter.controller";
        private static readonly CharacterStateId[] States = { CharacterStateId.Idle, CharacterStateId.Walk, CharacterStateId.Run };

        [MenuItem("VoxelMagic/복도/캐릭터 애니메이션 다시 만들기")]
        private static void BuildFromMenu()
        {
            // 프리팹에서 몸통 높이를 고쳤을 수 있으니 프리팹의 Body 위치를 기준으로 굽는다.
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(SideScrollCharacterBuilder.PrefabPath);
            var body = prefab != null ? prefab.transform.Find("Model/" + SideScrollCharacterBuilder.BodyPath) : null;
            if (body == null)
            {
                Debug.LogError("[CharacterAnimationBuilder] 캐릭터 프리팹(Model/Body)이 없습니다. 먼저 프리팹을 만드세요.");
                return;
            }
            Build(CorridorSceneBuilder.LoadOrCreateAnimationConfig(), body.localPosition.y);
        }

        public static RuntimeAnimatorController Build(CharacterAnimationConfig config, float bodyBaseY)
        {
            if (!AssetDatabase.IsValidFolder("Assets/_Project/Animations"))
                AssetDatabase.CreateFolder("Assets/_Project", "Animations");
            if (!AssetDatabase.IsValidFolder(Folder))
                AssetDatabase.CreateFolder("Assets/_Project/Animations", "SideScroll");

            var controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(ControllerPath)
                             ?? AnimatorController.CreateAnimatorControllerAtPath(ControllerPath);
            var machine = controller.layers[0].stateMachine;

            foreach (var state in States)
            {
                var clip = LoadOrCreateClip(state.ToString());
                Fill(clip, config.For(state), config.KeysPerCycle, bodyBaseY, state == CharacterStateId.Idle);

                var animatorState = Array.Find(machine.states, s => s.state.name == state.ToString()).state
                                    ?? machine.AddState(state.ToString());
                animatorState.motion = clip;
                if (state == CharacterStateId.Idle) machine.defaultState = animatorState;
            }
            EditorUtility.SetDirty(controller);
            AssetDatabase.SaveAssets();
            return controller;
        }

        private static AnimationClip LoadOrCreateClip(string name)
        {
            string path = $"{Folder}/{name}.anim";
            var clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(path);
            if (clip != null) return clip;
            clip = new AnimationClip { name = name };
            AssetDatabase.CreateAsset(clip, path);
            return clip;
        }

        // 팔은 같은 쪽 다리와 반대로, 왼쪽·오른쪽도 반대로 흔든다. Idle은 한 주기에 한 번, 걷기·달리기는 한 걸음마다 몸통이 오르내린다.
        private static void Fill(AnimationClip clip, LimbMotion motion, int keys, float bodyBaseY, bool breathe)
        {
            clip.ClearCurves();
            float period = motion.CycleSeconds;
            float omega = 2f * Mathf.PI / period;

            Func<float, float> swing = t => motion.SwingDegrees * Mathf.Sin(omega * t);
            SetRotationX(clip, SideScrollCharacterBuilder.ArmPaths[0], Sample(keys, period, swing));
            SetRotationX(clip, SideScrollCharacterBuilder.ArmPaths[1], Sample(keys, period, t => -swing(t)));
            SetRotationX(clip, SideScrollCharacterBuilder.LegPaths[0], Sample(keys, period, t => breathe ? 0f : -swing(t)));
            SetRotationX(clip, SideScrollCharacterBuilder.LegPaths[1], Sample(keys, period, t => breathe ? 0f : swing(t)));

            Func<float, float> bob = breathe
                ? t => bodyBaseY + motion.Bob * Mathf.Sin(omega * t)
                : t => bodyBaseY + motion.Bob * Mathf.Cos(2f * omega * t);
            string body = SideScrollCharacterBuilder.BodyPath;
            SetCurve(clip, body, "m_LocalPosition.x", AnimationCurve.Constant(0f, period, 0f));
            SetCurve(clip, body, "m_LocalPosition.y", Sample(keys, period, bob));
            SetCurve(clip, body, "m_LocalPosition.z", AnimationCurve.Constant(0f, period, 0f));
            SetRotationX(clip, body, AnimationCurve.Constant(0f, period, motion.LeanDegrees));

            var settings = AnimationUtility.GetAnimationClipSettings(clip);
            settings.loopTime = true;
            AnimationUtility.SetAnimationClipSettings(clip, settings);
            EditorUtility.SetDirty(clip);
        }

        private static AnimationCurve Sample(int keys, float period, Func<float, float> value)
        {
            var curve = new AnimationCurve();
            for (int i = 0; i <= keys; i++)
            {
                float t = period * i / keys;
                curve.AddKey(t, value(t));
            }
            for (int i = 0; i < curve.length; i++) curve.SmoothTangents(i, 0f);
            return curve;
        }

        private static void SetRotationX(AnimationClip clip, string path, AnimationCurve x)
        {
            float end = x.keys[x.length - 1].time;
            SetCurve(clip, path, "localEulerAnglesRaw.x", x);
            SetCurve(clip, path, "localEulerAnglesRaw.y", AnimationCurve.Constant(0f, end, 0f));
            SetCurve(clip, path, "localEulerAnglesRaw.z", AnimationCurve.Constant(0f, end, 0f));
        }

        private static void SetCurve(AnimationClip clip, string path, string property, AnimationCurve curve) =>
            AnimationUtility.SetEditorCurve(clip, EditorCurveBinding.FloatCurve(path, typeof(Transform), property), curve);
    }
}
