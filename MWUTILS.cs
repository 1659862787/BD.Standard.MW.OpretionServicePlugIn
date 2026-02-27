using Kingdee.BOS;
using Kingdee.BOS.App.Data;
using Kingdee.BOS.Core.DynamicForm;
using Kingdee.BOS.WebApi.FormService;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq; 
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;

namespace BD.Standard.MW.ListServicePlugIncs
{
    /// <summary>
    /// 工具类
    /// </summary>
    public class MWUTILS
    {
        /// <summary>
        /// 封装正向json数据
        /// </summary>
        /// <param name="datas">表数据 表1:配置信息，表二:基本信息，表三:明细信息</param>
        /// <returns>多张单据json组成的数组</returns>
        public static string WMSJson(DataSet datas)
        {

            JObject data = new JObject();
            JObject header = new JObject();
            JArray jsonHeader = new JArray();

            JObject jobH = null;
            if (datas.Tables[1].Rows.Count == 0) return null;
            //批量传输,表头明细的关联字段
            string id = string.Empty;
            foreach (DataRow dataH in datas.Tables[1].Rows)
            {
                jobH = new JObject();
                foreach (DataColumn columns in dataH.Table.Columns)
                {
                    string column = columns.ColumnName.ToString();
                    if (column.Equals("id"))
                    {
                        id = dataH["id"].ToString();
                        continue;
                    }
                    else jobH.Add(new JProperty(column, dataH[column].ToString()));
                }
                if (datas.Tables.Count == 3)
                {
                    JArray jsonEntry = new JArray();
                    foreach (DataRow dataE in datas.Tables[2].Rows)
                    {
                        if (!id.Equals(dataE["id"].ToString())) continue;
                        JObject jobE = new JObject();
                        foreach (DataColumn columns in dataE.Table.Columns)
                        {
                            string column = columns.ColumnName.ToString();
                            if (column.Equals("id")) continue;
                            else if (column.Equals("shelfLife") || column.Equals("qtyOrdered"))
                            {
                                jobE.Add(new JProperty(column, Convert.ToDecimal(dataE[column].ToString())));
                            }
                            else jobE.Add(new JProperty(column, dataE[column].ToString()));

                        }
                        if (jobE.Count > 0) jsonEntry.Add(jobE);
                        jobH["details"] = jsonEntry;
                    }
                }
                jsonHeader.Add(jobH);
            }
            header["header"] = jsonHeader;
            data["data"] = header;
            return data.ToString();
        }

        /// <summary>
        /// 封装反向json数据
        /// </summary>
        /// <param name="datas"></param>
        /// <returns></returns>
        public static string WMSUnJson(DataSet datas)
        {

            JObject data = new JObject();
            JObject ordernos = new JObject();
            JArray jsonHeader = new JArray();
            JObject jobH = null;
            foreach (DataRow dataH in datas.Tables[1].Rows)
            {
                jobH = new JObject();
                foreach (DataColumn columns in dataH.Table.Columns)
                {
                    string column = columns.ColumnName.ToString();
                    jobH.Add(new JProperty(column, dataH[column].ToString()));
                }
                jsonHeader.Add(jobH);
            }
            ordernos["ordernos"] = jsonHeader;
            data["data"] = ordernos;
            return data.ToString();
        }

