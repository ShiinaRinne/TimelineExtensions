using System;
using System.Collections.Generic;
using System.Linq;
using System.CodeDom.Compiler;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.Timeline;
using UnityEngine.Rendering;


namespace YMToonURP.Timeline.Editor
{
    public partial class MaoTimelinePlayableWizard : EditorWindow
    {
        public class Variable : IComparable
        {
            public string name;
            public UsableType usableType;

            int m_TypeIndex;

            public Variable(string name, UsableType usableType)
            {
                this.name = name;
                this.usableType = usableType;
            }

            public bool GUI(UsableType[] usableTypes)
            {
                bool removeThis = false;
                EditorGUILayout.BeginHorizontal();
                name = EditorGUILayout.TextField(name);
                m_TypeIndex = EditorGUILayout.Popup(m_TypeIndex, UsableType.GetNamewithSortingArray(usableTypes));
                usableType = usableTypes[m_TypeIndex];
                if (GUILayout.Button("Remove", GUILayout.Width(60f)))
                {
                    removeThis = true;
                }

                EditorGUILayout.EndHorizontal();

                return removeThis;
            }

            public int CompareTo(object obj)
            {
                if (obj == null)
                    return 1;

                UsableType other = (UsableType) obj;

                if (other == null)
                    throw new ArgumentException("This object is not a Variable.");

                return name.ToLower().CompareTo(other.name.ToLower());
            }

            public static UsableType[] GetUsableTypesFromVariableArray(Variable[] variables)
            {
                UsableType[] usableTypes = new UsableType[variables.Length];
                for (int i = 0; i < usableTypes.Length; i++)
                {
                    usableTypes[i] = variables[i].usableType;
                }

                return usableTypes;
            }
        }
        
        public class UsableType : IComparable
        {
            public readonly string name;
            public readonly string nameWithSorting;
            public readonly string additionalNamespace;
            public readonly GUIContent guiContentWithSorting;
            public readonly Type type;

            public readonly string[] unrequiredNamespaces =
            {
                "UnityEngine",
                "UnityEngine.Timeline",
                "UnityEngine.Playables"
            };

            public const string blankAdditionalNamespace = "";

            const string k_NameForNullType = "None";

            public UsableType(Type usableType)
            {
                type = usableType;

                if (type != null)
                {
                    name = usableType.Name;
                    nameWithSorting = name.ToUpper()[0] + "/" + name;
                    additionalNamespace = unrequiredNamespaces.All(t => usableType.Namespace != t)
                        ? usableType.Namespace
                        : blankAdditionalNamespace;
                }
                else
                {
                    name = k_NameForNullType;
                    nameWithSorting = k_NameForNullType;
                    additionalNamespace = blankAdditionalNamespace;
                }

                guiContentWithSorting = new GUIContent(nameWithSorting);
            }

            public UsableType(string name)
            {
                this.name = name;
                nameWithSorting = name.ToUpper()[0] + "/" + name;
                additionalNamespace = blankAdditionalNamespace;
                guiContentWithSorting = new GUIContent(nameWithSorting);
            }

            public int CompareTo(object obj)
            {
                if (obj == null)
                    return 1;

                UsableType other = (UsableType) obj;

                if (other == null)
                    throw new ArgumentException("This object is not a UsableType.");

                return name.ToLower().CompareTo(other.name.ToLower());
            }

            public static UsableType[] GetUsableTypeArray(Type[] types, params UsableType[] additionalUsableTypes)
            {
                List<UsableType> usableTypeList = new List<UsableType>();
                for (int i = 0; i < types.Length; i++)
                {
                    usableTypeList.Add(new UsableType(types[i]));
                }

                usableTypeList.AddRange(additionalUsableTypes);
                return usableTypeList.ToArray();
            }

            public static UsableType[] AmalgamateUsableTypes(UsableType[] usableTypeArray,
                params UsableType[] usableTypes)
            {
                List<UsableType> usableTypeList = new List<UsableType>();
                for (int i = 0; i < usableTypes.Length; i++)
                {
                    usableTypeList.Add(usableTypes[i]);
                }

                usableTypeList.AddRange(usableTypeArray);
                return usableTypeList.ToArray();
            }

            public static string[] GetNamewithSortingArray(UsableType[] usableTypes)
            {
                if (usableTypes == null || usableTypes.Length == 0)
                    return new string[0];

                string[] displayNames = new string[usableTypes.Length];
                for (int i = 0; i < displayNames.Length; i++)
                {
                    displayNames[i] = usableTypes[i].nameWithSorting;
                }

                return displayNames;
            }

            public static GUIContent[] GetGUIContentWithSortingArray(UsableType[] usableTypes)
            {
                if (usableTypes == null || usableTypes.Length == 0)
                    return new GUIContent[0];

                GUIContent[] guiContents = new GUIContent[usableTypes.Length];
                for (int i = 0; i < guiContents.Length; i++)
                {
                    guiContents[i] = usableTypes[i].guiContentWithSorting;
                }

                return guiContents;
            }

            public static string[] GetDistinctAdditionalNamespaces(UsableType[] usableTypes)
            {
                if (usableTypes == null || usableTypes.Length == 0)
                    return new string[0];

                string[] namespaceArray = new string[usableTypes.Length];
                for (int i = 0; i < namespaceArray.Length; i++)
                {
                    namespaceArray[i] = usableTypes[i].additionalNamespace;
                }

                return namespaceArray.Distinct().ToArray();
            }
        }
        
        public class UsableProperty : IComparable
        {
            public enum Usability
            {
                Blendable,
                Assignable,
                Not
            }

            public enum UsablePropertyType
            {
                Property,
                Field
            }
            
            /// ================================
            /// for PropertyAttribute
            public enum PropertyAttributesType
            {
                MinMax,
                Min,
                Max,
                Null
            }

            public PropertyAttributesType propertyAttributesType = PropertyAttributesType.Null;
            public float min;
            public float max;
            // =================================
            

            public string type;
            public string name;
            public string defaultValue;
            public Usability usability;
            public UsablePropertyType usablePropertyType;
            public PropertyInfo propertyInfo;
            public FieldInfo fieldInfo;

            int m_TypeIndex;

            public string NameWithCaptial
            {
                get { return name.First().ToString().ToUpper() + name.Substring(1); }
            }

            public string NameAsPrivate
            {
                get { return "m_" + NameWithCaptial; }
            }

            public string NameAsPrivateDefault
            {
                get { return "m_Default" + NameWithCaptial; }
            }

            public string NameAsPrivateAssigned
            {
                get { return "m_Assigned" + NameWithCaptial; }
            }

            public string NameAsLocalBlended
            {
                get { return "blended" + NameWithCaptial; }
            }

