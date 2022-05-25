using UnityEngine;

namespace Entities
{
    public class Assault : Unit
    {
        protected Assault(string name, int maintenancePrice, int damage, int health, int armor, double dodgeChance, double accuracy, Sprite sprite) : base(name, maintenancePrice, damage, health, armor, dodgeChance, accuracy, sprite)
        {
        }

        public void FirstAid()
        {
            // TODO (когда внедримм уровни и статы для юнитов)
            // Проверить, не превышает ли здоровье свое максимальное значение
            Health = (int)(Health * 1.2);
        }

        public void Burst(Unit enemy)
        {
            enemy.Health -= Damage * 2;
            
            if (enemy.Health < 0)
            {
                enemy.Health = 0;
            }
        }
    }
}