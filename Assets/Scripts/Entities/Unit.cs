using System;
using UnityEngine;

public class Unit : MonoBehaviour
{
    public string Name { get; }
    public int Level { get; set; } = 1;
    public int MaintenancePrice { get; set; }
    public int BuyPrice { get; }
    public int Damage { get; set; }
    public int ConstHealth { get; set; }
    public int Health { get; set; }
    public int Armor { get; set; }
    public double DodgeChance { get; set; }
    public double Accuracy { get;}
    
    public int AttackRange { get; set; }
    public int MovingRange { get; set; }
    
    public bool IsAlly { get; set; } = true;
    public bool IsMoved { get; set; } = false;
    public bool IsUsedAbility { get; set; } = false;
    
    public Sprite Sprite { get; set; }
    public Sprite[] Icons { get; set; }
    public string[] AbilitiesNames { get; set; }

    protected Unit(string name, bool isAlly, int buyPrice, int maintenancePrice, int damage, int health, int armor, double dodgeChance, double accuracy, int movingRange, int attackRange)
    {
        Name = name;
        BuyPrice = buyPrice;
        MaintenancePrice = maintenancePrice;
        Damage = damage;
        ConstHealth = health;
        Health = health;
        Armor = armor;
        DodgeChance = dodgeChance;
        Accuracy = accuracy;
        MovingRange = movingRange;
        AttackRange = attackRange;
        BuyPrice = buyPrice;
        IsAlly = isAlly;
    }

    public static Unit CreateInstance(string name, bool isAlly, int buyPrice, int maintenancePrice, int damage, int health, int armor, double dodgeChance, double accuracy, int movingRange, int attackRange)
    {
        return new Unit(name, isAlly, buyPrice, maintenancePrice, damage, health, armor, dodgeChance, accuracy, movingRange, attackRange);
    }

    public override int GetHashCode()
    {
        return ShiftAndWrap(Name.GetHashCode(), 2) ^ DodgeChance.GetHashCode() + ShiftAndWrap(Accuracy.GetHashCode(), 2) ^ MaintenancePrice.GetHashCode() + IsAlly.GetHashCode();
    }

    private int ShiftAndWrap(int value, int positions)
    {
        positions = positions & 0x1F;

        // Save the existing bit pattern, but interpret it as an unsigned integer.
        uint number = BitConverter.ToUInt32(BitConverter.GetBytes(value), 0);
        // Preserve the bits to be discarded.
        uint wrapped = number >> (32 - positions);
        // Shift and wrap the discarded bits.
        return BitConverter.ToInt32(BitConverter.GetBytes((number << positions) | wrapped), 0);
    }

    public void Moved()
    {
        IsMoved = true;
    }

    public void RefreshAbilities()
    {
        IsUsedAbility = false;
    }
    
    public void RefreshMoving()
    {
        IsMoved = false;
    }

    public void RefreshAbilitiesAndMoving()
    {
        RefreshAbilities();
        RefreshMoving();
    }

    public virtual void Ability1(Unit enemy)
    {
        Debug.Log("This is ability1!");
    }

    protected void CalculateEnemyHealth(double coofDamage, int armCoofDamage, double coofAccuracy, int count, Unit enemy)
    {
        int damage = 0;
        var random = new System.Random();

        for (int i = 0; i < count; i++)
        {
            if (random.NextDouble() <= (Accuracy + coofAccuracy) * (1 - enemy.DodgeChance)) 
                damage += (int) (Damage * coofDamage);
        }

        if (damage * armCoofDamage > enemy.Armor)
        {
            damage = (damage * armCoofDamage - enemy.Armor) / armCoofDamage;
            enemy.Armor = 0;
        }
        else
        {
            enemy.Armor -= damage * armCoofDamage;
            damage = 0;
        }


        if (enemy.Health - damage > 0)
        {
            enemy.Health -= damage;
        }
        else
        {
            enemy.Health = 0;
        }
    }
}
