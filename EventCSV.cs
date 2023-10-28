using System.Collections.Generic;
using System.Linq;
using UnityEngine;

using Object = UnityEngine.Object;

namespace Snorlax
{
    [CreateAssetMenu(menuName = "Snorlax's Tools/Animation/EventCSV")]
    public class EventCSV : ScriptableObject
    {
        public TextAsset textAssetData;

        public List<string> Headers = new List<string>();

        public List<AnimationData> AnimationData = new List<AnimationData>();
    }
}
