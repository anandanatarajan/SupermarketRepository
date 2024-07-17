using NPoco;
using NPoco.Expressions;
using System;
using System.Data.Common;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
namespace SupermarketRepository
{
    /// <summary>
    /// options class for settingup db connection
    /// </summary>
    public class MyDbOptions
    {
        /// <summary>
        /// Connection String
        /// </summary>
        public string? ConnectionString { get; set; }
        /// <summary>
        /// Gets or sets the database type.
        /// </summary>
        public NPoco.DatabaseType? DatabaseType { get; set; }
        /// <summary>
        /// Gets or sets the db provider factory.
        /// </summary>
        public DbProviderFactory? DbProviderFactory { get; set; }

    }
    /// <summary>
    /// The db command event args.
    /// </summary>
    public class DbCommandEventArgs
    {
        /// <summary>
        /// Gets or sets the command text.
        /// </summary>
        public string? CommandText { get; set; }
        /// <summary>
        /// Gets or sets the e date time.
        /// </summary>
        public DateTime EDateTime { get; set; }
    }

    /// <summary>
    /// The my d b class extensions.
    /// </summary>
    public static class MyDBClassExtensions
    {
        /// <summary>
        /// Adds the d b class with options.
        /// </summary>
        /// <param name="services">The services.</param>
        /// <param name="configureOptions">The configure options.</param>
        /// <returns>An IServiceCollection.</returns>
        public static IServiceCollection AddDBClassWithOptions(this IServiceCollection services, Action<MyDbOptions> configureOptions)
        {
            var options = new MyDbOptions();
            configureOptions(options);
            services.AddScoped<DBClass>(sp => new DBClass(options));
            return services;
        }
    }

    /// <summary>
    /// The d b class.
    /// </summary>
    public class DBClass : IDBCrud, IDisposable
    {
        /// <summary>
        /// Getcommand Event
        /// </summary>
        public event EventHandler<DbCommandEventArgs>? GetCommand;
        /// <summary>
        /// Mail sending request event
        /// </summary>
        public event EventHandler<bool>? MailSendingRequested;

        private bool disposedValue;
        private readonly Database db;
        /// <summary>
        /// Gets or sets a value indicating whether log last command.
        /// </summary>
        public bool LogLastCommand { get; set; }
        /// <summary>
        /// Gets or sets the last command.
        /// </summary>
        public string? LastCommand { get; set; }



        /// <summary>
        /// Initializes a new instance of the <see cref="DBClass"/> class.
        /// </summary>
        /// <param name="options">The options.</param>
        public DBClass(MyDbOptions options)
        {

            db = new Database(options.ConnectionString!, options.DatabaseType!, options.DbProviderFactory!);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DBClass"/> class.
        /// </summary>
        /// <param name="lastCommand">The last command.</param>
        public DBClass(string? lastCommand)
        {
            LastCommand = lastCommand;
        }

        /// <summary>
        /// Disposes the.
        /// </summary>
        /// <param name="disposing">If true, disposing.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    db?.Dispose();
                    // TODO: dispose managed state (managed objects)
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                disposedValue = true;
            }
        }

        // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        // ~DBClass()
        // {
        //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        //     Dispose(disposing: false);
        // }

        /// <summary>
        /// Disposes the.
        /// </summary>
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
        /// <summary>
        /// Ons the mail sending requested.
        /// </summary>
        /// <param name="result">If true, result.</param>
        protected virtual void OnMailSendingRequested(bool result)
        {
            MailSendingRequested?.Invoke(this, result);
        }
        /// <summary>
        /// Ons the get command.
        /// </summary>
        /// <param name="e">The e.</param>
        public virtual void OnGetCommand(DbCommandEventArgs e)
        {
            GetCommand?.Invoke(this, e);

        }
        /// <summary>
        /// Logdetails the.
        /// </summary>
        void Logdetails()
        {
            if (LogLastCommand)
            {
                DbCommandEventArgs args = new DbCommandEventArgs
                {
                    CommandText = db.LastCommand,
                    EDateTime = DateTime.Now
                };
                OnGetCommand(args);

            }
        }

        /// <summary>
        /// Gets the database.
        /// </summary>
        /// <returns>An IDatabase.</returns>
        public IDatabase GetDatabase() { return db; }
        /// <summary>
        /// Triggers the mail.
        /// </summary>
        /// <param name="result">If true, result.</param>
        /// <returns>A DBClass.</returns>
        public DBClass TriggerMail(bool result)
        {
            OnMailSendingRequested(result);
            return this;
        }
        /// <inheritdoc/>
        /// <summary>
        /// Adds the new record.
        /// </summary>
        /// <param name="item">The item.</param>
        
