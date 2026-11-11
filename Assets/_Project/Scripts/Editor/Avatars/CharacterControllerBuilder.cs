// 공용 Animator Controller(상태·전환·파라미터 한 벌)와 아바타별 Override Controller(클립만 교체)를 만든다.
// 이동(Idle/Walk/Run)은 코드가 섞어 바꾸고, 동작은 끝나면 Controller 전환으로 이어진다: FishCast→FishWait, FishReel↔FishReelBack, FishHook·Water→Idle.
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using VoxelMagic.Core;

namespace VoxelMagic.EditorTools
{
    public static class CharacterControllerBuilder
    {
        public const string BaseFolder = "Assets/_Project/Animations/Avatars/Base";
        public const string BaseControllerPath = BaseFolder + "/CharacterBase.controller";
        private const string PlaceholderFolder = BaseFolder + "/Placeholders";
        private const string OverrideFolder = "Assets/_Project/Animations/Avatars";
        public const string ActionSpeedParameter = "ActionSpeed";

        public const string ActionConfigPath = "Assets/_Project/Data/Config/CharacterActionConfig.asset";

        private static readonly CharacterStateId[] States = { CharacterStateId.Idle, CharacterStateId.Walk, CharacterStateId.Run };
        private static readonly CharacterActionId[] Actions =
            (CharacterActionId[])System.Enum.GetValues(typeof(CharacterActionId));

        [MenuItem("VoxelMagic/아바타/공용 Controller 다시 만들기 (모든 아바타)")]
        public static void RebuildAllMenu()
        {
            var lines = RebuildAll();
            Debug.Log("[공용 Controller]\n" + string.Join("\n", lines));
        }

        /// <summary>공용 Controller를 만들고, 모든 AvatarSetup의 동작 클립(빈 칸만)을 채운 뒤 Override를 만들어 연결한다. 복도 프리팹의 Animator도 바꾼다.</summary>
        public static List<string> RebuildAll()
        {
            var lines = new List<string>();
            var baseController = BuildBase();
            lines.Add($"공용 Controller: {BaseControllerPath} (상태 {baseController.layers[0].stateMachine.states.Length}개)");
            foreach (var setup in AvatarPipeline.ScanAvatars())
            {
                if (setup.Model == null) continue;
                FillActionClips(setup);
                var previous = setup.Controller;
                var overrideController = BuildOverride(setup, baseController);
                setup.SetRef("_controller", overrideController);
                int prefabs = Repoint(setup.CharacterPrefab, previous, overrideController);
                var empty = Actions.Where(a => setup.ActionClip(a) == null).ToArray();
                lines.Add($"{setup.name}: Override {overrideController.name}" + (prefabs > 0 ? $", 복도 프리팹 Animator 바꿈" : "") +
                          (empty.Length > 0 ? $", 빈 동작 {string.Join("·", empty)}" : ""));
            }
            AssetDatabase.SaveAssets();
            return lines;
        }

        public static AnimatorController BuildBase()
        {
            EnsureFolder(PlaceholderFolder);
            var controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(BaseControllerPath);
            if (controller == null) controller = AnimatorController.CreateAnimatorControllerAtPath(BaseControllerPath);

            // 매번 처음부터 다시 짠다 (같은 파일이라 GUID·Override 연결은 유지).
            var machine = controller.layers[0].stateMachine;
            foreach (var child in machine.states.ToArray()) machine.RemoveState(child.state);
            foreach (var parameter in controller.parameters.ToArray()) controller.RemoveParameter(parameter);

            var stateByName = new Dictionary<string, AnimatorState>();
            int column = 0;
            foreach (var state in States)
            {
                string parameter = AvatarSetup.SpeedParameter(state);
                controller.AddParameter(new AnimatorControllerParameter { name = parameter, type = AnimatorControllerParameterType.Float, defaultFloat = 1f });
                stateByName[state.ToString()] = AddState(machine, state.ToString(), parameter, new Vector3(250f, 60f * column++, 0f));
            }
            machine.defaultState = stateByName[CharacterStateId.Idle.ToString()];

            controller.AddParameter(new AnimatorControllerParameter { name = ActionSpeedParameter, type = AnimatorControllerParameterType.Float, defaultFloat = 1f });
            column = 0;
            foreach (var action in Actions)
            {
                // 기다리기는 반복 자세라 원래 속도로 (던지기 속도가 이어지지 않게).
                string speed = action == CharacterActionId.FishWait ? null : ActionSpeedParameter;
                stateByName[action.ToString()] = AddState(machine, action.ToString(), speed, new Vector3(550f, 60f * column++, 0f));
            }

            // 끝나면 저절로 이어지는 동작. 섞기 시간은 데이터(CharacterActionConfig), 올리기 왕복은 이음새가 같아 0.
            float chain = GetOrCreateActionConfig().ChainBlendSeconds;
            Chain(stateByName, CharacterActionId.FishCast.ToString(), CharacterActionId.FishWait.ToString(), chain);
            Chain(stateByName, CharacterActionId.FishReel.ToString(), CharacterActionId.FishReelBack.ToString(), 0f);
            Chain(stateByName, CharacterActionId.FishReelBack.ToString(), CharacterActionId.FishReel.ToString(), 0f);
            Chain(stateByName, CharacterActionId.FishHook.ToString(), CharacterStateId.Idle.ToString(), chain);
            Chain(stateByName, CharacterActionId.Water.ToString(), CharacterStateId.Idle.ToString(), chain);

            EditorUtility.SetDirty(controller);
            return controller;
        }

