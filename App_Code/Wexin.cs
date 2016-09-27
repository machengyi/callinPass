using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.Services;
using System.Net;
using System.IO;
using System.Web.Script.Serialization;
using System.Web.Script.Services;
using System.Web.Security;
using System.Data;
using System.Text;
using EFWeiXin;
using EFWeiXin.EasyFlowAPI;
using EFWeiXin.SDK;
using EFWeiXin.TencentAPI;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
/// <summary>
/// Class1 的摘要描述
/// </summary>
public class Wexin
{
    public Wexin()
    {
    }
    
    //最後取得tooken的時間
    
    private static DateTime LastTookenRequestTime;
    //最後取得的tooken
    private static AccessTokenResult AccessToken;
    
    private static int TimeoutSecond = 5400;
    private const string AGENT_ID = "2";
    //鼎截軟件(測試區)----------------------
    //private static  string CORP_ID = "wx5e4f505fa1a93684";
    //private static  string SECRET_ID = "ieECnbcGEIC8mfS2fa4QeHJLlFBBw-T_Bhxm-Jljc4VAqdtE_iKQI8LOPNBMw9Hq";
    //--------------------------------------
    //Digiwin企業號(正式區)-----------------
    private static string CORP_ID = "wxd96b6330455926b5";
    private static string SECRET_ID = "9P-TNGvtNxfyDjLoyIkuWRiSM8MJjxsQb4kZawy2gXlf51V6m6KdSTfV6Mrm8prK";
    //--------------------------------------
    private const string QY_API_URL = "https://qyapi.weixin.qq.com/cgi-bin/gettoken";
    private const string USER_ID_URL = "https://qyapi.weixin.qq.com/cgi-bin/user/getuserinfo";
    private const string USER_INFO_URL = "https://qyapi.weixin.qq.com/cgi-bin/user/get";
    private const string LOGIN_AD_URL = "http://yunkong.digiwin.com.cn:8020/agencytest/ldap4wechat.aspx";
    private const string MEDIA_ID_URL = "https://qyapi.weixin.qq.com/cgi-bin/media/get";

    public AccessTokenResult GetBusinessSpecialAssistantAccessToken()
    {
        if (isTimeout())
        {
            LastTookenRequestTime = DateTime.Now;
            AccessToken = getAccessToken();
        }

        return AccessToken;
    }

    //取得WeChatAccessToken
    public static AccessTokenResult getAccessToken()
    {
        JavaScriptSerializer serializer = new JavaScriptSerializer();
        string result;
        string url = QY_API_URL + "?corpid=" + CORP_ID + "&corpsecret=" + SECRET_ID;
        result = httpRequest(url, WebRequestMethods.Http.Get, "application/json");
        return serializer.Deserialize<AccessTokenResult>(result);
    }

    //取得WeChatTicket
    public static AccessTokenResult getTicket(string strToken)
    {
        JavaScriptSerializer serializer = new JavaScriptSerializer();
        string result;
                        
       string url = "https://qyapi.weixin.qq.com/cgi-bin/get_jsapi_ticket?access_token=" + strToken;

       // string url = "http://api.weixin.qq.com/cgi-bin/ticket/getticket?type=jsapi&access_token=" + strToken;
        result = httpRequest(url, WebRequestMethods.Http.Get, "application/json");
        return serializer.Deserialize<AccessTokenResult>(result);
    }