            public string NameAsLocalSerializedProperty
            {
                get { return name + "Prop"; }
            }

            public UsableProperty(PropertyInfo propertyInfo)
            {
                usablePropertyType = UsablePropertyType.Property;
                this.propertyInfo = propertyInfo;

                if (propertyInfo.PropertyType.Name == "Single")
                    type = "float";
                else if (propertyInfo.PropertyType.Name == "Int32")
                    type = "int";
                else if (propertyInfo.PropertyType.Name == "Double")
                    type = "double";
                else if (propertyInfo.PropertyType.Name == "Boolean")
                    type = "bool";
                else if (propertyInfo.PropertyType.Name == "String")
                    type = "string";
                else
                    type = propertyInfo.PropertyType.Name;

                name = propertyInfo.Name;

                if (IsTypeBlendable(propertyInfo.PropertyType))
                    usability = Usability.Blendable;
                else if (IsTypeAssignable(propertyInfo.PropertyType))
                    usability = Usability.Assignable;
                else
                    usability = Usability.Not;
            }

            public UsableProperty(FieldInfo fieldInfo)
            {
                usablePropertyType = UsablePropertyType.Field;
                this.fieldInfo = fieldInfo;

                if (fieldInfo.FieldType.Name == "Single" || fieldInfo.FieldType.Name.Contains("FloatParameter"))
                    type = "float";
                else if (fieldInfo.FieldType.Name == "Int32" || fieldInfo.FieldType.Name.Contains("IntParameter"))
                    type = "int";
                else if (fieldInfo.FieldType.Name == "Double")
                    type = "double";
                else if (fieldInfo.FieldType.Name == "Boolean" || fieldInfo.FieldType.Name.Contains("BoolParameter"))
                    type = "bool";
                else if (fieldInfo.FieldType.Name == "String")
                    type = "string";
                else if (fieldInfo.FieldType.Name == "Color" || fieldInfo.FieldType.Name.Contains("ColorParameter"))
                    type = "Color";
                else if (fieldInfo.FieldType.Name == "Vector2" || fieldInfo.FieldType.Name.Contains("Vector2Parameter"))
                    type = "Vector2";
                else if (fieldInfo.FieldType.Name == "Vector3" || fieldInfo.FieldType.Name.Contains("Vector3Parameter"))
                    type = "Vector3";
                else if (fieldInfo.FieldType.Name == "Vector4" || fieldInfo.FieldType.Name.Contains("Vector4Parameter"))
                    type = "Vector4";
                // TODO: Check Texture、Texture2D、Texture3D
                else if (fieldInfo.FieldType.Name == "Texture" || fieldInfo.FieldType.Name.Contains("TextureParameter"))
                    type = "Texture";
                else if (fieldInfo.FieldType.Name == "Texture2D" ||
                         fieldInfo.FieldType.Name.Contains("Texture2DParameter"))
                    type = "Texture2D";
                else if (fieldInfo.FieldType.Name == "Texture3D" ||
                         fieldInfo.FieldType.Name.Contains("Texture3DParameter"))
                    type = "Texture3D";
                else
                    type = fieldInfo.FieldType.Name;

                name = fieldInfo.Name;

                if (IsTypeBlendable(fieldInfo.FieldType))
                    usability = Usability.Blendable;
                else if (IsTypeAssignable(fieldInfo.FieldType))
                    usability = Usability.Assignable;
                else
                    usability = Usability.Not;
            }

            public string ZeroValueAsString()
            {
                switch (type)
                {
                    case "float":
                        return "0f";
                    case "int":
                        return "0";
                    case "double":
                        return "0.0";
                    case "Vector2":
                        return "Vector2.zero";
                    case "Vector3":
                        return "Vector3.zero";
                    case "Vector4":
                        return "Vector4.zero";
                    case "Color":
                        return "Color.clear";
                    case "bool":
                        return "false";
                    case "Texture":
                        return "new Texture2D(1,1)";
                    case "Texture2D":
                        return "new Texture2D(1,1)";
                    case "Texture3D":
                        return "new Texture3D(1, 1, 1, TextureFormat.ARGB32, false)";
                }


                return "";
            }

            public void CreateSettingDefaultValueString(Component defaultValuesComponent)
            {
                if (defaultValuesComponent == null)
                {
                    defaultValue = "";
                    return;
                }

                object defaultValueObj = usablePropertyType == UsablePropertyType.Property
                    ? propertyInfo.GetValue(defaultValuesComponent, null)
                    : fieldInfo.GetValue(defaultValuesComponent);

                switch (type)
                {
                    case "float":
                        float defaultFloatValue = (float) defaultValueObj;
                        defaultValue = defaultFloatValue + "f";
                        break;
                    case "int":
                        int defaultIntValue = (int) defaultValueObj;
                        defaultValue = defaultIntValue.ToString();
                        break;
                    case "double":
                        double defaultDoubleValue = (double) defaultValueObj;
                        defaultValue = defaultDoubleValue.ToString();
                        break;
                    case "Vector2":
                        Vector2 defaultVector2Value = (Vector2) defaultValueObj;
                        defaultValue = "new Vector2(" + defaultVector2Value.x + "f, " + defaultVector2Value.y + "f)";
                        break;
                    case "Vector3":
                        Vector3 defaultVector3Value = (Vector3) defaultValueObj;
                        defaultValue = "new Vector3(" + defaultVector3Value.x + "f, " + defaultVector3Value.y + "f, " +
                                       defaultVector3Value.z + "f)";
                        break;
                    case "Vector4":
                        Vector4 defaultVector4Value = (Vector4) defaultValueObj;
                        defaultValue = "new Vector4(" + defaultVector4Value.x + "f, " + defaultVector4Value.y + "f, " +
                                       defaultVector4Value.z + "f)";
                        break;
                    case "Color":
                        Color defaultColorValue = (Color) defaultValueObj;
                        defaultValue = "new Color(" + defaultColorValue.r + "f, " + defaultColorValue.g + "f, " +
                                       defaultColorValue.b + "f, " + defaultColorValue.a + "f)";
                        break;
                    case "string":
                        defaultValue = "\"" + defaultValueObj + "\"";
                        break;
                    case "bool":
                        bool defaultBoolValue = (bool) defaultValueObj;
                        defaultValue = defaultBoolValue.ToString().ToLower();
                        break;
                    case "Texture":
                        defaultValue = "";
                        break;
                    default:
                        Enum defaultEnumValue = (Enum) defaultValueObj;
                        Type enumSystemType = defaultEnumValue.GetType();
                        string[] splits = enumSystemType.ToString().Split('+');
                        string enumType = splits[splits.Length - 1];
                        string enumConstantName = Enum.GetName(enumSystemType, defaultEnumValue);
                        defaultValue = enumType + "." + enumConstantName;
                        break;
                }
            }

            
            /// <summary>
            /// 获取字段的Range，Min，Max属性
            /// </summary>
            /// <typeparam name="T">T: ClampedFloat, RangeInt, MinFloat, etc</typeparam>
            public void GetPropertyAttributes<T>(object parameter) where T : VolumeParameter
            {
                var typeName = typeof(T).Name;
                if(typeName.Contains("Range") || typeName.Contains("Clamped"))
                {
                    propertyAttributesType = PropertyAttributesType.MinMax;
                    min = (float)GetPropertyAttributesMin(parameter);
                    max = (float)GetPropertyAttributesMax(parameter);
                    
                }
                else if (typeName.Contains("Min"))
                {
                    propertyAttributesType = PropertyAttributesType.Min;
                    min = (float)GetPropertyAttributesMin(parameter);
                }
                else if (typeof(T).Name.Contains("Max"))
                {
                    propertyAttributesType = PropertyAttributesType.Max;
                    max = (float)GetPropertyAttributesMax(parameter);
                }
            }
            
