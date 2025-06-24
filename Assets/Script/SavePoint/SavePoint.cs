using System;
using UnityEngine;

public class SavePoint : MonoBehaviour
{
    private bool _playerIn;
    
    private PlayerPresenter _presenter;

    private void Update()
    {
        if (_playerIn)
        {
            if (_presenter.SelectAction)
            {
                _presenter.SavePlayerDataJson();
                _playerIn = false;
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            _playerIn = true;
            _presenter = other.gameObject.GetComponent<PlayerPresenter>();
        }
       
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            _playerIn = false;
            _presenter = null;
        }
    }
}