        /// <summary>공용 Controller가 있으면 그대로, 없으면 만든다.</summary>
        public static AnimatorController LoadOrBuildBase() =>
            AssetDatabase.LoadAssetAtPath<AnimatorController>(BaseControllerPath) ?? BuildBase();

        public static CharacterActionConfig GetOrCreateActionConfig()
        {
            var config = AssetDatabase.LoadAssetAtPath<CharacterActionConfig>(ActionConfigPath);
            if (config != null) return config;
            config = ScriptableObject.CreateInstance<CharacterActionConfig>();
            AssetDatabase.CreateAsset(config, ActionConfigPath);
            return config;
        }

        public static AnimatorOverrideController BuildOverride(AvatarSetup setup, AnimatorController baseController)
        {
            string path = $"{OverrideFolder}/{setup.name}.overrideController";
            var overrideController = AssetDatabase.LoadAssetAtPath<AnimatorOverrideController>(path);
            if (overrideController == null)
            {
                overrideController = new AnimatorOverrideController(baseController);
                AssetDatabase.CreateAsset(overrideController, path);
            }
            overrideController.runtimeAnimatorController = baseController;

            var pairs = new List<KeyValuePair<AnimationClip, AnimationClip>>();
            foreach (var state in States) pairs.Add(new KeyValuePair<AnimationClip, AnimationClip>(Placeholder(state.ToString()), setup.ClipFor(state)));
            foreach (var action in Actions) pairs.Add(new KeyValuePair<AnimationClip, AnimationClip>(Placeholder(action.ToString()), setup.ActionClip(action)));
            overrideController.ApplyOverrides(pairs);
            EditorUtility.SetDirty(overrideController);
            return overrideController;
        }

        /// <summary>비어 있는 동작 칸만 동작 클립 준비 결과(이름 규칙)로 채운다. Humanoid라 모든 아바타가 같은 클립을 쓸 수 있다.</summary>
        public static void FillActionClips(AvatarSetup setup)
        {
            var map = new Dictionary<CharacterActionId, string>
            {
                { CharacterActionId.FishCast, "Fishing Cast_Throw" },
                { CharacterActionId.FishWait, "Fishing Idle" },
                { CharacterActionId.FishReel, "Fishing Cast_Reel" },
                { CharacterActionId.FishReelBack, "Fishing Cast_Reel" + ActionClipTools.ReversedSuffix },
                { CharacterActionId.FishHook, "Fishing Cast_Throw" + ActionClipTools.ReversedSuffix },
                { CharacterActionId.Water, "Watering" },
            };
            var clips = AvatarPipeline.ScanClips().Where(c => c.isHuman).Select(c => c.clip).ToList();
            var so = new SerializedObject(setup);
            foreach (var pair in map)
            {
                var property = so.FindProperty(FieldOf(pair.Key));
                if (property.objectReferenceValue != null) continue;
                property.objectReferenceValue = clips.FirstOrDefault(c => c.name == pair.Value);
            }
            so.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(setup);
        }

        private static string FieldOf(CharacterActionId action) => "_" + char.ToLowerInvariant(action.ToString()[0]) + action.ToString().Substring(1);

        // 프리팹 안 Animator가 예전 Controller를 쓰면 새 Override로 바꾼다.
        private static int Repoint(GameObject prefab, RuntimeAnimatorController previous, RuntimeAnimatorController next)
        {
            if (prefab == null) return 0;
            string path = AssetDatabase.GetAssetPath(prefab);
            var root = PrefabUtility.LoadPrefabContents(path);
            int changed = 0;
            foreach (var animator in root.GetComponentsInChildren<Animator>(true))
            {
                if (animator.runtimeAnimatorController == next) continue;
                if (previous != null && animator.runtimeAnimatorController != previous) continue;
                animator.runtimeAnimatorController = next;
                changed++;
            }
            if (changed > 0) PrefabUtility.SaveAsPrefabAsset(root, path);
            PrefabUtility.UnloadPrefabContents(root);
            return changed;
        }

        private static AnimatorState AddState(AnimatorStateMachine machine, string name, string speedParameter, Vector3 position)
        {
            var state = machine.AddState(name, position);
            state.motion = Placeholder(name);
            if (speedParameter != null)
            {
                state.speedParameterActive = true;
                state.speedParameter = speedParameter;
            }
            state.writeDefaultValues = true;
            return state;
        }

        private static void Chain(Dictionary<string, AnimatorState> states, string from, string to, float blendSeconds)
        {
            var transition = states[from].AddTransition(states[to]);
            transition.hasExitTime = true;
            transition.exitTime = 1f;
            transition.hasFixedDuration = true;
            transition.duration = blendSeconds;
        }

        // Override의 열쇠가 되는 빈 클립 (상태 하나에 하나, 이름 = 상태 이름).
        private static AnimationClip Placeholder(string name)
        {
            string path = $"{PlaceholderFolder}/{name}.anim";
            var clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(path);
            if (clip != null) return clip;
            clip = new AnimationClip { name = name };
            AssetDatabase.CreateAsset(clip, path);
            return clip;
        }

        private static void EnsureFolder(string folder)
        {
            if (AssetDatabase.IsValidFolder(folder)) return;
            var parent = Path.GetDirectoryName(folder).Replace('\\', '/');
            EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, Path.GetFileName(folder));
        }
    }
}
