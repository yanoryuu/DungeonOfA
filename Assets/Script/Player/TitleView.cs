using System.IO;
using R3;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class TitleView : MonoBehaviour
{
    [SerializeField] private GameObject _titleCanvas;
    
    [SerializeField] private Button _newGameButton;
    public Button NewGameButton => _newGameButton;
    
    [SerializeField] private Button _continueButton;
    public Button ContinueButton => _continueButton;
    
    [SerializeField] private TextMeshProUGUI _errorMessageText;
    public TextMeshProUGUI ErrorMessageText => _errorMessageText;

    private ReactiveProperty<bool> _canContinue = new();
    public ReactiveProperty<bool> CanContinue => _canContinue;
    private string saveFilePath => Path.Combine(Application.persistentDataPath, "player_save.json");

    public void Start()
    {
        Bind();
        SoundManager.Instance.PlayBGM(SoundManager.Instance.startBGM);
    }

    public void Bind()
    {
        _canContinue
            .Subscribe(can => _continueButton.interactable = can)
            .AddTo(this);

        _newGameButton.OnClickAsObservable()
            .Subscribe(_ =>
            {
                if (File.Exists(saveFilePath)) File.Delete(saveFilePath);
                SceneManager.LoadScene("DungenOfA");
            })
            .AddTo(this);

        _continueButton.OnClickAsObservable()
            .Subscribe(_ =>
            {
                if (File.Exists(saveFilePath))
                {
                    SceneManager.LoadScene("DungenOfA");
                }
                else
                {
                    _errorMessageText.text = "セーブデータが見つかりません";
                    _errorMessageText.gameObject.SetActive(true);
                }
            })
            .AddTo(this);

    }
    public void Show()
    {
        _titleCanvas.SetActive(true);
        _errorMessageText.gameObject.SetActive(false);
        _canContinue.Value = File.Exists(saveFilePath);
    }

    public void Hide()
    {
        _titleCanvas.SetActive(false);
    }
}