            public object GetPropertyAttributesMax(object parameter)
            {
                return parameter.GetType().GetField("max").GetValue(parameter);
            }
            public object GetPropertyAttributesMin(object parameter)
            {
                return parameter.GetType().GetField("min").GetValue(parameter);
            }


            public void CreateSettingDefaultValueStringVolume<T>(Volume defaultValuesComponent, UsableProperty prop)
                where T : VolumeComponent
            {
                // var component = (T)FormatterServices.GetUninitializedObject(typeof(T));
                defaultValuesComponent.profile.TryGet<T>(out var component);
                if (component == null)
                {
                    defaultValue = "";
                    return;
                }
                var parameter = usablePropertyType == UsablePropertyType.Property
                    ? propertyInfo.GetValue(component, null)
                    : fieldInfo.GetValue(component);

                
                var method = typeof(VolumeParameter).GetMethod("GetValue");
                switch (prop.type)
                {
                    case "float":
                        float defaultFloatValue =
                            (float) method.MakeGenericMethod(typeof(float)).Invoke(parameter, null);
                        defaultValue = defaultFloatValue + "f";
                        break;
                    case "int":
                        int defaultIntValue = (int) method.MakeGenericMethod(typeof(int)).Invoke(parameter, null);
                        defaultValue = defaultIntValue.ToString();
                        break;
                    case "double":
                        double defaultDoubleValue =
                            (double) method.MakeGenericMethod(typeof(double)).Invoke(parameter, null);
                        defaultValue = defaultDoubleValue.ToString();
                        break;
                    case "Vector2":
                        Vector2 defaultVector2Value =
                            (Vector2) method.MakeGenericMethod(typeof(Vector2)).Invoke(parameter, null);
                        defaultValue = "new Vector2(" + defaultVector2Value.x + "f, " + defaultVector2Value.y + "f)";
                        break;
                    case "Vector3":
                        Vector3 defaultVector3Value =
                            (Vector3) method.MakeGenericMethod(typeof(Vector3)).Invoke(parameter, null);
                        defaultValue = "new Vector3(" + defaultVector3Value.x + "f, " + defaultVector3Value.y + "f, " +
                                       defaultVector3Value.z + "f)";
                        break;
                    case "Vector4":
                        Vector4 defaultVector4Value =
                            (Vector4) method.MakeGenericMethod(typeof(Vector4)).Invoke(parameter, null);
                        defaultValue = "new Vector4(" + defaultVector4Value.x + "f, " + defaultVector4Value.y + "f, " +
                                       defaultVector4Value.z + "f)";
                        break;
                    case "Color":
                        Color defaultColorValue =
                            (Color) method.MakeGenericMethod(typeof(Color)).Invoke(parameter, null);
                        defaultValue = "new Color(" + defaultColorValue.r + "f, " + defaultColorValue.g + "f, " +
                                       defaultColorValue.b + "f, " + defaultColorValue.a + "f)";
                        break;
                    case "string":
                        defaultValue = "\"" +
                                       (string) method.MakeGenericMethod(typeof(string)).Invoke(parameter, null) + "\"";
                        break;
                    case "bool":
                        bool defaultBoolValue = (bool) method.MakeGenericMethod(typeof(bool)).Invoke(parameter, null);
                        defaultValue = defaultBoolValue.ToString().ToLower();
                        break;
                    case "Texture":
                        defaultValue = "";
                        break;
                    default:
                        Enum defaultEnumValue = (Enum) method.MakeGenericMethod(typeof(Enum)).Invoke(parameter, null);
                        Type enumSystemType = defaultEnumValue.GetType();
                        string[] splits = enumSystemType.ToString().Split('+');
                        string enumType = splits[splits.Length - 1];
                        string enumConstantName = Enum.GetName(enumSystemType, defaultEnumValue);
                        defaultValue = enumType + "." + enumConstantName;
                        break;
                }
                
                try
                {
                    typeof(UsableProperty).GetMethod("GetPropertyAttributes")
                        .MakeGenericMethod(parameter.GetType()).Invoke(prop, new object[] {parameter});
                }catch (Exception e)
                {
                    Debug.Log(e + " " + parameter.GetType() + " " + prop.type + " "+ prop.name);
                }
            }

            public bool GUI(List<UsableProperty> allUsableProperties)
            {
                bool removeThis = false;
                EditorGUILayout.BeginHorizontal();

                m_TypeIndex = EditorGUILayout.Popup(m_TypeIndex, GetNameWithSortingArray(allUsableProperties),
                    GUILayout.Width(200f));
                type = allUsableProperties[m_TypeIndex].type;
                name = allUsableProperties[m_TypeIndex].name;
                usablePropertyType = allUsableProperties[m_TypeIndex].usablePropertyType;
                propertyInfo = allUsableProperties[m_TypeIndex].propertyInfo;
                fieldInfo = allUsableProperties[m_TypeIndex].fieldInfo;
                usability = allUsableProperties[m_TypeIndex].usability;
                GUILayout.Label(allUsableProperties[m_TypeIndex].type, GUILayout.Width(150f));
                if (GUILayout.Button("Remove", GUILayout.Width(60f)))
                {
                    removeThis = true;
                }

                EditorGUILayout.EndHorizontal();
                return removeThis;
            }

            public int CompareTo(object obj)
            {
                if (obj == null)
                    return 1;

                UsableType other = (UsableType) obj;

                if (other == null)
                    throw new ArgumentException("This object is not a UsableProperty.");

                return name.ToLower().CompareTo(other.name.ToLower());
            }

            public static string[] GetNameWithSortingArray(List<UsableProperty> usableProperties)
            {
                string[] returnVal = new string[usableProperties.Count];
                for (int i = 0; i < returnVal.Length; i++)
                {
                    returnVal[i] = usableProperties[i].name;
                }

                return returnVal;
            }

