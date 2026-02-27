
using Kingdee.BOS;
using Kingdee.BOS.Core;
using System.ComponentModel;
using Kingdee.BOS.Contracts;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System;
using Kingdee.BOS.WebApi.FormService;
using System.Data;
using System.Security.Cryptography;
using System.Security.Policy;
using Kingdee.BOS.App.Data;


//47
namespace BD.Standard.MW.ListServicePlugIncs
{
    [Kingdee.BOS.Util.HotUpdate]
    [Description("账单查询表单插件")]
    //BD.Standard.MW.ListServicePlugIncs.Runs,BD.Standard.MW.ListServicePlugIncs
    public class Runs : IScheduleService
    {
        public void Run(Context ctxold, Schedule schedule)
        {
            try
            {
                //参数：后台日志路径，默认空
                string parameter = schedule.Parameters;
                //表单参数id用于实际执行用户id
                string parameterFormId = schedule.ParameterFormId;
                //修改执行计划执行人
                var ctx = ctxold.Clone() as Context;
                ctx.CurrentOrganizationInfo = new OrganizationInfo();
                ctx.CurrentOrganizationInfo.ID = 127932; // 填入实际组织ID
                if (string.IsNullOrWhiteSpace(parameterFormId))
                {
                    ctx.UserId = 129866; // 填入实际用户id
                }
                else
                {
                    ctx.UserId =Convert.ToInt64(((string)parameterFormId).Trim());
                }
                string path = string.IsNullOrWhiteSpace(parameter) ? @"E:\MW\Log\" : parameter;
                DataSet dSbt = DBUtils.ExecuteDataSet(ctx,string.Format("EXEC MW_qt_generate"));

                //盘盈单
                foreach (DataRow item in dSbt.Tables[0].Rows)
                {
                    JArray jar = new JArray();
                    JObject FBillTypeID = new JObject()
                    {
                        new JProperty("FNUMBER",item.ItemArray[1].ToString()),

                    };
                    JObject FStockOrgId = new JObject()
                    {
                        new JProperty("FNumber",item.ItemArray[2].ToString()),

                    };

                    DataSet dSbtFentryid = DBUtils.ExecuteDataSet(ctx,string.Format("EXEC MW_qt_generateEntry '{0}'", item.ItemArray[3].ToString()));
                    foreach (DataRow itemS in dSbtFentryid.Tables[0].Rows)
                    {

                        JArray jar_link = new JArray();
                        JObject FMaterialId = new JObject()
                                {
                                    new JProperty("FNumber",itemS.ItemArray[1].ToString()),
                                };

                        JObject FStockId = new JObject()
                                {
                                    new JProperty("FNumber",itemS.ItemArray[6].ToString()),
                                };
                        JObject FLot = new JObject()
                            {
                                new JProperty("FNumber",itemS.ItemArray[3].ToString()),
                            };
                        JObject FStockStatus = new JObject()
                            {
                                new JProperty("FNumber",itemS.ItemArray[10].ToString()),
                            };
                        JObject FF100001 = new JObject()
                            {
                                new JProperty("FNumber",itemS.ItemArray[11].ToString()),
                            };
                        JObject FAuxPropId = new JObject()
                            {
                                new JProperty("FAUXPROPID__FF100001",FF100001),
                                new JProperty("FAUXPROPID__FF100002",itemS.ItemArray[12].ToString()),
                            };

                        JObject FBillEntry = new JObject()
                                {
                                    new JProperty("FMaterialId",FMaterialId),
                                    new JProperty("FCountQty",itemS.ItemArray[2].ToString()),
                                    new JProperty("FGainQty",itemS.ItemArray[2].ToString()),
                                    new JProperty("FLot",FLot),
                                    new JProperty("FStockId",FStockId),
                                    new JProperty("FStockStatusId",FStockStatus),
                                    new JProperty("FProduceDate",itemS.ItemArray[4].ToString()),
                                    new JProperty("FExpiryDate",itemS.ItemArray[5].ToString()),
                                    new JProperty("F_UJED_WMSCK",itemS.ItemArray[7].ToString()),
                                    new JProperty("F_UJED_WMSCW",itemS.ItemArray[8].ToString()),
                                    new JProperty("FMTONO",itemS.ItemArray[9].ToString()),
                                    new JProperty("FAuxPropId",FAuxPropId)
                                };

                        jar.Add(FBillEntry);

                    }
                    JObject Model = new JObject()
                            {
                                new JProperty("FBillNo",item.ItemArray[3].ToString()),
                                new JProperty("FDate",item.ItemArray[4].ToString()),
                                new JProperty("FBillTypeID",FBillTypeID),
                                new JProperty("FStockOrgId",FStockOrgId),
                        new JProperty("FBillEntry",jar),
                    };


                    MWUTILS.SaveS("STK_StockCountGain", Model, "t_getInventoryAdjustment", item.ItemArray[0].ToString(),"log_盘盈.txt", ctx, path);
                }


                //盘亏单

                foreach (DataRow item in dSbt.Tables[1].Rows)
                {
                    JArray jar = new JArray();
                    JObject FBillTypeID = new JObject()
                            {
                                new JProperty("FNUMBER",item.ItemArray[1].ToString()),

                            };
                    JObject FStockOrgId = new JObject()
                            {
                                new JProperty("FNumber",item.ItemArray[2].ToString()),

                            };

                    DataSet dSbtFentryid = DBUtils.ExecuteDataSet(ctx,string.Format("EXEC MW_qt_generateEntry '{0}'", item.ItemArray[3].ToString()));
                    foreach (DataRow itemS in dSbtFentryid.Tables[1].Rows)
                    {

                        JArray jar_link = new JArray();
                        JObject FMaterialId = new JObject()
                                {
                                    new JProperty("FNumber",itemS.ItemArray[1].ToString()),
                                };

                        JObject FStockId = new JObject()
                                {
                                    new JProperty("FNumber",itemS.ItemArray[6].ToString()),
                                };
                        JObject FLot = new JObject()
                            {
                                new JProperty("FNumber",itemS.ItemArray[3].ToString()),
                            };
                        JObject FStockStatus = new JObject()
                            {
                                new JProperty("FNumber",itemS.ItemArray[10].ToString()),
                            };
                        JObject FF100001 = new JObject()
                            {
                                new JProperty("FNumber",itemS.ItemArray[11].ToString()),
                            };
                        JObject FAuxPropId = new JObject()
                            {
                                new JProperty("FAUXPROPID__FF100001",FF100001),
                                new JProperty("FAUXPROPID__FF100002",itemS.ItemArray[12].ToString()),
                            };


                        JObject FBillEntry = new JObject()
                                {
                                    new JProperty("FMaterialId",FMaterialId),
                                    new JProperty("FCountQty",itemS.ItemArray[2].ToString()),
                                    new JProperty("FLossQty",itemS.ItemArray[2].ToString()),
                                    new JProperty("FLot",FLot),
                                    new JProperty("FStockId",FStockId),
                                    new JProperty("FStockStatusId",FStockStatus),
                                    new JProperty("FProduceDate",itemS.ItemArray[4].ToString()),
                                    new JProperty("FExpiryDate",itemS.ItemArray[5].ToString()),
                                    new JProperty("F_UJED_WMSCK",itemS.ItemArray[7].ToString()),
                                    new JProperty("F_UJED_WMSCW",itemS.ItemArray[8].ToString()),
                                    new JProperty("FMTONO",itemS.ItemArray[9].ToString()),
                                    new JProperty("FAuxPropId",FAuxPropId)
                                };

                        jar.Add(FBillEntry);

                    }

                    JObject Model = new JObject()
                            {
                                new JProperty("FBillNo",item.ItemArray[3].ToString()),
                                new JProperty("FDate",item.ItemArray[4].ToString()),
                                new JProperty("FBillTypeID",FBillTypeID),
                                new JProperty("FStockOrgId",FStockOrgId),
                        new JProperty("FBillEntry",jar),
                    };


                    MWUTILS.SaveS("STK_StockCountLoss", Model, "t_getInventoryAdjustment", item.ItemArray[0].ToString(),"log_盘亏单.txt", ctx, path);
                }
           

                //库存状态转换单

                foreach (DataRow item in dSbt.Tables[2].Rows)
                {
                    JArray jar = new JArray();
                    JObject FBillTypeID = new JObject()
                            {
                                new JProperty("FNUMBER",item.ItemArray[1].ToString()),

                            };
                    JObject FStockOrgId = new JObject()
                            {
                                new JProperty("FNumber",item.ItemArray[2].ToString()),

                            };

                    DataSet dSbtFentryid = DBUtils.ExecuteDataSet(ctx,string.Format("EXEC MW_qt_generateEntry '{0}'", item.ItemArray[3].ToString()));
                    foreach (DataRow itemS in dSbtFentryid.Tables[2].Rows)
                    {

                        JArray jar_link = new JArray();
                        JObject FMaterialId = new JObject()
                                {
                                    new JProperty("FNumber",itemS.ItemArray[3].ToString()),
                                };

                        JObject FStockId = new JObject()
                                {
                                    new JProperty("FNumber",itemS.ItemArray[8].ToString()),
                                };
                        JObject FLot = new JObject()
                            {
                                new JProperty("FNumber",itemS.ItemArray[5].ToString()),
                            };
                        JObject FStockStatus = new JObject()
                            {
                                new JProperty("FNumber",itemS.ItemArray[11].ToString()),
                            };
                        JObject FF100001 = new JObject()
                            {
                                new JProperty("FNumber",itemS.ItemArray[13].ToString()),
                            };
                        JObject FAuxPropId = new JObject()
                            {
                                new JProperty("FAUXPROPID__FF100001",FF100001),
                                new JProperty("FAUXPROPID__FF100002",itemS.ItemArray[10].ToString()),
                            };


                        JObject FBillEntry = new JObject()
                                {

                                    new JProperty("FConvertType",itemS.ItemArray[2].ToString()),
                                    new JProperty("FMATERIALID",FMaterialId),
                                    new JProperty("FLot",FLot),
                                    new JProperty("FConvertQty",itemS.ItemArray[4].ToString()),
                                    new JProperty("FSTOCKID",FStockId),
                                    new JProperty("FStockStatus",FStockStatus),
                                    new JProperty("FProduceDate",itemS.ItemArray[6].ToString()),
                                    new JProperty("FExpiryDate",itemS.ItemArray[7].ToString()),
                                    new JProperty("FMTONO",itemS.ItemArray[12].ToString()),
                                    new JProperty("FAuxPropId",FAuxPropId)
                                };

                        jar.Add(FBillEntry);

                    }


                    JObject Model = new JObject()
                        {
                                new JProperty("FBillNo",item.ItemArray[3].ToString()),
                                new JProperty("FDate",item.ItemArray[4].ToString()),
                                new JProperty("FBillTypeID",FBillTypeID),
                                new JProperty("FStockOrgId",FStockOrgId),
                                new JProperty("FEntity",jar),
                        };
                    string FOwnerId = dSbtFentryid.Tables[2].Rows[0].ItemArray[15].ToString();
                    JObject FOwnerIdHead = new JObject()
                        {
                            new JProperty("FNumber",FOwnerId),
                        };
                    if (!string.IsNullOrWhiteSpace(FOwnerId))
                    {
                        Model["FOwnerTypeIdHead"] = dSbtFentryid.Tables[2].Rows[0].ItemArray[14].ToString();
                        Model["FOwnerIdHead"] = FOwnerIdHead;
                    }


                    MWUTILS.SaveS("STK_StockConvert", Model, "t_getInventoryTransfer", item.ItemArray[0].ToString(),"log_库存状态转换单.txt", ctx, path);


                }
           

                //批号调整单

               
                foreach (DataRow item in dSbt.Tables[3].Rows)
                {
                    JArray jar = new JArray();
                    JObject FBillTypeID = new JObject()
                            {
                                new JProperty("FNUMBER",item.ItemArray[1].ToString()),

                            };
                    JObject FStockOrgId = new JObject()
                            {
                                new JProperty("FNumber",item.ItemArray[2].ToString()),

                            };

                    DataSet dSbtFentryid = DBUtils.ExecuteDataSet(ctx,string.Format("EXEC MW_qt_generateEntry '{0}'", item.ItemArray[3].ToString()));
                    foreach (DataRow itemS in dSbtFentryid.Tables[3].Rows)
                    {

                        JArray jar_link = new JArray();
                        JObject FMaterialId = new JObject()
                                {
                                    new JProperty("FNumber",itemS.ItemArray[3].ToString()),
                                };

                        JObject FStockId = new JObject()
                                {
                                    new JProperty("FNumber",itemS.ItemArray[8].ToString()),
                                };
                        JObject FLot = new JObject()
                            {
                                new JProperty("FNumber",itemS.ItemArray[5].ToString()),
                            };
                        JObject FStockStatus = new JObject()
                            {
                                new JProperty("FNumber",itemS.ItemArray[12].ToString()),
                            };
                        JObject FF100001 = new JObject()
                            {
                                new JProperty("FNumber",itemS.ItemArray[13].ToString()),
                            };
                        JObject FAuxPropId = new JObject()
                            {
                                new JProperty("FAUXPROPID__FF100001",FF100001),
                                new JProperty("FAUXPROPID__FF100002",itemS.ItemArray[10].ToString()),
                            };


                        JObject FBillEntry = new JObject()
                                {

                                    new JProperty("FGroup",Convert.ToInt32(itemS.ItemArray[1])),
                                    new JProperty("FConvertType",itemS.ItemArray[2].ToString()),
                                    new JProperty("FMATERIALID",FMaterialId),
                                    new JProperty("FLot",FLot),
                                    new JProperty("FQty",itemS.ItemArray[4].ToString()),
                                    new JProperty("FSTOCKID",FStockId),
                                    new JProperty("FStockStatusId",FStockStatus),
                                    new JProperty("FProduceDate",itemS.ItemArray[6].ToString()),
                                    new JProperty("FExpiryDate",itemS.ItemArray[7].ToString()),
                                    new JProperty("FMTONO",itemS.ItemArray[11].ToString()),
                                    new JProperty("FAuxPropId",FAuxPropId)
                                };
                        //FBillEntry["FGroup"] = itemS.ItemArray[1].ToString();
                        jar.Add(FBillEntry);

                    }

                    JObject Model = new JObject()
                            {
                                new JProperty("FBillNo",item.ItemArray[3].ToString()),
                                new JProperty("FDate",item.ItemArray[4].ToString()),
                                new JProperty("FBillTypeID",FBillTypeID),
                                new JProperty("FStockOrgId",FStockOrgId),
                                new JProperty("FSTK_LOTADJUSTENTRY",jar),
                            };
                    string FOwnerId = dSbtFentryid.Tables[3].Rows[0].ItemArray[15].ToString();
                    JObject FOwnerIdHead = new JObject()
                        {
                            new JProperty("FNumber",FOwnerId),
                        };
                    if (!string.IsNullOrWhiteSpace(FOwnerId))
                    {
                        Model["FOwnerTypeIdHead"] = dSbtFentryid.Tables[3].Rows[0].ItemArray[14].ToString();
                        Model["FOwnerIdHead"] = FOwnerIdHead;
                    }

                    MWUTILS.SaveS("STK_LOTADJUST", Model, "t_getInventoryTransfer", item.ItemArray[0].ToString(),"log_批号调整单.txt",ctx, path);


                }
           


            //形态转换单

                foreach (DataRow item in dSbt.Tables[4].Rows)
                {
                    JArray jar = new JArray();
                    JObject FBillTypeID = new JObject()
                            {
                                new JProperty("FNUMBER",item.ItemArray[1].ToString()),

                            };
                    JObject FStockOrgId = new JObject()
                            {
                                new JProperty("FNumber",item.ItemArray[2].ToString()),

                            };

                    DataSet dSbtFentryid = DBUtils.ExecuteDataSet(ctx,string.Format("EXEC MW_qt_generateEntry '{0}'", item.ItemArray[3].ToString()));
                    foreach (DataRow itemS in dSbtFentryid.Tables[4].Rows)
                    {

                        JArray jar_link = new JArray();
                        JObject FMaterialId = new JObject()
                        {
                            new JProperty("FNumber",itemS.ItemArray[3].ToString()),
                        };

                        JObject FStockId = new JObject()
                        {
                            new JProperty("FNumber",itemS.ItemArray[8].ToString()),
                        };
                        JObject FLot = new JObject()
                        {
                            new JProperty("FNumber",itemS.ItemArray[5].ToString()),
                        };
                        JObject FStockStatus = new JObject()
                        {
                            new JProperty("FNumber",itemS.ItemArray[12].ToString()),
                        };
                        JObject FF100001 = new JObject()
                        {
                            new JProperty("FNumber",itemS.ItemArray[13].ToString()),
                        };
                        JObject FAuxPropId = new JObject()
                        {
                            new JProperty("FAUXPROPID__FF100001",FF100001),
                            new JProperty("FAUXPROPID__FF100002",itemS.ItemArray[10].ToString()),
                        };


                        JObject FBillEntry = new JObject()
                                {

                                    new JProperty("FGroup",Convert.ToInt32(itemS.ItemArray[1])),
                                    new JProperty("FConvertType",itemS.ItemArray[2].ToString()),
                                    new JProperty("FMATERIALID",FMaterialId),
                                    new JProperty("FLot",FLot),
                                    new JProperty("FConvertQty",itemS.ItemArray[4].ToString()),
                                    new JProperty("FSTOCKID",FStockId),
                                    new JProperty("FStockStatus",FStockStatus),
                                    new JProperty("FProduceDate",itemS.ItemArray[6].ToString()),
                                    new JProperty("FExpiryDate",itemS.ItemArray[7].ToString()),
                                    new JProperty("FMTONO",itemS.ItemArray[11].ToString()),
                                    new JProperty("FAuxPropId",FAuxPropId)
                                };
                        //FBillEntry["FGroup"] = itemS.ItemArray[1].ToString();
                        jar.Add(FBillEntry);

                    }

                    JObject Model = new JObject()
                    {
                        new JProperty("FBillNo",item.ItemArray[3].ToString()),
                        new JProperty("FDate",item.ItemArray[4].ToString()),
                        new JProperty("FBillTypeID",FBillTypeID),
                        new JProperty("FStockOrgId",FStockOrgId),
                        new JProperty("FOwnerIdHead",FStockOrgId),
                        new JProperty("FEntity",jar),
                    };
                    string FOwnerId = dSbtFentryid.Tables[4].Rows[0].ItemArray[15].ToString();
                    JObject FOwnerIdHead = new JObject()
                        {
                            new JProperty("FNumber",FOwnerId),
                        };
                    if (!string.IsNullOrWhiteSpace(FOwnerId))
                    {
                        Model["FOwnerTypeIdHead"] = dSbtFentryid.Tables[4].Rows[0].ItemArray[14].ToString();
                        Model["FOwnerIdHead"] = FOwnerIdHead;
                    }

                    MWUTILS.SaveS("STK_StatusConvert", Model, "t_getInventoryTransfer", item.ItemArray[0].ToString(),"log_形态转换单.txt", ctx, path);


                }
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }





            //String MessageReturned = JsonConvert.SerializeObject(WebApiServiceCall.Save(ctx, formid, data));
            /*if (JObject.Parse(MessageReturned)["Result"]["ResponseStatus"]["IsSuccess"].ToString().Equals("True"))
            {
                return MessageReturned;
            }
            else
            {
                return MessageReturned;
            }
*/

        }
    }
}
