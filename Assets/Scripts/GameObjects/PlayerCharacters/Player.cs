using DiceyDungeonsAR.MyLevelGraph;
using System;
using UnityEngine;
using UnityEngine.Animations;

namespace DiceyDungeonsAR.GameObjects.Players
{
    public abstract class Player : MonoBehaviour
    {
        LevelGraph levelGraph;
        public Field currentField = null;
        Field targetField = null;
        float targetTime = -1;
        public abstract int MaxHealth { get; protected set; }
        protected int health;
        public int Health
        {
            get => health;
            private set
            {
                health = Mathf.Min(MaxHealth, value);
            }
        }
        public int Level { get; private set; } = 1;
        public int Experience { get; private set; } = 0;
        public int XPToNextLevel { get; private set; } = 2;

        public void Initialize()
        {
            health = MaxHealth;

            GameObject level = GameObject.FindWithTag("Level");
            if (level == null)
            {
                Debug.LogWarning("No level was found");
                return;
            }

            levelGraph = level.GetComponent<LevelGraph>();
            if (levelGraph == null) {
                Debug.LogWarning("Component LevelGraph wasn't found");
                return;
            }

 
            currentField = levelGraph.fields[0];
            targetField = currentField;
            transform.position = currentField.transform.position + new Vector3(0, 1f * targetField.transform.localScale.y, 0);
            currentField.MarkAdjacentFields();
        }

        void FixedUpdate()
        {
            if (targetField != currentField)
            {
                var offset = new Vector3(0, 1f * targetField.transform.localScale.y, 0);
                transform.position = Vector3.Lerp(currentField.transform.position, targetField.transform.position, Mathf.Min(1 - (targetTime-Time.time)/0.3f, 1)) + offset;
                if (Time.time >= targetTime) {
                    currentField = targetField;
                    if (targetField.PlacedItem != null)
                        targetField.PlacedItem.UseByPlayer(this);
                }
            }
        }

        public bool PlacePlayer(Field field)
        {
            if (!currentField.ConnectedFields().Contains(field))
            {
                return false;
            }
            field.MarkAdjacentFields();

            var targetPosition = field.transform.position;
            targetPosition.y = transform.position.y;
            transform.rotation = Quaternion.LookRotation(targetPosition - transform.position);

            targetField = field;
            targetTime = Time.time + 0.3f;

            return true;
        }

        public void AddXP(int experience)
        {
            if (experience <= 0)
                throw new ArgumentException();
            Experience += experience;
        }

        private void LevelUp()
        {

        }
    }
}
