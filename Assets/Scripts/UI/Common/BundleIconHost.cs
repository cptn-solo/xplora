using Assets.Scripts.Data;
using Assets.Scripts.UI.Common;
using System.Collections;
using UnityEngine;
using Zenject;
using Image = UnityEngine.UI.Image;

public class BundleIconHost : BaseContainableItem<BundleIconInfo>
{
    public class Factory : PlaceholderFactory<BundleIconHost> { }

    [SerializeField] private Transform movableCard;
    [SerializeField] private Image iconImage;

    public override Transform MovableCard => movableCard;

    protected override Image IconImage => iconImage;
    
    protected override void ApplyInfoValues(BundleIconInfo info)
    {
        ResolveIcon(iconImage, info.Icon);
        iconImage.color = info.Icon.IconMaterial() == BundleIconMaterial.Font ?
        info.IconColor : Color.white;

        if (sourceTransform == null || MovableCard == null)
            return;

        //Debug.Break();
        StartCoroutine(MoveCoroutine(MovableCard, sourceTransform, .5f));
    }

    private IEnumerator MoveCoroutine(Transform item, Transform source, float sec)
    {

        var destPosition = transform.position;

        item.position = source.position;

        var delta = 0f;
        var move = destPosition - source.position;
        var speed = move / sec;

        while (delta <= sec)
        {
            item.position += speed * Time.deltaTime;

            delta += Time.deltaTime;

            yield return null;
        }

        item.localPosition = Vector3.zero;
    }

}
