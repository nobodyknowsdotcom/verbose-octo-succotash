using UnityEngine;
using Random = System.Random;

namespace Entities
{
    public class Assault : Unit
    {
        private readonly Random m_Random = new Random();
        protected Assault(string name, bool isAlly, int maintenancePrice, int damage, int health, int armor, double dodgeChance, double accuracy, int movingRange, Sprite sprite) : base(name, isAlly, maintenancePrice, damage, health, armor, dodgeChance, accuracy, movingRange, sprite)
        {
        }
        
        public static Assault CreateInstance(string name, bool isAlly, int maintenancePrice, int damage, int health, int armor, double dodgeChance, double accuracy)
        {
            var sprite = Resources.Load<Sprite>(isAlly ? "Warriors/ally_assault" : "Warriors/enemy_assault");
            var movingRange = 3;
            return new Assault(name, isAlly, maintenancePrice, damage, health, armor, dodgeChance, accuracy, movingRange, sprite);
        }

        public void FirstAid()
        {
            // TODO (когда внедримм уровни и статы для юнитов)
            // Проверить, не превышает ли здоровье свое максимальное значение
            Health = (int)(Health * 1.2);
        }

        public void Burst(Unit enemy)
        {
            HealthCalc(1, 1, 0, 2, enemy);
        }

        public void GrenadeThrow(Unit[] enemies)
        {
            foreach(var enemy in enemies)
            {
                HealthCalc(2, 1, 0, 1, enemy);
            }
        }

        public void LeadRain(Unit enemy)
        {
            HealthCalc(1, 1, -0.15, 5, enemy);
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