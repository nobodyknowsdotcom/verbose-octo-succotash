using UnityEngine;

namespace Entities
{
    public class Swordsman : Unit
    {
        protected Swordsman(string name, bool isAlly, int maintenancePrice, int damage, int health, int armor, double dodgeChance, double accuracy, Sprite sprite) : base(name, isAlly, maintenancePrice, damage, health, armor, dodgeChance, accuracy, sprite)
        {
        }
        
        public static Swordsman CreateInstance(string name, bool isAlly, int maintenancePrice, int damage, int health, int armor, double dodgeChance, double accuracy)
        {
            var sprite = Resources.Load<Sprite>(isAlly ? "Warriors/ally_knight" : "Warriors/enemy_knight");
            return new Swordsman(name, isAlly, maintenancePrice, damage, health, armor, dodgeChance, accuracy, sprite);
        }

        public void BerserkerPath(Unit enemy)
        {
            
            if (enemy.Armor > 0)
            {
                enemy.Armor -= Damage * 4;
                // Поднимем броню противника до 0, чтобы не обратить её в отрицательные значения
                if (enemy.Armor < 0)
                {
                    enemy.Armor = 0;
                }
            }
            else
            {
                enemy.Health -= Damage * 2;
            }
        }
    }
}