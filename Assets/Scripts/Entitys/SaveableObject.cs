using System.Collections.Generic;
using Tools;
using UnityEngine;

namespace Entitys
{
    public class SaveableObject : MonoBehaviour
    {
        public byte[] Serialize()
        {
            ISaveable[] saves = GetComponents<ISaveable>();
            Vector3 pos = transform.position;
            Vector3 euler = transform.eulerAngles;

            BytesIO writer = new BytesIO();
            writer.Set(pos).Set(euler);

            for (int i = 0; i < saves.Length; ++i)
            {
                writer.Set(saves[i].OnSave());
            }

            return writer.ToBytes();
        }

        public void Deserialize(byte[] data)
        {
            ISaveable[] saves = GetComponents<ISaveable>();
            BytesIO reader = new BytesIO(data);
            Vector3 pos = reader.GetVector3();
            Vector3 euler = reader.GetVector3();

            transform.position = pos;
            transform.eulerAngles = euler;

            for (int i = 0; i < saves.Length; ++i)
            {
                ISaveable isa = saves[i];
                byte[] bytes = reader.GetBytes();
                isa.OnLoad(bytes);
            }
        }
    }
}
