// 뒷산 전용 서비스 묶음 (시간대·채집·디버그 키 목록). GameContext는 그대로 두고 뒷산 부트스트랩이 따로 만들어 나눠 준다.
using System;
using System.Collections.Generic;

namespace VoxelMagic.Core
{
    public interface IMountainContextBinder
    {
        void Bind(MountainContext context);
    }

    public sealed class MountainContext
    {
        public MountainContext(GameContext game, TimeOfDayState time, GatheringService gathering, DebugCommandList debug)
        {
            Game = game;
            Time = time;
            Gathering = gathering;
            Debug = debug;
        }

        public GameContext Game { get; }
        public TimeOfDayState Time { get; }
        public GatheringService Gathering { get; }
        public DebugCommandList Debug { get; }
    }

    public readonly struct DebugCommandInfo
    {
        public DebugCommandInfo(string key, string label)
        {
            Key = key;
            Label = label;
        }

        public string Key { get; }
        public string Label { get; }
    }

    /// <summary>화면 구석 안내에 띄울 디버그 키 목록. 키를 읽는 쪽이 등록하고 UI가 그린다.</summary>
    public sealed class DebugCommandList
    {
        private readonly List<DebugCommandInfo> _commands = new List<DebugCommandInfo>();

        public event Action Changed;

        public IReadOnlyList<DebugCommandInfo> Commands => _commands;

        public void Register(string key, string label)
        {
            _commands.Add(new DebugCommandInfo(key, label));
            Changed?.Invoke();
        }
    }
}
