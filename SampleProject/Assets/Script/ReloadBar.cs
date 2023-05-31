using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ReloadBar : MonoBehaviour
{

    public static float bar = 0;
    public Image img;

    void Update()
    {
        img.fillAmount = bar;
    }
}
