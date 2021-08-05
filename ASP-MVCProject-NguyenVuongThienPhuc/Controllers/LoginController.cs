using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ASP_MVCProject_NguyenVuongThienPhuc.Controllers
{
    public class LoginController : Controller
    {
        private readonly cinemaManagerEntities db = new cinemaManagerEntities();

        public ActionResult Index()
        {
            if (Session[Encryptor.SESSION_LOGIN_KEY] != null)
            {
                return Redirect("/");
            }
            return View();
        }

        [HttpPost, ActionName("Index")]
        [ValidateAntiForgeryToken]
        public ActionResult Login([Bind(Include = "username,password")]user user)
        {
            string userPassword = Encryptor.MD5Hash(user.password);
            user findUser = db.users.Where(
                                providedUser => providedUser.username == user.username &&
                                providedUser.password == userPassword
                            ).FirstOrDefault();

            if (findUser == null)
            {
                ModelState.AddModelError("", "Login fail, username or password is invalid !");

                return View("Index");
            }

            Session.Add(Encryptor.SESSION_LOGIN_KEY, findUser.userID);

            return Redirect("/");
        }
    }
}