<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="ImpresionBoletaEspecial.aspx.cs" Inherits="SecSisBoletas.Areas.Empresas.Reports.ImpresionBoletaEspecial" %>

<%@ Register assembly="Microsoft.ReportViewer.WebForms" namespace="Microsoft.Reporting.WebForms" tagprefix="rsweb" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
<meta http-equiv="Content-Type" content="text/html; charset=utf-8"/>
    <title></title>
</head>
<body>
    <form id="form1" runat="server">
        <div>
            <asp:ScriptManager ID="ScriptManager1" runat="server">
            </asp:ScriptManager>
            <asp:ImageButton ID="imgImprimir" runat="server" Height="62px" ImageAlign="Top" ImageUrl="~/Areas/Empresas/Content/images/logo-sindico-dark-400.png" onclick="imgImprimir_Click" ToolTip="Imprimir PDF" Width="139px" />
            <br />
        <rsweb:ReportViewer ID="ReportViewer1" runat="server" BackColor="" ClientIDMode="AutoID" HighlightBackgroundColor="" InternalBorderColor="204, 204, 204" InternalBorderStyle="Solid" InternalBorderWidth="1px" LinkActiveColor="" LinkActiveHoverColor="" LinkDisabledColor="" PrimaryButtonBackgroundColor="" PrimaryButtonForegroundColor="" PrimaryButtonHoverBackgroundColor="" PrimaryButtonHoverForegroundColor="" SecondaryButtonBackgroundColor="" SecondaryButtonForegroundColor="" SecondaryButtonHoverBackgroundColor="" SecondaryButtonHoverForegroundColor="" SplitterBackColor="" ToolbarDividerColor="" ToolbarForegroundColor="" ToolbarForegroundDisabledColor="" ToolbarHoverBackgroundColor="" ToolbarHoverForegroundColor="" ToolBarItemBorderColor="" ToolBarItemBorderStyle="Solid" ToolBarItemBorderWidth="1px" ToolBarItemHoverBackColor="" ToolBarItemPressedBorderColor="51, 102, 153" ToolBarItemPressedBorderStyle="Solid" ToolBarItemPressedBorderWidth="1px" ToolBarItemPressedHoverBackColor="153, 187, 226" Width="763px">
            <LocalReport ReportPath="Areas\Empresas\Reports\ImpresionBoletaEspecial.rdlc" DisplayName="ReportBoleta">
                <DataSources>
                    <rsweb:ReportDataSource DataSourceId="odsBoleta" Name="dsBoleta" />
                </DataSources>
            </LocalReport>
        </rsweb:ReportViewer>

        </div>

         <asp:ObjectDataSource ID="odsBoleta" runat="server" SelectMethod="GetBoletaEspecial" TypeName="SecSisBoletas.Areas.Empresas.Controllers.ImpresionBoletaController">
            <SelectParameters>
                <asp:SessionParameter Name="id" SessionField="IdBoletaEspecial" Type="Int32" />
            </SelectParameters>
        </asp:ObjectDataSource>
    </form>
</body>
</html>
