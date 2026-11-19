// 저장된 ID를 다시 에셋으로 찾는 표: 아이템(씨앗·작물·물고기·도구 포함)과 의뢰 목록. 에디터 메뉴가 프로젝트를 훑어 채운다.
using System.Collections.Generic;
using UnityEngine;

namespace VoxelMagic.Core
{
    [CreateAssetMenu(menuName = "VoxelMagic/Save/Definition Catalog", fileName = "DefinitionCatalog")]
    public sealed class DefinitionCatalog : ScriptableObject
    {
        [SerializeField] private List<ItemDefinition> _items = new List<ItemDefinition>();
        [SerializeField] private List<RequestDefinition> _requests = new List<RequestDefinition>();

        private Dictionary<string, ItemDefinition> _itemById;
        private Dictionary<string, RequestDefinition> _requestById;

        public IReadOnlyList<ItemDefinition> Items => _items;
        public IReadOnlyList<RequestDefinition> Requests => _requests;

        /// <summary>의뢰는 별도 ID 필드가 없어 에셋 이름을 ID로 쓴다.</summary>
        public static string IdOf(RequestDefinition request) => request != null ? request.name : null;

        public bool TryGetItem(string id, out ItemDefinition item)
        {
            item = null;
            if (string.IsNullOrEmpty(id)) return false;
            if (_itemById == null) _itemById = Build(_items, i => i.Id);
            return _itemById.TryGetValue(id, out item);
        }

        public bool TryGetRequest(string id, out RequestDefinition request)
        {
            request = null;
            if (string.IsNullOrEmpty(id)) return false;
            if (_requestById == null) _requestById = Build(_requests, IdOf);
            return _requestById.TryGetValue(id, out request);
        }

        /// <summary>코드·에디터에서 목록을 채운다 (찾기 표는 다시 만든다).</summary>
        public void SetEntries(IEnumerable<ItemDefinition> items, IEnumerable<RequestDefinition> requests)
        {
            _items = new List<ItemDefinition>(items);
            _requests = new List<RequestDefinition>(requests);
            _itemById = null;
            _requestById = null;
        }

        private void OnValidate()
        {
            _itemById = null;
            _requestById = null;
        }

        private static Dictionary<string, T> Build<T>(List<T> list, System.Func<T, string> idOf) where T : Object
        {
            var map = new Dictionary<string, T>();
            foreach (var entry in list)
            {
                if (entry == null) continue;
                string id = idOf(entry);
                if (!map.ContainsKey(id)) map.Add(id, entry);
                else Debug.LogWarning($"[DefinitionCatalog] ID '{id}'가 겹칩니다: {entry.name}");
            }
            return map;
        }
    }
}
