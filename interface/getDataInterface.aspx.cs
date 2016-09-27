using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

public partial class interface_getDataInterface : System.Web.UI.Page
{
    string DSC_CRM_SP_conn = ConfigurationManager.ConnectionStrings["DSC_CRM_SP_ConnectionString"].ConnectionString;
    string kfaj_conn = ConfigurationManager.ConnectionStrings["kfaj_ConnectionString"].ConnectionString;
    string systemCode = "ACP_CN";
    string appId = "100";
    bool isTestMode = true; //是否啟用測試模式
    protected void Page_Load(object sender, EventArgs e)
    {
        //測試模式
        if (isTestMode)
        {
            //測試模式須先將app首頁設為輸入工號頁面，再從這邊取得值進行設定
            if (Request.QueryString["UserID"] != null)
            {
                GetUserInfo();
                Response.Redirect("../home.html");
            }
        }
        else
        {
            //微信驗證
            ConfirmWeXin();
        }
        if (Request.Form.Count > 0)
        {
            string res_json = "";
            string strRequest = Request.Form[0];
            JObject req_json = (JObject)JsonConvert.DeserializeObject(strRequest);

            string funcName = req_json["func"].ToString();

            switch (funcName)
            {
                case"TransitionList":
                    res_json = getPassListData(req_json["tabName"].ToString());;
                    break;
                default:
                    break;
            }
            HttpContext.Current.Response.Clear();
            HttpContext.Current.Response.ContentType = "text/plain; charset=utf-8";
            Response.Write(res_json);
            HttpContext.Current.Response.End();
        }
    }
    //取得使用者相關資訊
    void GetUserInfo()
    {
       
        AppWS_SystemBase.AppSystemBase objApp_Auth_WS = new AppWS_SystemBase.AppSystemBase();

        HttpCookie cUserID = new HttpCookie("UserID");
        //設定單值
        cUserID.Value = Server.UrlEncode(Request.QueryString["UserID"].ToString().Trim());
        //設定過期日
        cUserID.Expires = DateTime.Now.AddMinutes(30);
        //寫到用戶端
        Response.Cookies.Add(cUserID);

    }

    #region 微信驗證
    protected void ConfirmWeXin()
    {
        //有code驗證碼，代表從企業號串接，第一次進入頁面
        if (!string.IsNullOrEmpty(Request.QueryString["code"]))
        {
            //取得ID
            string code = Request.QueryString["code"].ToString().Trim();
            string[] userAD = Wexin.getuseridfromCode(code).Split(',');
            if (userAD[0] == "1")
            {
                AppWS_SystemBase.AppSystemBase objApp_Auth_WS = new AppWS_SystemBase.AppSystemBase();


                HttpCookie cUserID = new HttpCookie("UserID");
                //設定單值

                cUserID.Value = Server.UrlEncode(userAD[1].ToString());
                //cUserID.Value = Server.UrlEncode("00097");
                //設定過期日
                cUserID.Expires = DateTime.Now.AddMinutes(30);
                //寫到用戶端
                Response.Cookies.Add(cUserID);



                //導向首頁
                Response.Redirect("home.html");
            }
            else
            {

                HttpContext.Current.Response.Clear();
                HttpContext.Current.Response.ContentType = "text/plain";
                HttpContext.Current.Response.Write("錯誤的請求!!伺服器訊息:" + userAD[1]);
                HttpContext.Current.Response.End();
            }
        }
        //查詢請求缺少必要參數
        else if (Request.Cookies["UserID"] == null)
        {

            HttpContext.Current.Response.Clear();
            HttpContext.Current.Response.ContentType = "application/x-www-form-encoded;charset=UTF-8";
            HttpContext.Current.Response.Write("錯誤的請求!!");
            HttpContext.Current.Response.End();
        }
    }
    #endregion

    /// <summary>
    /// pass清单该调用哪个方法
    /// </summary>
    /// <param name="tabName"></param>
    /// <returns></returns>
    public string getPassListData(string tabName) {
        string  result = "";
        switch (tabName)
        {
            case"title":
                result = getPassListTitle();
                break;
            case"content":
                result = getPassListContent();
                break;
            default:
                break;
        }
        return result;
    }

    /// <summary>
    /// 获取pass清单的标题（登录者名称）
    /// </summary>
    /// <returns></returns>
    public string getPassListTitle()
    {
        DataTable dt = new DataTable();
        string strJson = "";
        string sql = "select MV002 AS name from CMSMV where MV001='"+Request.Cookies["UserID"].Value.ToString()+"'";
        dt = projectDal.GetDataTable(sql, DSC_CRM_SP_conn);
        passListModel.GetTitle getTitle = new passListModel.GetTitle();
        getTitle.name = dt.Rows[0][0].ToString();
        strJson = JsonConvert.SerializeObject(getTitle).Replace("null", "").Replace("NULL", "");
        return strJson;
    }

    /// <summary>
    /// 获取pass清单的内容
    /// </summary>
    /// <returns></returns>
    public string getPassListContent() {
        DataTable dt = new DataTable();
        string sqlWhere = "";
        string strJson="";
        string sql = @"SELECT  CRMTF.TF007 AS dateTime1,GG001 AS customerNo,GG004 AS customerName, MV002 as passMan  FROM CRMTF 
                            JOIN CRMTJ ON TF017=TJ001
                            JOIN CRMGG ON GG001=TF002
                            JOIN CMSMV on MV001 = TF004
                            where TJ009='03' AND TJ023='05' AND CRMTF.TF007>'20160101'";
        if (AuthorityControl() == "101")
        {
            strJson = "101";//呼叫服务失败
        }
        else 
        {
            if (AuthorityControl() != "") 
            {
                sqlWhere += " AND TF006='" + AuthorityControl() + "'";
            }
            sqlWhere += "  order by CRMTF.TF007 asc";
            sql += sqlWhere;
            dt = projectDal.GetDataTable(sql, DSC_CRM_SP_conn);
            if (dt.Rows.Count > 0)
            {
                passListModel.GetContent[] content = new passListModel.GetContent[dt.Rows.Count];
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    content[i] = new passListModel.GetContent();
                    content[i].customerNo = dt.Rows[i]["customerNo"].ToString();
                    content[i].customerName = dt.Rows[i]["customerName"].ToString();
                    content[i].dateTime1 = dt.Rows[i]["dateTime1"].ToString();
                    content[i].passMan = dt.Rows[i]["passMan"].ToString();
                }
                strJson = JsonConvert.SerializeObject(content).Replace("null", "").Replace("NULL", "");
            }
            else 
            {
                strJson = "102";//没有数据
            }
        
        }
       return strJson;
    }

    /// <summary>
    /// 判断、获取使用者权限
    /// </summary>
    /// <returns></returns>
    protected string AuthorityControl() {
        string result = "";
        string system_Code = "ACP_CN";
        string progCode = "WCRMI31";
        string strCode = Request.Cookies["UserID"].Value.ToString();

        AppWS_SystemBase.AppSystemBase objApp_Auth_WS = new AppWS_SystemBase.AppSystemBase();
        string userInfo = objApp_Auth_WS.GetUserInfo(system_Code, strCode);
        dynamic json = JValue.Parse(userInfo);

        if (json.SuperUser == "Y")//判断是否为超级管理员
        {
            result = "";
        }
        else 
        {
            string isExcuteTable = objApp_Auth_WS.GetProgAuthorize(system_Code, strCode, progCode);//对此作业没有权限;
            if (isExcuteTable == "N")
            {
                result = "101";
            }
            else 
            {
                result = strCode;
            }
        }
        return result;
    }
}