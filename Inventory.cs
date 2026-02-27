using Kingdee.BOS;
using Kingdee.BOS.App.Data;
using Kingdee.BOS.Core.Bill.PlugIn;
using Kingdee.BOS.Core.DynamicForm;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System;
using System.Data;
using System.ComponentModel;
using System.Text;

namespace BD.Standard.MW.ListServicePlugIncs
{
    [Kingdee.BOS.Util.HotUpdate]
    [Description("库存对比表单插件")]
    public class Inventory: AbstractBillPlugIn
    {
        static int count = 1;
        public override void BeforeBindData(EventArgs e)
        {
            base.BeforeBindData(e);
            try
            {
                DataSet dy = new DataSet();
                IOperationResult operationResult = new OperationResult();
                //operationResult = MWUTILS.MWData(this.Context, operationResult, "Inventory", "", "", "", "");
                //清空中间表数据
                string sql = string.Format("delete from  MW_Inventory");
                DBUtils.Execute(this.Context, sql);

                //获取配置信息
                DataSet config = DBUtils.ExecuteDataSet(this.Context, string.Format("exec {0}", "MW_config"));
                string http = config.Tables[0].Rows[0].ItemArray[0].ToString();
                string format = config.Tables[0].Rows[0].ItemArray[1].ToString();
                string sign = config.Tables[0].Rows[0].ItemArray[2].ToString();
                string apptoken = config.Tables[0].Rows[0].ItemArray[3].ToString();
                string timestamp = config.Tables[0].Rows[0].ItemArray[4].ToString();
                //分页循环调用库存接口获取数据到表：MW_Inventory
                for (int i=1;i < 1000;i++) 
                {
                    dy = DBUtils.ExecuteDataSet(this.Context, string.Format("exec {0} "+i+"", "MW_queryInventory"));
                    string json = MWUTILS.WMSJson(dy);

                    string method = dy.Tables[0].Rows[0].ItemArray[0].ToString();
                    string url1 = http + "?method=" + method + "&format=" + format + "&sign=" + sign + "&apptoken=" + apptoken + "&timestamp=" + timestamp;
                    string ss = MWUTILS.HttpPost(url1, json);
                    ss = ss.Replace("0E-8", "0");
                    JObject jo = (JObject)JsonConvert.DeserializeObject(ss);
                    string returnDesc = jo["Response"]["return"]["returnDesc"].ToString();
                    string returnFlag = jo["Response"]["return"]["returnFlag"].ToString();
                    if (returnFlag.Equals("1"))
                    {
                        string Response = jo["Response"].ToString();
                        JObject items = (JObject)JsonConvert.DeserializeObject(Response);
                        if (items.Property("items")==null|| items.Property("items").ToString() == "")
                        {
                            break;
                        }
                        else
                        {
                            //循环取值
                            string sb =InsertInto(items, dy, "MW_Inventory",count);
                            DBUtils.Execute(this.Context, sb.ToString());
                        }    
                         
                    }
                }
                
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
           
        }

        /// <summary>
        /// 生成插入语句
        /// </summary>
        /// <param name="jsonstr">jsoN串</param>
        /// <param name="dy">表数据</param>
        /// <param name="table">表名</param>
        /// <returns></returns>
        public static string InsertInto(JObject json, DataSet dy, string table, int id)
        {
            JObject items = (JObject)json["items"];
            JArray item = (JArray)items["item"];

            StringBuilder sb = new StringBuilder();
            StringBuilder column = new StringBuilder();
            StringBuilder value = new StringBuilder();

            sb.Append(string.Format(@"/*dialect*/  insert into {0} ", table));
            value.Append("  values ");
            foreach (DataRow dataH in dy.Tables[0].Rows)
            {
                column.Append("(");
                foreach (DataColumn columns in dataH.Table.Columns)
                {
                    if (!columns.ColumnName.ToString().Equals("method") && !columns.ColumnName.ToString().Equals("a") && !columns.ColumnName.ToString().Equals("b"))
                    {
                        column.Append("" + dataH[columns.ColumnName.ToString()].ToString() + ",");

                    }
                }
            }
            column.Remove(column.Length - 1, 1);
            if (!string.IsNullOrWhiteSpace(column.ToString())) sb.Append(column.Append(")").ToString());

            foreach (JObject it in item)
            {
                value.Append("\r\n('" + id++ + "',");
                foreach (DataRow dataH in dy.Tables[0].Rows)
                {
                    foreach (DataColumn columns in dataH.Table.Columns)
                    {
                        if (!columns.ColumnName.ToString().Equals("method") && !columns.ColumnName.ToString().Equals("a") && !columns.ColumnName.ToString().Equals("b") && !columns.ColumnName.ToString().Equals("id"))
                        {
                            value.Append("'" + it[dataH[columns.ColumnName.ToString()].ToString()].ToString().Replace("'","''") + "',");
                        }

                    }
                }
                value.Remove(value.Length - 1, 1);
                value.Append(" ),");
            }
            sb.Append(value.ToString());
            sb.Remove(sb.Length - 1, 1);
            count = id;
            return sb.ToString();
        }


    }
}
