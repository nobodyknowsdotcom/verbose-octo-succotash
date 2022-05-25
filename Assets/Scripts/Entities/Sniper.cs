using UnityEngine;
using Random = System.Random;

namespace Entities
{
    public class Sniper : Unit
    {
        private readonly Random m_Random = new Random();
        protected Sniper(string name, int maintenancePrice, int damage, int health, int armor, double dodgeChance, double accuracy, Sprite sprite) : base(name, maintenancePrice, damage, health, armor, dodgeChance, accuracy, sprite)
        {
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