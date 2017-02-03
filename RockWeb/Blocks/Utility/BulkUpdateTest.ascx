<%@ Control Language="C#" AutoEventWireup="true" CodeFile="BulkUpdateTest.ascx.cs" Inherits="RockWeb.Blocks.Utility.BulkUpdateTest" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>

        <asp:Panel ID="pnlView" runat="server" CssClass="panel panel-block">
        
            <div class="panel-heading">
                <h1 class="panel-title"><i class="fa fa-star"></i> Bulk Update Test</h1>
            </div>
            
            <div class="panel-body">
                <asp:LinkButton ID="btnGo" runat="server" CssClass="btn btn-primary" Text="Go" OnClick="btnGo_Click" />
            </div>
        
        </asp:Panel>

    </ContentTemplate>
</asp:UpdatePanel>