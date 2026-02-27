using Kingdee.BOS.App.Data;
using Kingdee.BOS.Core.DynamicForm;
using Kingdee.BOS.Core.DynamicForm.PlugIn;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Orm.Metadata.DataEntity;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Text;
using Kingdee.BOS.Core.Metadata.FieldElement;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS;

namespace BD.Standard.MW.ListServicePlugIncs
{
    [Kingdee.BOS.Util.HotUpdate]
    [Description("单据操作服务插件")]

    /*
     * 
     * 更新时间：2024年11月8日15:37:02
     * 
     * **/
    public class BillData : AbstractOperationServicePlugIn
    {
        /// <summary>
        /// 数据初始化
        /// </summary>
        /// <param name="e"></param>
        public override void OnPreparePropertys(Kingdee.BOS.Core.DynamicForm.PlugIn.Args.PreparePropertysEventArgs e)
        {
            base.OnPreparePropertys(e);
            List<Field> file = this.BusinessInfo.GetFieldList();
            foreach (Field item in file)
            {
                e.FieldKeys.Add(item.Key);
            }
        }
        /// <summary>
        /// 菜单操作方法
        /// </summary>
        /// <param name="e"></param>
        public override void EndOperationTransaction(EndOperationTransactionArgs e)
        {
            base.EndOperationTransaction(e);
            try
            {
                string opera = this.FormOperation.Operation;
                IOperationResult operationResult = new OperationResult();
                foreach (DynamicObject entity in e.DataEntitys)
                {
                    //获取当前表单fid，单据编号、单据标识、当前操作用户
                    string fid = entity[0].ToString();
                    string fbillno = entity["billno"].ToString();
                    string fromid = entity["FFormId"].ToString();
                    string username = this.Context.UserName;
                    //其他出库单
                    if (fromid.Equals("STK_MisDelivery"))
                    {
                        //获取WMS出库类型
                        string org = ((DynamicObject)entity["StockOrgId"])["Number"].ToString();
                        //获取WMS出库类型
                        DynamicObject ass = (DynamicObject)entity["F_WMS_ASSISTANT"];
                        //迈威组织同步
                        if (org.Equals("102") && ass!=null)
                        {
                            string number = ass["FNumber"].ToString();
                            //提交操作下，出库方向是：RETURN，WMS出库类型不是QTBF，同步WMS
                            if (opera.Equals("Submit") && entity["StockDirect"].ToString().Equals("RETURN") && !number.Equals("QTBF"))
                            {
                                operationResult = MWUTILS.MWData(this.Context, operationResult, "bill", fromid, fbillno, fid, username);
                                if (operationResult == null) { continue; }
                            }
                            //审核操作下，出库方向是：GENERAL，WMS出库类型是QTBF，同步WMS
                            else if (opera.Equals("Audit") && entity["StockDirect"].ToString().Equals("GENERAL") && number.Equals("QTBF"))
                            {
                                operationResult = MWUTILS.MWData(this.Context, operationResult, "bill", fromid, fbillno, fid, username);
                                if (operationResult == null) { continue; }
                            }
                        }
                       

                    }
                    else
                    {
                        operationResult = MWUTILS.MWData(this.Context, operationResult, "bill", fromid, fbillno, fid, username);
                        if (operationResult == null) { continue; }
                    }

                    
                }
                this.OperationResult.MergeResult(operationResult);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }


    }
}