    //取得WeChatSignature
    public static string getSignature(string strTicket, string strurl, string timestamp, string noncestr)
    {
        // 这里参数的顺序要按照 key 值 ASCII 码升序排序  
        string rawstring = "jsapi_ticket=" + strTicket + "&noncestr=" + noncestr + "&timestamp=" + timestamp + "&url=" + strurl + "";
        string signature = FormsAuthentication.HashPasswordForStoringInConfigFile(rawstring, "SHA1").ToLower();
        return signature;
    }
    public static string saveFileFromWeiXinToServer(AccessTokenResult token, string media_id, string filePath,string fileName)
    {
        /*
         {
            HTTP/1.1 200 OK
            Connection: close
            Content-Type: image/jpeg 
            Content-disposition: attachment; filename="MEDIA_ID.jpg"
            Date: Sun, 06 Jan 2013 10:20:18 GMT
            Cache-Control: no-cache, must-revalidate
            Content-Length: 339721
   
            Xxxx
         }
         */
       
        string url = MEDIA_ID_URL + "?access_token=" + token.access_token + "&media_id=" + media_id;
        HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(url);
        request.Method = WebRequestMethods.Http.Get;
        request.Accept = "application/json";
        try
        {
            using (HttpWebResponse response = request.GetResponse() as HttpWebResponse)
            {//取得附檔名
               
                string fileExt = response.Headers["Content-Disposition"] != null ?
                        response.Headers["Content-Disposition"].Replace("attachment; filename=", "").Replace("\"", "").Split('.')[1] : "X";
                if (fileExt != "X")
                {
                    string saveToPath = filePath + fileName + "." + fileExt;
                    Stream streamObj = response.GetResponseStream();
                    byte[] fileByte = null;
                    fileByte = ReadFully(streamObj);
                    File.WriteAllBytes(saveToPath, fileByte);
                    return saveToPath;
                }

            }
        }catch(Exception ex){
            return "";
        }
        return "";
    }
    public static byte[] ReadFully(Stream input)
    {
        using (MemoryStream ms = new MemoryStream())
        {
            input.CopyTo(ms);
            return ms.ToArray();
        }
    }
    public static MemberResult getUserIDByCodeID(AccessTokenResult token, string codeId)
    {
        JavaScriptSerializer serializer = new JavaScriptSerializer();
        string result;
        string url = USER_ID_URL + "?access_token=" + token.access_token + "&code=" + codeId + "&agentid=" + AGENT_ID;
        result = httpRequest(url, WebRequestMethods.Http.Get, "application/json");
        return serializer.Deserialize<MemberResult>(result);

    }
    //預設7200秒後AccessToken會過期，所以縮小一下範圍，大於5400秒就重新取得
    private static bool isTimeout()
    {
        TimeSpan span = DateTime.Now.Subtract(LastTookenRequestTime);
        return span.TotalSeconds > TimeoutSecond;
    }

    private static string httpRequest(string url, string method, string accept)
    {
        string result;
        HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(url);
        request.Method = method;
        request.Accept = accept;
        using (HttpWebResponse response = request.GetResponse() as HttpWebResponse)
        {
            using (StreamReader sr = new StreamReader(response.GetResponseStream()))
            {
                result = sr.ReadToEnd();
            }
        }
        return result;
    }

    //noncestr 生成签名的随机串
    public static string createNonceStr()
    {
        int length = 16;
        string chars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        string str = "";
        Random rad = new Random();
        for (int i = 0; i < length; i++)
        {
            str += chars.Substring(rad.Next(0, chars.Length - 1), 1);
        }
        return str;
    }

    //timestamp生成签名的时间戳
    public static int ConvertDateTimeInt(System.DateTime time)
    {
        int intResult = 0;
        System.DateTime startTime = TimeZone.CurrentTimeZone.ToLocalTime(new System.DateTime(1970, 1, 1));
        intResult = Convert.ToInt32((time - startTime).TotalSeconds);
        return intResult;
    }


