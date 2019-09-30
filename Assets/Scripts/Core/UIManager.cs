using System;
using System.Collections;
using System.Collections.Generic;
using Timespawn.UnityEcsBspDungeon.Components;
using UnityEngine;
using UnityEngine.UI;

namespace Timespawn.UnityEcsBspDungeon.Core
{
    public class UIManager : MonoBehaviour
    {
        [Header("Room")] 
        public InputField MinRoomLengthInputField;
        public InputField MaxRoomLengthInputField;

        [Header("BSP Split Ratio")]
        public Slider MinSplitRatioSlider;
        public Text MinSplitRatioText;
        public Slider MaxSplitRatioSlider;
        public Text MaxSplitRatioText;

        [Header("Extra Paths")] 
        public InputField ExtraPathsInputField;

        [Header("Seed")]
        public InputField SeedInputField;
        public Toggle FixedSeedToggle;

        private static UIManager PrivateInstance;

        public static UIManager Instance()
        {
            return PrivateInstance;
        }

        private void Awake()
        {
            if (PrivateInstance)
            {
                Destroy(this);
            }
            else
            {
                PrivateInstance = this;
            }
        }

        public void Init(DungeonComponent dungeonComp)
        {
            MinRoomLengthInputField.text = dungeonComp.MinRoomLengthInCells.ToString();
            MaxRoomLengthInputField.text = dungeonComp.MaxRoomLengthInCells.ToString();

            MaxSplitRatioSlider.value = 1.0f;
            MinSplitRatioSlider.value = dungeonComp.MinSplitRatio;
            MaxSplitRatioSlider.value = dungeonComp.MaxSplitRatio;

            ExtraPathsInputField.text = dungeonComp.ExtraPathNum.ToString();

            SeedInputField.text = "0";
            FixedSeedToggle.isOn = false;
        }

        public void MinRoomLengthInputField_OnEndEdit(String inputString)
        {
            int value = int.Parse(inputString);
            int maxValue = int.Parse(MaxRoomLengthInputField.text);
            int clampedValue = Mathf.Clamp(value, 1, maxValue);
            MinRoomLengthInputField.text = clampedValue.ToString();
        }

        public void MaxRoomLengthInputField_OnEndEdit(String inputString)
        {
            int value = int.Parse(inputString);
            int minValue = int.Parse(MinRoomLengthInputField.text);
            int clampedValue = Mathf.Clamp(value, minValue, 999);
            MaxRoomLengthInputField.text = clampedValue.ToString();
        }

        public void ExtraPathsInputField_OnEndEdit(String inputString)
        {
            int value = int.Parse(inputString);
            int clampedValue = Mathf.Clamp(value, 0, 99);
            ExtraPathsInputField.text = clampedValue.ToString();
        }

        public void MinSplitRatioSlider_OnValueChanged(float value)
        {
            float clampedValue = Mathf.Clamp(value, 0, MaxSplitRatioSlider.value);
            MinSplitRatioSlider.value = clampedValue;
            MinSplitRatioText.text = clampedValue.ToString("0.00");
        }

        public void MaxSplitRatioSlider_OnValueChanged(float value)
        {
            float clampedValue = Mathf.Clamp(value, MinSplitRatioSlider.value, 1);
            MaxSplitRatioSlider.value = clampedValue;
            MaxSplitRatioText.text = clampedValue.ToString("0.00");
        }

        public void SeedInputField_OnEndEdit(String inputString)
        {
            // Fixed seed
            FixedSeedToggle.isOn = true;

            // Validate value
            int seed = 0;
            if (!int.TryParse(inputString, out seed))
            {
                SeedInputField.text = int.MaxValue.ToString();
            }
        }

        public void GenerateButton_OnClick()
        {
            // Dungeon
            DungeonComponent dungeonCompData = new DungeonComponent
            {
                MinRoomLengthInCells = int.Parse(MinRoomLengthInputField.text),
                MaxRoomLengthInCells = int.Parse(MaxRoomLengthInputField.text),
                MinSplitRatio = MinSplitRatioSlider.value,
                MaxSplitRatio = MaxSplitRatioSlider.value,
                ExtraPathNum = int.Parse(ExtraPathsInputField.text),
            };

            // Seed
            int seed = 0;
            if (FixedSeedToggle.isOn)
            {
                // Fixed seed
                if (!int.TryParse(SeedInputField.text, out seed))
                {
                    seed = int.MaxValue;
                }
            }
            else
            {
                // New seed
                seed = Environment.TickCount;
                SeedInputField.text = seed.ToString();
            }
            
            GameManager.Instance().GenerateDungeon(dungeonCompData, seed);
        }
    }
}