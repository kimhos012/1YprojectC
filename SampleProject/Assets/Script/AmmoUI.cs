using StarterAssets;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AmmoUI : MonoBehaviour
{
    public Text Ammo;
    public Text Gun;

    // Update is called once per frame
    void Update()
    {
        Gun.text = FirstPersonController.GunName;
        Ammo.text = FirstPersonController.Ammo + "/" + FirstPersonController.AmmoTotal;
    }
}
