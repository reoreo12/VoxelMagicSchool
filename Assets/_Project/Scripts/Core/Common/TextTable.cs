// 화면 문구 표(키 → 한국어 문장, UI_SPEC 4-A-3·13장). UI는 문자열을 직접 넣지 않고 키로 가져온다.
// 다국어가 필요해지면 ITextSource 구현만 바꾼다. 없는 키는 "#키"로 보여 빠진 문구가 눈에 띄게 한다.
using System;
using System.Collections.Generic;
using UnityEngine;

namespace VoxelMagic.Core
{
    public interface ITextSource
    {
        string Get(string key);
        bool TryGet(string key, out string text);
    }

    [CreateAssetMenu(menuName = "VoxelMagic/UI/문구 표", fileName = "TextTable")]
    public sealed class TextTable : ScriptableObject, ITextSource
    {
        [Serializable]
        public struct Entry
        {
            [SerializeField] private string _key;
            [SerializeField, TextArea] private string _text;

            public Entry(string key, string text)
            {
                _key = key;
                _text = text;
            }

            public string Key => _key;
            public string Text => _text;
        }

        [SerializeField, Tooltip("키 이름 예: ui.book.tab.spell, quest.list.title")] private List<Entry> _entries = new List<Entry>();

        private Dictionary<string, string> _lookup;

        /// <summary>문구가 바뀌었을 때 (에디터에서 고쳐도 화면이 바로 다시 그려지게).</summary>
        public event Action Changed;

        public IReadOnlyList<Entry> Entries => _entries;

        public string Get(string key) => TryGet(key, out var text) ? text : "#" + key;

        public bool TryGet(string key, out string text)
        {
            if (_lookup == null) BuildLookup();
            text = null;
            return key != null && _lookup.TryGetValue(key, out text);
        }

        /// <summary>에디터 도구가 문구를 추가·수정할 때 쓴다. 같은 키가 있으면 바꾼다.</summary>
        public void Set(string key, string text)
        {
            int index = _entries.FindIndex(e => e.Key == key);
            if (index >= 0) _entries[index] = new Entry(key, text);
            else _entries.Add(new Entry(key, text));
            _lookup = null;
            Changed?.Invoke();
        }

        private void OnValidate()
        {
            _lookup = null;
            Changed?.Invoke();
        }

        private void BuildLookup()
        {
            _lookup = new Dictionary<string, string>(_entries.Count);
            foreach (var entry in _entries)
            {
                if (string.IsNullOrEmpty(entry.Key)) continue;
                if (_lookup.ContainsKey(entry.Key)) Debug.LogWarning($"[TextTable] 키가 중복됩니다: {entry.Key} ({name})");
                _lookup[entry.Key] = entry.Text;
            }
        }
    }
}
