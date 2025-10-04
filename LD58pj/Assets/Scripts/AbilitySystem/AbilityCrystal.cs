using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AbilityCrystal : MonoBehaviour
{
    public string abilityTypeId;
    private AbilityManager abilityManager;
    
    private PlayerAbility ability;
    // Start is called before the first frame update
    void Start()
    {
        abilityManager = AbilityManager.Instance;
        ability = abilityManager.playerAbilities.Find(x => x.AbilityTypeId == abilityTypeId);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            abilityManager.ActivateAbility(abilityTypeId);
        }
    }
}
