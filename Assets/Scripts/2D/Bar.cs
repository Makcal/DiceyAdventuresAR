using System;
using DiceyAdventuresAR.MyLevelGraph;
using UnityEngine;
using UnityEngine.UI;

namespace DiceyAdventuresAR.UI
{
	[RequireComponent(typeof(Slider))]
	public class Bar : MonoBehaviour
	{
		[NonSerialized] public float maxValue = 100;
		[NonSerialized] public float startValue = 0;
		public Color32 mainColor = new Color32(255, 110, 110, 255);
		public Color32 backgroundColor = new Color32(140, 43, 43, 255);
		Slider slider;

		public float MaxValue
		{
			get => slider.maxValue;
			set
			{
				slider.maxValue = value;
				GetComponentInChildren<Text>().text = $"{CurrentValue} / {MaxValue}";
			}
		}

		public float CurrentValue 
		{
			get => slider.value;
			set
            {
				slider.value = value;
				GetComponentInChildren<Text>().text = $"{value} / {MaxValue}";
            }
		}

        public void Start()
		{
			slider = GetComponent<Slider>();

			var color = mainColor;
            slider.fillRect.GetComponent<Image>().color = color;

			color = backgroundColor;
			slider.transform.GetChild(0).GetComponent<Image>().color = color;

            MaxValue = maxValue;
			slider.minValue = 0;
			CurrentValue = startValue;
		}

		static public Bar CreateBar(Transform canvasTr, Vector2 anchorMin, Vector2 anchorMax)
		{
			Bar bar = Instantiate(LevelGraph.levelGraph.battle.barPrefab, canvasTr);
			var barTr = (RectTransform)bar.transform;
			barTr.anchorMin = anchorMin;
			barTr.anchorMax = anchorMax;

			return bar;
		}
	}
}