    public  class AccessTokenResult
    {
        public string access_token { get; set; } // 获取到的凭证
        public string Ticket { get; set; } // 获取到的凭证
        public string signature { get; set; } // 获取到的凭证
        public string expires_in { get; set; } // 凭证的有效时间（秒）
        public string errcode { get; set; } // 錯誤碼
        public string errmsg { get; set; } // 錯誤訊息
    }
    //通訊錄帳號
    public  class MemberResult
    {
        public string UserId { get; set; }   // 成员UserID
        public string DeviceId { get; set; }   // 手机设备号(由微信在安装时随机生成，删除重装会改变，升级不受影响)
        public string errcode { get; set; }   // 錯誤碼
        public string errmsg { get; set; }   // 錯誤訊息
    }
    //通訊錄成員詳細資料
    public  class MemberInfoResult
    {
        public int errcode { get; set; }   // 返回码
        public string errmsg { get; set; }   // 对返回码的文本描述内容
        public string userid { get; set; }   // 成员UserID。对应管理端的帐号
        public string name { get; set; }   // 成员名称
        public string[] department { get; set; }   // 成员所属部门id列表
        public string position { get; set; }   // 职位信息
        public string mobile { get; set; }   // 手机号码
        public string gender { get; set; }   // 性别。0表示未定义，1表示男性，2表示女性
        public string email { get; set; }   // 邮箱
        public string weixinid { get; set; }   // 微信号
        public string avatar { get; set; }   // 头像url。注：如果要获取小图将url最后的"/0"改成"/64"即可
        public string status { get; set; }   // 关注状态: 1=已关注，2=已冻结，4=未关注
        //public string extattr { get; set; } //	扩展属性
    }
   
    //從網址的Code取得目前操作的使用者userid(AD帳號)
    public static string getuseridfromCode(string Code){
        /*
        a)企业成员授权时返回示例如下： 回傳1 , userid
        {
            "UserId":"USERID",
            "DeviceId":"DEVICEID"
        }
        b)非企业成员授权时返回示例如下： 回傳2,OpenId
        {
            "OpenId":"OPENID",
            "DeviceId":"DEVICEID"
        }
        c)出错时返回示例如下：回傳0,errcode+errmsg
        {
            "errcode": "40029",
            "errmsg": "invalid code"
        }
        */
        string urlFormat = "https://qyapi.weixin.qq.com/cgi-bin/user/getuserinfo?access_token={0}&code={1}";
        string url = string.Format(urlFormat, Wexin.getAccessToken().access_token, Code);

        string result = "";
        
         try
         {
             result = MyHttpUtility.GetData(url);
             dynamic json = JValue.Parse(result);
             if (json.UserId != null)
             {
                 result = "1," + json.UserId;
             }
             else
             {
                 if (json.OpenId != null)
                 {
                     urlFormat = "https://qyapi.weixin.qq.com/cgi-bin/user/convert_to_userid?access_token={1}";
                     url = string.Format(urlFormat, Wexin.getAccessToken().access_token);
                     string strJson = "{\"openid\":"+ json.OpenId +"}";


                     try
                     {
                         result = MyHttpUtility.SendHttpRequest(url, strJson);
                         if (result != null)
                         {
                             json = JValue.Parse(result);
                             string errmsg = json.errmsg;
                             if (errmsg != "ok")
                             {
                                 result = "2," + json.errcode + json.errmsg;
                             }
                             else
                             {
                                 result = "1," + json.userid;
                             }
                         }
                     }
                     catch (Exception ex)
                     {
                         result = "2," + ex.ToString();

                     }
                 }
                 else
                 {
                     result = "0," + json.errcode + json.errmsg;
                 }
             }
         }
         catch(Exception ex)
         {
             result = "0," + ex.ToString();

         }
         return result;
    }

    //寫入log機制
    public static string insertWeixinLog(string appId, string userId, string objId, string type, string strSQL)
    {
        SqlDataSource sqlds = new SqlDataSource();
        sqlds.ConnectionString = "Data Source=10.20.99.18;Persist Security Info=True;Password=CrmSa!@;User ID=crm_sa;Initial Catalog=ITAS";
        try
        {
            sqlds.InsertCommand = " insert into weixinAppLog (appId, userId, createdate, objId, type, sqlCmd) " +
                                  " values ('" + appId + "', N'" + userId + "', getdate(), N'" + objId + "', N'" + type + "', N'" + strSQL + "') ";
            sqlds.Insert();
            return "{\"errcode\":0,\"errmsg\":\"ok\"}";
        }
        catch (Exception ex)
        {
            return "{\"errcode\":1,\"errmsg\":\"insert log error\"}";
        }
    }

