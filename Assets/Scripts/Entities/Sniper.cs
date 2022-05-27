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

        public void OffhandShot(Unit enemy)
        {
            if (m_Random.NextDouble() <= Accuracy - 0.1)
            {
                enemy.Health = HealthCalc(1.5, 1, -0.1, 1, enemy);
            }
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

        private int HealthCalc(double coofDamage, int armCoofDamage, double coofAccuracy, int count, Unit enemy)
        {
            int damage = 0;

            for (int i = 0; i < count; i++)
            {
                if (m_Random.NextDouble() <= (Accuracy + coofAccuracy) * (1 - enemy.DodgeChance)) 
                    damage += (int) (Damage * coofDamage);
            }

            if (damage * armCoofDamage > enemy.Armor)
            {
                damage = (int) ((damage * armCoofDamage - enemy.Armor) / armCoofDamage);
                enemy.Armor = 0;
            }
            else
            {
                enemy.Armor -= damage * armCoofDamage;
                damage = 0;
            }

            return enemy.Health - damage > 0 ? enemy.Health - damage : 0;
        }
    }
}