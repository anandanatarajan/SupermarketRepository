using NPoco.Expressions;
using SupermarketRepository;
using System.Collections.Generic;
using System.Text;

namespace SampleAPIapplication.Models
{
    public class Example
    {
        public readonly DBClass dbc;
        public readonly ILogger<Example> logger;
        public Example(ILogger<Example> _logger, DBClass _db)
        {
            logger = _logger;
            dbc = _db;
            dbc.LogLastCommand = true;
            dbc.GetCommand += Dbc_GetCommand;
            dbc.MailSendingRequested += Dbc_MailSendingRequested;
            dbc.UseExperimentalFeature = true;
        }

        private void Dbc_MailSendingRequested(object? sender, bool e)
        {
            if (e)
            {
                logger.LogInformation("Mail Sending event triggered");
            }


        }

        private void Dbc_GetCommand(object? sender, DbCommandEventArgs e)
        {
            logger.LogInformation(e.CommandText);
        }

        public void exInsertData()
        {

            try
            {
                ClassQrcode qrCode = new ClassQrcode();
                qrCode.rstatus = "I";
                qrCode.qrcode = Extensions.GenerateRandomString(5);
                qrCode.created_dt = DateTime.Now;
                var ret = dbc.AddNew<ClassQrcode>(qrCode);
                logger.LogInformation($"Id of record(s) inserted : {ret}");
            }
            catch (Exception ex)
            {
                logger.LogError($"error occured while inserting {ex}");
                throw;
            }

        }

        public async Task exInsertAsync()
        {
            try
            {
                CancellationToken cts = new CancellationToken();

                ClassQrcode qrCode = new ClassQrcode();
                qrCode.rstatus = "I";
                qrCode.qrcode = Extensions.GenerateRandomString(5);
                qrCode.created_dt = DateTime.Now;
                var ret = dbc.AddNewAsync<ClassQrcode>(qrCode, cts).Result;
                logger.LogInformation($"Number of records inserted : {ret}");
            }
            catch (Exception ex)
            {
                logger.LogError($"error occured while inserting {ex}");
                throw;
            }

        }

        public void exUpdate()
        {
            try
            {
                var expn = PredicateBuilder.Create<ClassQrcode>(x => x.rstatus == "I");
                var qrc = dbc.Select(expn).FirstOrDefault(); //example for selection by expression
                qrc.rstatus = "app";
                int ret = dbc.Update(qrc);
                logger.LogInformation($"Number of records updated : {ret}");
                var newexpn = PredicateBuilder.Create<ClassQrcode>(x => x.rstatus == "app");
               
                
                var qrc1 = dbc.SelectBySQL<ClassQrcode>("select * from tbl_Qrcode where rstatus=@0", "app").FirstOrDefault();//example for selection via sql
                logger.LogInformation($"after updating {qrc1.rstatus}");
                logger.LogInformation("Updating again");
                qrc1.rstatus = "upd";
                ret = dbc.Update(qrc1.uid, qrc);
                var newexpn1 = PredicateBuilder.Create<ClassQrcode>(x => x.rstatus == "upd");
                var qrc2 = dbc.SelectById<ClassQrcode>(qrc1.uid); //example for selection by id
                logger.LogInformation($"after updating {qrc2.rstatus}");
                logger.LogInformation("Updating again");
                qrc1.rstatus = "I";
                IEnumerable<string> updfield = ["rstatus"]; //careful this belongs to c# 12 syntax
                ret = dbc.Update<ClassQrcode>(qrc1, updfield);
                logger.LogInformation($"Number of records updated : {ret}");

            }
            catch (Exception ex)
            {
                logger.LogError($"error occured while inserting {ex}");
                throw;
            }
            finally
            {
                dbc.TriggerMail(true);
            }

        }

        public async Task exUpdateAsync(CancellationToken token) 
        {
            try
            {

                var expn = PredicateBuilder.Create<ClassQrcode>(x => x.rstatus == "I");
                var qrc = dbc.Select(expn).FirstOrDefault(); //example for selection by expression
                qrc.rstatus = "app";
                int ret = await dbc.UpdateAsync(qrc, token);
                logger.LogInformation($"Number of records updated : {ret}");
                var newexpn = PredicateBuilder.Create<ClassQrcode>(x => x.rstatus == "app");
                var qrc1 = dbc.Select(newexpn).FirstOrDefault(); //example for selection by expression
                qrc1.rstatus = "I";
                logger.LogInformation($"after updating {qrc1.rstatus}");
                logger.LogInformation("Updating again");
                int ret1 = await dbc.UpdateAsync(qrc, new string[] { "rstatus" }, token);
                logger.LogInformation($"Number of records updated : {ret1}");

            }
            catch (Exception ex)
            {
                logger.LogError($"error occured while inserting {ex}");
                throw;
            }
            finally
            {
                dbc.TriggerMail(true);
            }
        }

        public void exSelect()
        {

            try
            {
                var lst = dbc.SelectAll<ClassQrcode>();
                logger.LogInformation($"selectall returned {lst.Count} records");
                var dict = dbc.SelectAnonymous("select * from tbl_qrcode");
                logger.LogInformation($" anonymous selection results in first element {dict.FirstOrDefault().Key!} and {dict.FirstOrDefault().Value!}");
            }
            catch (Exception ex)
            {
                logger.LogError($"error occured while inserting {ex}");
                throw;

            }
            finally
            {

                 dbc.TriggerMail(true);
            }




        }

        public async Task exSelectAsync(CancellationToken token)
        {
            var lst = await dbc.SelectAllAsync<ClassQrcode>(token);
            logger.LogInformation($"selectall returned {lst.Count} records");
          
        }

        public void exDelete()
        { 
          int uid=  dbc.SelectAll<ClassQrcode>().LastOrDefault()!.uid; //c#12
          int ret = dbc.Delete<ClassQrcode>(uid);
            logger.LogInformation(message: $"deleted  {ret} records");
            var where = new ClassQrcode() { uid = 72 }.Where();
            dbc.Delete<ClassQrcode>(where);

        }
        public void exExecute()
        {
            string qry = "insert into tbl_qrcode(rstatus,qrcode,created_dt) values(@0,@1,@2)";
            int ret=dbc.Execute<ClassQrcode>(qry, "I", Extensions.GenerateRandomString(5), DateTime.Now.ToOADate());
            logger.LogInformation($"execute returned {ret} records");
            var ret1=dbc.ExecuteScalar<int>("select count(*) from tbl_qrcode");
            logger.LogInformation($"executescalar returned {ret.ToString()} records");

        }



    }
    public static class Extensions
    {
        private static readonly char[] Template = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz1234567890".ToCharArray();
        [ThreadStatic]
        private static Random random;

        private static Random Random => random ??= new Random();
        public static string GenerateRandomString(int length)
        {
            StringBuilder stringBuilder = new StringBuilder(length);
            for (int i = 0; i < length; i++)
            {
                int rndIndex = Random.Next(Template.Length);
                stringBuilder.Append(Template[rndIndex]);
            }

            return stringBuilder.ToString();
           
        }
    }
}
