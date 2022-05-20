using UnityEngine;

namespace Factories
{
    public class WarriorFactory : Warrior
    {
        public static Warrior ConvertToEnemy(Warrior warrior,int iconIndex)
        {
            var enemyWarrior = warrior;
            enemyWarrior.Sprite = SquadsManager.StaticWarriorIcons[iconIndex];

            return enemyWarrior;
        }

        public WarriorFactory(string name, int level, int maintenancePrice, int damage, int health, int defense, double dodgeChance, double accuracy, Sprite sprite) : base(name, level, maintenancePrice, damage, health, defense, dodgeChance, accuracy, sprite)
        {
        }

        public static Warrior GetWarrior()
        {
            Warrior warrior = new Warrior("Default", 1, 10, 20, 70, 5, 0.03, 0.8, SquadsManager.StaticWarriorIcons[0]);
            return warrior;
        }
    }
}