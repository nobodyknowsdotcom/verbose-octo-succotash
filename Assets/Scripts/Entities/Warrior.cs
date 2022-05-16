using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Warrior : MonoBehaviour
{
    private string _name { get; set; }
    private int _level { get; set; }
    private int _maintenancePrice { get; set; }
    private int _damage { get; set; }
    private int _health { get; set; }
    private int _defense { get; set; }
    private double _dodgeChance { get; set; }
    private double _accuracy { get; set; }
    
    private GameObject _icon { get; set; }
    
    public Warrior(string name, int level, int maintenancePrice, int damage, int health, int defense, double dodgeChance, double accuracy, GameObject icon)
    {
        _name = name;
        _level = level;
        _maintenancePrice = maintenancePrice;
        _damage = damage;
        _health = health;
        _defense = defense;
        _dodgeChance = dodgeChance;
        _accuracy = accuracy;
        _icon = icon;
    }
}
