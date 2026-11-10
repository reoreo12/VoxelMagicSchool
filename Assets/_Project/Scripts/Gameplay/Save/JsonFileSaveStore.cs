// 저장 파일을 Application.persistentDataPath/Saves/save_<슬롯>.json에 읽고 쓴다.
// 임시 파일에 다 쓴 뒤 바꿔치기해서, 쓰는 도중 게임이 꺼져도 이전 저장이 남게 한다.
using System.IO;
using UnityEngine;
using VoxelMagic.Core;

namespace VoxelMagic.Gameplay
{
    public sealed class JsonFileSaveStore : ISaveStore
    {
        private const string FolderName = "Saves";

        public static string Folder => Path.Combine(Application.persistentDataPath, FolderName);

        public static string PathOf(int slot) => Path.Combine(Folder, $"save_{slot}.json");

        public bool Exists(int slot) => File.Exists(PathOf(slot));

        public bool TryRead(int slot, out string json)
        {
            json = null;
            string path = PathOf(slot);
            if (!File.Exists(path)) return false;
            try
            {
                json = File.ReadAllText(path);
                return !string.IsNullOrEmpty(json);
            }
            catch (IOException e)
            {
                Debug.LogError($"[저장] '{path}'를 읽지 못했습니다: {e.Message}");
                return false;
            }
        }

        public void Write(int slot, string json)
        {
            Directory.CreateDirectory(Folder);
            string path = PathOf(slot);
            string temp = path + ".tmp";
            File.WriteAllText(temp, json);
            if (File.Exists(path)) File.Replace(temp, path, null);
            else File.Move(temp, path);
        }

        public void Delete(int slot)
        {
            string path = PathOf(slot);
            if (File.Exists(path)) File.Delete(path);
        }
    }
}
