using UnityEngine;
using Random = System.Random;

namespace Entities
{
    public class Swordsman : Unit
    {
        private Random m_Random = new Random();
        protected Swordsman(string name, bool isAlly, int maintenancePrice, int damage, int health, int armor, double dodgeChance, double accuracy, int movingRange, Sprite sprite) : base(name, isAlly, maintenancePrice, damage, health, armor, dodgeChance, accuracy, movingRange, sprite)
        {
        }
        
        public static Swordsman CreateInstance(string name, bool isAlly, int maintenancePrice, int damage, int health, int armor, double dodgeChance, double accuracy)
        {
            var sprite = Resources.Load<Sprite>(isAlly ? "Warriors/ally_knight" : "Warriors/enemy_knight");
            var movingRange = 4;
            return new Swordsman(name, isAlly, maintenancePrice, damage, health, armor, dodgeChance, accuracy, movingRange, sprite);
        }

        public void BerserkerPath(Unit enemy)
        {
            HealthCalc(1, 2, 0, 2, enemy);
        }

        public void ProtectiveStand()
        {
            DodgeChance += 0.7;
            //Через ход надо снять
        }

        public void WhirlwindDanceOfDeath(Unit[] enemies)
        {
            foreach (var enemy in enemies)
            {
                HealthCalc(1, 2, 0, 2, enemy);
            }
        }

        private void HealthCalc(double coofDamage, int armCoofDamage, double coofAccuracy, int count, Unit enemy)
        {
            int damage = 0;

            for (int i = 0; i < count; i++)
            {
                if (m_Random.NextDouble() <= (Accuracy + coofAccuracy) * (1 - enemy.DodgeChance)) 
                    damage += (int) (Damage * coofDamage);
            }

            if (damage * armCoofDamage > enemy.Armor)
            {
                damage = (damage * armCoofDamage - enemy.Armor) / armCoofDamage;
                enemy.Armor = 0;
            }
            else
            {
                enemy.Armor -= damage * armCoofDamage;
                damage = 0;
            }

            enemy.Health = enemy.Health - damage > 0 ? enemy.Health - damage : 0;
        }
    }
}