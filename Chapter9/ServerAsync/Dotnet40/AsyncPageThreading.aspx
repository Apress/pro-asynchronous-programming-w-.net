<%@ Page Async="true" Language="C#" AutoEventWireup="true" CodeBehind="AsyncPageThreading.aspx.cs" Inherits="Dotnet40.AsyncPageThreading" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
</head>
<body>
    <form id="form1" runat="server">
    <div>
        <asp:Label runat="server" ID="outputT1"></asp:Label>
    </div>
    <div>
        <asp:Label runat="server" ID="outputT2"></asp:Label>
    </div>
    </form>
    <div>
        <asp:Label runat="server" ID="output"></asp:Label>
    </div>
</body>
</html>
