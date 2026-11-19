// 에디터가 켜질 때 애니메이션 랩의 통로(AnimationLabBridge)에 파일·장착물 검색·Humanoid 임포트 기능을 연결한다.
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using VoxelMagic.Core;
using VoxelMagic.EditorTools;

namespace VoxelMagic.Tools.EditorTools
{
    [InitializeOnLoad]
    internal static class AnimationLabEditorBridge
    {
        static AnimationLabEditorBridge()
        {
            AnimationLabBridge.ScanClips = () =>
                AvatarPipeline.ScanClips().Select(c => new LabClip(c.path, c.clip, c.isHuman)).ToList();
            AnimationLabBridge.ScanAvatars = AvatarPipeline.ScanAvatars;
            AnimationLabBridge.ScanCreatureClips = ScanCreatureClips;
            AnimationLabBridge.ImportAsHumanoid = AvatarPipeline.ImportAnimationAsHumanoid;
            AnimationLabBridge.PrepareNewModels = () => AvatarPipeline.PrepareNewModels().Select(s => s.name).ToList();
            AnimationLabBridge.ScanAttachments = ScanAttachments;
        }

        internal static IReadOnlyList<LabClip> ScanCreatureClips()
        {
            var result = new List<LabClip>();
            foreach (var path in AssetDatabase.FindAssets("t:AnimationClip", new[] { "Assets/_Project/Art/Creatures" })
                .Select(AssetDatabase.GUIDToAssetPath).Distinct().OrderBy(p => p))
            {
                if (!path.EndsWith(".anim")) continue;
                var clip = AssetDatabase.LoadAssetAtPath<UnityEngine.AnimationClip>(path);
                if (clip != null) result.Add(new LabClip(path, clip, false));
            }
            return result;
        }

        internal static IReadOnlyList<AttachmentDefinition> ScanAttachments() =>
            AssetDatabase.FindAssets("t:" + nameof(AttachmentDefinition))
                .Select(guid => AssetDatabase.LoadAssetAtPath<AttachmentDefinition>(AssetDatabase.GUIDToAssetPath(guid)))
                .Where(a => a != null)
                .OrderBy(a => a.DisplayName)
                .ToList();
    }
}
