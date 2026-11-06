// 에디터가 켜질 때 애니메이션 랩의 통로(AnimationLabBridge)에 파일 검색·Humanoid 임포트 기능을 연결한다.
using System.Linq;
using UnityEditor;
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
            AnimationLabBridge.ImportAsHumanoid = AvatarPipeline.ImportAnimationAsHumanoid;
            AnimationLabBridge.PrepareNewModels = () => AvatarPipeline.PrepareNewModels().Select(s => s.name).ToList();
        }
    }
}
