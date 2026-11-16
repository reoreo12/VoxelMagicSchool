// 애니메이션 랩이 에디터 기능(파일·장착물 검색·Humanoid 임포트)을 쓰는 통로. 에디터 어셈블리가 연결하고, 빌드에서는 비어 있다.
using System;
using System.Collections.Generic;
using UnityEngine;
using VoxelMagic.Core;

namespace VoxelMagic.Tools
{
    public readonly struct LabClip
    {
        public LabClip(string path, AnimationClip clip, bool isHuman)
        {
            Path = path;
            Clip = clip;
            IsHuman = isHuman;
        }

        public string Path { get; }
        public AnimationClip Clip { get; }
        public bool IsHuman { get; }

        /// <summary>모델 파일(.fbx)에 든 클립만 Humanoid로 다시 임포트할 수 있다. .anim은 만든 대상 전용.</summary>
        public bool CanImport => !IsHuman && Path.EndsWith(".fbx", StringComparison.OrdinalIgnoreCase);
    }

    public static class AnimationLabBridge
    {
        public static Func<IReadOnlyList<LabClip>> ScanClips;
        /// <summary>크리처(Generic 리그) 전용 클립 (Art/Creatures 아래). Humanoid 목록과 따로 보여 준다.</summary>
        public static Func<IReadOnlyList<LabClip>> ScanCreatureClips;
        public static Func<IReadOnlyList<AvatarSetup>> ScanAvatars;
        public static Func<string, bool> ImportAsHumanoid;
        /// <summary>아직 아바타 데이터가 없는 캐릭터 모델을 준비하고, 새로 준비한 아바타 이름을 돌려준다.</summary>
        public static Func<IReadOnlyList<string>> PrepareNewModels;
        /// <summary>프로젝트의 모든 장착물 정의 (장착물 패널 목록).</summary>
        public static Func<IReadOnlyList<AttachmentDefinition>> ScanAttachments;

        public static bool IsConnected => ScanClips != null;
    }
}
