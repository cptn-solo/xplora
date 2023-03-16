using Assets.Scripts.UI.Data;
using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class UIActionButton : MonoBehaviour
{
    [SerializeField] private Actions action;

    public Actions Action => action;

    private event UnityAction OnButtonClick;
    private Button button;

    public event Action<Actions, Transform> OnActionButtonClick;
    
    protected Button Button => button;
    
    private void Awake()
    {
        button = GetComponent<Button>();
        OnButtonClick += UIActionButton_OnButtonClick;
    }

    private void UIActionButton_OnButtonClick() =>
        OnActionButtonClick?.Invoke(action, transform);

    private void OnEnable() =>
        button.onClick.AddListener(OnButtonClick);

    private void OnDisable() =>
        button.onClick.RemoveListener(OnButtonClick);

    internal void SetEnabled(bool v)
    {
        button.interactable = v;
        enabled = v;
    }
}
