using UnityEngine;
using UnityEngine.Serialization;

public class Warrior : MonoBehaviour
{
    public string Name { get; }
    public int Level { get; set; }
    public int MaintenancePrice { get; }
    public int Damage { get; set; }
    public int Health { get; set; }
    public int Armor { get; set; }
    public double DodgeChance { get;}
    public double Accuracy { get;}

    public Sprite Sprite { get; set; }
    
    public Warrior(string name, int level, int maintenancePrice, int damage, int health, int armor, double dodgeChance, double accuracy, Sprite sprite)
    {
        Name = name;
        Level = level;
        MaintenancePrice = maintenancePrice;
        Damage = damage;
        Health = health;
        Armor = armor;
        DodgeChance = dodgeChance;
        Accuracy = accuracy;
        Sprite = sprite;
    }
}
