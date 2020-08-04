using Facebook;
using Microsoft.AspNet.Identity.Owin;
using Newtonsoft.Json.Linq;
using oath_tests.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Tweetinvi;
using Tweetinvi.Models;
//using TweetSharp;
using oath_tests.Biz.Helpers;

namespace oath_tests.Controllers
{
    public class HomeController : Controller
    {
        public string TwitterConsumerKey { get { return System.Configuration.ConfigurationManager.AppSettings["twitterConsumerKey"]; } }
        public string TwitterConsumerSecret { get { return System.Configuration.ConfigurationManager.AppSettings["twitterConsumerSecret"]; } }
        public string FacebookAppId{ get { return System.Configuration.ConfigurationManager.AppSettings["facebookAppId"]; } }
        public string FacebookAppSecret { get { return System.Configuration.ConfigurationManager.AppSettings["facebookAppSecret"]; } }
        public string AuthorizeResponseUrl { get { return "http://simonyohannes.com/umbraco/surface/customerregister/googlecallback"; } }
        public string GoogleApiKey { get { return ConfigurationManager.AppSettings["GoogleClientId"]; } }
        public string GoogleClientSecret { get { return ConfigurationManager.AppSettings["GoogleClientSecret"]; } }


        private ApplicationSignInManager _signInManager;
        private ApplicationUserManager _userManager;

        public ApplicationSignInManager SignInManager
        {
            get
            {
                return _signInManager ?? HttpContext.GetOwinContext().Get<ApplicationSignInManager>();
            }
            private set
            {
                _signInManager = value;
            }
        }