            public UsableProperty GetDuplicate()
            {
                UsableProperty duplicate = usablePropertyType == UsablePropertyType.Property
                    ? new UsableProperty(propertyInfo)
                    : new UsableProperty(fieldInfo);
                duplicate.defaultValue = defaultValue;
                return duplicate;
            }
        }

        public enum CreationError
        {
            NoError,
            PlayableAssetAlreadyExists,
            PlayableBehaviourAlreadyExists,
            PlayableBehaviourMixerAlreadyExists,
            TrackAssetAlreadyExists,
            PlayableDrawerAlreadyExists,
        }

        public enum WorkType
        {
            Component,
            VolumeComponent
        }

        string m_RootFolderPath = "Assets/TimelineExtensions";

        public bool showHelpBoxes = true;
        public string playableName = "";

        public WorkType workType = WorkType.Component;

        public static UsableType trackBinding;
        public Component defaultValuesComponent;
        // public VolumeComponent defaultValuesVolumeComponent;
        public Volume defaultValuesVolume;
        public List<Variable> exposedReferences = new List<Variable>();
        public List<Variable> playableBehaviourVariables = new List<Variable>();

        public List<UsableProperty> standardBlendPlayableProperties = new List<UsableProperty>();

        public List<UsableProperty> postProcessVolumeProperties = new List<UsableProperty>();

        public ClipCaps clipCaps;

        public Color trackColor = new Color(240 / 255f, 248 / 255f, 255 / 255f);

        // int m_TrackBindingTypeIndex;
        int m_ComponentBindingTypeIndex;
        PropertyInfo[] m_TrackBindingProperties;
        FieldInfo[] m_TrackBindingFields;

        List<UsableProperty> m_TrackBindingUsableProperties = new List<UsableProperty>();

        // List<UsableProperty> m_TrackBindingUsableBlendProperties = new List<UsableProperty>();
        bool m_CreateDrawer;
        bool m_CreateButtonPressed;
        Vector2 m_ScrollViewPos;
        CreationError m_CreationError;

        #region GUIContent

        readonly GUIContent m_ShowHelpBoxesContent =
            new GUIContent("Show Help", "Do you want to see the help boxes as part of this wizard?");

        readonly GUIContent m_PlayableNameContent = new GUIContent("Playable Name",
            "This is the name that will represent the playable.  E.G. TransformTween.  It will be the basis for the class names so it is best not to use the postfixes: 'Clip', 'Behaviour', 'MixerBehaviour' or 'Drawer'.");

        readonly GUIContent m_StandardBlendPlayableContent = new GUIContent("Standard Blend Playable",
            "Often when creating a playable it's intended purpose is just to briefly override the properties of a component for the playable's duration and then blend back to the defaults.  For example a playable that changes the color of a Light but changes it back.  To make a playable with this functionality, check this box.");

        readonly GUIContent m_WorkType =
            new GUIContent("WorkType", "WorkType, now it's only support Component and VolumeComponent");

        readonly GUIContent m_TrackBindingTypeContent =
            new GUIContent("Track Binding Type",
                "This is the type of object the Playable will affect.  E.G. To affect the position choose Transform.");

        readonly GUIContent m_DefaultValuesComponentContent = new GUIContent("Default Values",
            "When the scripts are created, each of the selected properties are assigned a default from the selected Component.  If this is left blank no defaults will be used.");

        readonly GUIContent m_TrackColorContent = new GUIContent("Track Color",
            "Timeline tracks have a colored outline, use this to select that color for your track.");

        readonly GUIContent m_StandardBlendPlayablePropertiesContent = new GUIContent(
            "Standard Blend Playable Properties",
            "Having already selected a Track Binding type, you can select the properties of the bound component you want the playable to affect.  For example, if your playable is bound to a Transform, you can affect the position property.  Note that changing the component binding will clear the list of properties.");

        readonly GUIContent m_PostProcessVolumePropertiesContent =
            new GUIContent("PostProcess Playable Properties",
                "Having already selected a Track Binding type, you can select the properties of the bound component you want the playable to affect.  For example, if your playable is bound to a VolumeComponent(Bloom), you can affect the threshold 、intensity property.  Note that changing the component binding will clear the list of properties.");

        readonly GUIContent m_ClipCapsContent = new GUIContent("Clip Caps",
            "Clip Caps are used to change the way Timelines work with your playables.  For example, enabling Blending will mean that your playables can blend when they overlap and have ease in and out durations.  To find out a little about each hover the cursor over the options.  For details, please see the documentation.");

        readonly GUIContent m_CCNoneContent =
            new GUIContent("None", "Your playable supports none of the features below.");

        readonly GUIContent m_CCLoopingContent = new GUIContent("Looping",
            "Your playable has a specified time that it takes and will start again after it finishes until the clip's duration has played.");

        readonly GUIContent m_CCExtrapolationContent = new GUIContent("Extrapolation",
            "Your playable will persist beyond its end time and its results will continue until the next clip is encountered.");

        readonly GUIContent m_CCClipInContent =
            new GUIContent("Clip In", "Your playable need not be at the start of the Timeline.");

        readonly GUIContent m_CCSpeedMultiplierContent =
            new GUIContent("Speed Multiplier", "Your playable supports changes to the time scale.");

        readonly GUIContent m_CCBlendingContent = new GUIContent("Blending",
            "Your playable supports overlapping of clips to blend between them.");

        readonly GUIContent m_CCAllContent = new GUIContent("All", "Your playable supports all of the above features.");

        #endregion

        const string k_Tab = "    ";
        const string k_ShowHelpBoxesKey = "TimelinePlayableWizard_ShowHelpBoxes";
        const string k_TimelineClipAssetSuffix = "Clip";
        const string k_TimelineClipBehaviourSuffix = "Behaviour";

        const string k_PlayableBehaviourMixerSuffix = "MixerBehaviour";
        const string k_TrackAssetSuffix = "Track";
        const string k_PropertyDrawerSuffix = "Drawer";
        const int k_PlayableNameCharLimit = 64;
        const float k_WindowWidth = 500f;
        const float k_MaxWindowHeight = 800f;
        const float k_ScreenSizeWindowBuffer = 100f;

        static UsableType[] s_ComponentTypes;

        static UsableType[] s_VolumeComponentTypes;

        static UsableType[] s_TrackBindingTypes;
        static UsableType[] s_ExposedReferenceTypes;
        static UsableType[] s_BehaviourVariableTypes;

        static Type[] s_BlendableTypes =
        {
            typeof(float), typeof(int), typeof(double), typeof(Vector2), typeof(Vector3), typeof(Color),
            typeof(FloatParameter), typeof(IntParameter), typeof(Vector2Parameter), typeof(Vector3Parameter),
            typeof(ColorParameter)
        };

