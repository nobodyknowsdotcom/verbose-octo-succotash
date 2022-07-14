using UnityEngine;
using Random = System.Random;

namespace Entities
{
    public class Sniper : Unit
    {
        private readonly Random m_Random = new Random();
        protected Sniper(string name, bool isAlly, int buyPrice, int maintenancePrice, int damage, int health, int armor, double dodgeChance, double accuracy, int movingRange,  int attackRange) 
            : base(name, isAlly, buyPrice, maintenancePrice, damage, health, armor, dodgeChance, accuracy, movingRange, attackRange)
        {
        }

        public static Sniper CreateInstance(bool isAlly)
        {
            var attackRange = 5;
            var movingRange = 3;
            var name = "Снайпер";
            var buyPrice = 200;
            var maintenancePrice = 20;
            var health = 40;
            var damage = 45;
            var armor = 60;
            var dodge = 0.15;
            var accuracy = 0.95;
            var sniper = new Sniper(name, isAlly, buyPrice, maintenancePrice, damage, health, armor, dodge, accuracy, movingRange, attackRange);
            SetupAbilities(sniper);
            return sniper;
        }
        
        private static void SetupAbilities(Sniper sniper)
        {
            sniper.Sprite = Resources.Load<Sprite>(sniper.IsAlly ? "Warriors/Sniper" : "Warriors/Sniper Enemy");
            sniper.ActiveAbilitiesNames = new [] {"Выстрел навскидку", "Метка цели", "Точный выстрел", "Разгром"};
            sniper.ActiveAbilitiesIcons = new[] {Resources.Load<Sprite>("Skills/Off-hand Shot"),
                Resources.Load<Sprite>("Skills/Enemy Tag"),
                Resources.Load<Sprite>("Skills/Accurate Shot"),
                Resources.Load<Sprite>("Skills/Elimination")};
            
            sniper.PassiveAbilitiesIcons = new[] {Resources.Load<Sprite>("Skills/Nice View"),
                Resources.Load<Sprite>("Skills/Defense (Sniper)")};
            sniper.PassiveAbilitiesNames = new[] {"Хороший обзор", "Глухая оборона"};
            sniper.PassiveAbilitiesDescriptions = new[]
            {
                "Если Юнит засел на возвышенности, то его навыки будут усилены на 25% (не распространяется на дополнительные эффекты).",
                "Если боец находится в укрытии, то получает дополнительные +10% к Уклонению"
            };
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