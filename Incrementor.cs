using System.Linq.Expressions;
using System.Reflection;
using System.Collections.Concurrent;
namespace SupermarketRepository
{
    public class Incrementor
    {
        private static ConcurrentDictionary<PropertyInfo, Action<object, object>> _settersCache
            = new ConcurrentDictionary<PropertyInfo, Action<object, object>>();

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

        private static Action<object, object> CreateSetter(PropertyInfo propertyInfo)
        {
            var instance = Expression.Parameter(typeof(object), "instance");
            var value = Expression.Parameter(typeof(object), "value");

            var castInstance = Expression.Convert(instance, propertyInfo.DeclaringType);
            var castValue = Expression.Convert(value, propertyInfo.PropertyType);

            var body = Expression.Assign(Expression.Property(castInstance, propertyInfo), castValue);

            var lambda = Expression.Lambda<Action<object, object>>(body, instance, value);
            return lambda.Compile();
        }
    }

}
