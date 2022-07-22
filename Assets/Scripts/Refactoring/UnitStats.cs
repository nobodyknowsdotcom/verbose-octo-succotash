using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class UnitStats : MonoBehaviour
{
    // Основные характеристики
    public int Damage;
    public int MaxHealth;
    public int Health;
    public int Armor;
    public double DodgeChance;
    public double Accuracy;

    public bool IsDead() => (Health == 0 && Armor == 0);
}
