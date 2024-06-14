using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Navigation;

namespace Map_A_Milepost.Utils
{
    public static class CheckObject
    {
        /// <summary>
        /// -   Checks whether or not the properties of the SOEResponseModel are all null, indicating that the viewmodel's SOEResponse
        ///     hasn't been updated yet.
        /// </summary>
        /// <param name="myObject"></param>
        /// <returns></returns>
        public static bool HasBeenUpdated(object myObject)
        {
            foreach (PropertyInfo pi in myObject.GetType().GetProperties())
            {
                if (pi.PropertyType == typeof(string))
                {
                    string value = (string)pi.GetValue(myObject);
                    Console.WriteLine(pi.Name);
                    if (string.IsNullOrEmpty(value))
                    {
                        return false;
                    }
                }
            }
            return true;
        }
    }
}
