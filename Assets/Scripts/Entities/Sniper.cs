using UnityEngine;
using Random = System.Random;

namespace Entities
{
    public class Sniper : Unit
    {
        private readonly Random m_Random = new Random();
        protected Sniper(string name, bool isAlly, int maintenancePrice, int damage, int health, int armor, double dodgeChance, double accuracy, Sprite sprite) : base(name, isAlly, maintenancePrice, damage, health, armor, dodgeChance, accuracy, sprite)
        {
        }
        
        public static Sniper CreateInstance(string name, bool isAlly, int maintenancePrice, int damage, int health, int armor, double dodgeChance, double accuracy)
        {
            var sprite = Resources.Load<Sprite>(isAlly ? "Warriors/ally_sniper" : "Warriors/enemy_sniper");
            return new Sniper(name, isAlly, maintenancePrice, damage, health, armor, dodgeChance, accuracy, sprite);
        }

        public string OffhandShot(Unit enemy)
        {
            if (m_Random.Next(1) >= Accuracy - Accuracy / 10)
            {
                enemy.Health = (int)(Damage * 1.5);
                return "В яблочко!";
            }

            return "Ты промахнулся :-(";
        }
    }
}