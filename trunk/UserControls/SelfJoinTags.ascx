<%@ Control Language="c#" Inherits="ArenaWeb.UserControls.Custom.HDC.Misc.SelfJoinTags" CodeFile="SelfJoinTags.ascx.cs" CodeBehind="SelfJoinTags.ascx.cs" %>

<input type="hidden" id="iRedirect" runat="server" name="iRedirect" />

<script type="text/javascript">
    function sjt_updateStates() {
        var activeCount = 0;

        $('span.sjt_profile > input').each(function () {
            if ($(this).attr('checked'))
                activeCount += 1;
        });

        if (activeCount >= <%= MaxAnswersSetting %>) {
            $('span.sjt_profile > input[type=checkbox]:not(:checked)').filter(':not(span.sjt_full > input)').attr('disabled', 'disabled').addClass('sjt_disabled');
        }
        else {
            $('span.sjt_profile > input[type=checkbox]:not(:checked)').filter(':not(span.sjt_full > input)').removeAttr('disabled').removeClass('sjt_disabled');
        }
    }

    $(document).ready(function() {
        $('span.sjt_profile > input').click(sjt_updateStates);
        sjt_updateStates();
    });
</script>

<style type="text/css">
    input.sjt_disabled + label { color: Gray; }
    span.sjt_full > label { color: Gray; }
</style>

<asp:Panel ID="pnlSelectTags" Runat="server" Visible="True">
<div class="textWrap">
	<table border="0">
	    <tr>
	        <td style="width=20px;">&nbsp;</td>
	        <td><asp:PlaceHolder ID="phProfiles" runat="server"></asp:PlaceHolder></td>
	    </tr>
	</table>
	<p>
    <asp:Button ID="btnSubmit" Runat="server" CssClass="smallText" Text="Submit"></asp:Button>
    </p>
</div>
</asp:Panel>
