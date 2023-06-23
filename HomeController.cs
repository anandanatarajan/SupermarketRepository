using dllTest_core.Models;
using Microsoft.AspNetCore.Mvc;
using SupermarketRepository;
using System.Diagnostics;
using LinqKit.Core;
using LinqKit;
using NPoco.Expressions;
using NPoco;

namespace dllTest_core.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly CRUDRepository _crudRepository;
        private readonly IConfiguration manager;
        private readonly string connstring = string.Empty;
        public HomeController(ILogger<HomeController> logger, CRUDRepository repository, IConfiguration _config)
        {
            _logger = logger;
            _crudRepository = repository;
            manager = _config;
            connstring = manager.GetConnectionString("Default");
            
        }

        public IActionResult Index()
        {
            _logger.LogInformation("index page");
            
            
            ClassUserMachine cum = new() { DEVICEID = 2 };
            string where = cum.Where();
            //int ret = _crudRepository.AddNew(cum);
            return View();
        }


        public void Update()
        {
            var predicate = NPoco.Expressions.PredicateBuilder.Create<ClassUserMachine>(p => p.USERID == 232);
            
            var sngRecord = _crudRepository.Single<ClassUserMachine>(predicate);
#if DEBUG
            Debug.Assert(sngRecord != null);
            sngRecord.DEVICEID = 1;
            int updcount = _crudRepository.Update(sngRecord);
            Debug.Assert(updcount > 0);
            sngRecord.DEVICEID = 2;
            updcount = _crudRepository.Update<ClassUserMachine>(1, sngRecord);
            Debug.Assert(updcount > 0);
            sngRecord.DEVICEID = 232;
            updcount = _crudRepository.Update<ClassUserMachine>(sngRecord, new string[] { "DEVICEID" });
            Debug.Assert(TryValidateModel(sngRecord));
            Debug.Assert(updcount > 0);
#endif



        }

        public void Select()
        {
            var lst = _crudRepository.SelectAll<ClassUserMachine>();
            Debug.Assert(lst != null);
            var ret = _crudRepository.SelectAnonymous("select userid,deviceid from usersmachines");
            Debug.Assert(ret != null);
            var predict = NPoco.Expressions.PredicateBuilder.Create<ClassUserMachine>(x => x.USERID == 1);
            var lst1 = _crudRepository.Select<ClassUserMachine>(predict);
            Debug.Assert(lst1 != null);
            
            var lst2 = _crudRepository.SelectBySP<ClassUserMachine>("usp_SelectUserDevice",1);
            Debug.Assert(lst2 != null);

            var obj = _crudRepository.SelectById<ClassUserMachine>(1);
            Debug.Assert(obj != null);
        }


        public void Delete()
        {
            int ret=_crudRepository.Delete<ClassUserMachine>(2);
            Debug.Assert(ret != 0);
            var where = new ClassUserMachine() { DEVICEID = 222 }.Where();
            int ret1 = _crudRepository.Delete<ClassUserMachine>(where);
            Debug.Assert(ret1 != 0);

            
        }

        public void Misc()
        {
           var cls= _crudRepository.Execute<ClassUserMachine>("select top 1 * from usersmachines");
            Debug.Assert(TryValidateModel(cls));

           var obj= _crudRepository.ExecutScalar("select count(*) from usersmachines");
            Debug.Assert(obj!= null);
            var predict = NPoco.Expressions.PredicateBuilder.Create<ClassUserMachine>(x => x.USERID == 1);
            var cls1=_crudRepository.Single<ClassUserMachine>(predict);
            Debug.Assert(TryValidateModel(cls1));

        }


        public IActionResult Privacy()
        {
            Update();
            Select();
            List<ClassUserMachine> classUserMachines = new List<ClassUserMachine>() { 
                new ClassUserMachine() {USERID = 4,DEVICEID=44},
                new ClassUserMachine() {USERID=5,DEVICEID=55},
                new ClassUserMachine() {USERID=6,DEVICEID=66}
            };
         
            _crudRepository.BulkAdd(classUserMachines);
            Delete();
            Misc();

            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}