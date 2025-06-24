using System;
using System.IO;
using R3;
using UnityEngine;
using UnityEngine.UI;

public class GameOverView : MonoBehaviour
{
   [SerializeField] private GameObject _gameOverCanvas;
   
   [SerializeField] private Button _continueButton;
   public Button ContinueButton => _continueButton;
   
   [SerializeField] private Button _exitButton;
   public Button ExitButton => _exitButton;
   public void Show()
   {
      _gameOverCanvas.SetActive(true);
   }

   public void Hide()
   {
      _gameOverCanvas.SetActive(false);
   }
}
