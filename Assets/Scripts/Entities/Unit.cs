using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Unit : MonoBehaviour
{
    private string _name { get; set; }
    private int _level { get; set; }
    private int _maintenancePrice { get; set; }
    private int _damage { get; set; }
    private int _health { get; set; }
    private int _defense { get; set; }
    private double _dodgeChance { get; set; }
    private double _accuracy { get; set; }
    
    private Sprite _icon { get; set; }
    
    public Unit(string name, Sprite icon)
    {
        _name = name;
        _level = 1;
        _maintenancePrice = 20;
        _damage = 30;
        _health = 70;
        _defense = 10;
        _dodgeChance = 0.05;
        _accuracy = 0.8;
        _icon = icon;
    }
}
