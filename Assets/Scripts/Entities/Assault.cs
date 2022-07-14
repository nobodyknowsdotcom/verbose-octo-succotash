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
            SetupAbilities(assault);
            return assault;
        }

        private static void SetupAbilities(Assault assault)
        {
            assault.Sprite = Resources.Load<Sprite>(assault.IsAlly ? "Warriors/Assault" : "Warriors/Assault Enemy");
            assault.ActiveAbilitiesNames = new [] {"Очередь из автомата", "Первая помощь", "Бросок гранаты", "Свинцовый дождь"};
            assault.ActiveAbilitiesIcons = new[] {Resources.Load<Sprite>("Skills/Burst"), 
                Resources.Load<Sprite>("Skills/First Aid"), 
                Resources.Load<Sprite>("Skills/Grenade Throw"), 
                Resources.Load<Sprite>("Skills/Bullet Hail")};
            
            assault.PassiveAbilitiesIcons = new[] {Resources.Load<Sprite>("Skills/Defense (Assault)"),
                Resources.Load<Sprite>("Skills/Protection Layer")};
            assault.PassiveAbilitiesNames = new[] {"Глухая оборона", "Амортизирующая подкладка"};
            assault.PassiveAbilitiesDescriptions = new[]
            {
                "Если боец находится в укрытии, то получает дополнительные +10% к Уклонению",
                "Экипировка содержит дополнительные слои подкладки и пластины, помогающие гасить взрывную волну."
            };
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