        /// <summary>
        /// 传输方法类
        /// </summary>
        /// <param name="context">当前上下文</param>
        /// <param name="operationResult">操作结果</param>
        /// <param name="type">存储类型</param>
        /// <param name="fromid">表单标识</param>
        /// <param name="fnumber">表单编码</param>
        /// <param name="fid">表单主键</param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public static IOperationResult MWData(Context context, IOperationResult operationResult,string type,string fromid,string fnumber,string fid,string username)
        {
            //获取配置信息
            DataSet config = DBUtils.ExecuteDataSet(context, string.Format("exec {0}", "MW_config"));
            string http = config.Tables[0].Rows[0].ItemArray[0].ToString();
            string format = config.Tables[0].Rows[0].ItemArray[1].ToString();
            string sign = config.Tables[0].Rows[0].ItemArray[2].ToString();
            string apptoken = config.Tables[0].Rows[0].ItemArray[3].ToString();
            string timestamp = config.Tables[0].Rows[0].ItemArray[4].ToString();
            //根据单据编号和单据标识获取对应的取消数据
            DataSet dy=new DataSet();
            string json = string.Empty;
            string status = string.Empty;
            switch (type)
            {
                //基础资料
                case "basic":
                    dy = DBUtils.ExecuteDataSet(context, string.Format("exec {0} {1}", "MW_"+ fromid, fid));
                    json = MWUTILS.WMSJson(dy);
                    status = "同步成功";
                    break;
                //单据
                case "bill":
                    dy = DBUtils.ExecuteDataSet(context, string.Format("exec {0} {1},'{2}'", "MW_" + fromid, fid, username));
                    json = MWUTILS.WMSJson(dy);
                    status = "同步成功";
                    break;
                //反审核、撤销操作
                case "UnAudit":
                    dy = DBUtils.ExecuteDataSet(context, string.Format("exec {0} {1},'{2}'", "MW_UnAudit", fnumber, fromid));
                    json = MWUTILS.WMSUnJson(dy);
                    status = "取消成功";
                    break;
            }
            //dy获取表数据，表一：method：请求方法，table：基础资料表名，
            string method = dy.Tables[0].Rows[0].ItemArray[0].ToString();
            string table = dy.Tables[0].Rows[0].ItemArray[1].ToString();
            string tableid = dy.Tables[0].Rows[0].ItemArray[2].ToString();
            if (json == null) return null;
            //拼接地址，同步数据
            string url1 = http + "?method=" + method + "&format=" + format + "&sign=" + sign + "&apptoken=" + apptoken + "&timestamp=" + timestamp;
            string ss =HttpPost(url1, json);
            ss = ss.Replace("0E-8", "0");
            JObject jo = (JObject)JsonConvert.DeserializeObject(ss);
            string returnDesc = jo["Response"]["return"]["returnDesc"].ToString();
            string returnFlag = jo["Response"]["return"]["returnFlag"].ToString();
            DateTime date = DateTime.Now;
            //同步成功，输出消息，更改单据同步状态字段，记录日志
            if (returnFlag.Equals("1"))
            {
                operationResult.OperateResult.Add(new OperateResult()
                {
                    SuccessStatus = true,
                    Name = "同步消息",
                    Message = string.Format(fnumber + ":WMS"+ status),
                    MessageType = MessageType.Normal,
                    PKValue = 0,
                });
                string sql = string.Format("update {0} set F_WMS_STATUS='{3}' where {1}={2}", table, tableid,fid, status);
                string sql2 = string.Format("insert into MW_logs values('{0}','{1}','{2}','{3}','{4}')", fromid, fnumber, status, json.Replace("'"," ") + ss, date.ToString("yyyy-MM-dd"));
                DBUtils.Execute(context, sql);
                DBUtils.Execute(context, sql2);
            }
            else
            {
                throw new Exception("失败信息：" + returnDesc + "\r\n请求json:" + json);
            }
            return operationResult;
            
        }