        static Type[] s_AssignableTypes =
        {
            typeof(string), typeof(bool)
        };

        static string[] s_DisallowedPropertyNames =
        {
            "name",
        };

        [MenuItem("Window/MAO Timeline Playable Wizard")]
        static void CreateWindow()
        {
            MaoTimelinePlayableWizard wizard =
                GetWindow<MaoTimelinePlayableWizard>(true, "MAO Timeline Playable Wizard", true);

            Vector2 position = Vector2.zero;
            SceneView sceneView = SceneView.lastActiveSceneView;
            if (sceneView != null)
                position = new Vector2(sceneView.position.x, sceneView.position.y);
            wizard.position = new Rect(position.x + k_ScreenSizeWindowBuffer, position.y + k_ScreenSizeWindowBuffer,
                k_WindowWidth,
                Mathf.Min(Screen.currentResolution.height - k_ScreenSizeWindowBuffer, k_MaxWindowHeight));

            wizard.showHelpBoxes = EditorPrefs.GetBool(k_ShowHelpBoxesKey);
            wizard.Show();

            Init();
        }

        static void Init()
        {
            Type[] componentTypes = AppDomain.CurrentDomain.GetAssemblies().SelectMany(a => a.GetTypes())
                .Where(t => typeof(Component).IsAssignableFrom(t)).Where(t => t.IsPublic).ToArray();

            List<UsableType> componentUsableTypesList = UsableType.GetUsableTypeArray(componentTypes).ToList();
            componentUsableTypesList.Sort();
            s_ComponentTypes = componentUsableTypesList.ToArray();

            Type[] volumeComponentTypes = AppDomain.CurrentDomain.GetAssemblies().SelectMany(a => a.GetTypes())
                .Where(t => typeof(VolumeComponent).IsAssignableFrom(t)).Where(t => t.IsPublic).ToArray();

            List<UsableType> volumeComponentUsableTypesList =
                UsableType.GetUsableTypeArray(volumeComponentTypes).ToList();
            volumeComponentUsableTypesList.Sort();
            s_VolumeComponentTypes = volumeComponentUsableTypesList.ToArray();

            UsableType gameObjectUsableType = new UsableType(typeof(GameObject));
            UsableType[] defaultUsableTypes = UsableType.GetUsableTypeArray(componentTypes, gameObjectUsableType);

            List<UsableType> exposedRefTypeList = defaultUsableTypes.ToList();
            exposedRefTypeList.Sort();
            s_ExposedReferenceTypes = exposedRefTypeList.ToArray();

            UsableType noneType = new UsableType((Type) null);
            s_TrackBindingTypes = UsableType.AmalgamateUsableTypes(s_ExposedReferenceTypes, noneType);

            s_BehaviourVariableTypes = UsableType.AmalgamateUsableTypes
            (
                s_ExposedReferenceTypes,
                new UsableType("int"),
                new UsableType("bool"),
                new UsableType("float"),
                new UsableType("Color"),
                new UsableType("double"),
                new UsableType("string"),
                new UsableType("Vector2"),
                new UsableType("Vector3"),
                new UsableType("AudioClip"),
                new UsableType("AnimationCurve")
            );
            List<UsableType> scriptVariableTypeList = s_BehaviourVariableTypes.ToList();
            scriptVariableTypeList.Sort();
            s_BehaviourVariableTypes = scriptVariableTypeList.ToArray();
        }

        void OnGUI()
        {
            if (s_ComponentTypes == null || s_TrackBindingTypes == null || s_ExposedReferenceTypes == null ||
                s_BehaviourVariableTypes == null || s_VolumeComponentTypes == null)
                Init();

            if (s_ComponentTypes == null || s_TrackBindingTypes == null || s_ExposedReferenceTypes == null ||
                s_BehaviourVariableTypes == null || s_VolumeComponentTypes == null)
            {
                EditorGUILayout.HelpBox("Failed to initialise.", MessageType.Error);
                return;
            }

            m_ScrollViewPos = EditorGUILayout.BeginScrollView(m_ScrollViewPos);

            // Show help
            GUIShowHelpPart();


            // Playable name
            bool playableNameNotEmpty;
            bool playableNameFormatted;
            bool playableNameTooLong;
            GUIPlayableNamePart(out playableNameNotEmpty, out playableNameFormatted, out playableNameTooLong);


            // Work type
            WorkType oldWorkType = workType;
            GUIWorkTypePart();


            // Track binding type
            int oldIndex;
            GUITrackBindingTypePart(out oldIndex);


            // Property
            GUIPropertyPart(oldIndex, oldWorkType);


            // Track Color
            GUITrackColorPart();


            // Create
            GUICreatePart(playableNameNotEmpty, playableNameFormatted, playableNameTooLong);


            // Reset
            GUIResetPart();

            EditorGUILayout.EndScrollView();
        }

        void GUIShowHelpPart()
        {
            bool oldShowHelpBoxes = showHelpBoxes;
            showHelpBoxes = EditorGUILayout.Toggle(m_ShowHelpBoxesContent, showHelpBoxes);
            if (oldShowHelpBoxes != showHelpBoxes)
            {
                EditorPrefs.SetBool(k_ShowHelpBoxesKey, showHelpBoxes);
                EditorGUILayout.Space();
            }

            if (showHelpBoxes)
            {
                EditorGUILayout.HelpBox(
                    "This wizard is used to create the basics of a custom playable for the Timeline. "
                    + "It will create 4 scripts that you can then edit to complete their functionality. "
                    + "The purpose is to setup the boilerplate code for you.  If you are already familiar "
                    + "with playables and the Timeline, you may wish to create your own scripts instead.",
                    MessageType.None);
                EditorGUILayout.Space();
            }

            EditorGUILayout.Space();
            EditorGUILayout.Space();
        }

        void GUIPlayableNamePart(out bool playableNameNotEmpty, out bool playableNameFormatted,
            out bool playableNameTooLong)
        {
            EditorGUILayout.BeginVertical(GUI.skin.box);
            ShowHelpBoxInfo(showHelpBoxes, m_PlayableNameContent);

            playableName = EditorGUILayout.TextField(m_PlayableNameContent, playableName);

            playableNameNotEmpty = !string.IsNullOrEmpty(playableName);
            playableNameFormatted = CodeGenerator.IsValidLanguageIndependentIdentifier(playableName);
            if (!playableNameNotEmpty || !playableNameFormatted)
            {
                EditorGUILayout.HelpBox(
                    "The Playable needs a name which starts with a capital letter and contains no spaces or special characters.",
                    MessageType.Error);
            }

            playableNameTooLong = playableName.Length > k_PlayableNameCharLimit;
            if (playableNameTooLong)
            {
                EditorGUILayout.HelpBox(
                    "The Playable needs a name which is fewer than " + k_PlayableNameCharLimit + " characters long.",
                    MessageType.Error);
            }

            EditorGUILayout.EndVertical();

            EditorGUILayout.Space();
            EditorGUILayout.Space();
        }

