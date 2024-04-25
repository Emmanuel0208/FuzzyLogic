using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FuzzyLogic
{
    //Inicialization
    float _playerHealth;
    float _maxPlayerHealth;
    int   _ammo;
    int   _maxAmmo;
    float _distancePlayer;
    float _distanceAmmo;
    float _distanceHealth;
    float _health;
    float _maxHealth;

    //Fuzzy variables

    private float fuzzyPlayerHealth;
    private float fuzzyAmmo;
    private float fuzzyHealth;
    private float fuzzyDistancePlayer;
    private float fuzzyDistanceAmmo;
    private float fuzzyDistanceHealth;


    public FuzzyLogic(float playerHealth, float maxPlayerHealth, int ammo, int maxAmmo, float distancePlayer, float distanceAmmo, float distanceHealth, float health, float maxHealth)
    {
       _playerHealth = playerHealth;
        _maxPlayerHealth = maxPlayerHealth;
        _ammo = ammo;
        _maxAmmo = maxAmmo;
        _distancePlayer = distancePlayer;
        _distanceAmmo = distanceAmmo;
        _distanceHealth = distanceHealth;
        _health = health;
        _maxHealth = maxHealth;
    }

    public void Fuzzify()
    {
        fuzzyHealth = (_health * 100) / _maxHealth;
        fuzzyAmmo = (_ammo*100) / _maxAmmo;
        fuzzyPlayerHealth = (_playerHealth * 100) / _maxPlayerHealth;
        fuzzyDistancePlayer = 0;
        fuzzyDistanceAmmo = 0;
        fuzzyDistanceHealth = 0;

        
    }

    void SortLists(List<float> List)
    {
        List<float> SortedList = new List<float>();
        SortedList.Add(List[0]);
        List.RemoveAt(index: 0);
        int counter = 0;    
        foreach (float var in List) 
        { 
            if (SortedList[0] > var)
            {
                SortedList.Add(var);
                List.Remove(var);
            }
            foreach (var VARIABLE in COLLECTION)
            {

            }
            counter ++; 
        }
    }
}