        /// <returns>A Task.</returns>
        public int AddNew<T>(T item) where T : class, new()
        {

            try
            {
                using (var transaction = db.GetTransaction())
                {
                    var obj = db.Insert(item);
                    transaction.Complete();
                    int retval = -1;
                    if (obj != null)
                    {
                        retval = Convert.ToInt32(obj);
                    }
                    return retval;
                }
            }
            catch (Exception ex)
            {
                db?.Transaction?.Rollback();
                throw new ApplicationException("Bulk Addition Failed. Operation Rolled Back. ", ex);
            }
            finally
            {
                Logdetails();
            }

        }
        /// <summary>
        /// Adds the new async.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <param name="token">The token.</param>
        /// <returns>A Task.</returns>
        public async Task<int> AddNewAsync<T>(T item, CancellationToken token) where T : class, new()
        {
            try
            {
                if (token.IsCancellationRequested)
                {
                    throw new OperationCanceledException();
                }
                using (var transaction = db.GetTransaction())
                {
                    var obj = await db.InsertAsync(item);
                    transaction.Complete();
                    int retval = -1;
                    if (obj != null)
                    {
                        retval = Convert.ToInt32(obj);
                    }
                    return retval;
                }
            }
            catch (Exception ex)
            {
                db?.Transaction?.Rollback();
                throw new ApplicationException("Bulk Addition Failed. Operation Rolled Back. ", ex);
            }
            finally
            {
                Logdetails();
            }

        }

        /// <summary>
        /// Bulks the add.
        /// </summary>
        /// <param name="Items">The items.</param>
        /// <param name="Timeout">The timeout.</param>
        public void BulkAdd<T>(List<T> Items, int Timeout) where T : class, new()
        {
            InsertBulkOptions options = new()
            {
                BulkCopyTimeout = Timeout
            };

            using var transaction = db.GetTransaction();
            try
            {


                db.InsertBulk<T>(Items, options);
                transaction.Complete();
            }
            catch (Exception ex)
            {
                db?.Transaction?.Rollback();
                throw new ApplicationException("Bulk Addition Failed. Operation Rolled Back. ", ex);
            }
            finally
            {
                Logdetails();
            }

        }

        /// <summary>
        /// Updates the.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <returns>An int.</returns>
        public int Update<T>(T item) where T : class, new()
        {

            try
            {
                using (var transaction = db.GetTransaction())
                {
                    var retval = db.Update(item);
                    transaction.Complete();

                    return retval;
                }
            }
            catch (Exception ex)
            {
                db?.Transaction?.Rollback();
                throw new ApplicationException("Update Failed. Operation Rolled Back. ", ex);
            }
            finally
            {
                Logdetails();
            }

        }
        /// <summary>
        /// Updates the async.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <param name="token">The token.</param>
        /// <returns>A Task.</returns>
        public Task<int> UpdateAsync<T>(T item, CancellationToken token) where T : class, new()
        {

            try
            {
                if (token.IsCancellationRequested)
                {
                    throw new OperationCanceledException();
                }
                using (var transaction = db.GetTransaction())
                {
                    var retval = db.UpdateAsync(item);
                    transaction.Complete();

                    return retval;
                }
            }
            catch (Exception ex)
            {
                db?.Transaction?.Rollback();
                throw new ApplicationException("Update Failed. Operation Rolled Back. ", ex);
            }
            finally
            {
                Logdetails();
            }

        }

        /// <summary>
        /// Updates the.
        /// </summary>
        /// <param name="pk">The pk.</param>
        /// <param name="item">The item.</param>
        /// <returns>An int.</returns>
        public int Update<T>(object pk, T item) where T : class, new()
        {
            try
            {
                using (var transaction = db.GetTransaction())
                {
                    var retval = db.Update(item, pk);
                    transaction.Complete();

                    return retval;
                }
            }
            catch (Exception ex)
            {
                db?.Transaction?.Rollback();
                throw new ApplicationException("Update Failed. Operation Rolled Back. ", ex);
            }
            finally
            {
                Logdetails();
            }
        }


        /// <summary>
        /// Updates the.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <param name="UpdatebleFields">The updateble fields.</param>
        /// <returns>An int.</returns>
        public int Update<T>(T item, IEnumerable<string> UpdatebleFields) where T : class, new()
        {
            try
            {
                using (var transaction = db.GetTransaction())
                {

                    var retval = db.Update(item, UpdatebleFields);
                    transaction.Complete();


                    return retval;
                }
            }
            catch (Exception ex)
            {
                db?.Transaction?.Rollback();
                throw new ApplicationException("Update Failed. Operation Rolled Back. ", ex);
            }
            finally
            {
                Logdetails();
            }
        }

