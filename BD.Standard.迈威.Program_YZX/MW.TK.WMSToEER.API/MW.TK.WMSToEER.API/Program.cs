using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MW.TK.WMSToERP.API
{
    class Program
    {
        static void Main(string[] args)
        {
            string url = ConfigurationManager.AppSettings["url"];
            string accid = ConfigurationManager.AppSettings["db"];
            string uid = ConfigurationManager.AppSettings["uid"];
            string pwd = ConfigurationManager.AppSettings["pwd"];


            DB db = new DB(ConfigurationManager.AppSettings["con"]);


            string modeltype = "";
            if (args.Count() > 0)
            {
                modeltype = args[0];
            }
            else
            {
                Console.WriteLine("请输入需要的操作:");
                modeltype = Console.ReadLine();
            }

        /*    if (modeltype == "rk")
            {
                //采购入库
                try
                {
                    //调用金蝶登录接口，获取TOKEN
                    var Token = Helper.HelperClass.Login(url, accid, uid, pwd);

                    //执行存储。获取需要生成的单据数据
                    DataSet da = db.getDataSet("exec MW_rk_generate");
                    foreach (DataRow item in da.Tables[0].Rows)
                    {
                        //定义集合
                        JArray jarEntry = new JArray();
                        //拼接下推JSON
                        JObject obFormid = new JObject()
                         {
                                    new JProperty("Numbers",item.ItemArray[1].ToString()),
                                    new JProperty("RuleId",item.ItemArray[3].ToString()),
                                    new JProperty("IsEnableDefaultRule","false"),
                                    new JProperty("IsDraftWhenSaveFail","true"),
                         };

                        //获取接口下推后返回的FID（下推销售退货单）
                        var FID = Helper.HelperClass.Push("PUR_ReceiveBill", obFormid, item.ItemArray[1].ToString(), item.ItemArray[2].ToString(), url, Token);
                        //获取接口下推后返回的FID（下推销售退回收货单）
                        //var FIDTHSH = Helper.HelperClass.Push("PUR_ReceiveBill", obFormidS, item.ItemArray[1].ToString(), item.ItemArray[2].ToString(), url, Token);

                        //

                        //执行存储，获取明细数据
                        DataSet daEntry = db.getDataSet(string.Format("exec MW_rk_generateEntry '{0}'", item.ItemArray[4].ToString()));
                        foreach (DataRow itemEntry in daEntry.Tables[0].Rows)
                        {
                            //获取仓库信息
                            JObject ck = new JObject()
                            {
                                 new JProperty("FNumber",itemEntry.ItemArray[3].ToString()),
                            };

                            //    JObject CW = new JObject()
                            //{
                            //     new JProperty("FNumber",itemEntry.ItemArray[5].ToString()),
                            //};

                            //    JObject CWzj = new JObject()
                            //{
                            //     new JProperty("FSTOCKLOCID__FF100004",CW),
                            //};
                            JObject FStockStatus = new JObject()
                            {
                                new JProperty("FNumber",itemEntry.ItemArray[8].ToString()),
                            };
                            //获取批号信息
                            JObject PH = new JObject()
                            {
                                new JProperty("FNumber",itemEntry.ItemArray[2].ToString()),
                            };
                            //拼接明细JSON
                            JObject FInStockEntry = new JObject()
                            {
                                new JProperty("FEntryID",itemEntry.ItemArray[4].ToString()),
                                new JProperty("FRealQty",itemEntry.ItemArray[1].ToString()),
                                new JProperty("FStockId",ck),
                                new JProperty("FStockStatusId",FStockStatus),
                                new JProperty("FLot",PH),
                                new JProperty("FProduceDate",itemEntry.ItemArray[6].ToString()),
                                new JProperty("FExpiryDate",itemEntry.ItemArray[7].ToString()),
                                new JProperty("FMtoNo",itemEntry.ItemArray[9].ToString())

                            };

                            jarEntry.Add(FInStockEntry);
                        }

                        //拼接表头JSON
                        JObject model = new JObject()
                        {
                            new JProperty("FID",FID),
                            new JProperty("FBillNo",item.ItemArray[4].ToString()),
                            new JProperty("FDate",item.ItemArray[5].ToString()),
                            new JProperty("FInStockEntry",jarEntry),
                        };
                        //调用金蝶保存接口，新增单据
                        Helper.HelperClass.Save("STK_InStock", model, "t_getReceipt", item.ItemArray[0].ToString(), url, Token, FID, "log_采购入库_入库.txt");
                    }
                }
                catch (Exception)
                {

                }

                //销售退货
                try
                {
                    //调用金蝶登录接口，获取TOKEN
                    var Token = Helper.HelperClass.Login(url, accid, uid, pwd);

                    //执行存储。获取需要生成的单据数据
                    DataSet da = db.getDataSet("exec MW_rk_generate");
                    foreach (DataRow item in da.Tables[1].Rows)
                    {
                        //定义集合
                        JArray jarEntry = new JArray();
                        //定义集合
                        JArray jarEntryXSTH = new JArray();
                        //拼接下推JSON
                        JObject obFormid = new JObject()
                         {
                                    new JProperty("Numbers",item.ItemArray[1].ToString()),
                                    new JProperty("RuleId",item.ItemArray[3].ToString()),
                                    new JProperty("IsEnableDefaultRule","false"),
                                    new JProperty("IsDraftWhenSaveFail","true"),
                         };
                        //获取接口下推后返回的FID
                        var FID = Helper.HelperClass.Push("SAL_RETURNNOTICE", obFormid, item.ItemArray[1].ToString(), item.ItemArray[2].ToString(), url, Token);
                        //执行存储，获取明细数据
                        DataSet daEntry = db.getDataSet(string.Format("exec MW_rk_generateEntry '{0}'", item.ItemArray[4].ToString()));
                        foreach (DataRow itemEntry in daEntry.Tables[1].Rows)
                        {
                            //获取仓库信息
                            JObject ck = new JObject()
                            {
                                 new JProperty("FNumber",itemEntry.ItemArray[3].ToString()),
                            };


                            JObject FStockStatus = new JObject()
                            {
                                new JProperty("FNumber",itemEntry.ItemArray[9].ToString()),
                            };
                            //获取批号信息
                            JObject PH = new JObject()
                            {
                                new JProperty("FNumber",itemEntry.ItemArray[2].ToString()),
                            };
                            //拼接明细JSON
                            JObject FEntity = new JObject()
                            {
                                new JProperty("FEntryID",itemEntry.ItemArray[4].ToString()),
                                new JProperty("FRealQty",itemEntry.ItemArray[1].ToString()),
                                new JProperty("FStockId",ck),
                                new JProperty("FStockStatusId",FStockStatus),
                                new JProperty("FLot",PH),
                                new JProperty("FProduceDate",itemEntry.ItemArray[6].ToString()),
                                new JProperty("FExpiryDate",itemEntry.ItemArray[7].ToString()),
                                new JProperty("FMtoNo",itemEntry.ItemArray[10].ToString())
                            };

                            jarEntry.Add(FEntity);

                            JObject FEntityXSTH = new JObject()
                            {
                                new JProperty("FEntryID",itemEntry.ItemArray[8].ToString()),
                                new JProperty("FQty",itemEntry.ItemArray[1].ToString()),
                                new JProperty("FStockId",ck),
                                new JProperty("FLot",PH),
                                new JProperty("FProduceDate",itemEntry.ItemArray[6].ToString()),
                                new JProperty("FExpiryDate",itemEntry.ItemArray[7].ToString()),
                                new JProperty("FMtoNo",itemEntry.ItemArray[10].ToString())
                            };

                            jarEntryXSTH.Add(FEntityXSTH);
                        }

                        //拼接表头JSON
                        JObject model = new JObject()
                        {
                            new JProperty("FID",FID),
                            new JProperty("FBillNo",item.ItemArray[4].ToString()),
                            new JProperty("FDate",item.ItemArray[5].ToString()),
                            new JProperty("FEntity",jarEntry),
                        };

                        //拼接表头JSON
                        JObject modelXSTH = new JObject()
                        {
                            new JProperty("FID",item.ItemArray[6].ToString()),
                            new JProperty("FBillNo",item.ItemArray[4].ToString()),
                            new JProperty("FDate",item.ItemArray[5].ToString()),
                            new JProperty("FEntity",jarEntryXSTH),
                        };
                        //调用金蝶保存接口，修改保存审核销售退货单
                        Helper.HelperClass.Save("SAL_RETURNSTOCK", model, "t_getReceipt", item.ItemArray[0].ToString(), url, Token, FID, "log_销售退货_入库.txt");
                        //调用金蝶保存接口，修改保存审核销售退回收货单
                        Helper.HelperClass.Save("BGP_SaleReturnInvoice", modelXSTH, "t_getReceipt", item.ItemArray[0].ToString(), url, Token, item.ItemArray[6].ToString(), "log_销售退回_入库.txt");

                    }
                }
                catch (Exception)
                {

                }

                //直接调拨
                try
                {
                    //调用金蝶登录接口，获取TOKEN
                    var Token = Helper.HelperClass.Login(url, accid, uid, pwd);

                    //执行存储。获取需要生成的单据数据
                    DataSet da = db.getDataSet("exec MW_rk_generate");
                    foreach (DataRow item in da.Tables[2].Rows)
                    {
                        //定义集合
                        JArray jarEntry = new JArray();
                        //   //拼接下推JSON
                        //   JObject obFormid = new JObject()
                        //{
                        //           new JProperty("Numbers",item.ItemArray[1].ToString()),
                        //           new JProperty("RuleId",item.ItemArray[3].ToString()),
                        //           new JProperty("IsEnableDefaultRule","false"),
                        //           new JProperty("IsDraftWhenSaveFail","true"),
                        //};
                        //   //获取接口下推后返回的FID
                        //   var FID = Helper.HelperClass.Push("STK_TRANSFERAPPLY", obFormid, item.ItemArray[1].ToString(), item.ItemArray[2].ToString(), url, Token);
                        //执行存储，获取明细数据
                        DataSet daEntry = db.getDataSet(string.Format("exec MW_rk_generateEntry '{0}'", item.ItemArray[4].ToString()));
                        foreach (DataRow itemEntry in daEntry.Tables[2].Rows)
                        {
                            //获取仓库信息
                            JObject ck = new JObject()
                            {
                                 new JProperty("FNumber",itemEntry.ItemArray[3].ToString()),
                            };

                            //JObject CW = new JObject()
                            //{
                            //     new JProperty("FNumber",itemEntry.ItemArray[5].ToString()),
                            //};

                            //JObject CWzj = new JObject()
                            //{
                            //     new JProperty("FSTOCKLOCID__FF100004",CW),
                            //};
                            //获取批号信息
                            JObject PH = new JObject()
                            {
                                new JProperty("FNumber",itemEntry.ItemArray[2].ToString()),
                            };
                            //拼接明细JSON
                            JObject FEntity = new JObject()
                            {
                                new JProperty("FEntryID",itemEntry.ItemArray[4].ToString()),
                                new JProperty("FQty",itemEntry.ItemArray[1].ToString()),
                                new JProperty("FDestStockId",ck),
                                new JProperty("FLot",PH),
                                new JProperty("FProduceDate",itemEntry.ItemArray[6].ToString()),
                                new JProperty("FExpiryDate",itemEntry.ItemArray[7].ToString()),
                                new JProperty("FMtoNo",itemEntry.ItemArray[10].ToString())
                            };

                            jarEntry.Add(FEntity);
                        }

                        //拼接表头JSON
                        JObject model = new JObject()
                    {
                        new JProperty("FID",item.ItemArray[4].ToString()),
                        new JProperty("FBillNo",item.ItemArray[4].ToString()),
                        new JProperty("FDate",item.ItemArray[5].ToString()),
                        new JProperty("FBillEntry",jarEntry),
                    };
                        //调用金蝶保存接口，新增单据
                        //Helper.HelperClass.Save("STK_TransferDirect", model, "t_getReceipt", item.ItemArray[0].ToString(), url, Token, FID, "log_直接调拨_入库.txt");
                        Helper.HelperClass.SaveAudit("STK_TransferDirect", model, "t_getReceipt", item.ItemArray[0].ToString(), url, Token, item.ItemArray[4].ToString(), "");
                    }
                }
                catch (Exception)
                {

                }

                //其他出库（退货）
                try
                {
                    //调用金蝶登录接口，获取TOKEN
                    var Token = Helper.HelperClass.Login(url, accid, uid, pwd);

                    //执行存储。获取需要生成的单据数据
                    DataSet da = db.getDataSet("exec MW_rk_generate");
                    foreach (DataRow item in da.Tables[3].Rows)
                    {
                        //定义集合
                        JArray jarEntry = new JArray();
                        //下推JSON
                        
                        //执行存储，获取明细数据
                        DataSet daEntry = db.getDataSet(string.Format("exec MW_rk_generateEntry '{0}'", item.ItemArray[4].ToString()));
                        foreach (DataRow itemEntry in daEntry.Tables[3].Rows)
                        {
                            //获取仓库信息
                            JObject ck = new JObject()
                            {
                                 new JProperty("FNumber",itemEntry.ItemArray[3].ToString()),
                            };

                            //获取批号信息
                            JObject PH = new JObject()
                            {
                                new JProperty("FNumber",itemEntry.ItemArray[7].ToString()),
                            };
                            JObject FStockStatus = new JObject()
                            {
                                new JProperty("FNumber",itemEntry.ItemArray[10].ToString()),
                            };
                            //拼接明细JSON
                            JObject FEntity = new JObject()
                            {
                                new JProperty("FEntryID",itemEntry.ItemArray[4].ToString()),
                                new JProperty("FQty",itemEntry.ItemArray[1].ToString()),
                                //new JProperty("FStockId",ck),
                                new JProperty("FStockStatusId",FStockStatus),
                                new JProperty("FLot",PH),
                                new JProperty("FProduceDate",itemEntry.ItemArray[8].ToString()),
                                new JProperty("FExpiryDate",itemEntry.ItemArray[9].ToString()),
                                new JProperty("FMTONO",itemEntry.ItemArray[11].ToString())
                            };

                            jarEntry.Add(FEntity);
                        }

                        //拼接表头JSON
                        JObject model = new JObject()
                        {
                            new JProperty("FID",item.ItemArray[4].ToString()),
                            new JProperty("FBillNo",item.ItemArray[2].ToString()),
                            new JProperty("FDate",item.ItemArray[3].ToString()),
                            //new JProperty("FDate",item.ItemArray[5].ToString()),
                            new JProperty("FEntity",jarEntry),
                        };
                        //调用金蝶保存接口，新增单据
                        //Helper.HelperClass.Save("STK_MisDelivery", model, "t_getReceipt", item.ItemArray[0].ToString(), url, Token, FID, "log_其他出库_入库.txt");
                        Helper.HelperClass.SaveAudit("STK_MisDelivery", model, "t_getReceipt", item.ItemArray[0].ToString(), url, Token, item.ItemArray[4].ToString(), "");
                    }
                }
                catch (Exception)
                {

                }

                //生产退料
                try
                {
                    //调用金蝶登录接口，获取TOKEN
                    var Token = Helper.HelperClass.Login(url, accid, uid, pwd);

                    //执行存储。获取需要生成的单据数据
                    DataSet da = db.getDataSet("exec MW_rk_generate");
                    foreach (DataRow item in da.Tables[4].Rows)
                    {
                        //定义集合
                        JArray jarEntry = new JArray();
                        //执行存储，获取明细数据
                        DataSet daEntry = db.getDataSet(string.Format("exec MW_rk_generateEntry '{0}'", item.ItemArray[2].ToString()));
                        foreach (DataRow itemEntry in daEntry.Tables[4].Rows)
                        {
                            //获取仓库信息
                            JObject ck = new JObject()
                            {
                                 new JProperty("FNumber",itemEntry.ItemArray[5].ToString()),
                            };

                            //获取批号信息
                            JObject PH = new JObject()
                            {
                                new JProperty("FNumber",itemEntry.ItemArray[7].ToString()),
                            };
                            JObject FStockStatus = new JObject()
                            {
                                new JProperty("FNumber",itemEntry.ItemArray[10].ToString()),
                            };
                            //拼接明细JSON
                            JObject FEntity = new JObject()
                            {
                                new JProperty("FEntryID",itemEntry.ItemArray[4].ToString()),
                                new JProperty("FQty",itemEntry.ItemArray[6].ToString()),
                                new JProperty("FStockId",ck),
                                new JProperty("FStockStatusId",FStockStatus),
                                new JProperty("FLot",PH),
                                new JProperty("FProduceDate",itemEntry.ItemArray[8].ToString()),
                                new JProperty("FExpiryDate",itemEntry.ItemArray[9].ToString()),
                                new JProperty("FMtoNo",itemEntry.ItemArray[11].ToString())
                            };

                            jarEntry.Add(FEntity);
                        }

                        //拼接表头JSON
                        JObject model = new JObject()
                    {
                        new JProperty("FID",item.ItemArray[4].ToString()),
                        new JProperty("FBillNo",item.ItemArray[2].ToString()),
                        new JProperty("FDate",item.ItemArray[3].ToString()),
                        //new JProperty("FDate",item.ItemArray[5].ToString()),
                        new JProperty("FEntity",jarEntry),
                    };
                        //调用金蝶保存接口，修改数据审核单据
                        Helper.HelperClass.SaveAudit("PRD_ReturnMtrl", model, "t_getReceipt", item.ItemArray[0].ToString(), url, Token, item.ItemArray[4].ToString(), "");
                    }
                }
                catch (Exception)
                {

                }

                //生产入库
                try
                {
                    //调用金蝶登录接口，获取TOKEN
                    var Token = Helper.HelperClass.Login(url, accid, uid, pwd);

                    //执行存储。获取需要生成的单据数据
                    DataSet da = db.getDataSet("exec MW_rk_generate");
                    foreach (DataRow item in da.Tables[5].Rows)
                    {
                        //定义集合
                        JArray jarEntry = new JArray();
                        //执行存储，获取明细数据
                        DataSet daEntry = db.getDataSet(string.Format("exec MW_rk_generateEntry '{0}'", item.ItemArray[2].ToString()));
                        foreach (DataRow itemEntry in daEntry.Tables[5].Rows)
                        {
                            //获取仓库信息
                            JObject ck = new JObject()
                            {
                                 new JProperty("FNumber",itemEntry.ItemArray[5].ToString()),
                            };


                            JObject FStockStatus = new JObject()
                            {
                                new JProperty("FNumber",itemEntry.ItemArray[10].ToString()),
                            };
                            //JObject CWzj = new JObject()
                            //{
                            //     new JProperty("FSTOCKLOCID__FF100004",CW),
                            //};
                            //获取批号信息
                            JObject PH = new JObject()
                            {
                                new JProperty("FNumber",itemEntry.ItemArray[7].ToString()),
                            };
                            //拼接明细JSON
                            JObject FEntity = new JObject()
                            {
                                new JProperty("FEntryID",itemEntry.ItemArray[4].ToString()),
                                new JProperty("FRealQty",itemEntry.ItemArray[6].ToString()),
                                new JProperty("FStockId",ck),
                                new JProperty("FStockStatusId",FStockStatus),
                                new JProperty("FLot",PH),
                                new JProperty("FProduceDate",itemEntry.ItemArray[8].ToString()),
                                new JProperty("FExpiryDate",itemEntry.ItemArray[9].ToString()),
                                new JProperty("FMtoNo",itemEntry.ItemArray[11].ToString())
                            };

                            jarEntry.Add(FEntity);
                        }

                        //拼接表头JSON
                        JObject model = new JObject()
                    {
                        new JProperty("FID",item.ItemArray[4].ToString()),
                        new JProperty("FBillNo",item.ItemArray[2].ToString()),
                        new JProperty("FDate",item.ItemArray[3].ToString()),
                        //new JProperty("FDate",item.ItemArray[5].ToString()),
                        new JProperty("FEntity",jarEntry),
                    };
                        //调用金蝶保存接口，修改数据审核单据
                        Helper.HelperClass.SaveAudit("PRD_INSTOCK", model, "t_getReceipt", item.ItemArray[0].ToString(), url, Token, item.ItemArray[4].ToString(), "");
                    }
                }
                catch (Exception)
                {

                }

                //其他入库（寄库入库、编盲入库）
                try
                {
                    var Token = Helper.HelperClass.Login(url, accid, uid, pwd);


                    DataSet da = db.getDataSet("exec MW_rk_generate");
                    foreach (DataRow item in da.Tables[6].Rows)
                    {
                        JArray jarEntry = new JArray();

                        DataSet daEntry = db.getDataSet(string.Format("exec MW_rk_generateEntry '{0}'", item.ItemArray[2].ToString()));
                        foreach (DataRow itemEntry in daEntry.Tables[6].Rows)
                        {
                            JObject ck = new JObject()
                            {
                                 new JProperty("FNumber",itemEntry.ItemArray[5].ToString()),
                            };



                            //JObject CWzj = new JObject()
                            //{
                            //     new JProperty("FSTOCKLOCID__FF100004",CW),
                            //};
                            JObject FStockStatus = new JObject()
                            {
                                new JProperty("FNumber",itemEntry.ItemArray[10].ToString()),
                            };
                            JObject PH = new JObject()
                            {
                                new JProperty("FNumber",itemEntry.ItemArray[7].ToString()),
                            };
                            //拼接单据明细JSON
                            JObject FEntity = new JObject()
                            {
                                new JProperty("FEntryID",itemEntry.ItemArray[4].ToString()),
                                new JProperty("FQty",itemEntry.ItemArray[6].ToString()),
                                new JProperty("FStockId",ck),
                                new JProperty("FStockStatusId",FStockStatus),
                                new JProperty("FLot",PH),
                                new JProperty("FProduceDate",itemEntry.ItemArray[8].ToString()),
                                new JProperty("FExpiryDate",itemEntry.ItemArray[9].ToString()),
                                new JProperty("FMtoNo",itemEntry.ItemArray[11].ToString())
                            };

                            jarEntry.Add(FEntity);
                        }

                        //拼接单据表头JSON
                        JObject model = new JObject()
                        {
                            new JProperty("FID",item.ItemArray[4].ToString()),
                            new JProperty("FBillNo",item.ItemArray[2].ToString()),
                            new JProperty("FDate",item.ItemArray[3].ToString()),
                            //new JProperty("FDate",item.ItemArray[5].ToString()),
                            new JProperty("FEntity",jarEntry),
                        };
                        //调用金蝶保存接口，修改数据审核单据
                        Helper.HelperClass.SaveAudit("STK_MISCELLANEOUS", model, "t_getReceipt", item.ItemArray[0].ToString(), url, Token, item.ItemArray[4].ToString(), "");
                    }
                }
                catch (Exception)
                {

                }

                //分布式调入（国药调拨泰康仓）
                try
                {
                    var Token = Helper.HelperClass.Login(url, accid, uid, pwd);


                    DataSet da = db.getDataSet("exec MW_rk_generate");
                    foreach (DataRow item in da.Tables[7].Rows)
                    {
                        JArray jarEntry = new JArray();

                        DataSet daEntry = db.getDataSet(string.Format("exec MW_rk_generateEntry '{0}'", item.ItemArray[2].ToString()));
                        foreach (DataRow itemEntry in daEntry.Tables[7].Rows)
                        {
                            JObject ck = new JObject()
                            {
                                 new JProperty("FNumber",itemEntry.ItemArray[5].ToString()),
                            };
                            //JObject CWzj = new JObject()
                            //{
                            //     new JProperty("FSTOCKLOCID__FF100004",CW),
                            //};

                            JObject PH = new JObject()
                            {
                                new JProperty("FNumber",itemEntry.ItemArray[7].ToString()),
                            };

                            //拼接单据明细JSON
                            JObject FEntity = new JObject()
                            {
                                new JProperty("FEntryID",itemEntry.ItemArray[4].ToString()),
                                new JProperty("FQty",itemEntry.ItemArray[6].ToString()),
                                new JProperty("FStockId",ck),
                                new JProperty("FLot",PH),
                                new JProperty("FProduceDate",itemEntry.ItemArray[8].ToString()),
                                new JProperty("FExpiryDate",itemEntry.ItemArray[9].ToString()),
                                new JProperty("FMTONO",itemEntry.ItemArray[9].ToString())
                            };

                            jarEntry.Add(FEntity);
                        }

                        //拼接单据表头JSON
                        JObject model = new JObject()
                        {
                            new JProperty("FID",item.ItemArray[4].ToString()),
                            new JProperty("FBillNo",item.ItemArray[2].ToString()),
                            new JProperty("FDate",item.ItemArray[3].ToString()),
                            //new JProperty("FDate",item.ItemArray[5].ToString()),
                            new JProperty("FSTKTRSINENTRY",jarEntry),
                        };
                        //调用金蝶保存接口，修改数据审核单据
                        Helper.HelperClass.SaveAudit("STK_TRANSFERIN", model, "t_getReceipt", item.ItemArray[0].ToString(), url, Token, item.ItemArray[4].ToString(), "");
                    }
                }
                catch (Exception)
                {

                }

            }


            if (modeltype == "ck")
            {
                //销售出库
                try
                {
                    //调用金蝶登录接口，获取TOKEN
                    var Token = Helper.HelperClass.Login(url, accid, uid, pwd);

                    //执行存储。获取需要生成的单据数据
                    DataSet da = db.getDataSet("exec MW_ck_generate");
                    foreach (DataRow item in da.Tables[0].Rows)
                    {
                        //定义集合
                        JArray jarEntry = new JArray();
                        //拼接下推JSON
                        JObject obFormid = new JObject()
                         {
                                    new JProperty("Numbers",item.ItemArray[1].ToString()),
                                    new JProperty("RuleId",item.ItemArray[3].ToString()),
                                    new JProperty("IsEnableDefaultRule","false"),
                                    new JProperty("IsDraftWhenSaveFail","true"),
                         };
                        //获取接口下推后返回的FID
                        var FID = Helper.HelperClass.Push("SAL_DELIVERYNOTICE", obFormid, item.ItemArray[1].ToString(), item.ItemArray[2].ToString(), url, Token);
                        //执行存储，获取明细数据
                        DataSet daEntry = db.getDataSet(string.Format("exec MW_ck_generateEntry '{0}'", item.ItemArray[4].ToString()));
                        foreach (DataRow itemEntry in daEntry.Tables[0].Rows)
                        {
                            //获取仓库信息
                            JObject ck = new JObject()
                            {
                                 new JProperty("FNumber",itemEntry.ItemArray[3].ToString()),
                            };

                            //    JObject CW = new JObject()
                            //{
                            //     new JProperty("FNumber",itemEntry.ItemArray[5].ToString()),
                            //};

                            //    JObject CWzj = new JObject()
                            //{
                            //     new JProperty("FSTOCKLOCID__FF100004",CW),
                            //};
                            //获取批号信息
                            JObject PH = new JObject()
                            {
                                new JProperty("FNumber",itemEntry.ItemArray[2].ToString()),
                            };
                            //拼接明细JSON
                            JObject FInStockEntry = new JObject()
                            {
                                new JProperty("FENTRYID",itemEntry.ItemArray[4].ToString()),
                                new JProperty("FRealQty",itemEntry.ItemArray[1].ToString()),
                                new JProperty("FStockId",ck),
                                new JProperty("FLot",PH),
                                new JProperty("FProduceDate",itemEntry.ItemArray[6].ToString()),
                                new JProperty("FExpiryDate",itemEntry.ItemArray[7].ToString()),
                                new JProperty("F_UJED_WMSCK",itemEntry.ItemArray[8].ToString()),
                                new JProperty("F_UJED_WMSCW",itemEntry.ItemArray[9].ToString()),
                                new JProperty("FMTONO",itemEntry.ItemArray[9].ToString())
                            };

                            jarEntry.Add(FInStockEntry);
                        }

                        //拼接表头JSON
                        JObject model = new JObject()
                        {
                            new JProperty("FID",FID),
                            new JProperty("FBillNo",item.ItemArray[4].ToString()),
                            new JProperty("FDate",item.ItemArray[5].ToString()),
                            new JProperty("FEntity",jarEntry),
                        };
                        //调用金蝶保存接口，新增单据
                        Helper.HelperClass.SaveAudit("SAL_OUTSTOCK", model, "t_getShipment", item.ItemArray[0].ToString(), url, Token, FID,item.ItemArray[6].ToString());
                    }
                }
                catch (Exception)
                {

                }

                //采购退料
                try
                {
                    //调用金蝶登录接口，获取TOKEN
                    var Token = Helper.HelperClass.Login(url, accid, uid, pwd);

                    //执行存储。获取需要生成的单据数据
                    DataSet da = db.getDataSet("exec MW_ck_generate");
                    foreach (DataRow item in da.Tables[1].Rows)
                    {
                        //定义集合
                        JArray jarEntry = new JArray();
                        //拼接下推JSON
                     //   JObject obFormid = new JObject()
                     //{
                     //           new JProperty("Numbers",item.ItemArray[1].ToString()),
                     //           new JProperty("RuleId",item.ItemArray[3].ToString()),
                     //           new JProperty("IsEnableDefaultRule","false"),
                     //           new JProperty("IsDraftWhenSaveFail","true"),
                     //};
                     //   //获取接口下推后返回的FID
                     //   var FID = Helper.HelperClass.Push("PUR_MRAPP", obFormid, item.ItemArray[1].ToString(), item.ItemArray[2].ToString(), url, Token);
                     //   //执行存储，获取明细数据
                        DataSet daEntry = db.getDataSet(string.Format("exec MW_ck_generateEntry '{0}'", item.ItemArray[2].ToString()));
                        foreach (DataRow itemEntry in daEntry.Tables[1].Rows)
                        {
                            //获取仓库信息
                            JObject ck = new JObject()
                            {
                                 new JProperty("FNumber",itemEntry.ItemArray[5].ToString()),
                            };

                            //JObject CW = new JObject()
                            //{
                            //     new JProperty("FNumber",itemEntry.ItemArray[5].ToString()),
                            //};

                            //JObject CWzj = new JObject()
                            //{
                            //     new JProperty("FSTOCKLOCID__FF100004",CW),
                            //};
                            //获取批号信息
                            JObject PH = new JObject()
                            {
                                new JProperty("FNumber",itemEntry.ItemArray[7].ToString()),
                            };
                            //拼接明细JSON
                            JObject FEntity = new JObject()
                            {
                                new JProperty("FEntryID",itemEntry.ItemArray[4].ToString()),
                                new JProperty("FRealQty",itemEntry.ItemArray[6].ToString()),
                                new JProperty("FStockId",ck),
                                new JProperty("FLot",PH),
                                new JProperty("FProduceDate",itemEntry.ItemArray[8].ToString()),
                                new JProperty("FExpiryDate",itemEntry.ItemArray[9].ToString()),
                                new JProperty("F_UJED_WMSCK",itemEntry.ItemArray[10].ToString()),
                                new JProperty("F_UJED_WMSCW",itemEntry.ItemArray[11].ToString()),
                                new JProperty("FMTONO",itemEntry.ItemArray[13].ToString())

                            };

                            jarEntry.Add(FEntity);
                        }

                        //拼接表头JSON
                        JObject model = new JObject()
                    {
                        new JProperty("FID",item.ItemArray[4].ToString()),
                        new JProperty("FBillNo",item.ItemArray[2].ToString()),
                        new JProperty("FDate",item.ItemArray[3].ToString()),
                        new JProperty("FPURMRBENTRY",jarEntry),
                    };
                        //调用金蝶保存接口，新增单据
                       // Helper.HelperClass.Save("PUR_MRB", model, "t_getShipment", item.ItemArray[0].ToString(), url, Token, item.ItemArray[4].ToString(), "log_采购退料_出库.txt");
                        Helper.HelperClass.SaveAudit("PUR_MRB", model, "t_getShipment", item.ItemArray[0].ToString(), url, Token, item.ItemArray[4].ToString(), "");
                    }
                }
                catch (Exception)
                {

                }

                //直接调拨
                try
                {
                    //调用金蝶登录接口，获取TOKEN
                    var Token = Helper.HelperClass.Login(url, accid, uid, pwd);

                    //执行存储。获取需要生成的单据数据
                    DataSet da = db.getDataSet("exec MW_ck_generate");
                    foreach (DataRow item in da.Tables[2].Rows)
                    {
                        //定义集合
                        JArray jarEntry = new JArray();
                        //拼接下推JSON
                        JObject obFormid = new JObject()
                     {
                                new JProperty("Numbers",item.ItemArray[1].ToString()),
                                new JProperty("RuleId",item.ItemArray[3].ToString()),
                                new JProperty("IsEnableDefaultRule","false"),
                                new JProperty("IsDraftWhenSaveFail","true"),
                     };
                        //获取接口下推后返回的FID
                        var FID = Helper.HelperClass.Push("STK_TRANSFERAPPLY", obFormid, item.ItemArray[1].ToString(), item.ItemArray[2].ToString(), url, Token);
                        //执行存储，获取明细数据
                        DataSet daEntry = db.getDataSet(string.Format("exec MW_ck_generateEntry '{0}'", item.ItemArray[4].ToString()));
                        foreach (DataRow itemEntry in daEntry.Tables[2].Rows)
                        {
                            //获取仓库信息
                            JObject ck = new JObject()
                            {
                                 new JProperty("FNumber",itemEntry.ItemArray[3].ToString()),
                            };

                            //JObject CW = new JObject()
                            //{
                            //     new JProperty("FNumber",itemEntry.ItemArray[5].ToString()),
                            //};

                            //JObject CWzj = new JObject()
                            //{
                            //     new JProperty("FSTOCKLOCID__FF100004",CW),
                            //};
                            //获取批号信息
                            JObject PH = new JObject()
                            {
                                new JProperty("FNumber",itemEntry.ItemArray[2].ToString()),
                            };
                            //拼接明细JSON
                            JObject FEntity = new JObject()
                            {
                                new JProperty("FEntryID",itemEntry.ItemArray[4].ToString()),
                                new JProperty("FQty",itemEntry.ItemArray[1].ToString()),
                                //new JProperty("FSrcStockId",ck),
                                new JProperty("FLot",PH),
                                new JProperty("FProduceDate",itemEntry.ItemArray[6].ToString()),
                                new JProperty("FExpiryDate",itemEntry.ItemArray[7].ToString()),
                                new JProperty("F_UJED_WMSCK",itemEntry.ItemArray[8].ToString()),
                                new JProperty("F_UJED_WMSCW",itemEntry.ItemArray[9].ToString()),
                                new JProperty("FMTONO",itemEntry.ItemArray[10].ToString())
                            };

                            jarEntry.Add(FEntity);
                        }

                        //拼接表头JSON
                        JObject model = new JObject()
                    {
                        new JProperty("FID",FID),
                        new JProperty("FBillNo",item.ItemArray[4].ToString()),
                        new JProperty("FDate",item.ItemArray[5].ToString()),
                        new JProperty("FBillEntry",jarEntry),
                    };
                        //调用金蝶保存接口，新增单据
                        Helper.HelperClass.Save("STK_TransferDirect", model, "t_getShipment", item.ItemArray[0].ToString(), url, Token, FID, "log_直接调拨_出库.txt");
                    }
                }
                catch (Exception)
                {

                }

                //其他出库
                try
                {
                    //调用金蝶登录接口，获取TOKEN
                    var Token = Helper.HelperClass.Login(url, accid, uid, pwd);

                    //执行存储。获取需要生成的单据数据
                    DataSet da = db.getDataSet("exec MW_ck_generate");
                    foreach (DataRow item in da.Tables[3].Rows)
                    {
                        //定义集合
                        JArray jarEntry = new JArray();
                        //下推JSON
                        JObject obFormid = new JObject()
                         {
                                    new JProperty("Numbers",item.ItemArray[1].ToString()),
                                    new JProperty("RuleId",item.ItemArray[3].ToString()),
                                    new JProperty("IsEnableDefaultRule","false"),
                                    new JProperty("IsDraftWhenSaveFail","true"),
                         };


                        //获取接口下推后的FID
                        var FID = Helper.HelperClass.Push("STK_OutStockApply", obFormid, item.ItemArray[1].ToString(), item.ItemArray[2].ToString(), url, Token);
                        //执行存储，获取明细数据
                        DataSet daEntry = db.getDataSet(string.Format("exec MW_ck_generateEntry '{0}'", item.ItemArray[4].ToString()));
                        foreach (DataRow itemEntry in daEntry.Tables[3].Rows)
                        {
                            //获取仓库信息
                            JObject ck = new JObject()
                            {
                                 new JProperty("FNumber",itemEntry.ItemArray[3].ToString()),
                            };

                            //获取批号信息
                            JObject PH = new JObject()
                            {
                                new JProperty("FNumber",itemEntry.ItemArray[2].ToString()),
                            };
                            JObject FStockStatus = new JObject()
                            {
                                new JProperty("FNumber",itemEntry.ItemArray[10].ToString()),
                            };
                            //拼接明细JSON
                            JObject FEntity = new JObject()
                            {
                                new JProperty("FEntryID",itemEntry.ItemArray[4].ToString()),
                                new JProperty("FQty",itemEntry.ItemArray[1].ToString()),
                                new JProperty("FStockId",ck),
                                new JProperty("FLot",PH),
                                new JProperty("FProduceDate",itemEntry.ItemArray[6].ToString()),
                                new JProperty("FExpiryDate",itemEntry.ItemArray[7].ToString()),
                                new JProperty("F_UJED_WMSCK",itemEntry.ItemArray[8].ToString()),
                                new JProperty("F_UJED_WMSCW",itemEntry.ItemArray[9].ToString()),
                                new JProperty("FStockStatusId",FStockStatus),
                                new JProperty("FMTONO",itemEntry.ItemArray[11].ToString())
                            };

                            jarEntry.Add(FEntity);
                        }

                        //拼接表头JSON
                        JObject model = new JObject()
                    {
                        new JProperty("FID",FID),
                        new JProperty("FBillNo",item.ItemArray[4].ToString()),
                        new JProperty("FDate",item.ItemArray[5].ToString()),
                        //new JProperty("FDate",item.ItemArray[5].ToString()),
                        new JProperty("FEntity",jarEntry),
                    };
                        //调用金蝶保存接口，新增单据
                        Helper.HelperClass.Save("STK_MisDelivery", model, "t_getShipment", item.ItemArray[0].ToString(), url, Token, FID, "log_其他出库_入库.txt");
                    }
                }
                catch (Exception)
                {

                }

                //生产领料
                try
                {
                    //调用金蝶登录接口，获取TOKEN
                    var Token = Helper.HelperClass.Login(url, accid, uid, pwd);

                    //执行存储。获取需要生成的单据数据
                    DataSet da = db.getDataSet("exec MW_ck_generate");
                    foreach (DataRow item in da.Tables[4].Rows)
                    {
                        //定义集合
                        JArray jarEntry = new JArray();
                        //执行存储，获取明细数据
                        DataSet daEntry = db.getDataSet(string.Format("exec MW_ck_generateEntry '{0}'", item.ItemArray[2].ToString()));
                        foreach (DataRow itemEntry in daEntry.Tables[4].Rows)
                        {
                            //获取仓库信息
                            JObject ck = new JObject()
                            {
                                new JProperty("FNumber",itemEntry.ItemArray[5].ToString()),
                            };

                            //获取批号信息
                            JObject PH = new JObject()
                            {
                                new JProperty("FNumber",itemEntry.ItemArray[7].ToString()),
                            };

                            //拼接明细JSON
                            JObject FEntity = new JObject()
                            {
                                new JProperty("FEntryID",itemEntry.ItemArray[4].ToString()),
                                new JProperty("FQty",itemEntry.ItemArray[6].ToString()),
                                new JProperty("FStockId",ck),
                                new JProperty("FLot",PH),
                                new JProperty("FProduceDate",itemEntry.ItemArray[8].ToString()),
                                new JProperty("FExpiryDate",itemEntry.ItemArray[9].ToString()),
                                new JProperty("F_UJED_WMSCK",itemEntry.ItemArray[10].ToString()),
                                new JProperty("F_UJED_WMSCW",itemEntry.ItemArray[11].ToString()),
                                new JProperty("FMtoNo",itemEntry.ItemArray[13].ToString())
                            };

                            jarEntry.Add(FEntity);
                        }

                        //拼接表头JSON
                        JObject model = new JObject()
                        {
                            new JProperty("FID",item.ItemArray[4].ToString()),
                            new JProperty("FBillNo",item.ItemArray[2].ToString()),
                            new JProperty("FDate",item.ItemArray[3].ToString()),
                            //new JProperty("FDate",item.ItemArray[5].ToString()),
                            new JProperty("FEntity",jarEntry),
                        };
                        //调用金蝶保存接口，修改数据审核单据
                        Helper.HelperClass.SaveAudit("PRD_PickMtrl", model, "t_getShipment", item.ItemArray[0].ToString(), url, Token, item.ItemArray[4].ToString(), "");
                    }
                }
                catch (Exception)
                {

                }

                //生产补料
                try
                {
                    //调用金蝶登录接口，获取TOKEN
                    var Token = Helper.HelperClass.Login(url, accid, uid, pwd);

                    //执行存储。获取需要生成的单据数据
                    DataSet da = db.getDataSet("exec MW_ck_generate");
                    foreach (DataRow item in da.Tables[5].Rows)
                    {
                        //定义集合
                        JArray jarEntry = new JArray();
                        //执行存储，获取明细数据
                        DataSet daEntry = db.getDataSet(string.Format("exec MW_ck_generateEntry '{0}'", item.ItemArray[2].ToString()));
                        foreach (DataRow itemEntry in daEntry.Tables[5].Rows)
                        {
                            //获取仓库信息
                            JObject ck = new JObject()
                            {
                                 new JProperty("FNumber",itemEntry.ItemArray[5].ToString()),
                            };



                            //JObject CWzj = new JObject()
                            //{
                            //     new JProperty("FSTOCKLOCID__FF100004",CW),
                            //};
                            //获取批号信息
                            JObject PH = new JObject()
                            {
                                new JProperty("FNumber",itemEntry.ItemArray[7].ToString()),
                            };
                            //拼接明细JSON
                            JObject FEntity = new JObject()
                            {
                                new JProperty("FEntryID",itemEntry.ItemArray[4].ToString()),
                                new JProperty("FRealQty",itemEntry.ItemArray[6].ToString()),
                                new JProperty("FStockId",ck),
                                new JProperty("FLot",PH),
                                new JProperty("FProduceDate",itemEntry.ItemArray[8].ToString()),
                                new JProperty("FExpiryDate",itemEntry.ItemArray[9].ToString()),
                                new JProperty("F_UJED_WMSCK",itemEntry.ItemArray[10].ToString()),
                                new JProperty("F_UJED_WMSCW",itemEntry.ItemArray[11].ToString()),
                                 new JProperty("FMtoNo",itemEntry.ItemArray[13].ToString())
                            };

                            jarEntry.Add(FEntity);
                        }

                        //拼接表头JSON
                        JObject model = new JObject()
                        {
                            new JProperty("FID",item.ItemArray[4].ToString()),
                            new JProperty("FBillNo",item.ItemArray[2].ToString()),
                            new JProperty("FDate",item.ItemArray[3].ToString()),
                            //new JProperty("FDate",item.ItemArray[5].ToString()),
                            new JProperty("FEntity",jarEntry),
                        };
                        //调用金蝶保存接口，修改数据审核单据
                        Helper.HelperClass.SaveAudit("PRD_FeedMtrl", model, "t_getShipment", item.ItemArray[0].ToString(), url, Token, item.ItemArray[4].ToString(), "");
                    }
                }
                catch (Exception)
                {

                }

                #region
                //其他入库（寄库入库、编盲入库）
                //try
                //{
                //    var Token = Helper.HelperClass.Login(url, accid, uid, pwd);


                //    DataSet da = db.getDataSet("exec MW_rk_generate");
                //    foreach (DataRow item in da.Tables[6].Rows)
                //    {
                //        JArray jarEntry = new JArray();

                //        DataSet daEntry = db.getDataSet(string.Format("exec MW_rk_generateEntry '{0}'", item.ItemArray[2].ToString()));
                //        foreach (DataRow itemEntry in daEntry.Tables[6].Rows)
                //        {
                //            JObject ck = new JObject()
                //            {
                //                 new JProperty("FNumber",itemEntry.ItemArray[5].ToString()),
                //            };



                //            //JObject CWzj = new JObject()
                //            //{
                //            //     new JProperty("FSTOCKLOCID__FF100004",CW),
                //            //};

                //            JObject PH = new JObject()
                //            {
                //                new JProperty("FNumber",itemEntry.ItemArray[7].ToString()),
                //            };
                //            //拼接单据明细JSON
                //            JObject FEntity = new JObject()
                //            {
                //                new JProperty("FEntryID",itemEntry.ItemArray[4].ToString()),
                //                new JProperty("FQty",itemEntry.ItemArray[6].ToString()),
                //                new JProperty("FStockId",ck),
                //                new JProperty("FLot",PH),
                //                new JProperty("FProduceDate",itemEntry.ItemArray[8].ToString()),
                //                new JProperty("FExpiryDate",itemEntry.ItemArray[9].ToString()),
                //                new JProperty("F_UJED_WMSCK",itemEntry.ItemArray[10].ToString()),
                //                new JProperty("F_UJED_WMSCW",itemEntry.ItemArray[11].ToString())
                //            };

                //            jarEntry.Add(FEntity);
                //        }

                //        //拼接单据表头JSON
                //        JObject model = new JObject()
                //    {
                //        new JProperty("FID",item.ItemArray[4].ToString()),
                //        new JProperty("FBillNo",item.ItemArray[2].ToString()),
                //        new JProperty("FDate",item.ItemArray[3].ToString()),
                //        //new JProperty("FDate",item.ItemArray[5].ToString()),
                //        new JProperty("FEntity",jarEntry),
                //    };
                //        //调用金蝶保存接口，修改数据审核单据
                //        Helper.HelperClass.SaveAudit("STK_MISCELLANEOUS", model, "t_getReceipt", item.ItemArray[0].ToString(), url, Token, item.ItemArray[4].ToString());
                //    }
                //}
                //catch (Exception)
                //{

                //}
                #endregion

            }*/


            if (modeltype == "qt")
            {
                //盘盈单
                try
                {
                    var Token = Helper.HelperClass.Login(url, accid, uid, pwd);
                    DataSet dSbt = db.getDataSet(string.Format("EXEC MW_qt_generate"));
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

                        DataSet dSbtFentryid = db.getDataSet(string.Format("EXEC MW_qt_generateEntry '{0}'", item.ItemArray[3].ToString()));
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


                        Helper.HelperClass.SaveS("STK_StockCountGain", Model, "t_getInventoryAdjustment", item.ItemArray[0].ToString(), url, Token, "log_盘盈.txt");
                    }
                }
                catch (Exception)
                {
                }

                //盘亏单
                try
                {
                    var Token = Helper.HelperClass.Login(url, accid, uid, pwd);
                    DataSet dSbt = db.getDataSet(string.Format("EXEC MW_qt_generate"));
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

                        DataSet dSbtFentryid = db.getDataSet(string.Format("EXEC MW_qt_generateEntry '{0}'", item.ItemArray[3].ToString()));
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


                        Helper.HelperClass.SaveS("STK_StockCountLoss", Model, "t_getInventoryAdjustment", item.ItemArray[0].ToString(), url, Token, "log_盘亏单.txt");
                    }
                }
                catch (Exception)
                {
                }

                //库存状态转换单
                try
                {
                    var Token = Helper.HelperClass.Login(url, accid, uid, pwd);
                    DataSet dSbt = db.getDataSet(string.Format("EXEC MW_qt_generate"));
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

                        DataSet dSbtFentryid = db.getDataSet(string.Format("EXEC MW_qt_generateEntry '{0}'", item.ItemArray[3].ToString()));
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

                         //string FName = Model.ToString();

                        Helper.HelperClass.SaveS("STK_StockConvert", Model, "t_getInventoryTransfer", item.ItemArray[0].ToString(), url, Token, "log_库存状态转换单.txt");


                    }
                }
                catch (Exception)
                {
                }

                //批号调整单
                try
                {
                    var Token = Helper.HelperClass.Login(url, accid, uid, pwd);
                    DataSet dSbt = db.getDataSet(string.Format("EXEC MW_qt_generate"));
                    Console.WriteLine("获取数据:");
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

                        DataSet dSbtFentryid = db.getDataSet(string.Format("EXEC MW_qt_generateEntry '{0}'", item.ItemArray[3].ToString()));
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

                        Helper.HelperClass.SaveS("STK_LOTADJUST", Model, "t_getInventoryTransfer", item.ItemArray[0].ToString(), url, Token, "log_批号调整单.txt");

                        
                    }
                }
                catch (Exception e)
                {
                    
                    Console.WriteLine(e.Message);
                    throw new Exception(e.Message);
                }


                //形态转换单
                try
                {
                    var Token = Helper.HelperClass.Login(url, accid, uid, pwd);
                    DataSet dSbt = db.getDataSet(string.Format("EXEC MW_qt_generate"));
                    Console.WriteLine("获取数据:");
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

                        DataSet dSbtFentryid = db.getDataSet(string.Format("EXEC MW_qt_generateEntry '{0}'", item.ItemArray[3].ToString()));
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

                        Helper.HelperClass.SaveS("STK_StatusConvert", Model, "t_getInventoryTransfer", item.ItemArray[0].ToString(), url, Token, "log_形态转换单.txt");


                    }
                }
                catch (Exception e)
                {

                    Console.WriteLine(e.Message);
                    throw new Exception(e.Message);
                }


            }
        }
    }
}
