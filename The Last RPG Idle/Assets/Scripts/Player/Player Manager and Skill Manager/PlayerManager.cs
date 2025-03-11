using System.Collections;
using System.Collections.Generic;
using Unity.PlasticSCM.Editor.WebApi;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    public static PlayerManager instance;
    public Player player { get; private set; }


    public int currency;
    private void Awake()
    {
        if (instance != null)
        {
            Destroy(instance.gameObject);
            instance = this;
        }
        else
        {
            instance = this;
        }

        player = FindObjectOfType<Player>();
        if (player == null)
        {
            Debug.LogError("Player not found in the scene.");
        }
    }

    public bool HaveEnoughMoney(int _price)
    {
        if (_price > currency)
        {
            Debug.Log("Not enough money.");
            return false;
        }

        currency -= _price;

        return true;
    }
}
