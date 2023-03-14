using UnityEngine;

public class UIActionToggleButton : UIActionButton
{
    [SerializeField] private Sprite[] sprites;
    private bool localToggle = false;

    public void Toggle()
    {
        localToggle = !localToggle;
        Toggle(localToggle);
    }

    public void Toggle(bool toggle)
    {
        if (sprites.Length < 2)
            return;

        this.Button.image.sprite = sprites[toggle ? 1 : 0];
    }
}