        /// <summary>
        /// Updates the async.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <param name="UpdatebleFields">The updateble fields.</param>
        /// <param name="token">The token.</param>
        /// <returns>A Task.</returns>
        public Task<int> UpdateAsync<T>(T item, IEnumerable<string> UpdatebleFields, CancellationToken token) where T : class, new()
        {
            try
            {
                if (token.IsCancellationRequested)
                {
                    throw new OperationCanceledException();
                }
                using (var transaction = db.GetTransaction())
                {

                    var retval = db.UpdateAsync(item, UpdatebleFields);
                    transaction.Complete();


                    return retval;
                }
            }
            catch (Exception ex)
            {
                db?.Transaction?.Rollback();
                throw new ApplicationException("Update Failed. Operation Rolled Back. ", ex);
            }
            finally
            {
                Logdetails();
            }
        }

        /// <summary>
        /// Deletes the.
        /// </summary>
        /// <param name="id">The id.</param>
        /// <returns>An int.</returns>
        public int Delete<T>(int id) where T : class, new()
        {

            try
            {
                int? ret = db?.Delete<T>(id);

                return ret ?? -1;
            }
            catch
            {
                throw;
            }
            finally
            {
                Logdetails();
            }
        }

        /// <summary>
        /// Deletes the.
        /// </summary>
        /// <param name="Wherecondition">The wherecondition.</param>
        /// <returns>An int.</returns>
        public int Delete<T>(string Wherecondition) where T : class, new()
        {
            try
            {
                int? ret = db?.DeleteWhere<T>(Wherecondition);
                return ret ?? -1;
            }
            catch
            {
                throw;
            }
            finally
            {
                Logdetails();
            }
        }

        /// <summary>
        /// Selects the all.
        /// </summary>
        /// <returns>A list of TS.</returns>
        public List<T> SelectAll<T>() where T : class, new()
        {
            try
            {
                var ret = db.Fetch<T>();
                return ret;
            }
            catch
            {
                throw;
            }
            finally
            {
                Logdetails();
            }
        }
        /// <summary>
        /// Selects the all async.
        /// </summary>
        /// <param name="token">The token.</param>
        /// <returns>A Task.</returns>
        public Task<List<T>> SelectAllAsync<T>(CancellationToken token) where T : class, new()
        {
            try
            {
                if (token.IsCancellationRequested)
                {
                    throw new OperationCanceledException();
                }
                var ret = db.FetchAsync<T>();
                return ret;
            }
            catch
            {
                throw;
            }
            finally
            {
                Logdetails();
            }
        }

        /// <summary>
        /// Selects the.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <returns>A list of TS.</returns>
        public List<T> Select<T>(Expression<Func<T, bool>> expression) where T : class, new()
        {
            try
            {
                List<T> retval = db.Query<T>().Where(expression).ToList();
                return retval;
            }
            catch
            {
                throw;
            }
            finally
            {
                Logdetails();
            }


        }

        /// <summary>
        /// Singles the.
        /// </summary>
        /// <param name="sql">The sql.</param>
        /// <returns>A T.</returns>
        public T Single<T>(Sql sql) where T : class, new()
        {
            try
            {
                var ret = db.SingleOrDefault<T>(sql);

                return ret;
            }
            catch
            {
                throw;
            }
            finally
            {
                Logdetails();
            }
        }

        /// <summary>
        /// Selects the by id.
        /// </summary>
        /// <param name="id">The id.</param>
        /// <returns>A T.</returns>
        public T SelectById<T>(object id) where T : class, new()
        {
            try
            {
                var ret = db.SingleById<T>(id);

                return ret;
            }
            catch
            {
                throw;
            }
            finally
            {
                Logdetails();
            }

        }




        /// <summary>
        /// Selects the by s p.
        /// </summary>
        /// <param name="spname">The spname.</param>
        /// <param name="args">The args.</param>
        /// <returns>A list of TS.</returns>
        public IEnumerable<T> SelectBySP<T>(string spname, params object[] args) where T : class, new()
        {
            try
            {
                StringBuilder argcount = new StringBuilder();
                for (int i = 0; i < args.Length; i++)
                {
                    argcount.Append("@" + i.ToString() + ",");
                }
                string myargs = argcount.ToString().TrimEnd(',');

                var sql = ";exec " + spname + " " + myargs;
                return db.Fetch<T>(sql, args);
            }
            catch
            {
                throw;
            }
            finally
            {
                Logdetails();
            }
        }

