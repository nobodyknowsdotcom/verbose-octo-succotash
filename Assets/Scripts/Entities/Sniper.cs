using UnityEngine;
using Random = System.Random;

namespace Entities
{
    public class Sniper : Unit
    {
        private readonly Random m_Random = new Random();
        protected Sniper(string name, bool isAlly, int maintenancePrice, int damage, int health, int armor, double dodgeChance, double accuracy, int movingRange, Sprite sprite) : base(name, isAlly, maintenancePrice, damage, health, armor, dodgeChance, accuracy, movingRange, sprite)
        {
        }

        public static Sniper CreateInstance(string name, bool isAlly, int maintenancePrice, int damage, int health, int armor, double dodgeChance, double accuracy)
        {
            var sprite = Resources.Load<Sprite>(isAlly ? "Warriors/ally_sniper" : "Warriors/enemy_sniper");
            var movingRange = 3;
            return new Sniper(name, isAlly, maintenancePrice, damage, health, armor, dodgeChance, accuracy, movingRange, sprite);
        }

        public override void Ability1(Unit enemy)
        {
            if (m_Random.NextDouble() <= Accuracy - 0.1)
            {
                enemy.Health = HealthCalc(1.5, 1, -0.1, 1, enemy);
            }
            else
            {
                Debug.Log("Ты промазал!");
            }

            IsUsedAbility = true;
        }

        public void PreciseShot(Unit enemy)
        {
            MovingRange = 0;
            enemy.Health = HealthCalc(2, 1, 0, 1, enemy);
        }

        public void Destroy(Unit enemy)
        {
            enemy.Health = HealthCalc(1.5, 1, 0, 3, enemy);
        }
    }
}