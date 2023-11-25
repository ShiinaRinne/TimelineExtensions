// This code is automatically generated by MAO Timeline Playable Wizard.
// For more information, please visit 
// https://github.com/ShiinaRinne/TimelineExtensions

using System;
using UnityEngine;
using UnityEngine.Timeline;
using UnityEngine.Playables;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;


namespace MAOTimelineExtension.VolumeExtensions
{
    [Serializable]
    public class MAOChannelMixerClip : PlayableAsset, ITimelineClipAsset
    {
        public float redOutRedIn;
        public float redOutGreenIn;
        public float redOutBlueIn;
        public float greenOutRedIn;
        public float greenOutGreenIn;
        public float greenOutBlueIn;
        public float blueOutRedIn;
        public float blueOutGreenIn;
        public float blueOutBlueIn;


        public ClipCaps clipCaps
        {
            get { return ClipCaps.Blending; }
        }

        public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
        {
            var playable = ScriptPlayable<MAOChannelMixerBehaviour>.Create(graph);
            var behaviour = playable.GetBehaviour();

            behaviour.RedOutRedIn = redOutRedIn;
            behaviour.RedOutGreenIn = redOutGreenIn;
            behaviour.RedOutBlueIn = redOutBlueIn;
            behaviour.GreenOutRedIn = greenOutRedIn;
            behaviour.GreenOutGreenIn = greenOutGreenIn;
            behaviour.GreenOutBlueIn = greenOutBlueIn;
            behaviour.BlueOutRedIn = blueOutRedIn;
            behaviour.BlueOutGreenIn = blueOutGreenIn;
            behaviour.BlueOutBlueIn = blueOutBlueIn;


            return playable;
        }
    }
}