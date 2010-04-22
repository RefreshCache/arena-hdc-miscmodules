<%@ control language="c#" inherits="ArenaWeb.UserControls.Security.UserLogin, Arena" %>
<div class="textWrap">
<input type="hidden" id="iRedirect" runat="server" NAME="iRedirect">
<asp:label id="lblMessage" cssClass="errorText error" runat="server" Visible="false" />
<BR /><BR />
<h1 align=center> HDC MEMBER PORTAL </h1>
<hr size="8" align="center" width="60%" color="black">


<BR /><BR />
<asp:Panel id="pnlLogin" runat="server" Visible="true" CssClass="pnlLogin">
	<div class="loginWrap">
	    <table class="loginTable webForm" align="center">
		    <tr>
			    <td>
				    <%=LoginIDCaptionSetting%>:
			    </td>
			    <td>
				    <asp:TextBox id="txtLoginId" cssclass="formItem" runat="server" />
			    </td>
		    </tr>
		    <tr>
			    <td>
				    Password:
			    </td>
			    <td>
				    <asp:TextBox id="txtPassword" textmode="password" cssclass="formItem" runat="server" /><br/>
				    <asp:checkbox id="cbRemember" CssClass="smallText" runat="server" Text="Remember Password" />
			    </td>
		    </tr>
		    <tr>
			    <td>&nbsp;</td>
			    <td>
				    <asp:Button id="btnSignin" runat="server" text="Login" CssClass="smallText" onclick="btnSignin_Click"></asp:Button>
			    </td>
		    </tr>
	    </table>
        <asp:Panel ID="pnlImportantNote" runat="Server" CssClass="important notice" Visible="false"/>
	</div>
    
</asp:Panel>

<asp:Panel ID="pnlChangePassword" CssClass="changePass" runat="server" Visible="False" DefaultButton="btnChangePassword" align="center">
	<h3>Your password has expired.  Please change it before continuing.</h3>
	<table class="changePassTable">
		<tr>
			<td>New Password:</td>
			<td><asp:TextBox ID="txtNewPassword" TextMode="Password" CssClass="formItem" runat="server" />
				<asp:RequiredFieldValidator ControlToValidate="txtNewPassword" ID="rfvNewPassword" Runat= "server" ErrorMessage="Password is required!" CssClass="errorText error" Display="None" SetFocusOnError="true"></asp:RequiredFieldValidator>
				<asp:RegularExpressionValidator ControlToValidate="txtNewPassword" ID="revNewPassword" Runat="server" ErrorMessage="Invalid Password" CssClass="errorText error" ValidationExpression="\w+" EnableClientScript="false"></asp:RegularExpressionValidator>
			</td>
		</tr>
		<tr>
			<td>Confirm Password:</td>
			<td><asp:TextBox ID="txtNewPassword2" TextMode="Password" CssClass="formItem" runat="server" />
				<asp:RequiredFieldValidator ControlToValidate="txtNewPassword2" ID="rfvNewPassword2" Runat= "server" ErrorMessage="Password confirmation is required!" CssClass="errorText error" Display="None" SetFocusOnError="true"></asp:RequiredFieldValidator>
				<asp:CompareValidator ID="cvNewPassword2" Runat="server" ControlToValidate="txtNewPassword2" ControlToCompare="txtNewPassword" ErrorMessage="Password confirmation must match password!" CssClass="errorText error" Display="None" Operator="Equal"></asp:CompareValidator>
			</td>
		</tr>
		<tr>
			<td>&nbsp;</td>
			<td><asp:Button ID="btnChangePassword" runat="server" Text="Change Password" CssClass="smallText" OnClick="btnChangePassword_Click" /></td>
		</tr>
	</table>
</asp:Panel>
<BR /><BR />
<hr size="8" align="center" width="60%" color="black">
<BR /><BR />
<asp:Panel ID="pnlCreateAccount" CssClass="module createAccount" Runat="server" Visible="False" align=center>
	<h3>Don't have an HDC Member Portal account yet?</h3>
	<p>An HDC Member Portal account gives you access to sign up for HDC events, manage your giving, access small group resources and more. The HDC Member Portal is expanding regularly to offer more to our members. Create your account by clicking below.</p>
	<asp:Button ID="btnCreateAccount" Runat="server" Text="Create Account" CssClass="smallText"></asp:Button>
</asp:Panel>
<BR /><BR />
<hr size="8" align="center" width="60%" color="black">
<BR /><BR />
<asp:Panel ID="pnlForgot" CssClass="module forgotPass" Runat="server" Visible="False" align="center">
	<h3>Forgot your username or password?</h3>
	<p>Click below to reset your password or have your username emailed to you. You will need email access to retrieve you username or new password.</p>
	<asp:Button ID="btnSend" Runat="server" Text="Reset Password" CssClass="smallText"></asp:Button>
	
	<asp:Button ID="btnLoginSend" Runat="server" Text="Forgot Login ID" CssClass="smallText"></asp:Button>
	
</asp:Panel>
</div>