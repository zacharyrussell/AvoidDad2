using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitStamina
{
    //Fields
    float _currentStamina;
    float _currentMaxStamina;
    float _staminaRegenSpeed;
    bool _pauseStaminaRegen = false;

    //Properties
    public float Stamina
    {
        get
        {
            return _currentStamina;
        }
        set
        {
            _currentStamina = value;
        }
    }
    
 
    public float MaxStamina
    {
        get
        {
            return _currentMaxStamina;
        }
        set
        {
            _currentMaxStamina = value;
        }
    }
    
    public float StaminaRegenSpeed
    {
        get
        {
            return _staminaRegenSpeed;
        }
        set
        {
            _staminaRegenSpeed = value;
        }
    }
    
    public bool PauseStaminaRegen
    {
        get
        {
            return _pauseStaminaRegen;
        }
        set
        {
            _pauseStaminaRegen = value;
        }
    }

    //Constructor
    public UnitStamina(float stamina, float maxStamina, float staminaRegenSpeed, bool pauseStaminaRegen)
    {
        _currentStamina = stamina;
        _currentMaxStamina = maxStamina;
        _staminaRegenSpeed = staminaRegenSpeed;
        _pauseStaminaRegen = pauseStaminaRegen;
    }

    //Methods
    public void UseStamina(float StaminaAmount)
    {
        if(_currentStamina >= StaminaAmount)
        {
            _currentStamina -= StaminaAmount;
        }
    }

    public void RegenStamina(float RegenAmount)
    {
        if(_currentStamina < _currentMaxStamina && !_pauseStaminaRegen)
        {
            _currentStamina += RegenAmount * Time.deltaTime;
        }
    }
}
