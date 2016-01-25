using System.ComponentModel;

namespace CloneBehave
{
    public static class CopyExtensions
    {
        /// <summary>
        /// Simple mechanism for copying Properties
        /// </summary>
        public static void CopyProperties<T>(this T src, T destination)
        {
            foreach (PropertyDescriptor item in TypeDescriptor.GetProperties(src))
            {
                item.SetValue(destination, item.GetValue(src));
            }
        }
    }
}