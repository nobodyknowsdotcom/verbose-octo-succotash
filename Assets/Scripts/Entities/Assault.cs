using UnityEngine;
using Random = System.Random;

namespace Entities
{
    public class Assault : Unit
    {
        private readonly Random m_Random = new Random();
        protected Assault(string name, bool isAlly, int buyPrice, int maintenancePrice, int damage, int health,
            int armor, double dodgeChance, double accuracy, int movingRange, int attackRange) : base(name, isAlly, buyPrice, maintenancePrice, damage, health, armor, dodgeChance, accuracy, movingRange,  attackRange)
        {
        }
        
        public static Assault CreateInstance(bool isAlly)
        {
            var movingRange = 3;
            var attackRange = 4;
            var name = "Штурмовик";
            var buyPrice = 100;
            var maintenancePrice = 10;
            var health = 75;
            var damage = 20;
            var armor = 100;
            var dodge = 0.1;
            var accuracy = 0.75;
            var assault = new Assault(name, isAlly, buyPrice, maintenancePrice, damage, health, armor, dodge, accuracy, movingRange, attackRange);
            SetupSprites(assault);
            return assault;
        }

        private static void SetupSprites(Assault assault)
        {
            var sprite = Resources.Load<Sprite>(assault.IsAlly ? "Warriors/Assault" : "Warriors/Assault Enemy");
            Sprite firstAbility = Resources.Load<Sprite>("Skills/Burst");
            Sprite secondAbility = Resources.Load<Sprite>("Skills/First Aid");
            Sprite thirdAbility = Resources.Load<Sprite>("Skills/Grenade Throw");
            var abilitiesNames = new string[] {"Очередь из автомата", "Первая помощь", "Бросок гранаты"};
            assault.Sprite = sprite;
            assault.AbilitiesNames = abilitiesNames;
            assault.Icons = new[] {firstAbility, secondAbility, thirdAbility};
        }

        public void FirstAid()
        {
            // TODO (когда внедримм уровни и статы для юнитов)
            // Проверить, не превышает ли здоровье свое максимальное значение
            Health = (int)(Health * 1.2);
        }

        public override void Ability1(Unit enemy)
        {
            CalculateEnemyHealth(1, 1, 0, 2, enemy);
            IsUsedAbility = true;
        }

        public void GrenadeThrow(Unit[] enemies)
        {
            foreach(var enemy in enemies)
            {
                CalculateEnemyHealth(2, 1, 0, 1, enemy);
            }
        }

        public void LeadRain(Unit enemy)
        {
            CalculateEnemyHealth(1, 1, -0.15, 5, enemy);
        }
    }
}