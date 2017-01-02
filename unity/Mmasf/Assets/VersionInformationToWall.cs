using System;
using System.Collections.Generic;
using System.Linq;
using Mmasf.Assets;
using UnityEngine;
using UnityEngine.UI;

namespace Mmasf
{
    public sealed class VersionInformationToWall : MonoBehaviour
    {
        void Start() { GetComponent<Text>().text = "Factorio !!!"; }

        void Update() { GetComponent<Text>().text = Information; }

        static string Information
        {
            get
            {
                try
                {
                    return MmasfContext.Instance.FactorioInformation;
                }
                catch(Exception e)
                {
                    return ExceptionDump(e);
                }
            }
        }

        static string ExceptionDump(Exception exception)
        {
            var head = exception.GetType().Name + ": " + exception.Message;
            var stack = "stack: " + exception.StackTrace;
            return head + "\n" +
                stack +
                ExceptionDumpTail(exception);
        }

        static string ExceptionDumpTail(Exception exception)
        {
            if(exception.InnerException == null)
                return "";

            return "\n" + ExceptionDump(exception.InnerException);
        }
    }
}