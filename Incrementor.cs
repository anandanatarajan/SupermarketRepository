using System.Linq.Expressions;
using System.Reflection;
using System.Collections.Concurrent;
namespace SupermarketRepository
{
    /// <summary>
    /// Provides functionality to increment properties of a class annotated with a specific attribute by querying the
    /// maximum value from a database and applying an increment step.
    /// </summary>
    /// <remarks>The <see cref="Incrementor"/> class is designed to work with classes that have properties
    /// marked with the <c>SuperAutoIncrementAttribute</c>. It retrieves the current maximum value of the annotated
    /// property from the database, increments it by a specified step, and assigns the new value to the property.  This
    /// class uses a caching mechanism to optimize the creation of property setters, ensuring efficient repeated use.
    /// The database interaction relies on the NPoco library, and the table name is determined using NPoco's
    /// PocoDataFactory.</remarks>
    public class Incrementor
    {
        private static ConcurrentDictionary<PropertyInfo, Action<object, object>> _settersCache
            = new ConcurrentDictionary<PropertyInfo, Action<object, object>>();

        /// <summary>
        /// create new incremented value based on max value in the database and assign to the property
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="instance"></param>
        /// <param name="db"></param>
        /// <param name="step"></param>
        public static void Increment<T>(T instance, NPoco.Database db, int step = 1) where T : class
        {
            PropertyInfo[] properties = typeof(T).GetProperties();
            string tableName = db.PocoDataFactory.TableInfoForType(typeof(T)).TableName;

            foreach (var property in properties)
            {
                if (property.GetCustomAttribute(typeof(SuperAutoIncrementAttribute)) != null)
                {
                    if (!_settersCache.TryGetValue(property, out var setter))
                    {
                        setter = CreateSetter(property);
                        _settersCache[property] = setter;
                    }

                    // Fetch the current maximum value from the database
                    object currentMaxValue = db.ExecuteScalar<object>($"SELECT MAX(ISNULL({property.Name}, 0)) FROM {tableName}");

                    int newValue = (currentMaxValue != null) ? Convert.ToInt32(currentMaxValue) + step : step;
                    setter(instance, newValue);
                }
            }
        }
        /// <summary>
        /// Creates a delegate that sets the value of the specified property on a given object.
        /// </summary>
        /// <remarks>The returned delegate uses dynamic type casting to handle the object instance and
        /// value,  making it suitable for scenarios where the property type is not known at compile time. Ensure that
        /// the provided object instance is of the type that declares the property,  and the value is compatible with
        /// the property's type to avoid runtime exceptions.</remarks>
        /// <param name="propertyInfo">The <see cref="PropertyInfo"/> representing the property for which the setter delegate is created. Must not
        /// be null.</param>
        /// <returns>A delegate of type <see cref="Action{T1, T2}"/> that takes two parameters:  the object instance on which to
        /// set the property value, and the value to set.  The object instance and value are cast to the appropriate
        /// types internally.</returns>
        private static Action<object, object> CreateSetter(PropertyInfo propertyInfo)
        {
            var instance = Expression.Parameter(typeof(object), "instance");
            var value = Expression.Parameter(typeof(object), "value");
            if (propertyInfo.DeclaringType == null)
            {
                throw new ArgumentException("Property does not have a declaring type.", nameof(propertyInfo));
            }
            var castInstance = Expression.Convert(instance, type: propertyInfo.DeclaringType);
            var castValue = Expression.Convert(value, propertyInfo.PropertyType);

            var body = Expression.Assign(Expression.Property(castInstance, propertyInfo), castValue);

            var lambda = Expression.Lambda<Action<object, object>>(body, instance, value);
            return lambda.Compile();
        }
    }

}
