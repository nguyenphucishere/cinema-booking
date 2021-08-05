using BotDetect.Web.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ASP_MVCProject_NguyenVuongThienPhuc.Controllers
{
    public class RegisterController : Controller
    {
        private cinemaManagerEntities db = new cinemaManagerEntities();

        public ActionResult Index()
        {
            return View();
        }

        [HttpPost, ActionName("Index")]
        [CaptchaValidationActionFilter("CaptchaCode", "Captcha", "Wrong Captcha!")]
        [ValidateAntiForgeryToken]
        public ActionResult CreateNewUser([Bind(Include = "userID,fullName,phone,email,dateOfBirth,memberCard,username,password")]user user, HttpPostedFileBase avatar, string agreeTerm, string reTypePassword)
        {
            List<user> checkUser = db.users.Where(info => info.username == user.username || info.email == user.email || info.phone == user.phone).ToList();

            if (!ModelState.IsValid || agreeTerm == null) 
            {
                ModelState.AddModelError("", "Please agree our term and services before you register account");
                MvcCaptcha.ResetCaptcha("Captcha");
                return View("Index");
            }
            if(user.password != reTypePassword)
            {
                ModelState.AddModelError("", "The retype password feild are not the same with password");
                MvcCaptcha.ResetCaptcha("Captcha");
                return View("Index");
            }
            if(checkUser.Count > 0)
            {
                ModelState.AddModelError("", "Your infomation is duplicate, please try again with another username, email or phone number");
                MvcCaptcha.ResetCaptcha("Captcha");
                return View("Index");
            }

            user registeredUser = user;
            registeredUser.password = Encryptor.MD5Hash(user.password);
            db.users.Add(registeredUser);

            if (avatar != null && avatar.ContentLength > 0)
            {
                if (!avatar.ContentType.Contains("image/"))
                {
                    ModelState.AddModelError("", "Your avatar must be a image !" + avatar.ContentType);
                    MvcCaptcha.ResetCaptcha("Captcha");
                    return View("Index");
                }

                string type = avatar.ContentType.Replace("image/", "");
                db.users.Add(registeredUser);
                db.SaveChanges();

                registeredUser.avatar = "/Content/images/users/" + registeredUser.userID + "." + type;
                string urlImage = Server.MapPath("~" + registeredUser.avatar);
                avatar.SaveAs(urlImage);

            }

            db.SaveChanges();

            return Redirect("/Login");
        }
    }
}