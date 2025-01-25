using Redbox.HAL.Component.Model.Extensions;
using System;

namespace Redbox.HAL.Core
{
    public static class CommandLineOption
    {
        public const string ColonDelimiter = ":";
        public const string AssignDelimiter = "=";

        public static string GetOptionValue(string option)
        {
            return CommandLineOption.GetOptionValue(option, ":");
        }

        public static string GetOptionValue(string option, string delimiter)
        {
            if (!option.Contains(delimiter))
                return string.Empty;
            string[] strArray = option.Split(new string[1]
            {
        delimiter
            }, StringSplitOptions.RemoveEmptyEntries);
            return strArray.Length != 2 ? string.Empty : strArray[1];
        }

        public static T GetOptionVal<T>(string option, T defVal)
        {
            string optionValue = CommandLineOption.GetOptionValue(option);
            return string.IsNullOrEmpty(optionValue) ? defVal : ConversionHelper.ChangeType<T>((object)optionValue);
        }

        public static T GetOptionVal<T>(string option, string delimiter, T defVal)
        {
            string optionValue = CommandLineOption.GetOptionValue(option, delimiter);
            return string.IsNullOrEmpty(optionValue) ? defVal : ConversionHelper.ChangeType<T>((object)optionValue);
        }
    }
}
