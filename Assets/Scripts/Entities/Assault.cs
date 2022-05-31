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
        
        public static Assault CreateInstance(bool isAlly, int maintenancePrice, int damage, int health, int armor, double dodgeChance, double accuracy)
        {
            var sprite = Resources.Load<Sprite>(isAlly ? "Warriors/ally_assault" : "Warriors/enemy_assault");
            var movingRange = 3;
            var name = "Штурмовик";
            return new Assault(name, isAlly, maintenancePrice, damage, health, armor, dodgeChance, accuracy, movingRange, sprite);
        }

        public void FirstAid()
        {
            // TODO (когда внедримм уровни и статы для юнитов)
            // Проверить, не превышает ли здоровье свое максимальное значение
            Health = (int)(Health * 1.2);
        }

        public override void Ability1(Unit enemy)
        {
            enemy.Health = HealthCalc(1, 1, 0, 2, enemy);
            IsUsedAbility = true;
        }

        public void GrenadeThrow(Unit[] enemies)
        {
            foreach(var enemy in enemies)
            {
                enemy.Health = HealthCalc(2, 1, 0, 1, enemy);
            }
        }

        public void LeadRain(Unit enemy)
        {
            enemy.Health = HealthCalc(1, 1, -0.15, 5, enemy);
        }
    }
}