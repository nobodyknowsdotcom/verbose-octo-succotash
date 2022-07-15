using UnityEngine;
using Random = System.Random;

namespace Entities
{
    public class Swordsman : Unit
    {
        private Random m_Random = new Random();
        protected Swordsman(string name, bool isAlly, int buyPrice, int maintenancePrice, int damage, int health, int armor, double dodgeChance, double accuracy, int movingRange, int attackRange) 
            : base(name, isAlly, buyPrice, maintenancePrice, damage, health, armor, dodgeChance, accuracy, movingRange, attackRange)
        {
        }
        
        public static Swordsman CreateInstance(bool isAlly)
        {
            var attackRange = 1;
            var movingRange = 4;
            var name = "Мечник";
            var buyPrice = 120;
            var maintenancePrice = 12;
            var health = 100;
            var damage = 100;
            var armor = 150;
            var dodge = 0.1;
            var accuracy = 0.9;
            
            var swordsman = new Swordsman("Мечник", isAlly, buyPrice, maintenancePrice, damage, health, armor, dodge, accuracy, movingRange, attackRange);
            SetupAbilities(swordsman);
            return swordsman;
        }
        
        private static void SetupAbilities(Swordsman swordsman)
        {
            swordsman.Sprite = Resources.Load<Sprite>(swordsman.IsAlly ? "Warriors/Swordsman" : "Warriors/Swordsman Enemy");
            swordsman.ActiveAbilitiesNames = new [] {"Путь война", "Рывок", "Защитная стойка", "Вихрь"};
            swordsman.ActiveAbilitiesIcons = new[] {Resources.Load<Sprite>("Skills/Warrior Path"),
                Resources.Load<Sprite>("Skills/Dash"), 
                Resources.Load<Sprite>("Skills/Defense Stance"), 
                Resources.Load<Sprite>("Skills/Death Whirl")};
            
            swordsman.PassiveAbilitiesIcons = new[] {Resources.Load<Sprite>("Skills/Counter Attack"),
                Resources.Load<Sprite>("Skills/Vengeance")};
            swordsman.PassiveAbilitiesNames = new[] {"Контратака", "Месть"};
            swordsman.PassiveAbilitiesDescriptions = new[]
            {
                "Боец контратакует обидчика, если он на соседней клетке.",
                "Если боец получил урон, то на время следующего хода его урон увеличится на 25%."
            };
        }

        public override void Ability1(Unit enemy)
        {
            CalculateEnemyHealth(1, 2, 0, 2, enemy);
            IsUsedAbility = true;
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
               CalculateEnemyHealth(1, 2, 0, 2, enemy);
            }
        }
    }
}