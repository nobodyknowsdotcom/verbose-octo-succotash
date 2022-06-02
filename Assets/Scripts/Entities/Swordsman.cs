﻿using UnityEngine;
using Random = System.Random;

namespace Entities
{
    public class Swordsman : Unit
    {
        private Random m_Random = new Random();
        protected Swordsman(string name, bool isAlly, int buyPrice, int maintenancePrice, int damage, int health, int armor, double dodgeChance, double accuracy, Sprite sprite) 
            : base(name, isAlly, buyPrice, maintenancePrice, damage, health, armor, dodgeChance, accuracy,sprite)
        {
        }
        
        public static Swordsman CreateInstance(bool isAlly)
        {
            var sprite = Resources.Load<Sprite>(isAlly ? "Warriors/ally_knight" : "Warriors/enemy_knight");
            var movingRange = 4;
            var name = "Мечник";
            var buyPrice = 120;
            var maintenancePrice = 12;
            var health = 100;
            var damage = 100;
            var armor = 150;
            var dodge = 0.1;
            var accuracy = 0.9;
            
            return new Swordsman("Мечник", isAlly, buyPrice, maintenancePrice, damage, health, armor, dodge, accuracy, sprite);
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