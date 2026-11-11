// 동작 클립 준비 도구: ClipImportPlan 표대로 애니메이션 FBX를 Humanoid로 다시 임포트하며 프레임 구간별 클립으로 자르고,
// 필요한 구간은 거꾸로 뒤집은 .anim(Animations/Mixamo/Reversed/)을 만든다. 뒤집은 클립은 정방향 재생이라 Animator 전환이 단순해진다.
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace VoxelMagic.EditorTools
{
    public static class ActionClipTools
    {
        public const string PlanPath = "Assets/_Project/Data/Avatars/ClipImportPlan.asset";
        public const string ReversedFolder = "Assets/_Project/Animations/Mixamo/Reversed";
        public const string ReversedSuffix = "_Reverse";
        private const string MixamoFolder = "Assets/_Project/Animations/Mixamo";

        [MenuItem("VoxelMagic/아바타/동작 클립 준비 (ClipImportPlan)")]
        public static void PrepareFromMenu()
        {
            var plan = GetOrCreatePlan();
            var report = Prepare(plan);
            Selection.activeObject = plan;
            Debug.Log("[동작 클립] " + string.Join("\n", report), plan);
        }

        /// <summary>표대로 임포트·자르기·뒤집기. 결과 줄 목록을 돌려준다.</summary>
        public static List<string> Prepare(ClipImportPlan plan)
        {
            var report = new List<string>();
            foreach (var source in plan.Sources)
            {
                if (source.File == null) continue;
                string path = AssetDatabase.GetAssetPath(source.File);
                if (!(AssetImporter.GetAtPath(path) is ModelImporter importer))
                {
                    report.Add($"{path}: 모델 파일이 아님 — 건너뜀");
                    continue;
                }
                int totalFrames = TotalFrames(importer, out string takeName);
                importer.animationType = ModelImporterAnimationType.Human;
                importer.avatarSetup = ModelImporterAvatarSetup.CreateFromThisModel;
                importer.importAnimation = true;
                importer.clipAnimations = source.Segments.Select(s => ToClip(s, totalFrames, takeName)).ToArray();
                importer.SaveAndReimport();

                var clips = AssetDatabase.LoadAllAssetsAtPath(path).OfType<AnimationClip>()
                    .Where(c => !c.name.StartsWith("__preview__")).ToDictionary(c => c.name);
                foreach (var segment in source.Segments)
                {
                    if (!clips.TryGetValue(segment.Name, out var clip))
                    {
                        report.Add($"{segment.Name}: 클립을 만들지 못함");
                        continue;
                    }
                    string line = $"{segment.Name}: {clip.length:0.00}초, Humanoid={clip.humanMotion}, 반복={segment.Loop}";
                    if (segment.MakeReversed) line += $" → 뒤집은 클립 {CreateReversed(clip).name}";
                    report.Add(line);
                }
            }
            AssetDatabase.SaveAssets();
            return report;
        }

        /// <summary>시간을 거꾸로 한 복사본을 만든다 (Humanoid 근육·루트 곡선 포함). 이미 있으면 덮어쓴다.</summary>
        public static AnimationClip CreateReversed(AnimationClip source)
        {
            EnsureFolder(ReversedFolder);
            string path = $"{ReversedFolder}/{source.name}{ReversedSuffix}.anim";
            var reversed = AssetDatabase.LoadAssetAtPath<AnimationClip>(path);
            bool isNew = reversed == null;
            if (isNew) reversed = new AnimationClip();
            else reversed.ClearCurves();
            reversed.name = source.name + ReversedSuffix;
            reversed.frameRate = source.frameRate;

            float length = source.length;
            foreach (var binding in AnimationUtility.GetCurveBindings(source))
            {
                var curve = AnimationUtility.GetEditorCurve(source, binding);
                var keys = curve.keys;
                var flipped = new Keyframe[keys.Length];
                for (int i = 0; i < keys.Length; i++)
                {
                    var k = keys[keys.Length - 1 - i];
                    flipped[i] = new Keyframe(length - k.time, k.value, -k.outTangent, -k.inTangent, k.outWeight, k.inWeight)
                        { weightedMode = Swap(k.weightedMode) };
                }
                AnimationUtility.SetEditorCurve(reversed, binding, new AnimationCurve(flipped));
            }
            var settings = AnimationUtility.GetAnimationClipSettings(source);
            settings.cycleOffset = 0f;
            AnimationUtility.SetAnimationClipSettings(reversed, settings);

            if (isNew) AssetDatabase.CreateAsset(reversed, path);
            else EditorUtility.SetDirty(reversed);
            return reversed;
        }

        /// <summary>표 에셋. 없으면 지금 Mixamo 폴더의 낚시·물 주기 파일로 처음 표를 만든다 (2026-11-10 사용자 지정 구간).</summary>
        public static ClipImportPlan GetOrCreatePlan()
        {
            var plan = AssetDatabase.LoadAssetAtPath<ClipImportPlan>(PlanPath);
            if (plan != null) return plan;
            plan = ScriptableObject.CreateInstance<ClipImportPlan>();
            GameObject File(string name) => AssetDatabase.LoadAssetAtPath<GameObject>($"{MixamoFolder}/Y Bot@{name}.fbx");
            plan.SetSources(new[]
            {
                // 던지기 = 0~120, 입질 후 올리기 = 120~끝(왕복 반복은 뒤집은 클립과 번갈아), 낚아채기 = 던지기를 거꾸로
                new ClipImportPlan.Source(File("Fishing Cast"),
                    new ClipImportPlan.Segment("Fishing Cast_Throw", 0, 120, false, true),
                    new ClipImportPlan.Segment("Fishing Cast_Reel", 120, -1, false, true)),
                new ClipImportPlan.Source(File("Fishing Idle"),
                    new ClipImportPlan.Segment("Fishing Idle", 0, -1, true, false)),
                new ClipImportPlan.Source(File("Watering"),
                    new ClipImportPlan.Segment("Watering", 0, -1, false, false)),
            });
            AssetDatabase.CreateAsset(plan, PlanPath);
            return plan;
        }

        // 메뉴 2(ImportAnimationAsHumanoid)와 같은 루트 설정: 제자리, 원래 방향·높이 유지.
        private static ModelImporterClipAnimation ToClip(ClipImportPlan.Segment segment, int totalFrames, string takeName) => new ModelImporterClipAnimation
        {
            name = segment.Name,
            takeName = takeName,
            firstFrame = segment.FirstFrame,
            lastFrame = segment.LastFrame < 0 ? totalFrames : segment.LastFrame,
            loopTime = segment.Loop,
            loopPose = segment.Loop,
            lockRootRotation = true,
            lockRootHeightY = true,
            lockRootPositionXZ = true,
            keepOriginalOrientation = true,
            keepOriginalPositionY = true,
            keepOriginalPositionXZ = true,
            events = segment.Events
                .Where(e => !string.IsNullOrEmpty(e.Id))
                .Select(e => new AnimationEvent
                    { functionName = VoxelMagic.Gameplay.CharacterAnimationEvents.FunctionName, stringParameter = e.Id, time = e.NormalizedTime })
                .ToArray()
        };

        private static int TotalFrames(ModelImporter importer, out string takeName)
        {
            takeName = null;
            var takes = importer.importedTakeInfos;
            if (takes == null || takes.Length == 0) return 0;
            var take = takes[0];
            takeName = take.name;
            return Mathf.RoundToInt((take.stopTime - take.startTime) * take.sampleRate);
        }

        private static WeightedMode Swap(WeightedMode mode) =>
            mode == WeightedMode.In ? WeightedMode.Out : mode == WeightedMode.Out ? WeightedMode.In : mode;

        private static void EnsureFolder(string folder)
        {
            if (AssetDatabase.IsValidFolder(folder)) return;
            var parent = Path.GetDirectoryName(folder).Replace('\\', '/');
            EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, Path.GetFileName(folder));
        }
    }
}
