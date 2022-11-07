using Assets.Scripts.UI;
using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
public class UIMenuButton : MonoBehaviour
{
    [SerializeField] private Screens screen;

    private event UnityAction OnButtonClick;
    private Button button;

    public event Action<Screens> OnMenuButtonClick;
    private void Awake()
    {
        button = GetComponent<Button>();
        OnButtonClick += UIMenuButton_OnButtonClick;
    }

    private void UIMenuButton_OnButtonClick() =>
        OnMenuButtonClick?.Invoke(screen);

    private void OnEnable() =>
        button.onClick.AddListener(OnButtonClick);

    private void OnDisable() =>
        button.onClick.RemoveListener(OnButtonClick);
}