        void GUIWorkTypePart()
        {
            EditorGUILayout.BeginVertical(GUI.skin.box);
            // TODO: Edit the help tooltip
            ShowHelpBoxInfo(showHelpBoxes, m_WorkType);

            workType = (WorkType) EditorGUILayout.EnumPopup("Work type", workType);

            EditorGUILayout.EndVertical();

            EditorGUILayout.Space();
            EditorGUILayout.Space();
        }

        void GUITrackBindingTypePart(out int oldIndex)
        {
            EditorGUILayout.BeginVertical(GUI.skin.box);
            ShowHelpBoxInfo(showHelpBoxes, m_TrackBindingTypeContent);

            oldIndex = m_ComponentBindingTypeIndex;
            if (workType == WorkType.Component)
            {
                m_ComponentBindingTypeIndex = EditorGUILayout.Popup(m_TrackBindingTypeContent,
                    m_ComponentBindingTypeIndex, UsableType.GetGUIContentWithSortingArray(s_ComponentTypes));
                trackBinding = s_ComponentTypes[m_ComponentBindingTypeIndex];

                EditorGUILayout.Space();

                defaultValuesComponent = EditorGUILayout.ObjectField(m_DefaultValuesComponentContent,
                        defaultValuesComponent, trackBinding.type, true)
                    as Component;
            }
            else if (workType == WorkType.VolumeComponent)
            {
                m_ComponentBindingTypeIndex = EditorGUILayout.Popup(m_TrackBindingTypeContent,
                    m_ComponentBindingTypeIndex, UsableType.GetGUIContentWithSortingArray(s_VolumeComponentTypes));
                trackBinding = s_VolumeComponentTypes[m_ComponentBindingTypeIndex];

                EditorGUILayout.Space();

                // TODO: Automatically obtain the Global Volume in the scenario where the user is creating a Volume Component
                defaultValuesVolume = EditorGUILayout.ObjectField(m_DefaultValuesComponentContent, defaultValuesVolume,
                    typeof(Volume), true) as Volume;
            }

            EditorGUILayout.EndVertical();
        }

        void GUIPropertyPart(int oldIndex, WorkType oldWorkType)
        {
            if (workType == WorkType.Component)
            {
                StandardBlendPlayablePropertyGUI(oldIndex != m_ComponentBindingTypeIndex ||
                                                 oldWorkType != WorkType.Component);
            }
            else
            {
                VolumeComponentPropertyGUI(oldIndex != m_ComponentBindingTypeIndex ||
                                           oldWorkType != WorkType.VolumeComponent);
            }

            EditorGUILayout.Space();
            EditorGUILayout.Space();
        }

        void GUITrackColorPart()
        {
            EditorGUILayout.BeginVertical(GUI.skin.box);

            ShowHelpBoxInfo(showHelpBoxes, m_TrackColorContent);

            trackColor = EditorGUILayout.ColorField(m_TrackColorContent, trackColor);
            EditorGUILayout.EndVertical();
        }

        void GUICreatePart(bool playableNameNotEmpty, bool playableNameFormatted, bool playableNameTooLong)
        {
            if (playableNameNotEmpty && playableNameFormatted
                                     // && allUniqueVariableNames && exposedVariablesNamesValid && scriptVariablesNamesValid 
                                     && !playableNameTooLong)
            {
                if (GUILayout.Button("Create", GUILayout.Width(60f)))
                {
                    m_CreateButtonPressed = true;

                    if (workType == WorkType.Component)
                    {
                        foreach (var prop in standardBlendPlayableProperties)
                        {
                            prop.CreateSettingDefaultValueString(defaultValuesComponent);
                        }
                    }
                    else if (workType == WorkType.VolumeComponent)
                    {
                        var genericMethod = typeof(UsableProperty).GetMethod("CreateSettingDefaultValueStringVolume")
                            .MakeGenericMethod(trackBinding.type);

                        foreach (var prop in postProcessVolumeProperties)
                        {
                            genericMethod.Invoke(prop, new object[] {defaultValuesVolume, prop});
                        }
                    }

                    m_CreationError = CreateScripts();

                    if (m_CreationError == CreationError.NoError)
                    {
                        Close();
                    }
                }
            }

            EditorGUILayout.Space();
            EditorGUILayout.Space();

            CheckCreateButtonPressed();
        }

        void CheckCreateButtonPressed()
        {
            if (m_CreateButtonPressed)
            {
                switch (m_CreationError)
                {
                    case CreationError.NoError:
                        EditorGUILayout.HelpBox("Playable was successfully created.", MessageType.Info);
                        break;
                    case CreationError.PlayableAssetAlreadyExists:
                        EditorGUILayout.HelpBox(
                            "The type " + playableName + k_TimelineClipAssetSuffix +
                            " already exists, no files were created.", MessageType.Error);
                        break;
                    case CreationError.PlayableBehaviourAlreadyExists:
                        EditorGUILayout.HelpBox(
                            "The type " + playableName + k_TimelineClipBehaviourSuffix +
                            " already exists, no files were created.", MessageType.Error);
                        break;
                    case CreationError.PlayableBehaviourMixerAlreadyExists:
                        EditorGUILayout.HelpBox(
                            "The type " + playableName + k_PlayableBehaviourMixerSuffix +
                            " already exists, no files were created.", MessageType.Error);
                        break;
                    case CreationError.TrackAssetAlreadyExists:
                        EditorGUILayout.HelpBox(
                            "The type " + playableName + k_TrackAssetSuffix + " already exists, no files were created.",
                            MessageType.Error);
                        break;
                    case CreationError.PlayableDrawerAlreadyExists:
                        EditorGUILayout.HelpBox(
                            "The type " + playableName + k_PropertyDrawerSuffix +
                            " already exists, no files were created.", MessageType.Error);
                        break;
                }
            }
        }

        void GUIResetPart()
        {
            if (GUILayout.Button("Reset", GUILayout.Width(60f)))
            {
                ResetWindow();
            }
        }

