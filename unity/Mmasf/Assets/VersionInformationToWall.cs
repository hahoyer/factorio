using System;
using System.Collections.Generic;
using System.Linq;
using ManageModsAndSavefiles;
using UnityEngine;
using UnityEngine.UI;

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
        return exception.GetType().Name + ": " + 
            exception.Message + 
            ExceptionDumpTail(exception);
    }

    static string ExceptionDumpTail(Exception exception)
    {
        if(exception.InnerException == null)
            return "";

        return "\n" + ExceptionDump(exception.InnerException);
    }
}