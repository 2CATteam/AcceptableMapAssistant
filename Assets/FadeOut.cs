using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FadeOut : MonoBehaviour
{
    public float fadeTime;

    float startAlpha;
    Renderer itemRenderer;

    // Start is called before the first frame update
    void Start()
    {
        itemRenderer = gameObject.GetComponent<Renderer>();
        startAlpha = gameObject.GetComponent<Renderer>().material.color.a;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (itemRenderer.material.color.a == 0)
        {
            Destroy(gameObject);
        }
        itemRenderer.material.color = new Color(itemRenderer.material.color.r, itemRenderer.material.color.g, itemRenderer.material.color.b, itemRenderer.material.color.a - startAlpha / fadeTime * Time.deltaTime);
        if (itemRenderer.material.color.a < 0)
        {
            Destroy(gameObject);
            itemRenderer.material.color = new Color(itemRenderer.material.color.r, itemRenderer.material.color.g, itemRenderer.material.color.b, 0);
        }
    }
}
