﻿using System;
using System.IO;
using System.Linq;
using UnityEditor;
using MAOTimelineExtension.Scripts;

namespace MAOTimelineExtension.Editor
{
    public partial class MaoTimelinePlayableWizard : EditorWindow
    {
        const string Note =
@"// This code is automatically generated by MAO Timeline Playable Wizard.
// For more information, please visit 
// https://github.com/ShiinaRinne/TimelineExtensions";
        
        CreationError CreateScripts()
        {
            if (ScriptAlreadyExists(playableName + k_TimelineClipAssetSuffix))
                return CreationError.PlayableAssetAlreadyExists;

            if (ScriptAlreadyExists(playableName + k_TimelineClipBehaviourSuffix))
                return CreationError.PlayableBehaviourAlreadyExists;

            if (ScriptAlreadyExists(playableName + k_PlayableBehaviourMixerSuffix))
                return CreationError.PlayableBehaviourMixerAlreadyExists;

            if (ScriptAlreadyExists(playableName + k_TrackAssetSuffix))
                return CreationError.TrackAssetAlreadyExists;

            if (m_CreateDrawer && ScriptAlreadyExists(playableName + k_PropertyDrawerSuffix))
                return CreationError.PlayableDrawerAlreadyExists;

            if (!Directory.Exists(Config.rootFolderPath))
            {
                Directory.CreateDirectory(Config.rootFolderPath);
            }

            AssetDatabase.CreateFolder(Config.rootFolderPath, playableName);

            if (workType == WorkType.Component)
            {
                CreateScript(playableName + k_TimelineClipAssetSuffix, StandardBlendPlayableAsset());
                CreateScript(playableName + k_TimelineClipBehaviourSuffix, StandardBlendPlayableBehaviour());
                CreateScript(playableName + k_PlayableBehaviourMixerSuffix, StandardBlendPlayableBehaviourMixer());
                CreateScript(playableName + k_TrackAssetSuffix, StandardBlendTrackAssetScript());
            }
            else if (workType == WorkType.VolumeComponent)
            {
                CreateScript(playableName + k_TimelineClipAssetSuffix, VolumeBlendPlayableAsset());
                CreateScript(playableName + k_TimelineClipBehaviourSuffix, VolumeBlendPlayableBehaviour());
                CreateScript(playableName + k_PlayableBehaviourMixerSuffix, VolumeBlendPlayableBehaviourMixer());
                CreateScript(playableName + k_TrackAssetSuffix, VolumeTrackAssetScript());
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            return CreationError.NoError;
        }

        static bool ScriptAlreadyExists(string scriptName)
        {
            string[] guids = AssetDatabase.FindAssets(scriptName);

            if (guids.Length == 0)
                return false;

            foreach (var guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                Type assetType = AssetDatabase.GetMainAssetTypeAtPath(path);
                if (assetType == typeof(MonoScript))
                    return true;
            }

            return false;
        }

        void CreateScript(string fileName, string content)
        {
            string path = $"{Config.rootFolderPath}/{playableName}/{fileName}.cs";
            using var writer = File.CreateText(path);
            writer.Write(content);
        }
        
        string NameSpaceStart()
        {
            return string.IsNullOrEmpty(Config.defaultNameSpace) ? "" : $@"namespace {Config.defaultNameSpace}
{{
";
        }
        
        string NameSpaceEnd()
        {
            return string.IsNullOrEmpty(Config.defaultNameSpace) ? "" : "}";
        }

        string VolumeBlendScriptPlayablePropertiesInitialize()
        {
            string returnVal = "";
            foreach (var prop in postProcessVolumeProperties)
            {
                returnVal += $"{k_Tab.Repeat(3)}behaviour.{prop.NameWithCapital} = {prop.name};\r\n";
            }

            return returnVal;
        }

        string VolumeBlendPlayableAsset()
        {
            return "" +
@$"{Note}

{AllNeededNameSpace()}

{NameSpaceStart()}
    [Serializable]
    public class {playableName}{k_TimelineClipAssetSuffix} : PlayableAsset, ITimelineClipAsset
    {{
{VolumeBlendScriptPlayablePropertiesToStringWithDefaultValue()}

        public ClipCaps clipCaps
        {{
            get {{ return ClipCaps.Blending; }}
        }}

        public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
        {{
            var playable = ScriptPlayable<{playableName}{k_TimelineClipBehaviourSuffix}>.Create(graph);
            var behaviour = playable.GetBehaviour();

{VolumeBlendScriptPlayablePropertiesInitialize()}

            return playable;
        }}
    }}
{NameSpaceEnd()}
";
        }
        
        string VolumeBlendPlayableBehaviour()
        {
            return "" +
@$"{Note}

{AllNeededNameSpace()}

{NameSpaceStart()}
    public class {playableName}{k_TimelineClipBehaviourSuffix} : PlayableBehaviour
    {{
{VolumeBlendScriptPlayablePropertiesToString()}
    }}
{NameSpaceEnd()}
";
        }
        
        string VolumeBlendPlayableBehaviourMixer()
        {
            return "" +
@$"{Note}

{AllNeededNameSpace()}

{NameSpaceStart()}
    public class {playableName}{k_PlayableBehaviourMixerSuffix} : PlayableBehaviour
    {{
{VolumeBlendTrackBindingPropertiesDefaultsDeclarationToString()}
        {TrackBinding.name} m_TrackBinding;
        bool m_FirstFrameHappened;

        public override void ProcessFrame(Playable playable, FrameData info, object playerData)
        {{
            if (playerData is null)
            {{
                Debug.LogError(""{playableName} Track Binding is null, you need to assign a `VolumeSettings` Component"");
                return;
            }}
{MixerTrackBindingLocalVariableToString()}

            int inputCount = playable.GetInputCount();
{VolumeBlendedVariablesCreationToString()}
            float totalWeight = 0f;
            float greatestWeight = 0f;
            int currentInputs = 0;

            for(int i = 0; i < inputCount; i++)
            {{
                float inputWeight = playable.GetInputWeight(i);
                ScriptPlayable<{playableName}{k_TimelineClipBehaviourSuffix}> inputPlayable =(ScriptPlayable<{playableName}{k_TimelineClipBehaviourSuffix}>)playable.GetInput(i);
                {playableName}{k_TimelineClipBehaviourSuffix} input = inputPlayable.GetBehaviour();
                
{VolumeAssignedVariablesWeightedIncrementationToString()}
                totalWeight += inputWeight;

                if (inputWeight > greatestWeight)
                {{
                    greatestWeight = inputWeight;
                }}

                if (!Mathf.Approximately (inputWeight, 0f))
                    currentInputs++;
            }}
{VolumeTrackBindingPropertiesAssignableAssignmentToString()}
        }}



        public override void OnPlayableDestroy (Playable playable)
        {{
            m_FirstFrameHappened = false;

            if(m_TrackBinding == null)
                return;

{VolumeRecoveryOriginalValue()}
        }}
    }}
{NameSpaceEnd()}

";
        }
        
        string VolumeTrackAssetScript()
        {
            return  "" + 
@$"{Note}

{AllNeededNameSpace()}

{NameSpaceStart()}
    [TrackColor({trackColor.r}f, {trackColor.g}f, {trackColor.b}f)]
    [TrackClipType(typeof({playableName}{k_TimelineClipAssetSuffix}))]
    {TrackBindingToString()}
    public class {playableName}{k_TrackAssetSuffix} : TrackAsset
    {{
        public override Playable CreateTrackMixer(PlayableGraph graph, GameObject go, int inputCount)
        {{
            return ScriptPlayable<{playableName}{k_PlayableBehaviourMixerSuffix}>.Create(graph, inputCount);
        }}
    }}
{NameSpaceEnd()}

";
        }
        
        string VolumeBlendScriptPlayablePropertiesGetPropertyAttributes(UsableProperty prop)
        {
            string isFloat = prop.type == "float" ? "f" : "";
            return prop.propertyAttributesType switch
            {
                UsableProperty.PropertyAttributesType.MinMax => $"[Range({prop.min}{isFloat}, {prop.max}{isFloat})] ",
                UsableProperty.PropertyAttributesType.Min => $"[Min({prop.min}{isFloat})] ",
                UsableProperty.PropertyAttributesType.Max => $"[Max({prop.max}{isFloat})] ",
                UsableProperty.PropertyAttributesType.ColorHDR => "[ColorUsage(true, true)] ",
                _ => ""
            };
        }

        string VolumeBlendScriptPlayablePropertiesToStringWithDefaultValue()
        {
            string returnVal = "";
            foreach (var prop in postProcessVolumeProperties)
            {
                string attributes = VolumeBlendScriptPlayablePropertiesGetPropertyAttributes(prop);
                if (prop.defaultValue == "")
                {
                    returnVal += $"{k_Tab.Repeat(2)}{attributes}public {prop.type} {prop.name};\r\n";
                }
                else
                {
                    returnVal += $"{k_Tab.Repeat(2)}{attributes}public {prop.type} {prop.name} = {prop.defaultValue};\r\n";
                }
            }

            return returnVal;
        }

        string VolumeBlendScriptPlayablePropertiesToString()
        {
            string returnVal = "";
            foreach (var prop in postProcessVolumeProperties)
            {
                returnVal += $"{k_Tab.Repeat(2)}public {prop.type} {prop.NameWithCapital};\r\n";
            }

            return returnVal;
        }

        string VolumeBlendTrackBindingPropertiesDefaultsDeclarationToString()
        {
            string returnVal = "";
            foreach (var prop in postProcessVolumeProperties)
            {
                returnVal += $"{k_Tab.Repeat(2)}{prop.type} {prop.NameAsPrivateDefault};\n";
            }

            return returnVal;
        }

        string VolumeSaveOriginalValue()
        {
            string returnVal = "";
            foreach (var prop in postProcessVolumeProperties)
            {
                returnVal += $"{k_Tab.Repeat(4)}{prop.NameAsPrivateDefault} = m_TrackBinding.{prop.name}.value;\n";
            }

            return returnVal;
        }

        string VolumeRecoveryOriginalValue()
        {
            string returnVal = "";
            foreach (var prop in postProcessVolumeProperties)
            {
                returnVal += $"{k_Tab.Repeat(3)}m_TrackBinding.{prop.name}.value = {prop.NameAsPrivateDefault};\n";
            }

            return returnVal;
        }

        string VolumeBlendedVariablesCreationToString()
        {
            string returnVal = "";
            foreach (var prop in postProcessVolumeProperties)
            {
                string type = prop.type == "int" ? "float" : prop.type;
                string zeroVal = type == "int" ? "0f" : prop.ZeroValueAsString();
                returnVal += $"{k_Tab.Repeat(3)}{type} {prop.NameAsLocalBlended} = {zeroVal};\n";
            }
            
            return returnVal;
        }

        string VolumeAssignedVariablesWeightedIncrementationToString()
        {
            string returnVal = "";
            foreach (var prop in postProcessVolumeProperties)
            {
                if (prop.usability != UsableProperty.Usability.Blendable)
                {
                    returnVal += $"{k_Tab.Repeat(4)}{prop.NameAsLocalBlended} = inputWeight > 0.5f ? input.{prop.name.Title()} : {prop.NameAsLocalBlended};\n";
                }
                else
                {
                    returnVal += $"{k_Tab.Repeat(4)}{prop.NameAsLocalBlended} += input.{prop.name.Title()} * inputWeight;\n";
                }
            }

            return returnVal;
        }

        string VolumeTrackBindingPropertiesAssignableAssignmentToString()
        {
            string returnVal = "";
            foreach (var prop in postProcessVolumeProperties)
            {
                if (prop.usability != UsableProperty.Usability.Blendable)
                {
                    returnVal += $"{k_Tab.Repeat(3)}m_TrackBinding.{prop.name}.value = {prop.NameAsLocalBlended};\n";
                }
                else
                {
                    if (prop.type == "int")
                        returnVal +=
                            $"{k_Tab.Repeat(3)}m_TrackBinding.{prop.name}.value = Mathf.RoundToInt({prop.NameAsLocalBlended} + {prop.NameAsPrivateDefault} * (1f-totalWeight));\n";
                    else
                        returnVal +=
                            $"{k_Tab.Repeat(3)}m_TrackBinding.{prop.name}.value = {prop.NameAsLocalBlended} + {prop.NameAsPrivateDefault} * (1f-totalWeight);\n";
                }
            }

            return returnVal;
        }
        
        string AllNeededNameSpace()
        {
            return @"using System;
using UnityEngine;
using UnityEngine.Timeline;
using UnityEngine.Playables;
using UnityEngine.Rendering;
using MAOTimelineExtension;
" + AdditionalNamespacesToString();
        }
        
        string TrackBindingToString()
        {
            return workType switch
            {
                WorkType.Component => $"[TrackBindingType(typeof({TrackBinding.name}))]",
                WorkType.VolumeComponent => $"[TrackBindingType(typeof(MAOTimelineExtensionVolumeSettings))]",
                _ => ""
            };
        }
        
        string AdditionalNamespacesToString()
        {
            UsableType[] exposedReferenceTypes  = Variable.GetUsableTypesFromVariableArray(exposedReferences.ToArray());
            UsableType[] behaviourVariableTypes = Variable.GetUsableTypesFromVariableArray(playableBehaviourVariables.ToArray());
            UsableType[] allUsedTypes = new UsableType[exposedReferenceTypes.Length + behaviourVariableTypes.Length + 1];
    
            exposedReferenceTypes.CopyTo(allUsedTypes, 0);
            behaviourVariableTypes.CopyTo(allUsedTypes, exposedReferenceTypes.Length);
            allUsedTypes[^1] = TrackBinding;

            return string.Join("\n", 
                UsableType.GetDistinctAdditionalNamespaces(allUsedTypes)
                    .Where(x => !string.IsNullOrEmpty(x))
                    .Select(x => $"using {x};"));
        }

        #region Original Unused Code

        string ExposedReferencesToString()
        {
            string expRefText = "";
            foreach (var expRef in exposedReferences)
                expRefText += k_Tab + "public ExposedReference<" + expRef.usableType.name + "> " + expRef.name + ";\n";
            return expRefText;
        }

        string ExposedReferencesResolvingToString()
        {
            string returnVal = "";
            returnVal += k_Tab + k_Tab + playableName + k_TimelineClipBehaviourSuffix +
                         " clone = playable.GetBehaviour();\n";

            foreach (var reference in exposedReferences)
            {
                returnVal += k_Tab + k_Tab + "clone." + reference.name + " = " + reference.name + ".Resolve(graph.GetResolver());\n";
            }

            return returnVal;
        }

        /*string OnCreateFunctionToString()
        {
            if(!setClipDefaults)
                return "";
    
            string returnVal = "\n";
                returnVal += k_Tab + "public override void OnCreate()\n";
                returnVal += k_Tab + "{\n";
                returnVal += k_Tab + k_Tab + "owner.duration = " + clipDefaultDurationSeconds + ";\n";
                returnVal += k_Tab + k_Tab + "owner.easeInDuration = " + clipDefaultEaseInSeconds + ";\n";
                returnVal += k_Tab + k_Tab + "owner.easeOutDuration = " + clipDefaultEaseOutSeconds + ";\n";
                returnVal += k_Tab + k_Tab + "owner.clipIn = " + clipDefaultClipInSeconds + ";\n";
                returnVal += k_Tab + k_Tab + "owner.timeScale = " + clipDefaultSpeedMultiplier + ";\n";
                returnVal += k_Tab + "}\n";
            return returnVal;
        }*/

        string ClipCapsToString()
        {
            string message = clipCaps.ToString();
            string[] splits = message.Split(' ');

            for (int i = 0; i < splits.Length; i++)
            {
                if (splits[i][splits[i].Length - 1] == ',')
                    splits[i] = splits[i].Substring(0, splits[i].Length - 1);
            }

            string returnVal = "";
            for (int i = 0; i < splits.Length; i++)
            {
                returnVal += "ClipCaps." + splits[i];

                if (i < splits.Length - 1)
                    returnVal += " | ";
            }

            return returnVal;
        }

        string ExposedReferencesAsScriptVariablesToString()
        {
            string returnVal = "";
            foreach (var reference in exposedReferences)
            {
                returnVal += k_Tab + "clone." + reference.name + " = " + reference.name + ";\n";
            }

            return returnVal;
        }

        #endregion

        string MixerTrackBindingLocalVariableToString() =>
            workType switch
            {
                WorkType.Component =>
                    k_Tab + k_Tab + TrackBinding.name + " trackBinding = playerData as " + TrackBinding.name + ";\n\n" +
                    k_Tab + k_Tab + "if(!trackBinding)\n" +
                    k_Tab + k_Tab + k_Tab + "return;\n" +
                    "\n",

                WorkType.VolumeComponent =>
                    @$"{k_Tab.Repeat(3)}((MAOTimelineExtensionVolumeSettings) playerData).VolumeProfile.TryGet(out m_TrackBinding);
            if (m_TrackBinding == null)
                return;
            
            if(!m_FirstFrameHappened)
            {{
{VolumeSaveOriginalValue()}
                m_FirstFrameHappened = true;
            }}
",
                _ => ""
            };

        string StandardBlendPlayableAsset()
        {
            return "using System;\n" +
                   "using UnityEngine;\n" +
                   "using UnityEngine.Playables;\n" +
                   "using UnityEngine.Timeline;\n" +
                   "\n" +
                   "[Serializable]\n" +
                   "public class " + playableName + k_TimelineClipAssetSuffix + " : PlayableAsset, ITimelineClipAsset\n" +
                   "{\n" +
                   k_Tab + "public " + playableName + k_TimelineClipBehaviourSuffix + " template = new " + playableName +
                   k_TimelineClipBehaviourSuffix + "();\n" +
                   "\n" +
                   k_Tab + "public ClipCaps clipCaps\n" +
                   k_Tab + "{\n" +
                   k_Tab + k_Tab + "get { return ClipCaps.Blending; }\n" +
                   k_Tab + "}\n" +
                   "\n" +
                   k_Tab + "public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)\n" +
                   k_Tab + "{\n" +
                   k_Tab + k_Tab + "var playable = ScriptPlayable<" + playableName + k_TimelineClipBehaviourSuffix +
                   ">.Create(graph, template);\n" +
                   k_Tab + k_Tab + "return playable;\n" +
                   k_Tab + "}\n" +
                   "}\n";
        }

        string StandardBlendPlayableBehaviour()
        {
            return "using System;\n" +
                   "using UnityEngine;\n" +
                   "using UnityEngine.Playables;\n" +
                   "using UnityEngine.Timeline;\n" +
                   AdditionalNamespacesToString() +
                   "\n" +
                   "[Serializable]\n" +
                   "public class " + playableName + k_TimelineClipBehaviourSuffix + " : PlayableBehaviour\n" +
                   "{\n" +
                   StandardBlendScriptPlayablePropertiesToString() +
                   "}\n";
        }
        
        string StandardBlendPlayableBehaviourMixer()
        {
            return "using System;\n" +
                   "using UnityEngine;\n" +
                   "using UnityEngine.Playables;\n" +
                   "using UnityEngine.Timeline;\n" +
                   AdditionalNamespacesToString() +
                   "\n" +
                   "public class " + playableName + k_PlayableBehaviourMixerSuffix + " : PlayableBehaviour\n" +
                   "{\n" +
                   StandardBlendTrackBindingPropertiesDefaultsDeclarationToString() +
                   "\n" +
                   StandardBlendTrackBindingPropertiesBlendedDeclarationToString() +
                   "\n" +
                   k_Tab + TrackBinding.name + " m_TrackBinding;\n" +
                   "\n" +
                   k_Tab + "public override void ProcessFrame(Playable playable, FrameData info, object playerData)\n" +
                   k_Tab + "{\n" +
                   k_Tab + k_Tab + "m_TrackBinding = playerData as " + TrackBinding.name + ";\n" +
                   "\n" +
                   k_Tab + k_Tab + "if(m_TrackBinding == null)\n" +
                   k_Tab + k_Tab + k_Tab + "return;\n" +
                   "\n" +
                   StandardBlendTrackBindingPropertiesDefaultsAssignmentToString() +
                   "\n" +
                   k_Tab + k_Tab + "int inputCount = playable.GetInputCount();\n" +
                   "\n" +
                   StandardBlendBlendedVariablesCreationToString() +
                   k_Tab + k_Tab + "float totalWeight = 0f;\n" +
                   k_Tab + k_Tab + "float greatestWeight = 0f;\n" +
                   StandardBlendPlayableCurrentInputsDeclarationToString() +
                   "\n" +
                   k_Tab + k_Tab + "for(int i = 0; i < inputCount; i++)\n" +
                   k_Tab + k_Tab + "{\n" +
                   k_Tab + k_Tab + k_Tab + "float inputWeight = playable.GetInputWeight(i);\n" +
                   k_Tab + k_Tab + k_Tab + "ScriptPlayable<" + playableName + k_TimelineClipBehaviourSuffix +
                   "> inputPlayable =(ScriptPlayable<" + playableName + k_TimelineClipBehaviourSuffix +
                   ">)playable.GetInput(i);\n" +
                   k_Tab + k_Tab + k_Tab + playableName + k_TimelineClipBehaviourSuffix +
                   " input = inputPlayable.GetBehaviour();\n" +
                   k_Tab + k_Tab + k_Tab + "\n" +
                   StandardBlendBlendedVariablesWeightedIncrementationToString() +
                   k_Tab + k_Tab + k_Tab + "totalWeight += inputWeight;\n" +
                   "\n" +
                   StandardBlendAssignableVariablesAssignedBasedOnGreatestWeightToString() +
                   StandardBlendPlayableCurrentInputIterationToString() +
                   k_Tab + k_Tab + "}\n" +
                   StandardBlendTrackBindingPropertiesBlendedAssignmentToString() +
                   StandardBlendTrackBindingPropertiesAssignableAssignmentToString() +
                   k_Tab + "}\n" +
                   "}\n";
        }

        string StandardBlendTrackAssetScript()
        {
            return "using UnityEngine;\n" +
                   "using UnityEngine.Playables;\n" +
                   "using UnityEngine.Timeline;\n" +
                   "using System.Collections.Generic;\n" +
                   AdditionalNamespacesToString() +
                   "\n" +
                   "[TrackColor(" + trackColor.r + "f, " + trackColor.g + "f, " + trackColor.b + "f)]\n" +
                   "[TrackClipType(typeof(" + playableName + k_TimelineClipAssetSuffix + "))]\n" +
                   StandardBlendComponentBindingToString() +
                   "public class " + playableName + k_TrackAssetSuffix + " : TrackAsset\n" +
                   "{\n" +
                   k_Tab +
                   "public override Playable CreateTrackMixer(PlayableGraph graph, GameObject go, int inputCount)\n" +
                   k_Tab + "{\n" +
                   k_Tab + k_Tab + "return ScriptPlayable<" + playableName + k_PlayableBehaviourMixerSuffix +
                   ">.Create(graph, inputCount);\n" +
                   k_Tab + "}\n" +
                   "\n" +
                   k_Tab + "// Please note this assumes only one component of type " + TrackBinding.name +
                   " on the same gameobject.\n" +
                   k_Tab +
                   "public override void GatherProperties(PlayableDirector director, IPropertyCollector driver)\n" +
                   k_Tab + "{\n" +
                   "#if UNITY_EDITOR\n" +
                   k_Tab + k_Tab + TrackBinding.name + " trackBinding = director.GetGenericBinding(this) as " +
                   TrackBinding.name + ";\n" +
                   k_Tab + k_Tab + "if(trackBinding == null)\n" +
                   k_Tab + k_Tab + k_Tab + "return;\n" +
                   "\n" +
                   StandardBlendPropertiesAssignedToPropertyDriverToString() +
                   "#endif\n" +
                   k_Tab + k_Tab + "base.GatherProperties(director, driver);\n" +
                   k_Tab + "}\n" +
                   "}\n";
        }
        
        string StandardBlendScriptPlayablePropertiesToString()
        {
            string returnVal = "";
            foreach (var prop in standardBlendPlayableProperties)
            {
                if (prop.defaultValue == "")
                    returnVal += k_Tab + "public " + prop.type + " " + prop.name + ";\n";
                else
                {
                    returnVal += k_Tab + "public " + prop.type + " " + prop.name + " = " + prop.defaultValue + ";\n";
                }
            }

            return returnVal;
        }

        string StandardBlendTrackBindingPropertiesDefaultsDeclarationToString()
        {
            string returnVal = "";
            foreach (var prop in standardBlendPlayableProperties)
            {
                returnVal += k_Tab + prop.type + " " + prop.NameAsPrivateDefault + ";\n";
            }

            return returnVal;
        }

        string StandardBlendTrackBindingPropertiesBlendedDeclarationToString()
        {
            string returnVal = "";
            foreach (var prop in standardBlendPlayableProperties)
            {
                returnVal += k_Tab + prop.type + " " + prop.NameAsPrivateAssigned + ";\n";
            }

            return returnVal;
        }

        string StandardBlendTrackBindingPropertiesDefaultsAssignmentToString()
        {
            string returnVal = "";
            foreach (var prop in standardBlendPlayableProperties)
            {
                switch (prop.type)
                {
                    case "float":
                        returnVal += k_Tab + k_Tab + "if(!Mathf.Approximately(m_TrackBinding." + prop.name + ", " +
                                     prop.NameAsPrivateAssigned + "))\n";
                        returnVal += k_Tab + k_Tab + k_Tab + prop.NameAsPrivateDefault + " = m_TrackBinding." +
                                     prop.name + ";\n";
                        break;
                    case "double":
                        returnVal += k_Tab + k_Tab + "if(!Mathf.Approximately((float)m_TrackBinding." + prop.name +
                                     ",(float)" + prop.NameAsPrivateAssigned + "))\n";
                        returnVal += k_Tab + k_Tab + k_Tab + prop.NameAsPrivateDefault + " = m_TrackBinding." +
                                     prop.name + ";\n";
                        break;
                    default:
                        returnVal += k_Tab + k_Tab + "if(m_TrackBinding." + prop.name + " != " +
                                     prop.NameAsPrivateAssigned + ")\n";
                        returnVal += k_Tab + k_Tab + k_Tab + prop.NameAsPrivateDefault + " = m_TrackBinding." +
                                     prop.name + ";\n";
                        break;
                }
            }

            return returnVal;
        }

        string StandardBlendBlendedVariablesCreationToString()
        {
            string returnVal = "";
            foreach (var prop in standardBlendPlayableProperties)
            {
                if (prop.usability != UsableProperty.Usability.Blendable)
                    continue;

                string type = prop.type == "int" ? "float" : prop.type;
                string zeroVal = prop.type == "int" ? "0f" : prop.ZeroValueAsString();
                returnVal += k_Tab + k_Tab + type + " " + prop.NameAsLocalBlended + " = " + zeroVal + ";\n";
            }

            return returnVal;
        }

        string StandardBlendPlayableCurrentInputsDeclarationToString()
        {
            if (standardBlendPlayableProperties.Any(x => x.usability == UsableProperty.Usability.Assignable))
            {
                return k_Tab + k_Tab + "int currentInputs = 0;\n";
            }

            return "";
        }

        string StandardBlendBlendedVariablesWeightedIncrementationToString()
        {
            string returnVal = "";
            foreach (var prop in standardBlendPlayableProperties)
            {
                if (prop.usability == UsableProperty.Usability.Blendable)
                    returnVal += k_Tab + k_Tab + k_Tab + prop.NameAsLocalBlended + " += input." + prop.name + " * inputWeight;\n";
            }

            return returnVal;
        }

        string StandardBlendAssignableVariablesAssignedBasedOnGreatestWeightToString()
        {
            if (standardBlendPlayableProperties.Count == 0)
                return "";

            string returnVal = k_Tab + k_Tab + k_Tab + "if(inputWeight > greatestWeight)\n";
            returnVal += k_Tab + k_Tab + k_Tab + "{\n";
            
            foreach (var prop in standardBlendPlayableProperties)
            {
                if (prop.usability == UsableProperty.Usability.Assignable)
                {
                    returnVal += k_Tab + k_Tab + k_Tab + k_Tab + prop.NameAsPrivateAssigned + " = input." + prop.name +
                                 ";\n";
                    returnVal += k_Tab + k_Tab + k_Tab + k_Tab + "m_TrackBinding." + prop.name + " = " +
                                 prop.NameAsPrivateAssigned + ";\n";
                }
            }

            returnVal += k_Tab + k_Tab + k_Tab + k_Tab + "greatestWeight = inputWeight;\n";
            returnVal += k_Tab + k_Tab + k_Tab + "}\n";
            return returnVal;
        }

        string StandardBlendPlayableCurrentInputIterationToString()
        {
            if (standardBlendPlayableProperties.Any(x => x.usability == UsableProperty.Usability.Assignable))
            {
                string returnVal = "\n";
                returnVal += k_Tab + k_Tab + k_Tab + "if(!Mathf.Approximately(inputWeight, 0f))\n";
                returnVal += k_Tab + k_Tab + k_Tab + k_Tab + "currentInputs++;\n";
                return returnVal;
            }

            return "";
        }

        string StandardBlendTrackBindingPropertiesBlendedAssignmentToString()
        {
            string returnVal = "";
            bool firstNewLine = false;

            foreach (var prop in standardBlendPlayableProperties)
            {
                if (prop.usability != UsableProperty.Usability.Blendable)
                    continue;

                if (!firstNewLine)
                {
                    firstNewLine = true;
                    returnVal += "\n";
                }

                if (prop.type == "int")
                    returnVal += k_Tab + k_Tab + prop.NameAsPrivateAssigned + " = Mathf.RoundToInt(" +
                                 prop.NameAsLocalBlended + " + " + prop.NameAsPrivateDefault +
                                 " *(1f - totalWeight));\n";
                else
                    returnVal += k_Tab + k_Tab + prop.NameAsPrivateAssigned + " = " + prop.NameAsLocalBlended + " + " +
                                 prop.NameAsPrivateDefault + " *(1f - totalWeight);\n";

                returnVal += k_Tab + k_Tab + "m_TrackBinding." + prop.name + " = " + prop.NameAsPrivateAssigned + ";\n"; 
            }

            return returnVal;
        }

        string StandardBlendTrackBindingPropertiesAssignableAssignmentToString()
        {
            if (standardBlendPlayableProperties.Count == 0)
                return "";

            if (standardBlendPlayableProperties.Any(x => x.usability == UsableProperty.Usability.Assignable))
            {
                string returnVal = "\n" + k_Tab + k_Tab +
                                   "if(currentInputs != 1 && 1f - totalWeight > greatestWeight)\n";
                returnVal += k_Tab + k_Tab + "{\n";
                
                foreach (var prop in standardBlendPlayableProperties)
                {
                    if (prop.usability != UsableProperty.Usability.Assignable)
                        continue;

                    returnVal += k_Tab + k_Tab + k_Tab + "m_TrackBinding." + prop.name + " = " +
                                 prop.NameAsPrivateDefault + ";\n";
                }

                returnVal += k_Tab + k_Tab + "}\n";
                return returnVal;
            }

            return "";
        }

        string StandardBlendComponentBindingToString()
        {
            return "[TrackBindingType(typeof(" + TrackBinding.name + "))]\n";
        }

        string StandardBlendPropertiesAssignedToPropertyDriverToString()
        {
            if (standardBlendPlayableProperties.Count == 0)
                return "";

            string 
            returnVal  = k_Tab + k_Tab + "// These field names are procedurally generated estimations based on the associated property names.\n";
            returnVal += k_Tab + k_Tab + "// If any of the names are incorrect you will get a DrivenPropertyManager error saying it has failed to register the name.\n";
            returnVal += k_Tab + k_Tab + "// In this case you will need to find the correct backing field name.\n";
            returnVal += k_Tab + k_Tab + "// The suggested way of finding the field name is to:\n";
            returnVal += k_Tab + k_Tab + "// 1. Make sure your scene is serialized to text.\n";
            returnVal += k_Tab + k_Tab + "// 2. Search the text for the track binding component type.\n";
            returnVal += k_Tab + k_Tab + "// 3. Look through the field names until you see one that looks correct.\n";

            foreach (var prop in standardBlendPlayableProperties)
            {
                if (prop.usablePropertyType == UsableProperty.UsablePropertyType.Field)
                {
                    returnVal += k_Tab + k_Tab + "driver.AddFromName<" + TrackBinding.name +
                                 ">(trackBinding.gameObject, \"" + prop.name + "\");\n";
                }
                else
                {
                    returnVal += k_Tab + k_Tab + "driver.AddFromName<" + TrackBinding.name +
                                 ">(trackBinding.gameObject, \"" + prop.NameAsPrivate + "\");\n";
                }
            }

            return returnVal;
        }
    }
}