        void StandardBlendPlayablePropertyGUI(bool findNewProperties)
        {
            if (findNewProperties || m_TrackBindingProperties == null && m_TrackBindingFields == null)
            {
                m_TrackBindingUsableProperties.Clear();

                IEnumerable<PropertyInfo> propertyInfos = trackBinding.type.GetProperties(
                    BindingFlags.Instance | BindingFlags.Public | BindingFlags.SetProperty | BindingFlags.GetProperty);
                propertyInfos =
                    propertyInfos.Where(x => IsTypeBlendable(x.PropertyType) || IsTypeAssignable(x.PropertyType));
                propertyInfos = propertyInfos.Where(x => x.CanWrite && x.CanRead);
                propertyInfos = propertyInfos.Where(x => HasAllowedName(x));
                // Uncomment the below to stop Obsolete properties being selectable.
                //propertyInfos = propertyInfos.Where (x => !Attribute.IsDefined (x, typeof(ObsoleteAttribute)));
                m_TrackBindingProperties = propertyInfos.ToArray();
                foreach (PropertyInfo trackBindingProperty in m_TrackBindingProperties)
                {
                    m_TrackBindingUsableProperties.Add(new UsableProperty(trackBindingProperty));
                }

                IEnumerable<FieldInfo> fieldInfos =
                    trackBinding.type.GetFields(BindingFlags.Instance | BindingFlags.Public);
                fieldInfos = fieldInfos.Where(x => IsTypeBlendable(x.FieldType) || IsTypeAssignable(x.FieldType));
                m_TrackBindingFields = fieldInfos.ToArray();
                foreach (FieldInfo trackBindingField in m_TrackBindingFields)
                {
                    m_TrackBindingUsableProperties.Add(new UsableProperty(trackBindingField));
                }

                m_TrackBindingUsableProperties = m_TrackBindingUsableProperties.OrderBy(x => x.name).ToList();
                standardBlendPlayableProperties.Clear();
            }

            EditorGUILayout.BeginVertical(GUI.skin.box);

            ShowHelpBoxInfo(showHelpBoxes, m_StandardBlendPlayablePropertiesContent);

            EditorGUILayout.LabelField(m_StandardBlendPlayablePropertiesContent);

            int indexToRemove = -1;
            for (int i = 0; i < standardBlendPlayableProperties.Count; i++)
            {
                if (standardBlendPlayableProperties[i].GUI(m_TrackBindingUsableProperties))
                    indexToRemove = i;
            }

            if (indexToRemove != -1)
                standardBlendPlayableProperties.RemoveAt(indexToRemove);

            if (GUILayout.Button("Add", GUILayout.Width(40f)))
            {
                standardBlendPlayableProperties.Add(m_TrackBindingUsableProperties[0].GetDuplicate());
            }

            if (standardBlendPlayableProperties.Any(IsObsolete))
                EditorGUILayout.HelpBox(
                    "One or more of your chosen properties are marked 'Obsolete'.  Consider changing them to avoid deprecation with future versions of Unity.",
                    MessageType.Warning);

            EditorGUILayout.EndVertical();
        }

        void VolumeComponentPropertyGUI(bool findNewProperties)
        {
            if (findNewProperties || m_TrackBindingProperties == null && m_TrackBindingFields == null)
            {
                m_TrackBindingUsableProperties.Clear();

                IEnumerable<PropertyInfo> propertyInfos = trackBinding.type.GetProperties();
                propertyInfos = propertyInfos.Where(x => x.Name == "parameters");

                m_TrackBindingProperties = propertyInfos.ToArray();

                IEnumerable<FieldInfo> fieldInfos = trackBinding.type.GetFields();

                m_TrackBindingFields = fieldInfos.ToArray();
                foreach (FieldInfo trackBindingField in m_TrackBindingFields)
                {
                    if (trackBindingField.Name == "active")
                    {
                        continue;
                    }

                    m_TrackBindingUsableProperties.Add(new UsableProperty(trackBindingField));
                }

                // fieldInfos = fieldInfos.Where(x => IsTypeBlendable(x.FieldType));
                // m_TrackBindingFields = fieldInfos.ToArray();
                // foreach (FieldInfo trackBindingField in m_TrackBindingFields)
                // {
                //     m_TrackBindingUsableBlendProperties.Add(new UsableProperty(trackBindingField));
                // }

                m_TrackBindingUsableProperties = m_TrackBindingUsableProperties.ToList();
                // m_TrackBindingUsableBlendProperties = m_TrackBindingUsableBlendProperties.ToList();

                postProcessVolumeProperties.Clear();
            }

            // Standard Blend Playable Properties
            EditorGUILayout.BeginVertical(GUI.skin.box);

            // normal properties
            ShowHelpBoxInfo(showHelpBoxes, m_PostProcessVolumePropertiesContent);

            EditorGUILayout.LabelField(m_PostProcessVolumePropertiesContent);

            int indexToRemove = -1;
            for (int i = 0; i < postProcessVolumeProperties.Count; i++)
            {
                if (postProcessVolumeProperties[i].GUI(m_TrackBindingUsableProperties))
                    indexToRemove = i;
            }

            if (indexToRemove != -1)
                postProcessVolumeProperties.RemoveAt(indexToRemove);

            if (GUILayout.Button("Add", GUILayout.Width(40f)))
            {
                postProcessVolumeProperties.Add(m_TrackBindingUsableProperties[0].GetDuplicate());
            }

            if (postProcessVolumeProperties.Any(IsObsolete))
                EditorGUILayout.HelpBox(
                    "One or more of your chosen properties are marked 'Obsolete'.  Consider changing them to avoid deprecation with future versions of Unity.",
                    MessageType.Warning);

            var q = postProcessVolumeProperties.GroupBy(x => x.name).Where(g => g.Count() > 1)
                .Select(y => y.Key).ToList();
            if (q.Count > 0)
                EditorGUILayout.HelpBox("Cannot have the same attribute", MessageType.Error);


            EditorGUILayout.EndVertical();
        }

        static bool IsTypeBlendable(Type type)
        {
            for (int i = 0; i < s_BlendableTypes.Length; i++)
            {
                if (type == s_BlendableTypes[i] || type.BaseType == s_BlendableTypes[i])
                    return true;
            }

            return false;
        }

        static bool IsTypeAssignable(Type type)
        {
            for (int i = 0; i < s_AssignableTypes.Length; i++)
            {
                if (type == s_AssignableTypes[i] || type.IsEnum)
                    return true;
            }

            return false;
        }

        static bool HasAllowedName(PropertyInfo propertyInfo)
        {
            for (int i = 0; i < s_DisallowedPropertyNames.Length; i++)
            {
                if (propertyInfo.Name == s_DisallowedPropertyNames[i])
                    return false;
            }

            return true;
        }

        static bool IsObsolete(UsableProperty usableProperty)
        {
            if (usableProperty.usablePropertyType == UsableProperty.UsablePropertyType.Field)
                return Attribute.IsDefined(usableProperty.fieldInfo, typeof(ObsoleteAttribute));
            return Attribute.IsDefined(usableProperty.propertyInfo, typeof(ObsoleteAttribute));
        }

