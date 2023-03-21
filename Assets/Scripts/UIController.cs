using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UIController : MonoBehaviour
{
    [SerializeField] public Image end;
    [SerializeField] public TMPro.TextMeshProUGUI quitText;
    [SerializeField] public Button retry;
    [SerializeField] public Image retryImage;
    [SerializeField] public TMPro.TextMeshProUGUI retryText;
    [SerializeField] public TMPro.TextMeshProUGUI resultNotificationText;
    public void EndGameUI(bool win)
    {

        end.enabled = true;
        quitText.text = ((win) ? "Bỏ đi" : "Chạy");
        retry.enabled = true;
        retryImage.enabled = true;
        retryText.enabled = true;
        resultNotificationText.enabled = true;
        retryText.text = ((win) ? "Tiếp" : "Gỡ");
        if (win)
        {
            quitText.text = "Bỏ đi";
            retryText.text = "Tiếp";
            resultNotificationText.text = "YOU WIN";
        }
        else
        {
            quitText.text = "Chạy";
            retryText.text = "Gỡ";
            resultNotificationText.text = "YOU LOSE";
        }
    }
}
