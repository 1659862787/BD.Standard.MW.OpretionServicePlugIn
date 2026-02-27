using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MW.TK.WMSToERP.API.Helper
{
    public class HelperClass
    {
        //登录
        public static string Login(string url, string accid, string uid, string pwd)
        {
            var client = new RestClient("" + url + "Kingdee.BOS.WebApi.ServicesStub.AuthService.ValidateUser.common.kdsvc");
            client.Timeout = -1;
            var request = new RestRequest(Method.POST);
            request.AlwaysMultipartFormData = true;
            request.AddParameter("acctID", accid);
            request.AddParameter("username", uid);
            request.AddParameter("Password", pwd);
            request.AddParameter("lcid", "2052");
            IRestResponse response = client.Execute(request);
            var responseMsglogin = response.Content;//获取返回Response消息
            //接收返回值 Cookie
            var resultBacklogin = JsonConvert.DeserializeObject<Root>(responseMsglogin);

            return resultBacklogin.KDSVCSessionId.ToString();
        }

        //下推
        public static string Push(string fromid, object date, string sqlname, string id, string url, string KDSVCSessionId)
        {
            string FID = "";
            try
            {

                DB db = new DB(ConfigurationManager.AppSettings["con"]);
                JObject obFormid = new JObject()
                {
                     new JProperty("FormId",fromid),
                     new JProperty("data",date),
                };

                var client = new RestClient("" + url + "Kingdee.BOS.WebApi.ServicesStub.DynamicFormService.Push.common.kdsvc");
                client.Timeout = -1;
                var request = new RestRequest(Method.POST);
                request.AddHeader("kdservice-sessionid", KDSVCSessionId);
                request.AddHeader("Content-Type", "application/json");
                var body = obFormid;
                request.AddParameter("application/json", body, ParameterType.RequestBody);
                IRestResponse response = client.Execute(request);
                var responseMsg = response.Content;//获取返回Response消息
                Log.writestr("下推：" + id.ToString(), body.ToString(), responseMsg.ToString(), "log_下推.txt");
                var resultBackPush = JsonConvert.DeserializeObject<PushClass.Root>(responseMsg);

                if (resultBackPush.Result.ResponseStatus.IsSuccess == "True")
                {
                    FID = resultBackPush.Result.ResponseStatus.SuccessEntitys.FirstOrDefault()?.Id.ToString();
                    //DataSet dSbt = db.getDataSet(string.Format("update a set a.F_QLQL_FID = '{2}',a.Ferror = 'null' from {0} a where a.FBILLNO = '{1}'",
                    //sqlname, id, resultBackPush.Result.ResponseStatus.SuccessEntitys.FirstOrDefault()?.Id.ToString()));

                }


            }
            catch (Exception)
            {

            }
            return FID;
        }
        //保存
        public static void Save(string fromid, object date, string sqlname, string id, string url, string KDSVCSessionId,string FID,string logname)
        {
            DB db = new DB(ConfigurationManager.AppSettings["con"]);

            //string[] array2 = new string[] { "FBillNo","FDate", "FEntity.Fentryid", "FStockId","FStockStatus","FQty","FLot","FProduceDate","FExpiryDate","F_UJED_WMSCK","F_UJED_WMSCW","FMTONO"};
            //string[] array2 = new string[] { "FStockStatus"};
            JObject strDate = new JObject()
            {
                    
                   // new JProperty("NeedUpDateFields",array2),
                    new JProperty("IsAutoAdjustField","true"),
                    new JProperty("IsDraftWhenSaveFail","true"),
                    new JProperty("IsAutoSubmitAndAudit","true"),
                    new JProperty("IsDeleteEntry","true"),
                    new JProperty("Model",date),
            };

            JObject obFormid = new JObject()
            {
                 new JProperty("FormId",fromid),
                 new JProperty("data",strDate),
            };

            var client = new RestClient("" + url + "Kingdee.BOS.WebApi.ServicesStub.DynamicFormService.Save.common.kdsvc");
            client.Timeout = -1;
            var request = new RestRequest(Method.POST);
            request.AddHeader("kdservice-sessionid", KDSVCSessionId);
            request.AddHeader("Content-Type", "application/json");
            var body = obFormid;
            request.AddParameter("application/json", body, ParameterType.RequestBody);
            IRestResponse response = client.Execute(request);
            var responseMsg = response.Content;//获取返回Response消息
            Log.writestr("修改保存：" + id.ToString(), body.ToString(), responseMsg.ToString(), logname);

            var resultBackSave = JsonConvert.DeserializeObject<dtos.Root>(responseMsg);

            if (resultBackSave.Result.ResponseStatus.IsSuccess == "True")
            {
                DataSet dSbt = db.getDataSet(string.Format("update a set Fstatus='1',a.error = '' from {0} a where a.id = '{1}'",
                sqlname, id));

                if (fromid == "BGP_SaleReturnInvoice")
                {
                    DataSet da = db.getDataSet(string.Format("exec MW_FH '{0}'", FID));
                    foreach (DataRow item in da.Tables[1].Rows)
                    {
                        Submit("BGP_SaleReturn", item.ItemArray[0].ToString(),url,KDSVCSessionId,"",FID,"");
                    }
                }
                if (fromid == "BGP_SaleOutbound")
                {

                    Audit("SAL_OUTSTOCK", FID, url, KDSVCSessionId, sqlname, id);
                    
                }

            }
            else if (resultBackSave.Result.ResponseStatus.IsSuccess == "False")
            {
                Delete(fromid, FID, url, KDSVCSessionId);
                var resultBackSaveErro = JsonConvert.DeserializeObject<Erro.Root>(responseMsg);
                DataSet dSbt = db.getDataSet(string.Format("update a set a.error = '{2}' from {0} a where a.id = '{1}'",
                sqlname, id, resultBackSaveErro.Result.ResponseStatus.Errors.FirstOrDefault()?.Message.ToString()));

            }
        }


        //保存
        public static void SaveS(string fromid, object date, string sqlname, string id, string url, string KDSVCSessionId, string logname)
        {
            DB db = new DB(ConfigurationManager.AppSettings["con"]);
            JObject strDate = new JObject()
            {

                    new JProperty("IsAutoAdjustField","true"),
                    new JProperty("IsDraftWhenSaveFail","true"),
                    new JProperty("IsAutoSubmitAndAudit","true"),
                    new JProperty("IsDeleteEntry","true"),
                    new JProperty("Model",date),
            };

            JObject obFormid = new JObject()
            {
                 new JProperty("FormId",fromid),
                 new JProperty("data",strDate),
            };

            var client = new RestClient("" + url + "Kingdee.BOS.WebApi.ServicesStub.DynamicFormService.Save.common.kdsvc");
            client.Timeout = -1;
            var request = new RestRequest(Method.POST);
            request.AddHeader("kdservice-sessionid", KDSVCSessionId);
            request.AddHeader("Content-Type", "application/json");
            var body = obFormid;
            request.AddParameter("application/json", body, ParameterType.RequestBody);
            IRestResponse response = client.Execute(request);
            var responseMsg = response.Content;//获取返回Response消息
            Log.writestr("修改保存：" + id.ToString(), body.ToString(), responseMsg.ToString(), logname);

            var resultBackSave = JsonConvert.DeserializeObject<dtos.Root>(responseMsg);
            
            if (resultBackSave.Result.ResponseStatus.IsSuccess == "True")
            {
                if (fromid.Equals("STK_LOTADJUST"))
                {
                    DataSet dSbt = db.getDataSet(string.Format("update a set FstatusPh='1',a.errorPH = '' from {0} a where a.id = '{1}'",
                sqlname, id));
                }
                else if (fromid.Equals("STK_StatusConvert"))
                {
                    DataSet dSbt = db.getDataSet(string.Format("update a set FstatusPh1='1',a.errorPH1 = '' from {0} a where a.id = '{1}'",
                sqlname, id));
                }
                else 
                {
                    DataSet dSbt = db.getDataSet(string.Format("update a set Fstatus='1',a.error = '' from {0} a where a.id = '{1}'",
                sqlname, id));
                }
                

            }
            else if (resultBackSave.Result.ResponseStatus.IsSuccess == "False")
            {
                if (fromid.Equals("STK_LOTADJUST"))
                {
                    var resultBackSaveErro = JsonConvert.DeserializeObject<Erro.Root>(responseMsg);
                    DataSet dSbt = db.getDataSet(string.Format("update a set a.errorPH = '{2}' from {0} a where a.id = '{1}'",
                    sqlname, id, resultBackSaveErro.Result.ResponseStatus.Errors.FirstOrDefault()?.Message.ToString()));
                }
                else if (fromid.Equals("STK_StatusConvert"))
                {
                    var resultBackSaveErro = JsonConvert.DeserializeObject<Erro.Root>(responseMsg);
                    DataSet dSbt = db.getDataSet(string.Format("update a set a.errorPH1 = '{2}' from {0} a where a.id = '{1}'",
                    sqlname, id, resultBackSaveErro.Result.ResponseStatus.Errors.FirstOrDefault()?.Message.ToString()));
                }
                else
                {
                    var resultBackSaveErro = JsonConvert.DeserializeObject<Erro.Root>(responseMsg);
                    DataSet dSbt = db.getDataSet(string.Format("update a set a.error = '{2}' from {0} a where a.id = '{1}'",
                    sqlname, id, resultBackSaveErro.Result.ResponseStatus.Errors.FirstOrDefault()?.Message.ToString()));
                }
            }
        }

        //保存
        public static void SaveQR(string fromid, object date, string sqlname, string id, string url, string KDSVCSessionId, string logname)
        {
            DB db = new DB(ConfigurationManager.AppSettings["con"]);
            JObject strDate = new JObject()
            {

                    new JProperty("IsAutoAdjustField","true"),
                    new JProperty("IsDraftWhenSaveFail","true"),
                    new JProperty("IsAutoSubmitAndAudit","true"),
                    new JProperty("IsDeleteEntry","true"),
                    new JProperty("Model",date),
            };

            JObject obFormid = new JObject()
            {
                 new JProperty("FormId",fromid),
                 new JProperty("data",strDate),
            };

            var client = new RestClient("" + url + "Kingdee.BOS.WebApi.ServicesStub.DynamicFormService.Save.common.kdsvc");
            client.Timeout = -1;
            var request = new RestRequest(Method.POST);
            request.AddHeader("kdservice-sessionid", KDSVCSessionId);
            request.AddHeader("Content-Type", "application/json");
            var body = obFormid;
            request.AddParameter("application/json", body, ParameterType.RequestBody);
            IRestResponse response = client.Execute(request);
            var responseMsg = response.Content;//获取返回Response消息
            Log.writestr("修改保存：" + id.ToString(), body.ToString(), responseMsg.ToString(), logname);

            var resultBackSave = JsonConvert.DeserializeObject<dtos.Root>(responseMsg);

            if (resultBackSave.Result.ResponseStatus.IsSuccess == "True")
            {
                DataSet dSbt = db.getDataSet(string.Format("update a set FstatusPh='1',a.errorPH = '' from {0} a where a.id = '{1}'",
                sqlname, id));

            }
            else if (resultBackSave.Result.ResponseStatus.IsSuccess == "False")
            {
                var resultBackSaveErro = JsonConvert.DeserializeObject<Erro.Root>(responseMsg);
                DataSet dSbt = db.getDataSet(string.Format("update a set a.errorPH = '{2}' from {0} a where a.id = '{1}'",
                sqlname, id, resultBackSaveErro.Result.ResponseStatus.Errors.FirstOrDefault()?.Message.ToString()));

            }
        }

        //保存
        public static void SaveAudit(string fromid, object date, string sqlname, string id, string url, string KDSVCSessionId, string FID, string fromidxsfh)
        {
            DB db = new DB(ConfigurationManager.AppSettings["con"]);
            JObject strDate = new JObject()
            {

                    new JProperty("IsAutoAdjustField","true"),
                    new JProperty("IsDraftWhenSaveFail","true"),
                    //new JProperty("IsAutoSubmitAndAudit","true"),
                    new JProperty("IsDeleteEntry","true"),
                    new JProperty("Model",date),
            };

            JObject obFormid = new JObject()
            {
                 new JProperty("FormId",fromid),
                 new JProperty("data",strDate),
            };

            var client = new RestClient("" + url + "Kingdee.BOS.WebApi.ServicesStub.DynamicFormService.Save.common.kdsvc");
            client.Timeout = -1;
            var request = new RestRequest(Method.POST);
            request.AddHeader("kdservice-sessionid", KDSVCSessionId);
            request.AddHeader("Content-Type", "application/json");
            var body = obFormid;
            request.AddParameter("application/json", body, ParameterType.RequestBody);
            IRestResponse response = client.Execute(request);
            var responseMsg = response.Content;//获取返回Response消息
            Log.writestr("修改保存：" + id.ToString(), body.ToString(), responseMsg.ToString(), "log_修改审核.txt");

            var resultBackSave = JsonConvert.DeserializeObject<dtos.Root>(responseMsg);

            if (resultBackSave.Result.ResponseStatus.IsSuccess == "True")
            {

                if (fromid == "PRD_PickMtrl" || fromid == "PRD_FeedMtrl" || fromid == "PRD_ReturnMtrl" ||
                    fromid == "STK_MISCELLANEOUS" || fromid == "PRD_INSTOCK" || fromid == "STK_MisDelivery" || 
                    fromid == "PUR_MRB" || fromid == "STK_TransferDirect")
                {
                    Audit(fromid,FID,url,KDSVCSessionId,sqlname,id);
                }
                else if (fromid == "SAL_OUTSTOCK")
                {
                    Submit(fromid, FID, url, KDSVCSessionId, sqlname, id, fromidxsfh);
                }
                else if (fromid == "STK_TRANSFERIN")
                {
                    DataSet dSbt = db.getDataSet(string.Format("update a set Fstatus='1',a.error = '' from {0} a where a.id = '{1}'",
                    sqlname, FID));
                }
            }
            else if (resultBackSave.Result.ResponseStatus.IsSuccess == "False")
            {
                //Delete(fromid, FID, url, KDSVCSessionId);
                var resultBackSaveErro = JsonConvert.DeserializeObject<Erro.Root>(responseMsg);
                DataSet dSbt = db.getDataSet(string.Format("update a set a.error = '{2}' from {0} a where a.id = '{1}'",
                sqlname, id, resultBackSaveErro.Result.ResponseStatus.Errors.FirstOrDefault()?.Message.ToString()));

            }
        }


        //删除
        public static void Delete(string fromid, string id, string url, string KDSVCSessionId)
        {
            //DB db = new DB(ConfigurationManager.AppSettings["con"]);

            JObject strDate = new JObject()
            {
                new JProperty("FormId",fromid),
                new JProperty("ids",id),
            };

            JObject obFormid = new JObject()
            {
                 new JProperty("FormId",fromid),
                 new JProperty("data",strDate),
            };

            var client = new RestClient("" + url + "Kingdee.BOS.WebApi.ServicesStub.DynamicFormService.Delete.common.kdsvc");
            client.Timeout = -1;
            var request = new RestRequest(Method.POST);
            request.AddHeader("kdservice-sessionid", KDSVCSessionId);
            request.AddHeader("Content-Type", "application/json");
            var body = obFormid;
            request.AddParameter("application/json", body, ParameterType.RequestBody);
            IRestResponse response = client.Execute(request);
            var responseMsg = response.Content;//获取返回Response消息


        }

        //审核
        public static void Audit(string fromid, string id, string url, string KDSVCSessionId,string sqlname,string FID)
        {
            DB db = new DB(ConfigurationManager.AppSettings["con"]);

            JObject strDate = new JObject()
            {
                new JProperty("FormId",fromid),
                new JProperty("ids",id),
            };

            JObject obFormid = new JObject()
            {
                 new JProperty("FormId",fromid),
                 new JProperty("data",strDate),
            };

            var client = new RestClient("" + url + "Kingdee.BOS.WebApi.ServicesStub.DynamicFormService.Audit.common.kdsvc");
            client.Timeout = -1;
            var request = new RestRequest(Method.POST);
            request.AddHeader("kdservice-sessionid", KDSVCSessionId);
            request.AddHeader("Content-Type", "application/json");
            var body = obFormid;
            request.AddParameter("application/json", body, ParameterType.RequestBody);
            IRestResponse response = client.Execute(request);
            var responseMsg = response.Content;//获取返回Response消息
            Log.writestr("审核：" + id.ToString(), body.ToString(), responseMsg.ToString(), "log_修改审核.txt");
            var resultBackSave = JsonConvert.DeserializeObject<dtos.Root>(responseMsg);
            if (resultBackSave.Result.ResponseStatus.IsSuccess == "True")
            {

                DataSet dSbt = db.getDataSet(string.Format("update a set Fstatus='1',a.error = '' from {0} a where a.id = '{1}'",
                sqlname, FID));

            }
            else if (resultBackSave.Result.ResponseStatus.IsSuccess == "False")
            {
                //Delete(fromid, FID, url, KDSVCSessionId);
                var resultBackSaveErro = JsonConvert.DeserializeObject<Erro.Root>(responseMsg);
                DataSet dSbt = db.getDataSet(string.Format("update a set a.error = '{2}' from {0} a where a.id = '{1}'",
                sqlname, FID, resultBackSaveErro.Result.ResponseStatus.Errors.FirstOrDefault()?.Message.ToString()));

            }

            

        }


        //提交（销售出库提交后自动下推销售复核单，修改并审核销售出库复核单）
        public static void Submit(string fromid, string id, string url, string KDSVCSessionId, string sqlname, string FID,string fromidxsfh)
        {
            DB db = new DB(ConfigurationManager.AppSettings["con"]);

            JObject strDate = new JObject()
            {
                new JProperty("FormId",fromid),
                new JProperty("ids",id),
            };

            JObject obFormid = new JObject()
            {
                 new JProperty("FormId",fromid),
                 new JProperty("data",strDate),
            };

            var client = new RestClient("" + url + "Kingdee.BOS.WebApi.ServicesStub.DynamicFormService.Submit.common.kdsvc");
            client.Timeout = -1;
            var request = new RestRequest(Method.POST);
            request.AddHeader("kdservice-sessionid", KDSVCSessionId);
            request.AddHeader("Content-Type", "application/json");
            var body = obFormid;
            request.AddParameter("application/json", body, ParameterType.RequestBody);
            IRestResponse response = client.Execute(request);
            var responseMsg = response.Content;//获取返回Response消息
            Log.writestr("提交：" + id.ToString(), body.ToString(), responseMsg.ToString(), "log_提交.txt");
            var resultBackSave = JsonConvert.DeserializeObject<dtos.Root>(responseMsg);
            if (resultBackSave.Result.ResponseStatus.IsSuccess == "True")
            {

                if (fromid!= "BGP_SaleReturn")
                {
                    JObject obFormidS = new JObject()
                         {
                                    new JProperty("Ids",id),
                                    new JProperty("RuleId",fromidxsfh),
                                    new JProperty("IsEnableDefaultRule","false"),
                                    new JProperty("IsDraftWhenSaveFail","true"),
                         };
                    //获取接口下推后返回的FID
                    var FIDS = Helper.HelperClass.Push(fromid, obFormidS, "", id, url, KDSVCSessionId);
                    //查询销售复核单明细内码
                    JArray jarEntry = new JArray();
                    DataSet da = db.getDataSet(string.Format("exec MW_FH '{0}'", FIDS));

                    foreach (DataRow item in da.Tables[0].Rows)
                    {

                        //获取仓库信息
                        JObject wl = new JObject()
                            {
                                 new JProperty("FNumber",item.ItemArray[2].ToString()),
                            };

                        JObject obentry = new JObject()
                    {
                        new JProperty("FEntryID",item.ItemArray[0].ToString()),
                        //new JProperty("F_BGP_MaterialId",wl),
                        new JProperty("F_BGP_CheckResult",item.ItemArray[1].ToString())

                    };
                        jarEntry.Add(obentry);
                    }

                    JObject obfid = new JObject()
                    {
                        new JProperty("FID",FIDS),
                        new JProperty("FEntity",jarEntry)

                    };
                    Save("BGP_SaleOutbound", obfid, "t_getShipment", FID, url, KDSVCSessionId, id, "log_销售出库复核单修改审核.txt");
                }
                else if (fromid == "BGP_SaleReturn")
                {
                    Audit(fromid,id,url,KDSVCSessionId,"",FID);
                }

                
            }
            else if (resultBackSave.Result.ResponseStatus.IsSuccess == "False")
            {
                //Delete(fromid, FID, url, KDSVCSessionId);
                var resultBackSaveErro = JsonConvert.DeserializeObject<Erro.Root>(responseMsg);
                DataSet dSbt = db.getDataSet(string.Format("update a set a.error = '{2}' from {0} a where a.id = '{1}'",
                sqlname, FID, resultBackSaveErro.Result.ResponseStatus.Errors.FirstOrDefault()?.Message.ToString()));

            }



        }
        public class Root
        {
            /// <summary>
            /// 
            /// </summary>
            public string KDSVCSessionId { get; set; }
        }


        public class dtos
        {
            public class ResponseStatus
            {
                /// <summary>
                /// 
                /// </summary>
                public int ErrorCode { get; set; }
                /// <summary>
                /// 
                /// </summary>
                public string IsSuccess { get; set; }
            }

            public class NeedReturnDataItem
            {
            }

            public class Result
            {
                /// <summary>
                /// 
                /// </summary>
                public ResponseStatus ResponseStatus { get; set; }
                /// <summary>
                /// 
                /// </summary>
                public string Id { get; set; }
                /// <summary>
                /// 
                /// </summary>
                public string Number { get; set; }
                /// <summary>
                /// 
                /// </summary>
                public List<NeedReturnDataItem> NeedReturnData { get; set; }
            }

            public class Root
            {
                /// <summary>
                /// 
                /// </summary>
                public Result Result { get; set; }
            }

        }

        public class Erro
        {
            public class ErrorsItem
            {
                /// <summary>
                /// 
                /// </summary>
                public string FieldName { get; set; }
                /// <summary>
                /// 编码为20231018的物料，编码唯一
                /// </summary>
                public string Message { get; set; }
                /// <summary>
                /// 
                /// </summary>
                public int DIndex { get; set; }
            }

            public class ResponseStatus
            {
                /// <summary>
                /// 
                /// </summary>
                public int ErrorCode { get; set; }
                /// <summary>
                /// 
                /// </summary>
                public string IsSuccess { get; set; }
                /// <summary>
                /// 
                /// </summary>
                public List<ErrorsItem> Errors { get; set; }
                /// <summary>
                /// 
                /// </summary>
                public List<string> SuccessEntitys { get; set; }
                /// <summary>
                /// 
                /// </summary>
                public List<string> SuccessMessages { get; set; }
                /// <summary>
                /// 
                /// </summary>
                public int MsgCode { get; set; }
            }

            public class NeedReturnDataItem
            {
            }

            public class Result
            {
                /// <summary>
                /// 
                /// </summary>
                public ResponseStatus ResponseStatus { get; set; }
                /// <summary>
                /// 
                /// </summary>
                public string Id { get; set; }
                /// <summary>
                /// 
                /// </summary>
                public string Number { get; set; }
                /// <summary>
                /// 
                /// </summary>
                public List<NeedReturnDataItem> NeedReturnData { get; set; }
            }

            public class Root
            {
                /// <summary>
                /// 
                /// </summary>
                public Result Result { get; set; }
            }
        }


        public class PushClass
        {
            public class SuccessEntitysItem
            {
                /// <summary>
                /// 
                /// </summary>
                public int Id { get; set; }
                /// <summary>
                /// 
                /// </summary>
                public string Number { get; set; }
                /// <summary>
                /// 
                /// </summary>
                public int DIndex { get; set; }
            }

            public class ResponseStatus
            {
                /// <summary>
                /// 
                /// </summary>
                public string IsSuccess { get; set; }
                /// <summary>
                /// 
                /// </summary>
                public List<string> Errors { get; set; }
                /// <summary>
                /// 
                /// </summary>
                public List<SuccessEntitysItem> SuccessEntitys { get; set; }
                /// <summary>
                /// 
                /// </summary>
                public List<string> SuccessMessages { get; set; }
                /// <summary>
                /// 
                /// </summary>
                public int MsgCode { get; set; }
            }

            public class SuccessEntitysItem1
            {
                /// <summary>
                /// 
                /// </summary>
                public int Id { get; set; }
                /// <summary>
                /// 
                /// </summary>
                public string Number { get; set; }
                /// <summary>
                /// 
                /// </summary>
                public int DIndex { get; set; }
            }

            public class ConvertResponseStatus
            {
                /// <summary>
                /// 
                /// </summary>
                public string IsSuccess { get; set; }
                /// <summary>
                /// 
                /// </summary>
                public List<string> Errors { get; set; }
                /// <summary>
                /// 
                /// </summary>
                public List<SuccessEntitysItem1> SuccessEntitys { get; set; }
                /// <summary>
                /// 
                /// </summary>
                public List<string> SuccessMessages { get; set; }
                /// <summary>
                /// 
                /// </summary>
                public int MsgCode { get; set; }
            }

            public class Result
            {
                /// <summary>
                /// 
                /// </summary>
                public ResponseStatus ResponseStatus { get; set; }
                /// <summary>
                /// 
                /// </summary>
                public ConvertResponseStatus ConvertResponseStatus { get; set; }
            }

            public class Root
            {
                /// <summary>
                /// 
                /// </summary>
                public Result Result { get; set; }
            }
        }
    }
}
