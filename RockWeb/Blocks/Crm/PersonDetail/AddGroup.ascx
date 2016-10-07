<%@ Control Language="C#" AutoEventWireup="true" CodeFile="AddGroup.ascx.cs" Inherits="RockWeb.Blocks.Crm.PersonDetail.AddGroup" %>

<asp:UpdatePanel ID="upAddGroup" runat="server">
    <ContentTemplate>

        

        <div class="panel panel-block">
            <div class="panel-heading">
                <h1 class="panel-title"><i class="fa fa-plus-square-o"></i>
                    <asp:Literal ID="lTitle" runat="server"></asp:Literal></h1>
            </div>
            <div class="panel-body">

                <asp:ValidationSummary ID="valSummaryTop" runat="server" HeaderText="Please Correct the Following" CssClass="alert alert-danger" />

                <asp:Panel ID="pnlGroupData" runat="server">

                    <div class="row" id="divGroupName" runat="server">
                        <div class="col-md-4">
                            <Rock:RockTextBox ID="tbGroupName" runat="server" Label="Group Name" Required="true" />
                        </div>
                        <div class="col-md-8">
                        </div>
                    </div>

                    <div class="row">
                        <div class="col-md-12">
                            <h4><%=_groupTypeName %> Members</h4>
                        </div>
                    </div>

                    <div class="row">
                        <div class="col-md-12">
                            <Rock:NewGroupMembers ID="nfmMembers" runat="server" OnAddGroupMemberClick="nfmMembers_AddGroupMemberClick" />
                        </div>
                    </div>

                    <div class="row">
                        <div class="col-md-4">
                            <Rock:CampusPicker ID="cpCampus" runat="server" Required="true" />
                            <Rock:RockDropDownList ID="ddlMaritalStatus" runat="server" Label="Marital Status of Adults"
                                Help="The marital status to use for the adults in this family." />
                        </div>

                        <div class="col-md-8">
                            <Rock:AddressControl ID="acAddress" Label="Address" runat="server" UseStateAbbreviation="false" UseCountryAbbreviation="false" />
                        </div>
                    </div>

                </asp:Panel>

                <asp:Panel ID="pnlAddressInUseWarning" runat="server" Visible="false">
                    <Rock:HiddenFieldWithClass ID="hfSelectedFamilyGroupId" runat="server" CssClass="js-selectedfamilychoice" />
                    <div class="alert alert-warning">
                        <h4>Address Already In Use</h4>
                        <p>This address already has a family assigned to it. Select the family if you would prefer to add the individuals as new family members. You may also continue adding the new family if you believe this is the correct information.</p>
                        <div class="row">
                            <div class="col-md-4">
                                <Rock:RockRadioButton ID="rbNewFamily" runat="server" CssClass="js-familychoice" GroupName="groupFamilyChoice" Checked="true" DisplayInline="false" />
                                <strong>New Family</strong>
                                <br />
                            </div>
                            <asp:Repeater ID="rptFamiliesAtAddress" runat="server" OnItemDataBound="rptFamiliesAtAddress_ItemDataBound">
                                <ItemTemplate>
                                    <div class="col-md-4">
                                        <Rock:RockRadioButton ID="rbFamilyToUse" runat="server" CssClass="js-familychoice" GroupName="groupFamilyChoice" DisplayInline="false" />
                                        <strong><%# Eval("FamilyTitle") %></strong>
                                        <br />
                                        <%# Eval( "GroupLocation.GroupLocationTypeValue" )%>: <%# Eval( "GroupLocation.Location" )%>
                                        <asp:Literal ID="lFamilyMembersHtml" runat="server" />
                                    </div>
                                    <asp:Literal ID="lNewRowHtml" runat="server" />
                                </ItemTemplate>
                            </asp:Repeater>
                        </div>
                        <div class="row">
                        </div>
                    </div>
                </asp:Panel>

                <asp:Panel ID="pnlContactInfo" runat="server" Visible="false">
                    <Rock:NewGroupContactInfo ID="nfciContactInfo" runat="server" />
                </asp:Panel>

                <asp:Panel ID="pnlAttributes" runat="server" Visible="false">
                </asp:Panel>

                <asp:Panel ID="pnlDuplicateWarning" runat="server" Visible="false">
                    <Rock:NotificationBox ID="nbDuplicateWarning" runat="server" NotificationBoxType="Warning" Title="Possible Duplicates!"
                        Text="<p>One ore more of the people you are adding may already exist! Please confirm that none of the existing people below are the same person as someone that you are adding." />
                    <asp:PlaceHolder ID="phDuplicates" runat="server" />
                </asp:Panel>

                <div class="actions">
                    <asp:LinkButton ID="btnPrevious" runat="server" Text="Previous" CssClass="btn btn-link" OnClick="btnPrevious_Click" Visible="false" CausesValidation="false" />
                    <asp:LinkButton ID="btnNext" runat="server" Text="Next" CssClass="btn btn-primary" OnClick="btnNext_Click" />
                </div>
            </div>
        </div>

        <script type="text/javascript">
            $(document).ready(function () {
                Sys.Application.add_load(function () {
                    <%-- workaround for RadioButtons in Repeaters https://support.microsoft.com/en-us/kb/316495 --%>
                    //$('.js-groupfamilychoice').attr('Name', 'groupFamilyChoice');
                    $('.js-familychoice').click(function (a, b, c) {
                        $('.js-familychoice').not($(this)).prop('checked', false);

                        $('.js-selectedfamilychoice').val($(this).attr('data-familygroupid'));
                    });
                });
            });
        </script>

    </ContentTemplate>
</asp:UpdatePanel>
