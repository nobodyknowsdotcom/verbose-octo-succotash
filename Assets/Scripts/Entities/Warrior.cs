using UnityEngine;

public class Warrior : MonoBehaviour
{
    private string _name;
    private int _level;
    private int _maintenancePrice;
    private int _damage;
    private int _health;
    private int _defense;
    private double _dodgeChance;
    private double _accuracy;

    private Sprite _sprite;
    
    public Warrior(string name, int level, int maintenancePrice, int damage, int health, int defense, double dodgeChance, double accuracy, Sprite sprite)
    {
        _name = name;
        _level = level;
        _maintenancePrice = maintenancePrice;
        _damage = damage;
        _health = health;
        _defense = defense;
        _dodgeChance = dodgeChance;
        _accuracy = accuracy;
        _sprite = sprite;
    }

    public string GetName()
    {
        return _name;
    }
    
    public int GetMaintenance()
    {
        return _maintenancePrice;
    }
    
    public int GetLevel()
    {
        return _level;
    }
    
    public int GetDamage()
    {
        return _damage;
    }
    
    public int GetHealth()
    {
        return _health;
    }
    
    public int GetDefennse()
    {
        return _defense;
    }
    
    public double GetDodgeChance()
    {
        return _dodgeChance;
    }
    
    public double GetAccuracy()
    {
        return _accuracy;
    }

    public Sprite GetSprite()
    {
        return _sprite;
    }
}
