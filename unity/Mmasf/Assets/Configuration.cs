using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace Mmasf.Assets
{
    public sealed class Configuration : MonoBehaviour
    {
        Text PortalText;
        public string Name;

        void Start()
        {
            var find = GameObject.Find("PortalText");
            PortalText = find.GetComponent<Text>();
        }

        void Update() { PortalText.text = Name; }
    }
}