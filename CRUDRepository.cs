using NPoco;
using NPoco.Expressions;
using System;
using System.Linq.Expressions;
using System.Text;

namespace SupermarketRepository
{
    /// <summary>
/// A kind of single repository pattern implementation
/// </summary>
    public partial class CRUDRepository : ICurdSuperMarketSQL
    {
        private readonly IDatabase db;
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="db">npoco db object</param>
        public CRUDRepository(Database db)
        {
            this.db = db;
        }
        /// <summary>
        /// Add new item to the table based on class
        /// </summary>
        /// <typeparam name="T">Class Name</typeparam>
        /// <param name="item">Class object</param>
        /// <returns>if sucess returns positive int else return -1/0</returns>
        public int AddNew<T>(T item) where T : class, new()
        {
            
            var obj = db.Insert(item);
            int retval;
            if (obj == null)
            {
                retval = -1;
            }
            else
            {
                retval = Convert.ToInt32(obj);
            }
            return retval;
        }
        /// <summary>
        /// Add a list of items to the database works like bulk copy command
        /// </summary>
        /// <typeparam name="T">dbobject (class)</typeparam>
        /// <param name="item">List of db object (class)</param>
        /// <exception cref="ApplicationException">Throws generated exception and rollback operation</exception>
        public void BulkAdd<T>(List<T> item) where T : class, new()
        {
            InsertBulkOptions options = new()
            {
                BulkCopyTimeout = 30
            };
            try
            {

                db.BeginTransaction();
                db.InsertBulk<T>(item, options);
                db.CompleteTransaction();
            }
            catch (Exception ex)
            {
                db.Transaction.Rollback();
                throw new ApplicationException("Bulk Addition Failed Operation Rolled Back ", ex);
            }
        }
        /// <summary>
        /// Delete a record Permanently from table
        /// </summary>
        /// <typeparam name="T">table object (class)</typeparam>
        /// <param name="id">Primary Key</param>
        /// <returns>Success returns postive int else failure</returns>
        public int Delete<T>(int id) where T : class, new()
        {
           return db.Delete<T>(id);
        }
        /// <summary>
        /// Delete a record Permanently from table based on where condition 
        /// </summary>
        /// <typeparam name="T">table object(class)</typeparam>
        /// <param name="wherecondt">where condition like id=1 and name="abcd"</param>
        /// <returns>Success returns postive int else failure</returns>
        
        public int Delete<T>(string wherecondt) where T : class, new()
        {
            return db.DeleteWhere<T>(wherecondt);
        }
        /// <summary>
        /// sql query with optional arguments as param array
        /// </summary>
        /// <typeparam name="T">class-table object </typeparam>
        /// <param name="sql">sql query as string</param>
        /// <param name="args">conditions as param array</param>
        /// <returns>records affected</returns>
        public int Execute<T>(string sql, params object[] args) where T : class, new()
        {
            return db.Execute(sql, args);
        }
        /// <summary>
        /// 
        /// </summary>
        
        /// <param name="sql"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public object ExecutScalar(string sql, params object[] args) 
        {
            return db.ExecuteScalar<object>(sql, args);
        }

