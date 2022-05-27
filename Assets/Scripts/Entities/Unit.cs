using UnityEngine;

public class Unit : MonoBehaviour
{
    public string Name { get; }
    public int Level { get; set; } = 1;
    public bool IsAlly { get; set; } = true;
    public bool IsMoved { get; set; } = false;
    public int MaintenancePrice { get; }
    public int Damage { get; set; }
    public int Health { get; set; }
    public int Armor { get; set; }
    public double DodgeChance { get; set; }
    public double Accuracy { get;}
    private int AttackRange { get; set; }
    public int MovingRange { get; set; }

    public Sprite Sprite { get; set; }

    protected Unit(string name, bool isAlly, int maintenancePrice, int damage, int health, int armor, double dodgeChance, double accuracy, int movingRange, Sprite sprite)
    {
        Name = name;
        MaintenancePrice = maintenancePrice;
        Damage = damage;
        Health = health;
        Armor = armor;
        DodgeChance = dodgeChance;
        Accuracy = accuracy;
        Sprite = sprite;
        IsAlly = isAlly;
        MovingRange = movingRange;
    }

    public static Unit CreateInstance(string name, bool isAlly, int maintenancePrice, int damage, int health, int armor, double dodgeChance, double accuracy, int movingRange, Sprite sprite)
    {
        return new Unit(name, isAlly, maintenancePrice, damage, health, armor, dodgeChance, accuracy, movingRange, sprite);
    }

    public void Moved()
    {
        IsMoved = true;
    }
}
