using UnityEngine;
using UnityEngine.Rendering;

namespace MAOTimelineExtension
{
    [ExecuteAlways]
    public class TimelineExtensionVolumeSettings : MonoBehaviour
    {
        // https://docs.unity3d.com/Packages/com.unity.render-pipelines.high-definition@16.0/manual/Volumes-API.html#access-a-shared-volume-profile
        [Tooltip(""+ 
        @"- Access a shared Volume Profile
          One method is to access the Volume's shared Profile. You do this via the Volume's sharedProfile property. This gives you a reference to the instance of the Volume Profile asset. 
          If you modify this Volume Profile:
            - HDRP applies any changes you make to every Volume that uses this Volume Profile asset.
            - The modifications you make affect the actual Volume Profile asset which means they do not reset when you exit Play mode
          Note the sharedProfile property can return null if the Volume does not reference a Volume Profile asset.


        - Access an owned Volume Profile
          The other method is to clone the Volume Profile asset. The advantage of this is that your modifications only affect the Volume component you clone the Volume Profile from and don't affect any other Volumes that use the same Volume Profile asset. 
          To do this, use the Volume's profile property. This returns a reference to a new instance of a Volume Profile (if not already created). 
          If you were already modifying the Volume's sharedProfile, any changes you made are copied over to the new instance. 
          If you modify this Volume Profile:
            - HDRP only applies changes to the particular Volume.
            - The modification you make reset when you exit Play mode.
            - It is your responsibility to destroy the duplicate Volume Profile when you no longer need it.
          Note that you can use this property to assign a different Volume Profile to the Volume.")]
        public VolumeAccessType volumeAccessType = VolumeAccessType.Profile;

        private Volume _volume;
        private Volume VolumeComponent
        {
            get
            {
                if (_volume == null)
                {
                    _volume = GetComponent<Volume>();
                }
                return _volume;
            }
        }

        private VolumeProfile _volumeProfile;
        public VolumeProfile VolumeProfile
        {
            get
            {
                if (_volumeProfile == null)
                {
                    _volumeProfile = volumeAccessType == VolumeAccessType.Profile? VolumeComponent.profile : VolumeComponent.sharedProfile;
                }
                return _volumeProfile;
            }
        }

        private VolumeAccessType _volumeAccessTypeCache;

        private void OnEnable()
        {
            _volumeAccessTypeCache = volumeAccessType;
            UpdateVolumeProfile();
        }

        private void Update()
        {
            if (VolumeProfile == null || _volumeAccessTypeCache != volumeAccessType)
            {
                UpdateVolumeProfile();
            }
        }

        public enum VolumeAccessType
        {
            Profile,
            SharedProfile
        }

        private void UpdateVolumeProfile()
        {
            _volumeAccessTypeCache = volumeAccessType;
            _volumeProfile = null; // 强制更新 VolumeProfile
        }
    }
}