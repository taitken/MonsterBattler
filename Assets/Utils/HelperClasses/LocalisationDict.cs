using System;
using System.Collections.Generic;

namespace UtilityClasses
{
    public class LocalisationDict
    {
        static public string mass = "kg";

        static public string GetMassString(decimal massNumber)
        {
            return AddCommas(massNumber) + mass;
        }

        static public string AddCommas(decimal massNumber)
        {
            string massString = massNumber.ToString();
            while ((massString.IndexOf(",") == -1 && massString.Length > 3) ||
                (massString.IndexOf(",") != -1 && massString.Substring(0, massString.IndexOf(",")).Length > 3))
            {
                massString = massString.Insert(massString.Length - 3, ",");
            }
            return massString;
        }

    }
}