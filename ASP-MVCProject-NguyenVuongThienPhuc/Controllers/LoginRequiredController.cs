using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Configuration;
using System.Web.Mvc;

namespace ASP_MVCProject_NguyenVuongThienPhuc.Controllers
{
    public class LoginRequiredController : Controller
    {
        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if(Session[Encryptor.SESSION_LOGIN_KEY] == null)
            {
                filterContext.Result = Redirect("/Login");
            }
            base.OnActionExecuting(filterContext);
        }
    }
}