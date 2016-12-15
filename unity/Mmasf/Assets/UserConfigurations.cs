using System;
using System.Collections.Generic;
using System.Linq;
using hw.Helper;
using ManageModsAndSavefiles;
using UnityEngine;

namespace Mmasf.Assets
{
    public class UserConfigurations : MonoBehaviour
    {
        public Transform configurationPrefab;

        void Start()
        {
            var context = MmasfContext.Instance;
            var userConfigurations = context.UserConfigurations;

            var position = new Vector3(0, 0, 0);
            foreach(var configuration in userConfigurations)
            {
                var item = Instantiate(configurationPrefab, position, Quaternion.identity);
                var portal = item.GetComponent<Configuration>();
                portal.Name = configuration.Path.FileHandle().Name;
                position += new Vector3(10, 0, 0);
            }
        }
    }
}