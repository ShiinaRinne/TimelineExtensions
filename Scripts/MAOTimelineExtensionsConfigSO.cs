using UnityEngine;
using UnityEngine.Serialization;

namespace MAOTimelineExtension
{
    [CreateAssetMenu(fileName = "MAOTimelineExtensionsConfigSO", menuName = "MAOTimelineExtensions/ConfigSO", order = 0)]
    public class MAOTimelineExtensionsConfigSO : ScriptableObject
    {
        [FormerlySerializedAs("SavePath")] public string RootFolderPath = "Assets/TimelineExtensions";
        public string DefaultNameSpace = "MAOTimelineExtension.VolumeExtensions";
    }
}