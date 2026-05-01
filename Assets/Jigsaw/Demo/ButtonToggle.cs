using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Jigsaw.Demo
{

  public class ButtonToggle : MonoBehaviour
  {
    // Ссылки на кнопки Show и Hide
    public Button showButton;
    public Button hideButton;
    public Button menuButton;

    // Ссылка на объект, который содержит список кнопок
    public List<GameObject> buttonList;

    private void Start()
    {
      // Подключаем методы к событиям нажатия кнопок
      showButton.onClick.AddListener(ShowButtonList);
      hideButton.onClick.AddListener(HideButtonList);
      menuButton.onClick.AddListener(MenuBack);

      ShowButtonList();
    }

    private void MenuBack()
    {
      SceneManager.LoadScene(1);
    }

    // Метод для показа списка кнопок и переключения на кнопку Hide
    private void ShowButtonList()
    {
      buttonList.ForEach(x => x.SetActive(true)); // Показываем список кнопок
      showButton.gameObject.SetActive(false); // Скрываем кнопку Show
      hideButton.gameObject.SetActive(true);  // Показываем кнопку Hide
    }

    // Метод для скрытия списка кнопок и переключения на кнопку Show
    private void HideButtonList()
    {
      buttonList.ForEach(x => x.SetActive(false)); // Скрываем список кнопок
      showButton.gameObject.SetActive(true);  // Показываем кнопку Show
      hideButton.gameObject.SetActive(false); // Скрываем кнопку Hide
    }
  }

}