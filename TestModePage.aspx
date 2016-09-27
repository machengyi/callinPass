<%@ Page Language="C#" AutoEventWireup="true" CodeFile="TestModePage.aspx.cs" Inherits="TestModePage" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <meta http-equiv="Content-Type" content="text/html; charset=utf-8" />
    <meta name="viewport" content="initial-scale=1, maximum-scale=1, user-scalable=no, width=device-width">
    <title>測試頁面</title>

    <link href="lib/ionic/css/ionic.min.css" rel="stylesheet" />
</head>
<body>
    <form id="form1" runat="server">
        <label class="item item-input">
            <span class="input-label">輸入工號</span>
            <asp:TextBox ID="txtUserID" runat="server"></asp:TextBox>
        </label>
        <p />
        <div class="row">
            
                <asp:Button CssClass="button button-positive col" ID="btnSend" runat="server" Text="送出" OnClick="btnSend_Click" />
            
        
        </div>
    </form>
</body>
</html>
