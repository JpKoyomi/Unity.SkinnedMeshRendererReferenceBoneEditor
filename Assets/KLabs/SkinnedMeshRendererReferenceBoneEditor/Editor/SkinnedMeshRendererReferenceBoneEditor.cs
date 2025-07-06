using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace KLabs.Utils.Editor
{
	internal sealed class SkinnedMeshRendererReferenceBoneEditor : EditorWindow
	{
		public const string TITLE = "Skinned Mesh Renderer Reference Bone Editor";

		[MenuItem("Tools/KLabs/Skinned Mesh Renderer Reference Bone Editor")]
		public static void EditorWindowShow()
		{
			var window = CreateWindow<SkinnedMeshRendererReferenceBoneEditor>();
			window.titleContent = new GUIContent(TITLE);
			window.Show();
		}

		private SkinnedMeshRenderer Source { get; set; }

		private float[] BoneWeights { get; set; }

		private ListView ListView { get; set; }

		private void CreateGUI()
		{
			var root = rootVisualElement;
			var sourceField = new ObjectField("Source")
			{
				objectType = typeof(SkinnedMeshRenderer)
			};
			sourceField.RegisterCallback<ChangeEvent<Object>>(ChangeSourceRendererCallback);
			root.Add(sourceField);

			ListView = new ListView(new Transform[0])
			{
				makeItem = MakeItem,
				bindItem = BindItem
			};

			root.Add(ListView);
		}

		private VisualElement MakeItem()
		{
			var item = new VisualElement();
			item.style.flexDirection = FlexDirection.Row;
			var o = new ObjectField
			{
				objectType = typeof(Transform),
				userData = null
			};
			o.style.flexGrow = 1;
			o.style.flexBasis = 100;
			item.Add(o);
			o.RegisterCallback<ChangeEvent<Object>>((e) =>
			{
				var newValue = e.newValue as Transform;
				if (newValue != null && o.userData is int index && Source.bones[index] != newValue)
				{
					var bones = (Transform[])Source.bones.Clone();
					bones[index] = newValue;
					Undo.RecordObject(Source, "Change SkinnedMeshRenderer bone reference");
					Source.bones = bones;
				}
			});
			var f = new FloatField();
			f.style.width = 60;
			item.Add(f);
			return item;
		}

		void BindItem(VisualElement item, int index)
		{
			var o = item.Q<ObjectField>();
			var f = item.Q<FloatField>();
			o.value = Source.bones[index];
			o.userData = index;
			f.value = BoneWeights[index];
		}

		private void ChangeSourceRendererCallback(ChangeEvent<Object> e)
		{
			Source = e.newValue as SkinnedMeshRenderer;
			if (Source != null)
			{
				CalcBoneWeightsPercentage();
				ListView.itemsSource = Source.bones;
			}
			else
			{
				CalcBoneWeightsPercentage();
				ListView.itemsSource = new Transform[0];
			}
		}

		private void CalcBoneWeightsPercentage()
		{
			if (Source != null)
			{
				if (Source.sharedMesh != null)
				{
					var mesh = Source.sharedMesh;

					var sum = 0.0;
					var weights = mesh.boneWeights;
					var boneWeights = new double[Source.bones.Length];
					for (int i = 0; i < weights.Length; i++)
					{
						var w = weights[i];
						boneWeights[w.boneIndex0] += w.weight0;
						boneWeights[w.boneIndex1] += w.weight1;
						boneWeights[w.boneIndex2] += w.weight2;
						boneWeights[w.boneIndex3] += w.weight3;
						sum += w.weight0 + w.weight1 + w.weight2 + w.weight3;
					}
					BoneWeights = new float[boneWeights.Length];
					for (int i = 0; i < boneWeights.Length; i++)
					{
						var value = System.Math.Ceiling((boneWeights[i] / sum) * 1000.0);
						BoneWeights[i] = boneWeights[i] == 0 ? 0 : value < 1 ? 0.001f : (float)(value * 0.001);
					}
				}
				else
				{
					BoneWeights = new float[Source.bones.Length];
				}
			}
			else
			{
				BoneWeights = null;
			}
		}
	}
}