        /// <summary>
        /// Executes the.
        /// </summary>
        /// <param name="sql">The sql.</param>
        /// <param name="args">The args.</param>
        /// <returns>An int.</returns>
        public int Execute<T>(string sql, params object[] args) where T : class, new()
        {
            try
            {
                int ret = db.Execute(sql, args);
                return ret;
            }
            catch
            {
                throw;
            }
            finally
            {
                Logdetails();
            }
        }

        /// <summary>
        /// Executes the async.
        /// </summary>
        /// <param name="sql">The sql.</param>
        /// <param name="token">The token.</param>
        /// <param name="args">The args.</param>
        /// <returns>A Task.</returns>
        public Task<int> ExecuteAsync<T>(string sql, CancellationToken token, params object[] args) where T : class, new()
        {
            try
            {
                if (token.IsCancellationRequested)
                {
                    throw new OperationCanceledException();
                }
                var ret = db.ExecuteAsync(sql, args);
                return ret;
            }
            catch
            {
                throw;
            }
            finally
            {
                Logdetails();
            }
        }

        /// <summary>
        /// Selects the anonymous.
        /// </summary>
        /// <param name="sql">The sql.</param>
        /// <param name="args">The args.</param>
        /// <returns>A Dictionary.</returns>
        public Dictionary<string, object> SelectAnonymous(string sql, params object[] args)
        {
            try
            {
                var returnedItems = db.Dictionary<string, object>(sql, args);
                return returnedItems;
            }
            catch
            {
                throw;
            }
            finally
            {
                Logdetails();
            }
        }

        /// <summary>
        /// Executes the scalar.
        /// </summary>
        /// <param name="sql">The sql.</param>
        /// <param name="args">The args.</param>
        /// <returns>An object.</returns>
        public object ExecuteScalar<T>(string sql, params object[] args) where T : class, new()
        {
            try
            {
                var returnedItems = db.ExecuteScalar<T>(sql, args);
                return returnedItems;
            }
            catch
            {
                throw;
            }
            finally
            {
                Logdetails();
            }
        }

        /// <summary>
        /// Selects the by s q l.
        /// </summary>
        /// <param name="sql">The sql.</param>
        /// <param name="args">The args.</param>
        /// <returns>A list of TS.</returns>
        public IEnumerable<T> SelectBySQL<T>(string sql, params object[] args) where T : class, new()
        {
            try
            {
                var items = db.Query<T>(sql, args);
                return items;
            }
            catch
            {
                throw;
            }
            finally
            {
                Logdetails();
            }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="textFieldName"></param>
        /// <param name="valueFieldName"></param>
        /// <returns></returns>
        /// <exception cref="KeyNotFoundException">if any one of the property not found in the model this expression will throw</exception>
        public List<MyDropdownItems> GetDropdownItems<T>(string textFieldName = "", string valueFieldName = "") where T : class, new()
        {
            // string txtfield=GetMemberName(textFieldName);
            // string valfield = GetMemberName(valueFieldName);
            if (string.IsNullOrEmpty(textFieldName) | string.IsNullOrWhiteSpace(valueFieldName))
            {
                throw new KeyNotFoundException();
            }
            var itm = db.Query<T>().ToEnumerable();
            PropertyInfo? txtproperty = typeof(T).GetProperty(textFieldName);
            PropertyInfo? valproperty = typeof(T).GetProperty(valueFieldName);
            if (txtproperty == null | valproperty == null)
            {
                throw new KeyNotFoundException();
            }
            List<MyDropdownItems> dropdownItems = new();
            foreach (var it in itm)
            {
                MyDropdownItems items = new MyDropdownItems() { Text = (string)(txtproperty?.GetValue(it) ?? ""), Value = valproperty?.GetValue(it) ?? "" };
                dropdownItems.Add(items);
            }

            return dropdownItems;
            //var sql = $@"SELECT {txtfield} as Text, {valfield} AS Value FROM [{typeof(T).Name}]";
            //return db.Fetch<MyDropdownItems>(sql);
        }


        /// <summary>
        /// Gets the member name.
        /// </summary>
        /// <param name="textExpression">The text expression.</param>
        /// <returns>A string.</returns>
        public string GetMemberName<T>(Expression<Func<T, string>> textExpression)
        {
            if (textExpression == null)
            {
                return "";
            }
            if (textExpression.Body is MemberExpression memberExpression)
            {
                return memberExpression.Member.Name;
            }
            return "";
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
    /// <summary>
    /// A simple struct for binding dropdown items with text and value as string and object
    /// </summary>
    public struct MyDropdownItems
    {
/// <summary>
/// 
/// </summary>
        public string Text;
        /// <summary>
        /// 
        /// </summary>
        public object Value;
    }
}