        public ApplicationUserManager UserManager
        {
            get
            {
                return _userManager ?? HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
            private set
            {
                _userManager = value;
            }
        }

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
        private IAuthenticationContext _authenticationContext;
        public ActionResult TwitterLogin() {

            var appCreds = new ConsumerCredentials(TwitterConsumerKey, TwitterConsumerSecret);

            // Specify the url you want the user to be redirected to
            var redirectURL = "http://127.0.0.1:64663/home/twittercallback";
            _authenticationContext = AuthFlow.InitAuthentication(appCreds, redirectURL);
            return new RedirectResult(_authenticationContext.AuthorizationURL);
        }

        public async Task<ActionResult> TwitterCallback(string oauth_token, string oauth_verifier,string authorization_id)
        {
            var context = HttpContext.GetOwinContext().Get<ApplicationDbContext>();
            // Create the user credentials
            var userCreds = AuthFlow.CreateCredentialsFromVerifierCode(oauth_verifier,authorization_id);

            // Do whatever you want with the user now!
            var twitterUser = Tweetinvi.User.GetAuthenticatedUser(userCreds);
            ViewBag.User = twitterUser;
            //search for registered user by their twitter id
            var registeredUser = context.Users.SingleOrDefault(x => x.TwitterId == twitterUser.Id);
            if (registeredUser != null)
            {
                await SignInManager.SignInAsync(registeredUser, false, false);
            }else{
                var user = new ApplicationUser { Email = twitterUser.Email, UserName = twitterUser.Email };
                user.TwitterId = twitterUser.Id;
                var createResult = await UserManager.CreateAsync(user);
                await SignInManager.SignInAsync(user, false, false);
            }                       

            return View();
        }

        public ActionResult FacebookLogin() {

            var fb = new FacebookClient();
            var loginUrl = fb.GetLoginUrl(new
            {
                client_id = FacebookAppId,
                client_secret = FacebookAppSecret,
                redirect_uri = "http://localhost:64663/home/facebookcallback",
                response_type = "code",
                scope = "email"
                //,auth_type = "reauthenticate"
            });

            return Redirect(loginUrl.AbsoluteUri);
        }

        public async Task<ActionResult> FacebookCallback(string code) {
            var context = HttpContext.GetOwinContext().Get<ApplicationDbContext>();
            var fb = new FacebookClient();
            dynamic result = fb.Post("oauth/access_token", new
            {
                client_id = FacebookAppId,
                client_secret = FacebookAppSecret,
                redirect_uri = "http://localhost:64663/home/facebookcallback",
                code = code                
            });

            var accessToken = result.access_token;

            // Store the access token in the session for farther use
            Session["AccessToken"] = accessToken;

            // update the facebook client with the access token so
            // we can make requests on behalf of the user
            fb.AccessToken = accessToken;

            // Get the user's information, like email, first name, middle name etc
            dynamic me = fb.Get("me?fields=first_name,middle_name,last_name,id,email");
            string email = me.email;
            string firstname = me.first_name;
            string middlename = me.middle_name;
            string lastname = me.last_name;
            long id =long.Parse(me.id);
            //search for registered user by their facebook id
            var registeredUser = context.Users.SingleOrDefault(x => x.FacebookId == id);
            if (registeredUser != null)
            {
                await SignInManager.SignInAsync(registeredUser, false, false);
            }
            else
            {
                var user = new ApplicationUser { Email = email, UserName = email };
                user.FacebookId = id;
                var createResult = await UserManager.CreateAsync(user);
                await SignInManager.SignInAsync(user, false, false);
            }


            return View();
        }

        public ActionResult GoogleLogin()
        {
            var GoogleEndpointsHelper = new GoogleEndpointsHelper();
            var url = GoogleEndpointsHelper.AuthorizationEndpoint + "?" +
                      "client_id=" + GoogleApiKey + "&" +
                      "response_type=code&" +
                      "scope=openid%20email%20profile&" +
                      "redirect_uri=" + AuthorizeResponseUrl + "&" +
                      //"state=" + Session["SessionId"] + "&" +
                      //"login_hint=" + Session["Email"] + "&" +
                      "access_type=offline";

            return Redirect(url);
        }
        public async Task<ActionResult> GoogleCallback()
        {
            var GoogleEndpointsHelper = new GoogleEndpointsHelper();
            var state = Request.QueryString["state"];

            var code = Request.QueryString["code"];

            var values = new Dictionary<string, string>
            {
                {"code", code},
                {"redirect_uri", AuthorizeResponseUrl},
                {"client_id", GoogleApiKey},
                {"client_secret", GoogleClientSecret},
                {"grant_type", "authorization_code"},
                {"scope", ""}
            };

            var webmethods = new WebMethodsHelper();
            var tokenResponse = await webmethods.Post(GoogleEndpointsHelper.TokenEndpoint, values);

            var jobject = JObject.Parse(tokenResponse);
            var access_token = jobject.SelectToken("access_token");
            var refresh_token = jobject.SelectToken("refresh_token");

            if (access_token == null || access_token.ToString().Trim().Length == 0)
            {
                TempData["message"] = "Sorry, something went wrong, try a different method for logging in.";
                //Response.Redirect(SitePathsHelper.CustomerLoginPath);
            }
            string id = "";
            string email = "";
            string firstName = "";
            string surname = "";
            using (var client = new WebClient())
            {
                var response = client.DownloadString($"https://www.googleapis.com/oauth2/v1/userinfo?alt=json&access_token={access_token}");
                var profileResponseJObject = JObject.Parse(response);
                id = profileResponseJObject.SelectToken("id").ToString();
                email = profileResponseJObject.SelectToken("email").ToString();
                firstName = profileResponseJObject.SelectToken("given_name").ToString();
                surname = profileResponseJObject.SelectToken("family_name").ToString();

            }
            if (string.IsNullOrEmpty(id) || string.IsNullOrEmpty(email))
            {
                //something went wrong, take user back to login page and display error
                TempData["message"] = "Sorry, something went wrong. Please try a different method for logging in.";
                //Response.Redirect(SitePathsHelper.CustomerLoginPath);
            }

            //search for registered user by their facebook id
            


            return View();
        }

    }
}