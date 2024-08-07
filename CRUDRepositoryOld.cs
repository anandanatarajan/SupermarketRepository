﻿using NPoco;
using System.Linq.Expressions;

namespace SupermarketRepository
{
    /// <summary>
    /// A kind of single repository pattern implementation more like a facade pattern for npoco
    /// </summary>
    public partial class CRUDRepositoryOld : ICurdSuperMarketSQL, IDisposable
    {
        private readonly IDatabase db;
        private bool disposedValue;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="db">npoco db object</param>
        public CRUDRepositoryOld(Database db)
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
            using (var transaction = db.GetTransaction())
            {
                var obj = db.Insert(item);
                transaction.Complete();
                int retval=-1;
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
        }
        
        /// <summary>
        /// Add a list of items to the database works like bulk copy command
        /// </summary>
        /// <typeparam name="T">dbobject (class)</typeparam>
        /// <param name="Items">List of db object (class)</param>
        /// <exception cref="ApplicationException">Throws generated exception and rollback operation</exception>
        public void BulkAdd<T>(List<T> Items) where T : class, new()
        {
            InsertBulkOptions options = new()
            {
                BulkCopyTimeout = 30
            };
            using (var transaction = db.GetTransaction())
            {
                try
                {

                    
                    db.InsertBulk<T>(Items, options);
                    transaction.Complete();
                }
                catch (Exception ex)
                {
                    db.Transaction.Rollback();
                    throw new ApplicationException("Bulk Addition Failed. Operation Rolled Back. ", ex);
                }
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
        /// <param name="Wherecondition">where condition like id=1 and name="abcd"</param>
        /// <returns>Success returns postive int else failure</returns>

        public int Delete<T>(string Wherecondition) where T : class, new()
        {
            return db.DeleteWhere<T>(Wherecondition);
        }
        /// <summary>
        /// sql query with optional arguments as param array for insert delete and update purpose
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
        /// for selecting aggregate functions to a field
        /// </summary>
        
        /// <param name="sql"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public object ExecuteScalar(string sql, params object[] args) 
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
                argcount += "@" + i.ToString()  + ",";
            }
            argcount = argcount.TrimEnd(',');
            
            var sql= ";exec " + spname + " " + argcount;
            return db.Fetch<T>(sql, args);
        }
        /// <summary>
        /// send plain sql query
        /// </summary>
        /// <typeparam name="T">return class type</typeparam>
        /// <param name="sql"></param>
        /// <returns>list of return class</returns>
        public IEnumerable<T> SelectBySQL<T>(string sql) where T : class, new()
        {
            return db.Query<T>(sql);
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
            // return db.Update(item);
            using (var transaction = db.GetTransaction())
            {
                var retval = db.Update(item);
                transaction.Complete();
               
                return retval;
            }
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
            // return db.Update(item, pk);
            using (var transaction = db.GetTransaction())
            {
                var retval = db.Update(item,pk);
                transaction.Complete();

                return retval;
            }
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
            // return db.Update(item, UpdatebleFields);
            using (var transaction = db.GetTransaction())
            {
                var retval = db.Update(item,UpdatebleFields);
                transaction.Complete();

                return retval;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="startpage"></param>
        /// <param name="pagesize"></param>
        /// <param name="sql"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public Page<T> GetPage<T>(int startpage, int pagesize, string sql) where T : class, new()
        {
            return db.Page<T>(startpage, pagesize, sql);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="disposing"></param>
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects)
                    db?.Dispose();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                disposedValue = true;
            }
        }

        // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        // ~CRUDRepository()
        // {
        //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        //     Dispose(disposing: false);
        // }

        void IDisposable.Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
