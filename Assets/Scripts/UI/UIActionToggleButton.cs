using UnityEngine;

public class UIActionToggleButton : UIActionButton
{
    [SerializeField] private Sprite[] sprites;

    public void Toggle(bool toggle)
    {
        if (sprites.Length < 2)
            return;

        this.Button.image.sprite = sprites[toggle ? 1 : 0];
    }
}
