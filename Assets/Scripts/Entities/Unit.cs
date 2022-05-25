using UnityEngine;
using UnityEngine.Serialization;

public class Unit : MonoBehaviour
{
    public string Name { get; }
    public int Level { get; set; } = 1;
    public bool IsAlly { get; set; } = true;
    private int Range { get; set; }
    public int MaintenancePrice { get; }
    public int Damage { get; set; }
    public int Health { get; set; }
    public int Armor { get; set; }
    public double DodgeChance { get;}
    public double Accuracy { get;}

    public Sprite Sprite { get; set; }

    protected Unit(string name, int maintenancePrice, int damage, int health, int armor, double dodgeChance, double accuracy, Sprite sprite)
    {
        Name = name;
        MaintenancePrice = maintenancePrice;
        Damage = damage;
        Health = health;
        Armor = armor;
        DodgeChance = dodgeChance;
        Accuracy = accuracy;
        Sprite = sprite;
    }

    public static Unit CreateInstance(string name, int maintenancePrice, int damage, int health, int armor, double dodgeChance, double accuracy, Sprite sprite)
    {
        return new Unit(name, maintenancePrice, damage, health, armor, dodgeChance, accuracy, sprite);
    }
}
