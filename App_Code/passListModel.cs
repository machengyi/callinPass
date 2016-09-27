using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

/// <summary>
/// passListModel 的摘要说明
/// </summary>
public class passListModel
{
    public class GetTitle
    {
        public string name { get; set; }//使用者名称
    }

    public class GetContent
    {
        public string dateTime1 { get; set; }//电销指派给主管的日期
        public string customerNo { get; set; }//客户代号
        public string customerName { get; set; }//客户名称
        public string passMan { get; set; }//建单电销名称
    }
}