using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Jigsaw.Scripts.UI
{
    public class ButtonToggle : MonoBehaviour
    {
        public Button showButton;
        public Button hideButton;
        public Button menuButton;

        public List<GameObject> buttonList;

        private void Start()
        {
            showButton.onClick.AddListener(ShowButtonList);
            hideButton.onClick.AddListener(HideButtonList);
            menuButton.onClick.AddListener(MenuBack);

            ShowButtonList();
        }

        private void MenuBack()
        {
            SceneManager.LoadScene(0);
        }

        private void ShowButtonList()
        {
            buttonList.ForEach(x => x.SetActive(true)); 
            showButton.gameObject.SetActive(false); 
            hideButton.gameObject.SetActive(true); 
        }

        private void HideButtonList()
        {
            buttonList.ForEach(x => x.SetActive(false)); 
            showButton.gameObject.SetActive(true); 
            hideButton.gameObject.SetActive(false); 
        }
    }
}