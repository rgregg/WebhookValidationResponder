using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using OneDriveWebhookTranslator.Auth;
using System.Threading.Tasks;
using OneDriveWebhookTranslator.Models;

namespace OneDriveWebhookTranslator.Controllers
{
    public class AccountController : Controller
    {
        private string RedirectUri
        {
            get
            {
                 return Url.Action("Redirect", "Account", null, this.Request.Url.Scheme);
            }
        }

        public ActionResult SignInBusiness()
        {
            QueryStringBuilder builder = new QueryStringBuilder();
            builder.Add("client_id", ConfigurationManager.AppSettings["ida:AADAppId"]);
            builder.Add("response_type", "code");
            builder.Add("redirect_uri", this.RedirectUri);
            builder.Add("state", "business");

            string targetUrl = ConfigurationManager.AppSettings["ida:AADAuthService"] + builder.ToString();
            return Redirect(targetUrl);
        }

        public ActionResult SignInPersonal()
        {

            QueryStringBuilder builder = new QueryStringBuilder();
            builder.Add("client_id", ConfigurationManager.AppSettings["ida:MSAAppId"]);
            builder.Add("response_type", "code");
            builder.Add("redirect_uri", this.RedirectUri);
            builder.Add("state", "personal");
            builder.Add("scope", ConfigurationManager.AppSettings["ida:MSAScopes"]);

            string targetUrl = ConfigurationManager.AppSettings["ida:MSAAuthService"] + builder.ToString();
            return Redirect(targetUrl);
        }

        public ActionResult SignOut()
        {
            OneDriveUser.ClearResponseCookie(this.Response);
            return Redirect(Url.Action("Index", "Home"));
        }

        public async Task<ActionResult> Redirect(string code, string state)
        {
            OAuthHelper helper;
            try {
                helper = OAuthHelper.HelperForService(state, this.RedirectUri);
            }
            catch (ArgumentException ex)
            {
                ViewBag.Message = ex.Message;
                return View("Error");
            }

            string discoveryResource = "https://api.office.com/discovery/";
            if (state == "business")
                discoveryResource = "https://prepsp-my.spoppe.com";


            var token = await helper.RedeemAuthorizationCodeAsync(code, discoveryResource);
            if (null == token)
            {
                ViewBag.Message = "Invalid response from token service. Unable to login. Try again later.";
                return View("Error");
            }

            OneDriveUser user = new OneDriveUser(token, helper, discoveryResource);
            user.SetResponseCookie(this.Response);

            return Redirect(Url.Action("Index", "Subscription"));
        }


    }
}