        /// <summary>
        /// http请求
        /// </summary>
        /// <param name="url"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public static string HttpPost(string url, string data)
        {
            HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(url);
            httpWebRequest.Method = "POST";
            using (StreamWriter streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
            {
                streamWriter.Write(data);
                streamWriter.Close();
            }
            HttpWebResponse httpWebResponse = (HttpWebResponse)httpWebRequest.GetResponse();
            string text = httpWebResponse.ContentEncoding;
            bool flag = text == null || text.Length < 1;
            if (flag)
            {
                text = "UTF-8";
            }
            StreamReader streamReader = new StreamReader(httpWebResponse.GetResponseStream(), Encoding.GetEncoding(text));
            return streamReader.ReadToEnd();
        }

        /// <summary>
        /// MD5加密
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static string GetMD5_32(string s)
        {
            MD5 mD = MD5.Create();
            byte[] array = mD.ComputeHash(Encoding.GetEncoding("utf-8").GetBytes(s));
            StringBuilder stringBuilder = new StringBuilder();
            for (int i = 0; i < array.Length; i++)
            {
                stringBuilder.Append(array[i].ToString("x").PadLeft(2, '0'));
            }

            return stringBuilder.ToString();
        }


        /// <summary>
        /// 保存接口
        /// </summary>
        /// <param name="formid"></param>
        /// <param name="date"></param>
        /// <param name="sqlname"></param>
        /// <param name="id"></param>
        /// <param name="logname"></param>
        /// <param name="ctx"></param>
        /// <param name="path"></param>
        public static void SaveS(string formid, object date, string sqlname, string id, string logname,Context ctx,string path)
        {
            
            JObject strDate = new JObject()
            {

                    new JProperty("IsAutoAdjustField","true"),
                    new JProperty("IsDraftWhenSaveFail","true"),
                    new JProperty("IsAutoSubmitAndAudit","true"),
                    new JProperty("IsDeleteEntry","true"),
                    new JProperty("Model",date),
            };

            String responseMsg = JsonConvert.SerializeObject(WebApiServiceCall.Save(ctx, formid, strDate.ToString()));
            Log.writestr("修改保存：" + id.ToString(), strDate.ToString(), responseMsg.ToString(), logname, path);
            

            var resultBackSave = JsonConvert.DeserializeObject<dtos.Root>(responseMsg);


            //中间表反写数据状态
            if (resultBackSave.Result.ResponseStatus.IsSuccess == "True")
            {
                //批号调整单
                if (formid.Equals("STK_LOTADJUST"))
                {
                    DataSet dSbt = DBUtils.ExecuteDataSet(ctx,string.Format("/*dialect*/ update a set FstatusPh='1',a.errorPH = '' from {0} a where a.id = '{1}'",
                sqlname, id));
                }
                //库存章台转换
                else if (formid.Equals("STK_StatusConvert"))
                {
                    DataSet dSbt = DBUtils.ExecuteDataSet(ctx,string.Format("/*dialect*/ update a set FstatusPh1='1',a.errorPH1 = '' from {0} a where a.id = '{1}'",
                sqlname, id));
                }
                else
                {
                    DataSet dSbt = DBUtils.ExecuteDataSet(ctx,string.Format("/*dialect*/ update a set Fstatus='1',a.error = '' from {0} a where a.id = '{1}'",
                sqlname, id));
                }
            }
            else if (resultBackSave.Result.ResponseStatus.IsSuccess == "False")
            {
                if (formid.Equals("STK_LOTADJUST"))
                {
                    var resultBackSaveErro = JsonConvert.DeserializeObject<Erro.Root>(responseMsg);
                    DataSet dSbt = DBUtils.ExecuteDataSet(ctx,string.Format("/*dialect*/update a set a.errorPH = '{2}' from {0} a where a.id = '{1}'",
                    sqlname, id, resultBackSaveErro.Result.ResponseStatus.Errors.FirstOrDefault()?.Message.ToString()));
                }
                else if (formid.Equals("STK_StatusConvert"))
                {
                    var resultBackSaveErro = JsonConvert.DeserializeObject<Erro.Root>(responseMsg);
                    DataSet dSbt = DBUtils.ExecuteDataSet(ctx,string.Format("/*dialect*/update a set a.errorPH1 = '{2}' from {0} a where a.id = '{1}'",
                    sqlname, id, resultBackSaveErro.Result.ResponseStatus.Errors.FirstOrDefault()?.Message.ToString()));
                }
                else
                {
                    var resultBackSaveErro = JsonConvert.DeserializeObject<Erro.Root>(responseMsg);
                    DataSet dSbt = DBUtils.ExecuteDataSet(ctx,string.Format("/*dialect*/update a set a.error = '{2}' from {0} a where a.id = '{1}'",
                    sqlname, id, resultBackSaveErro.Result.ResponseStatus.Errors.FirstOrDefault()?.Message.ToString()));
                }
            }
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
}
