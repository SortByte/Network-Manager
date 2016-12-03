using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WinLib.IO
{
    public static class Xml
    {
        /// <summary>
        /// Recursively checks each object's fields if they are null, and if they are, it assigns to them a value form the dafault object
        /// </summary>
        /// <param name="type">Object type</param>
        /// <param name="targetObj">Target object to check</param>
        /// <param name="defaultObj">Default object from which to extract the default values (must be of the same type as the target)</param>
        /// <returns></returns>
        public static object CheckIfNull(Type type, object targetObj, object defaultObj)
        {
            var fields = type.GetFields();
            foreach (var field in fields)
            {
                var targetValue = field.GetValue(targetObj);
                var defaultValue = field.GetValue(defaultObj);
                if ((targetValue != null) && (field.FieldType.Namespace != "System"))
                {
                    field.SetValue(targetObj, CheckIfNull(field.FieldType, targetValue, defaultValue));
                }
                else if (targetValue == null)
                    field.SetValue(targetObj, defaultValue);
            }
            return targetObj;
        }
    }
}