        /// <summary>
        /// select based on where condition
        /// </summary>
        /// <typeparam name="T">class-table object</typeparam>
        /// <param name="expression">where condition as expression</param>
        /// <returns>ienumerable of selected object</returns>
        public List<T> Select<T>(Expression<Func<T, bool>> expression) where T : class, new()
        {
            List<T> retval = db.Query<T>().Where(expression).ToList();
            return retval;
        }
        /// <summary>
        /// select all available records
        /// </summary>
        /// <typeparam name="T">class-table object</typeparam>
        /// <returns>List of records</returns>
        public List<T> SelectAll<T>() where T : class, new()
        {
            return db.Fetch<T>();
        }
        /// <summary>
        /// no need to create mapping class just use dictionary to get data from table temporarily
        /// </summary>
        /// <param name="sql">sql query</param>
        /// <param name="args">conditions as args</param>
        /// <returns>list of dictionary</returns>
        public List<Dictionary<string, object>> SelectAnonymous(string sql, params object[] args)
        {
            return db.Fetch<Dictionary<string, object>>(sql, args);
        }
        /// <summary>
        /// select a row based on string guid 
        /// </summary>
        /// <typeparam name="T">class-table object</typeparam>
        /// <param name="guid">guid must be primarykey</param>
        /// <returns>class-table object</returns>
        public T SelectByGuid<T>(string guid) where T : class, new()
        {
            return db.SingleOrDefaultById<T>(guid);
        }
        /// <summary>
        /// select a row based on int primarykey 
        /// </summary>
        /// <typeparam name="T">class-table object</typeparam>
        /// <param name="id">id must be primarykey integer</param>
        /// <returns>class-table object</returns>
        public T SelectById<T>(int id) where T : class, new()
        {
            return db.SingleOrDefaultById<T>(id);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="spname"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public IEnumerable<T> SelectBySP<T>(string spname, params object[] args) where T : class, new()
        {
            string argcount = "";
            for (int i = 0; i < args.Length; i++)
            {
                argcount += "@" + i.ToString();
            }
            var sql= ";exec " + spname + " " + argcount;
            return db.Fetch<T>(sql, args);
        }
        /// <summary>
        /// return single object pertains to where condition
        /// </summary>
        /// <typeparam name="T">class-table object</typeparam>
        /// <param name="expression">where condition as expression</param>
        /// <returns>single object</returns>
        public T Single<T>(Expression<Func<T, bool>> expression) where T : class, new()
        {           
         
            return db.Query<T>().Where(expression).FirstOrDefault();
        }
        /// <summary>
        /// update a single object fetched and returned 
        /// </summary>
        /// <typeparam name="T">class-table </typeparam>
        /// <param name="item">object</param>
        /// <returns>updated record count</returns>
        public int Update<T>(T item) where T : class, new()
        {
            return db.Update(item);
        }
        /// <summary>
        /// update object based on primary key
        /// </summary>
        /// <typeparam name="T">class-table object</typeparam>
        /// <param name="pk">primary key object</param>
        /// <param name="item">object to be updated</param>
        /// <returns>int update record count</returns>
        public int Update<T>(object pk, T item) where T : class, new()
        {
            return db.Update(item, pk);
        }
        /// <summary>
        /// update particular fileds based on string array provide name of fields only
        /// </summary>
        /// <typeparam name="T">class-table object</typeparam>
        /// <param name="item">object to be updated</param>
        /// <param name="UpdatebleFields">string array of filed names which is to be updated</param>
        /// <returns>int update record count</returns>
        public int Update<T>(T item, string[] UpdatebleFields) where T : class, new()
        {
            return db.Update(item, UpdatebleFields);
        }

        
    }
    /// <summary>
    /// Extension to generate where condition
    /// </summary>
    public static class ConditionGenerator
    {
        /// <summary>
        /// where condition generator
        /// </summary>
        /// <param name="myclass"></param>
        /// <returns></returns>
        public static string Where<T>(this T myclass) where T : class, new()
        {
            StringBuilder stringBuilder = new StringBuilder();
            var typ = myclass.GetType();
            var props = typ.GetProperties();
            foreach (var prop in props)
            {
                if (prop != null)
                {


                    var val = prop.GetValue(myclass, null);
                    if (val != null)
                    {
                        if (prop.PropertyType.GetType() == Type.GetType("string"))
                        {
                            stringBuilder.Append(prop.Name + "='" + prop.GetValue(myclass, null) + "' and ");
                        }
                        else
                        {
                            stringBuilder.Append(prop.Name + "=" + prop.GetValue(myclass, null) + " and ");
                        }
                    }

                }

            }
            string ret = stringBuilder.ToString();
            ret = ret.Substring(0, ret.Length - 4);
            return ret.ToString();
        }
    }
}