    #region JSONTool JSON 格式语句
    //20150408 edit by 06475 吳俐君 WEIXINSYNORG批次檔 確認書:MDA_0001↓
    public  class JSONTool
    {

        public JSONTool() { }
        /// <summary> 
        /// 对象转JSON 
        /// </summary> 
        /// <param name="obj">对象</param> 
        /// <returns>JSON格式的字符串</returns> 
        public static string ObjectToJSON(object obj)
        {
            JavaScriptSerializer jss = new JavaScriptSerializer();
            try
            {
                return jss.Serialize(obj);
            }
            catch (Exception ex)
            {
                throw new Exception("JSONHelper.ObjectToJSON(): " + ex.Message);
            }
        }

        /// <summary> 
        /// 数据表转键值对集合 
        /// 把DataTable转成 List集合, 存每一行 
        /// 集合中放的是键值对字典,存每一列 
        /// </summary> 
        /// <param name="dt">数据表</param> 
        /// <returns>哈希表数组</returns> 
        public static List<Dictionary<string, object>> DataTableToList(DataTable dt)
        {
            List<Dictionary<string, object>> list = new List<Dictionary<string, object>>();
            foreach (DataRow dr in dt.Rows)
            {
                Dictionary<string, object> dic = new Dictionary<string, object>();
                foreach (DataColumn dc in dt.Columns)
                {
                    dic.Add(dc.ColumnName, dr[dc.ColumnName]);
                }
                list.Add(dic);
            }
            return list;
        }
        /// <summary> 
        /// 数据集转键值对数组字典 
        /// </summary> 
        /// <param name="dataSet">数据集</param> 
        /// <returns>键值对数组字典</returns> 
        public static Dictionary<string, List<Dictionary<string, object>>> DataSetToDic(DataSet ds)
        {
            Dictionary<string, List<Dictionary<string, object>>> result = new Dictionary<string, List<Dictionary<string, object>>>();
            foreach (DataTable dt in ds.Tables)
                result.Add(dt.TableName, DataTableToList(dt));
            return result;
        }
        /// <summary> 
        /// 数据表转JSON 
        /// </summary> 
        /// <param name="dataTable">数据表</param> 
        /// <returns>JSON字符串</returns> 
        public static string DataTableToJSON(DataTable dt)
        {
            return ObjectToJSON(DataTableToList(dt));
        }
        /// <summary> 
        /// JSON文本转对象,泛型方法 
        /// </summary> 
        /// <typeparam name="T">类型</typeparam> 
        /// <param name="jsonText">JSON文本</param> 
        /// <returns>指定类型的对象</returns> 
        public static T JSONToObject<T>(string jsonText)
        {
            JavaScriptSerializer jss = new JavaScriptSerializer();
            try
            {
                return jss.Deserialize<T>(jsonText);
            }
            catch (Exception ex)
            {
                throw new Exception("JSONHelper.JSONToObject(): " + ex.Message);
            }
        }
        /// <summary> 
        /// 将JSON文本转换为数据表数据 
        /// </summary> 
        /// <param name="jsonText">JSON文本</param> 
        /// <returns>数据表字典</returns> 
        public static Dictionary<string, List<Dictionary<string, object>>> TablesDataFromJSON(string jsonText)
        {
            return JSONToObject<Dictionary<string, List<Dictionary<string, object>>>>(jsonText);
        }

        /// <summary> 
        /// 将JSON文本转换成数据行 
        /// </summary> 
        /// <param name="jsonText">JSON文本</param> 
        /// <returns>数据行的字典</returns> 
        public static Dictionary<string, object> DataRowFromJSON(string jsonText)
        {
            return JSONToObject<Dictionary<string, object>>(jsonText);
        }
    }
    //20150408 edit by 06475 吳俐君 WEIXINSYNORG批次檔 確認書:MDA_0001↑
    #endregion
	}
