using Kingdee.BOS;
using Kingdee.BOS.App.Data;
using Kingdee.BOS.Core.DynamicForm;
using Kingdee.BOS.Core.DynamicForm.PlugIn;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Core.Metadata.EntityElement;
using Kingdee.BOS.Core.Metadata.FieldElement;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.Orm.Metadata.DataEntity;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace BD.Standard.MW.ListServicePlugIncs
{
    [Kingdee.BOS.Util.HotUpdate]
    [Description("基础资料操作服务插件")]
    /*
     * 
     * 更新时间：2024年11月8日15:37:02
     * 
     * **/
    public class BasicData : AbstractOperationServicePlugIn
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
                IOperationResult operationResult = new OperationResult();
                foreach (DynamicObject entity in e.DataEntitys)
                {
                    //获取当前表单fid与编码
                    string fid = entity[0].ToString();
                    string fnumber = entity["number"].ToString();
                    //获取单据实体名
                    DynamicObjectType types = entity.DynamicObjectType;
                    string type = types.Name;
                    //数据处理公共方法
                    operationResult = MWUTILS.MWData(this.Context, operationResult, "basic", type, fnumber, fid,"");
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
