using UnityEngine;
using Random = System.Random;

namespace Entities
{
    public class Swordsman : Unit
    {
        private Random m_Random = new Random();
        protected Swordsman(string name, bool isAlly, int maintenancePrice, int damage, int health, int armor, double dodgeChance, double accuracy, int movingRange, Sprite sprite) : base(name, isAlly, maintenancePrice, damage, health, armor, dodgeChance, accuracy, movingRange, sprite)
        {
        }
        
        public static Swordsman CreateInstance(bool isAlly, int maintenancePrice, int damage, int health, int armor, double dodgeChance, double accuracy)
        {
            var sprite = Resources.Load<Sprite>(isAlly ? "Warriors/ally_knight" : "Warriors/enemy_knight");
            var movingRange = 4;
            var name = "Мечник";
            return new Swordsman(name, isAlly, maintenancePrice, damage, health, armor, dodgeChance, accuracy, movingRange, sprite);
        }

        public override void Ability1(Unit enemy)
        {
            enemy.Health = HealthCalc(1, 2, 0, 2, enemy);
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
                enemy.Health = HealthCalc(1, 2, 0, 2, enemy);
            }
        }
    }
}