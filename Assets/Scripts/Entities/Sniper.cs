using UnityEngine;
using Random = System.Random;

namespace Entities
{
    public class Sniper : Unit
    {
        private readonly Random m_Random = new Random();
        protected Sniper(string name, bool isAlly, int buyPrice, int maintenancePrice, int damage, int health, int armor, double dodgeChance, double accuracy, int movingRange, Sprite sprite) : base(name, isAlly, buyPrice, maintenancePrice, damage, health, armor, dodgeChance, accuracy, movingRange, sprite)
        {
        }

        public static Sniper CreateInstance(bool isAlly)
        {
            var sprite = Resources.Load<Sprite>(isAlly ? "Warriors/ally_sniper" : "Warriors/enemy_sniper");
            var movingRange = 3;
            var name = "Снайпер";
            var buyPrice = 200;
            var maintenancePrice = 20;
            var health = 40;
            var damage = 45;
            var armor = 60;
            var dodge = 0.15;
            var accuracy = 0.95;
            return new Sniper(name, isAlly, buyPrice, maintenancePrice, damage, health, armor, dodge, accuracy, movingRange, sprite);
        }

        public override void Ability1(Unit enemy)
        {
            if (m_Random.NextDouble() <= Accuracy - 0.1)
            {
                CalculateEnemyHealth(1.5, 1, -0.1, 1, enemy);
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
            CalculateEnemyHealth(2, 1, 0, 1, enemy);
        }

        public void Destroy(Unit enemy)
        {
            CalculateEnemyHealth(1.5, 1, 0, 3, enemy);
        }
    }
}