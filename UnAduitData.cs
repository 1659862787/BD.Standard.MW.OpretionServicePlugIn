using Kingdee.BOS.App.Data;
using Kingdee.BOS.Core.DynamicForm;
using Kingdee.BOS.Core.DynamicForm.PlugIn;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Orm.DataEntity;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using Kingdee.BOS.Core.Metadata.FieldElement;

namespace BD.Standard.MW.ListServicePlugIncs
{
    [Kingdee.BOS.Util.HotUpdate]
    [Description("单据反审核插件操作服务插件")]
    public class UnAduitData : AbstractOperationServicePlugIn
    {
        public override void OnPreparePropertys(Kingdee.BOS.Core.DynamicForm.PlugIn.Args.PreparePropertysEventArgs e)
        {
            base.OnPreparePropertys(e);
            List<Field> file = this.BusinessInfo.GetFieldList();
            foreach (Field item in file)
            {
                e.FieldKeys.Add(item.Key);
            }
        }
        public override void EndOperationTransaction(EndOperationTransactionArgs e)
        {
            base.EndOperationTransaction(e);
            try
            {
                IOperationResult operationResult = new OperationResult();
                foreach (DynamicObject entity in e.DataEntitys)
                {
                    //获取当前表单fid，单据编号、单据标识
                    string fid = entity[0].ToString();
                    string fbillno = entity["billno"].ToString();
                    string fromid = entity["FFormId"].ToString();
                    //获取传输状态，成功状态才会调用取消同步
                    string status = entity["F_WMS_STATUS"].ToString();
                    if (!status.Equals("同步成功")) continue;

                    operationResult = MWUTILS.MWData(this.Context, operationResult, "UnAudit", fromid, fbillno, fid, "");

                    if (operationResult == null) { continue; }

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