        bool VariableListGUI(List<Variable> variables, UsableType[] usableTypes, GUIContent guiContent, string newName)
        {
            EditorGUILayout.BeginVertical(GUI.skin.box);

            ShowHelpBoxInfo(showHelpBoxes, guiContent);

            EditorGUILayout.LabelField(guiContent);

            int indexToRemove = -1;
            bool allNamesValid = true;
            for (int i = 0; i < variables.Count; i++)
            {
                if (variables[i].GUI(usableTypes))
                    indexToRemove = i;

                if (!CodeGenerator.IsValidLanguageIndependentIdentifier(variables[i].name))
                {
                    allNamesValid = false;
                }
            }

            if (indexToRemove != -1)
                variables.RemoveAt(indexToRemove);

            if (GUILayout.Button("Add", GUILayout.Width(40f)))
                variables.Add(new Variable(newName, usableTypes[0]));

            if (!allNamesValid)
                EditorGUILayout.HelpBox(
                    "One of the variables has an invalid character, make sure they don't contain any spaces or special characters.",
                    MessageType.Error);

            EditorGUILayout.EndVertical();

            return allNamesValid;
        }

        bool AllVariablesUniquelyNamed()
        {
            for (int i = 0; i < exposedReferences.Count; i++)
            {
                string exposedRefName = exposedReferences[i].name;

                for (int j = 0; j < exposedReferences.Count; j++)
                {
                    if (i != j && exposedRefName == exposedReferences[j].name)
                        return false;
                }

                for (int j = 0; j < playableBehaviourVariables.Count; j++)
                {
                    if (exposedRefName == playableBehaviourVariables[j].name)
                        return false;
                }
            }

            for (int i = 0; i < playableBehaviourVariables.Count; i++)
            {
                string scriptPlayableVariableName = playableBehaviourVariables[i].name;

                for (int j = 0; j < exposedReferences.Count; j++)
                {
                    if (scriptPlayableVariableName == exposedReferences[j].name)
                        return false;
                }

                for (int j = 0; j < playableBehaviourVariables.Count; j++)
                {
                    if (i != j && scriptPlayableVariableName == playableBehaviourVariables[j].name)
                        return false;
                }
            }

            return true;
        }

        void ClipCapsGUI()
        {
            EditorGUILayout.BeginVertical(GUI.skin.box);

            ShowHelpBoxInfo(showHelpBoxes, m_ClipCapsContent);

            EditorGUILayout.LabelField(m_ClipCapsContent);

            bool isLooping = (clipCaps & ClipCaps.Looping) == ClipCaps.Looping;
            bool isExtrapolation = (clipCaps & ClipCaps.Extrapolation) == ClipCaps.Extrapolation;
            bool isClipIn = (clipCaps & ClipCaps.ClipIn) == ClipCaps.ClipIn;
            bool isSpeedMultiplier = (clipCaps & ClipCaps.SpeedMultiplier) == ClipCaps.SpeedMultiplier;
            bool isBlending = (clipCaps & ClipCaps.Blending) == ClipCaps.Blending;

            bool isNone = !isLooping && !isExtrapolation && !isClipIn && !isSpeedMultiplier && !isBlending;
            bool isAll = isLooping && isExtrapolation && isClipIn && isSpeedMultiplier && isBlending;

            EditorGUI.BeginChangeCheck();
            isNone = EditorGUILayout.ToggleLeft(m_CCNoneContent, isNone);
            if (EditorGUI.EndChangeCheck())
            {
                if (isNone)
                {
                    isLooping = false;
                    isExtrapolation = false;
                    isClipIn = false;
                    isSpeedMultiplier = false;
                    isBlending = false;
                    isAll = false;
                }
            }

            EditorGUI.BeginChangeCheck();
            isLooping = EditorGUILayout.ToggleLeft(m_CCLoopingContent, isLooping);
            isExtrapolation = EditorGUILayout.ToggleLeft(m_CCExtrapolationContent, isExtrapolation);
            isClipIn = EditorGUILayout.ToggleLeft(m_CCClipInContent, isClipIn);
            isSpeedMultiplier = EditorGUILayout.ToggleLeft(m_CCSpeedMultiplierContent, isSpeedMultiplier);
            isBlending = EditorGUILayout.ToggleLeft(m_CCBlendingContent, isBlending);
            if (EditorGUI.EndChangeCheck())
            {
                isNone = !isLooping && !isExtrapolation && !isClipIn && !isSpeedMultiplier && !isBlending;
                isAll = isLooping && isExtrapolation && isClipIn && isSpeedMultiplier && isBlending;
            }

            EditorGUI.BeginChangeCheck();
            isAll = EditorGUILayout.ToggleLeft(m_CCAllContent, isAll);
            if (EditorGUI.EndChangeCheck())
            {
                if (isAll)
                {
                    isNone = false;
                    isLooping = true;
                    isExtrapolation = true;
                    isClipIn = true;
                    isSpeedMultiplier = true;
                    isBlending = true;
                }
            }

            EditorGUILayout.EndVertical();

            clipCaps = ClipCaps.None;

            if (isNone)
                return;

            if (isAll)
            {
                clipCaps = ClipCaps.All;
                return;
            }

            if (isLooping)
                clipCaps |= ClipCaps.Looping;

            if (isExtrapolation)
                clipCaps |= ClipCaps.Extrapolation;

            if (isClipIn)
                clipCaps |= ClipCaps.ClipIn;

            if (isSpeedMultiplier)
                clipCaps |= ClipCaps.SpeedMultiplier;

            if (isBlending)
                clipCaps |= ClipCaps.Blending;
        }

        static void ShowHelpBoxInfo(bool showHelpBoxes, GUIContent content)
        {
            if (showHelpBoxes)
            {
                EditorGUILayout.HelpBox(content.tooltip, MessageType.Info);
                EditorGUILayout.Space();
            }
        }

        void ResetWindow()
        {
            playableName = "";
            workType = WorkType.Component;
            trackBinding = s_TrackBindingTypes[0];
            defaultValuesComponent = null;
            defaultValuesVolume = null;
            // defaultValuesVolumeComponent = null;
            exposedReferences = new List<Variable>();
            playableBehaviourVariables = new List<Variable>();
            standardBlendPlayableProperties = new List<UsableProperty>();
            postProcessVolumeProperties = new List<UsableProperty>();
            clipCaps = ClipCaps.None;
            trackColor = new Color(240 / 255f, 248 / 255f, 255 / 255f); // Alice-Blue ~

            m_ComponentBindingTypeIndex = 0;
            m_TrackBindingProperties = null;
            m_TrackBindingFields = null;
            m_TrackBindingUsableProperties = new List<UsableProperty>();
            m_CreateDrawer = false;
        }
    }
}