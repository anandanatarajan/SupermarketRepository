using System.Reflection;

namespace SupermarketRepository
{
    /// <summary>
    /// Provides utility methods for mapping and merging object properties.
    /// </summary>
    /// <remarks>The <see cref="ObjectMapper"/> class is designed to assist with operations involving object
    /// property mapping, such as merging properties from one object into another. It is particularly useful for
    /// scenarios where selective property updates are required while preserving the original object's state.</remarks>
    public static class ObjectMapper
    {
        /// <summary>
        /// Merges non-null properties from source into target, returning a new object.
        /// </summary>
        public static T MergeNonNull<T>(T original, T updated) where T : class, new()
        {
            if (original == null) throw new ArgumentNullException(nameof(original));
            if (updated == null) throw new ArgumentNullException(nameof(updated));

            T result = new T();
            var properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (var prop in properties)
            {
                if (!prop.CanRead || !prop.CanWrite) continue;

                var updatedValue = prop.GetValue(updated);
                var valueToSet = updatedValue ?? prop.GetValue(original);
                prop.SetValue(result, valueToSet);
            }

            return result;
        }
    }
}
