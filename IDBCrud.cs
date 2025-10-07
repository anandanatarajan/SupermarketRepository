using NPoco;
using System.Linq.Expressions;
using static SupermarketRepository.DBClass;

namespace SupermarketRepository
{
    /// <summary>
    /// DBCurd with updates and bug fixes
    /// </summary>
    public interface IDBCrud
    {
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="item"></param>
        /// <returns></returns>
        object AddNew<T>(T item) where T : class, new();
        /// <summary>
        /// Add a list of items to the database works like bulk copy command
        /// </summary>
        /// <typeparam name="T">dbobject (class)</typeparam>
        /// <param name="item">List of db object (class)</param>
        /// <param name="Timeout">Timeout</param>
        /// <exception cref="ApplicationException">Throws generated exception and rollback operation</exception>
        void BulkAdd<T>(List<T> item, int Timeout) where T : class, new();
        /// <summary>
        /// update a single object fetched and returned 
        /// </summary>
        /// <typeparam name="T">class-table </typeparam>
        /// <param name="item">object</param>
        /// <returns>updated record count</returns>
        int Update<T>(T item) where T : class, new();
        /// <summary>
        /// update object based on primary key
        /// </summary>
        /// <typeparam name="T">class-table object</typeparam>
        /// <param name="pk">primary key object</param>
        /// <param name="item">object to be updated</param>
        /// <returns>int update record count</returns>
        int Update<T>(object pk, T item) where T : class, new();
        /// <summary>
        /// update particular fileds based on string array provide name of fields only
        /// </summary>
        /// <typeparam name="T">class-table object</typeparam>
        /// <param name="item">object to be updated</param>
        /// <param name="UpdatebleFields">string array of filed names which is to be updated</param>
        /// <returns>int update record count</returns>
        int Update<T>(T item, IEnumerable<string> UpdatebleFields) where T : class, new();
        /// <summary>
        /// Delete a record Permanently from table
        /// </summary>
        /// <typeparam name="T">table object (class)</typeparam>
        /// <param name="id">Primary Key</param>
        /// <returns>Success returns postive int else failure</returns>
        int Delete<T>(int id) where T : class, new();
        /// <summary>
        /// Delete a record Permanently from table based on where condition provided as expression
        /// </summary>
        /// <typeparam name="T">table object(class)</typeparam>
        /// <param name="wherecondt">where condition as express</param>
        /// <param name="Soft">true for soft delete false for hard delete</param>
        /// <returns>Success returns postive int else failure</returns>
        int Delete<T>(string wherecondt,bool Soft=false) where T : class, new();
        /// <summary>
        /// select all available records
        /// </summary>
        /// <typeparam name="T">class-table object</typeparam>
        /// <returns>List of records</returns>
        List<T> SelectAll<T>() where T : class, new();
        /// <summary>
        /// select based on where condition
        /// </summary>
        /// <typeparam name="T">class-table object</typeparam>
        /// <param name="expression">where condition as expression</param>
        /// <returns>ienumerable of selected object</returns>
        List<T> Select<T>(Expression<Func<T, bool>> expression) where T : class, new();
        /// <summary>
        /// return single object pertains to where condition
        /// </summary>
        /// <typeparam name="T">class-table object</typeparam>
        /// <param name="sql">sql statement</param>
        /// <returns>single object</returns>
        T Single<T>(Sql sql) where T : class, new();
        /// <summary>
        /// select a row based on int primarykey select by guid has been merged with selectbyid by converting id to object instead of int
        /// </summary>
        /// <typeparam name="T">class-table object</typeparam>
        /// <param name="id">id must be primarykey integer</param>
        /// <returns>class-table object</returns>
        T SelectById<T>(object id) where T : class, new();

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="spname"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        IEnumerable<T> SelectBySP<T>(string spname, params object[] args) where T : class, new();
        /// <summary>
        /// sql query with optional arguments as param array
        /// </summary>
        /// <typeparam name="T">class-table object </typeparam>
        /// <param name="sql">sql query as string</param>
        /// <param name="args">conditions as param array</param>
        /// <returns>records affected</returns>
        int Execute<T>(string sql, params object[] args) where T : class, new();
        /// <summary>
        /// no need to create mapping class just use dictionary to get data from table temporarily
        /// </summary>
        /// <param name="sql">sql query</param>
        /// <param name="args">conditions as args</param>
        /// <returns>list of dictionary</returns>
        Dictionary<string, object> SelectAnonymous(string sql, params object[] args);
        /// <summary>
        /// 
        /// </summary>

        /// <param name="sql"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        object ExecuteScalar<T>(string sql, params object[] args) where T : new();
       /// <summary>
       /// 
       /// </summary>
       /// <typeparam name="T"></typeparam>
       /// <param name="sql"></param>
       /// <param name="args"></param>
       /// <returns></returns>
        IEnumerable<T> SelectBySQL<T>(string sql, params object[] args) where T : class, new();

       /// <summary>
       /// 
       /// </summary>
       /// <typeparam name="T"></typeparam>
       /// <param name="item"></param>
       /// <param name="token"></param>
       /// <returns></returns>
        Task<object> AddNewAsync<T>(T item, CancellationToken token) where T : class, new();
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="textFieldName"></param>
        /// <param name="valueFieldName"></param>
        /// <returns></returns>
        List<MyDropdownItems> GetDropdownItems<T>(string textFieldName, string valueFieldName) where T : class, new();

        // Add these async methods to IDBCrud
        /// <summary>
        /// async select all
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="token"></param>
        /// <returns></returns>
        Task<List<T>> SelectAllAsync<T>(CancellationToken token) where T : class, new();

        /// <summary>
        /// update async method
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="item"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        Task<int> UpdateAsync<T>(T item, CancellationToken token) where T : class, new();

        /// <summary>
        /// update async method with primary key
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="item"></param>
        /// <param name="UpdatebleFields"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        Task<int> UpdateAsync<T>(T item, IEnumerable<string> UpdatebleFields, CancellationToken token) where T : class, new();

        /// <summary>
        /// execute async method
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sql"></param>
        /// <param name="token"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        Task<int> ExecuteAsync<T>(string sql, CancellationToken token, params object[] args) where T : class, new();
        // Database access
        /// <summary>
        /// Get the underlying NPoco database instance for advanced operations
        /// </summary>
        /// <returns></returns>
        IDatabase GetDatabase();

        // Helper methods for mail triggering
        /// <summary>
        /// trigger mail event
        /// </summary>
        /// <param name="result"></param>
        /// <returns></returns>
        DBClass TriggerMail(bool result);
        /// <summary>
        /// get command for logging purpose
        /// </summary>
        event EventHandler<DbCommandEventArgs> GetCommand;
        /// <summary>
        /// Occurs when a request is made to send mail.
        /// </summary>
        /// <remarks>The event provides a boolean value indicating whether the mail sending request is
        /// approved. Subscribers can handle this event to perform actions before the mail is sent or to cancel the
        /// operation.</remarks>
        event EventHandler<bool> MailSendingRequested;
        /// <summary>
        /// Updates based on where condition.
        /// </summary>
        /// <returns>An int.</returns>
        int UpdateWhere<T>(string Wherecondition, T item, params object[] args) where T : class, new();

        

    }
}
