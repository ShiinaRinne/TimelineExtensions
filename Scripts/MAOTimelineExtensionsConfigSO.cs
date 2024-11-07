using UnityEngine;
using UnityEngine.Serialization;

namespace MAOTimelineExtension
{
    [CreateAssetMenu(fileName = "MAOTimelineExtensionsConfigSO", menuName = "MAOTimelineExtensions/ConfigSO", order = 0)]
    public class MAOTimelineExtensionsConfigSO : ScriptableObject
    {
        public string rootFolderPath = "Assets/TimelineExtensions";
        public string defaultNameSpace = "MAOTimelineExtension.VolumeExtensions";
    }
}