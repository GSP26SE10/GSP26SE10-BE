using System.ComponentModel;
using System.Reflection;

namespace BookfetSystem.Services.Helpers
{
    public static class EnumDescriptionHelper
    {
        public static string GetDescription(System.Enum value)
        {
            var field = value.GetType().GetField(value.ToString());
            var attr = field.GetCustomAttribute<DescriptionAttribute>();

            return attr?.Description ?? value.ToString();
        }

        public static T GetEnumFromDescription<T>(string description) where T : System.Enum
        {
            foreach (var field in typeof(T).GetFields())
            {
                var attr = field.GetCustomAttribute<DescriptionAttribute>();

                if ((attr != null && attr.Description == description)
                    || field.Name == description)
                {
                    return (T)field.GetValue(null);
                }
            }

            throw new Exception("Invalid enum description");
        }
    }
}