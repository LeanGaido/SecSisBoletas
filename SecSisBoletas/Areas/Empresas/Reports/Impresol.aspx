<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Impresol.aspx.cs" Inherits="SecSisBoletas.Areas.Empresas.Reports.Impresol" %>

<%@ Register assembly="Microsoft.ReportViewer.WebForms" namespace="Microsoft.Reporting.WebForms" tagprefix="rsweb" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml" >
<head runat="server">
    <title></title>
    <style type="text/css">
        #form1
        {
            height: 757px;
            width: 1184px;
        }
        .style1
        {
            width: 100%;
        }
        .style2
        {
            height: 114px;
        }
    </style>
</head>
<body>
    <form id="form1" runat="server">
    <div style="height: 759px; width: 1187px">
    
        <table class="style1" cellpadding="5" cellspacing="5">
            <tr>
                <td bgcolor="Black" class="style2">
                    <asp:Label ID="lbltitulo" runat="server" Font-Bold="True" Font-Names="Arial" 
                        Font-Size="Large" ForeColor="White" Text="Impresión de Boleta de Aportes"></asp:Label>
                    <br />
                    <br />
                    <asp:Label ID="Label2" runat="server" Font-Bold="True" Font-Italic="True" 
                        Font-Names="Arial" Font-Size="Medium" Font-Underline="True" ForeColor="White" 
                        Text="Instructivo de Impresión"></asp:Label>
                    <br />
                    <br />
                    <asp:Label ID="lbltexto" runat="server" Font-Names="Raavi" Font-Size="Small" 
                        ForeColor="White" 
                        
                        
                        
                        Text="De acuerdo al navegador o browser de internet que este utilizando, puede ocurrir que la vista previa de la boleta de aportes se vea en  forma distorsionada. Esto no afecta en nada a la exportación e impresión de la boleta de aportes con el codigo de barras correspondiente. Para poder generar la impresión de la boleta de aportes, deberá elegir en la caja de selección &quot;Select a Format&quot; la opción &quot;Acrobat (PDF) File&quot; y luego presionar el enlace &quot;Exportar&quot;. El sistema generará la boleta de aportes con código de barra en formato PDF y por último deberá presionar el icono &quot;Imprimir&quot; para que el sistema imprima la boleta correspondiente."></asp:Label>
                </td>
            </tr>
        </table>
    
    </div>
    
    </form>
</body>
</html>
