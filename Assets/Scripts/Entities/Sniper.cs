using UnityEngine;
using Random = System.Random;

namespace Entities
{
    public class Sniper : Unit
    {
        private readonly Random m_Random = new Random();
        protected Sniper(string name, bool isAlly, int buyPrice, int maintenancePrice, int damage, int health, int armor, double dodgeChance, double accuracy, int movingRange) 
            : base(name, isAlly, buyPrice, maintenancePrice, damage, health, armor, dodgeChance, accuracy, movingRange)
        {
        }

        public static Sniper CreateInstance(bool isAlly)
        {
            var movingRange = 3;
            var name = "Снайпер";
            var buyPrice = 200;
            var maintenancePrice = 20;
            var health = 40;
            var damage = 45;
            var armor = 60;
            var dodge = 0.15;
            var accuracy = 0.95;
            var sniper = new Sniper(name, isAlly, buyPrice, maintenancePrice, damage, health, armor, dodge, accuracy, movingRange);
            SetupSprites(sniper);
            return sniper;
        }
        
        private static void SetupSprites(Sniper sniper)
        {
            var sprite = Resources.Load<Sprite>(sniper.IsAlly ? "Warriors/ally_sniper" : "Warriors/enemy_sniper");
            Sprite firstAbility = Resources.Load<Sprite>("Skills/Off-hand Shot");
            Sprite secondAbility = Resources.Load<Sprite>("Skills/Enemy Tag");
            Sprite thirdAbility = Resources.Load<Sprite>("Skills/Accurate Shot");
            var abilitiesNames = new [] {"Выстрел навскидку", "Метка цели", "Точный выстрел"};
            sniper.Sprite = sprite;
            sniper.AbilitiesNames = abilitiesNames;
            sniper.Icons = new[] {firstAbility, secondAbility, thirdAbility};
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