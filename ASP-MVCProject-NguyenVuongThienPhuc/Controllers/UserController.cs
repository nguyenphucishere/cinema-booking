using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Security.Policy;
using System.Text;
using System.Threading;
using System.Web;
using System.Web.Hosting;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.UI.HtmlControls;
using ASP_MVCProject_NguyenVuongThienPhuc;
using Microsoft.Ajax.Utilities;

namespace ASP_MVCProject_NguyenVuongThienPhuc.Controllers
{
    public class UserController : LoginRequiredController
    {
        private readonly cinemaManagerEntities db = new cinemaManagerEntities();

        // GET: User
        public ActionResult Index()
        {
            return View(db.users.ToList());
        }

        // GET: User/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            user user = db.users.Find(id);
            if (user == null)
            {
                return HttpNotFound();
            }
            return View(user);
        }

        // GET: User/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: User/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "userID,fullName,phone,email,dateOfBirth,memberCard,username,password,avatar")] user user)
        {
            if (ModelState.IsValid)
            {
                db.users.Add(user);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(user);
        }

        // GET: User/Edit/5
        public ActionResult Edit(int? id, string area)
        {
            if (id == null && area == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            if (area == "user")
            {
                id = (int)Session[Encryptor.SESSION_LOGIN_KEY];
            }

            user user = db.users.Find(id);
            if (user == null)
            {
                return HttpNotFound();
            }
            return View(user);
        }

        // POST: User/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "userID,fullName,phone,email,dateOfBirth,memberCard,username,password,avatar")] user user)
        {
            if (ModelState.IsValid)
            {
                db.Entry(user).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(user);
        }

        // GET: User/Delete/5
        public ActionResult Delete(string area, int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            user user = db.users.Find(id);
            if (user == null)
            {
                return HttpNotFound();
            }
            return View(user);
        }

        // POST: User/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            user user = db.users.Find(id);
            db.users.Remove(user);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        public static user GetUserByID(int id)
        {
            return new UserController().db.users.Find(id);
        }

        public ActionResult LogOut()
        {
            Session.Remove(Encryptor.SESSION_LOGIN_KEY);
            Session.Remove(Encryptor.SHOPPING_BAG_KEY);
            return Redirect("/");
        }

        public ActionResult Profile()
        {
            user user = db.users.Find(Session[Encryptor.SESSION_LOGIN_KEY]);

            if (user == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.NotFound);
            }
            return View(model: user);
        }

        public string GetSchedules(int? id)
        {
            if(id == null)
            {
                return "Unable to loading data";
            }

            StringBuilder htmlCode = new StringBuilder("<h4>Choose the time in the schedule</h4>");

            List<schedule> schedules = db.schedules.Where(scheduleMovie => scheduleMovie.movieID == id && DateTime.Now < scheduleMovie.scheduleTime).ToList();
            foreach(schedule schedule in schedules)
            {
                htmlCode.Append($"<a class='btn btn-default border radius-pill ml-2' href='/User/BuyTicket/{schedule.scheduleID}'>{schedule.scheduleTime}</a>");
            }

            return htmlCode.ToString();
        }

        public ActionResult BuyTicket(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            schedule schedule = db.schedules.Find(id);
            if(schedule == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ViewBag.roomID = schedule.roomID;
            List<seat> seats = db.seats.Where(seatInfo => seatInfo.roomName == schedule.roomID).OrderBy(seatInfo => seatInfo.seatName).ToList(); ;

            return View(model: seats);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult BuyTicket(int? id, string[] seats)
        {
            if (seats == null || id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            foreach(string seatID in seats)
            {
                seat seat = db.seats.Find(int.Parse(seatID));
                if(seat == null)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }
                if(seat.isAvailable == false)
                {
                    ModelState.AddModelError("", "Sorry, the seat that you choose is booked. Sorry :(");
                    return View("BuyTicket");
                }
            }
            object[] shoppingData = new object[] { seats, id };

            //many orders in session 

            //if (Session[Encryptor.SHOPPING_BAG_KEY] == null)
            //{
            //    bagContainer = new List<object[]>();
            //    bagContainer.Add(shoppingData);

            //    Session.Add(Encryptor.SHOPPING_BAG_KEY, bagContainer);
            //}else
            //{
            //    bagContainer = (List<object[]>)Session[Encryptor.SHOPPING_BAG_KEY];
            //    bagContainer.Add(shoppingData);

            //    for (int i = 0; i < bagContainer.Count; i++)
            //    {
            //        if((int)bagContainer[i][1] == id)
            //        {
            //            bagContainer.RemoveAt(bagContainer.Count - 1);
            //            seats.Append(bagContainer[i][0]);
            //            bagContainer[i] = new object[] { seats, id };
            //            break;
            //        }
            //    }
            //    Session[Encryptor.SHOPPING_BAG_KEY] = bagContainer;
            //}

            if (Session[Encryptor.SHOPPING_BAG_KEY] == null)
            {
                Session.Add(Encryptor.SHOPPING_BAG_KEY, shoppingData);
            }
            else 
            {
                Session[Encryptor.SHOPPING_BAG_KEY] = shoppingData;
            }

                return Redirect("/User/ShoppingBag");
        }

        public ActionResult ShoppingBag()
        {
            return View();
        }

        [HttpPost()]
        [ValidateAntiForgeryToken]
        public string LoadBagItem()
        {
            object[] shoppingBagData = (object[])Session[Encryptor.SHOPPING_BAG_KEY];
            if (shoppingBagData == null)
            {
                return Encryptor.Base64Encode("Nothing here :(");
            }
            schedule schedule = db.schedules.Find(shoppingBagData[1]);
            string[] seats = new string[((string[])shoppingBagData[0]).Length];

            for (int i = 0; i < seats.Length; i++)
            {
                seat findSeat = new cinemaManagerEntities().seats.Find(
                    int.Parse(
                        ((string[])shoppingBagData[0])[i]
                    )
                );
                seats[i] = findSeat.seatName;
            }

            string responsive = @"
    <script>
        $('#remove').click(function () {
            $.post('/User/DeleteBagItem', {
                __RequestVerificationToken: $('input[name=\'__RequestVerificationToken\']').val()
            }, function (result){
                alert(result);
                loadData();
            });
        });
    </script>
                    <div class='row'>
                <div class='col-md-9 col-12'>
                    <table class='table table-hover w-100'>
                        <thead>
                            <tr>
                                <th>Product</th>
                                <th class='text-center'>Price</th>
                                <th class='text-center'>Total</th>
                                <th></th>
                            </tr>
                        </thead> 
                        <tbody id='bag-container'>
                            <tr><td class='col-sm-8 col-md-6 p-0'><div class='media'><img class='media-object' src='" + schedule.movy.imageLink + @"' style='width: 100px; height: auto;'><div class='media-body ml-md-2'><h4 class='media-heading'>" + schedule.movy.movieName + @"</h4><h5 class='media-heading'>Room " + schedule.roomID + @"</h5><span>Seat: </span><span class='text-success'><strong>" + String.Join(", ", seats) + @"</strong></span></div>
                                    </div>
                                </td>
                                <td class='col-sm-1 col-md-1 text-center'><strong>" + schedule.movy.price + @"$</strong></td>
                                <td class='col-sm-1 col-md-1 text-center'><strong>" + (schedule.movy.price * seats.Length) + @"$</strong></td>
                                <td class='col-sm-1 col-md-1'>
                                    <button type='button' class='btn btn-danger' id='remove'>
                                        <span class='glyphicon glyphicon-remove'></span> Remove
                                    </button>
                                </td>
                            </tr>
                        </tbody>
                    </table>
                </div>
                <div class='col-md-3 col-12 card p-3'>
                    <h3>Total</h3>
                    <h3><strong>" + (schedule.movy.price * seats.Length) + @"$</strong></h3>
                    <a class='btn btn-info' href='/'>
                        Continue Shopping
                    </a>
                    <a href='/User/Checkout' class='btn btn-success'>
                        Checkout<span class='glyphicon glyphicon-play'></span>
                    </a>
                </div>
            </div>";

            return Encryptor.Base64Encode(responsive);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public string DeleteBagItem()
        {
            try
            {
                Session.Remove(Encryptor.SHOPPING_BAG_KEY);
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
            return "Delete successful";
        }

        public ActionResult Checkout()
        {
            if (Session[Encryptor.SHOPPING_BAG_KEY] == null)
            {
                ModelState.AddModelError("", "You don't have any thing on shopping bag!");
                return Redirect("User/ShoppingBag");
            }

            object[] shoppingBagData = (object[])Session[Encryptor.SHOPPING_BAG_KEY];
            if (shoppingBagData == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            int _scheduleID = (int)shoppingBagData[1];
            DateTime _orderDate = DateTime.Now;
            int _paymentMethod = 0;
            int _userID = (int)Session[Encryptor.SESSION_LOGIN_KEY];

            order order = new order();
            order.orderDate = _orderDate;
            order.scheduleID = _scheduleID;
            order.userID = _userID;
            order.paymentMethod = _paymentMethod;
            db.orders.Add(order);

            string[] seatIDs = (string[])shoppingBagData[0];
            user user = db.users.Find((int)Session[Encryptor.SESSION_LOGIN_KEY]);
            foreach (string seatID in seatIDs)
            {
                seat seat = db.seats.Find(int.Parse(seatID));
                seat.isAvailable = false;

                order_items orderItem = new order_items();
                orderItem.orderID = order.orderID;
                orderItem.ticketID = DateTime.Now.Month + user.userID + seat.seatName + _scheduleID;
                orderItem.seatID = int.Parse(seatID);
                orderItem.isAdult = user.dateOfBirth.Year - DateTime.Now.Year >= 18;
                orderItem.discount = orderItem.isAdult ? 0 : 10;
                db.order_items.Add(orderItem);

                db.SaveChanges();
            }

            ViewBag.ID = order.orderID;

            //Send EMAIL
            SmtpClient smtp = new SmtpClient();
            smtp.Host = "smtp.gmail.com";
            smtp.Port = 587;
            smtp.EnableSsl = true;
            smtp.Credentials = new NetworkCredential("ph.cinema.aptech@gmail.com", "phcinema@aptech2020");

            MailMessage body = new MailMessage("ph.cinema.aptech@gmail.com",
               user.email, "You was book ticket in PH Cinema. Thank you for choosing us!",
               "<h3 class='color: red'>Thank you for chooosing PH Cinema. We hope you will be satisfied :)</h3><b>Please go to see more detail:</b> domain.do/User/CheckTicket/" + order.orderID);
            body.IsBodyHtml = true;

            smtp.Send(body);
            
            Session.Remove(Encryptor.SHOPPING_BAG_KEY);
            return View();
        }

        public string GetViewOrder(int? id) //id is OrderID
        {
            if(id == null)
            {
                return "Could'n find anything :(";
            }

            IEnumerable<order_items> order_Items = db.order_items.Where(modelInfo => modelInfo.orderID == id);
            schedule schedule = db.schedules.Find(db.orders.Find(id).scheduleID);

            string respond = @"
                <link rel='stylesheet' href='/Content/css/CheckOutPageStyles.css'>
                <script src='//netdna.bootstrapcdn.com/bootstrap/3.0.0/js/bootstrap.min.js'></script>
                <script src='//code.jquery.com/jquery-1.11.1.min.js'></script>
             ";

            foreach (order_items item in order_Items)
            {
                respond += $@"
                <div class='ticket' style='width: 1000px!important'>
                    <div class='stub' style='width: 300px!important'>
                    <div class='top'>
                        <span class='admit'></span>
                        <span class='line'></span>
                        <span class='num'>
                        Ticket ID
                        <span>{item.ticketID}</span>
                        </span>
                    </div>
                    <div class='number'>{db.seats.Find(item.seatID).seatName}</div>
                    <div class='invite'></div>
                </div>
                    <div class='check' style='width: 500px!important'>
                    <div class='big'>
                        {schedule.movy.movieName}
                    </div>
                    <div class='number'></div>
                    <div class='info'>
                        <section>
                        <div class='title'>Time</div>
                        <div>{schedule.scheduleTime}</div>
                        </section>
                        <section>
                        <div class='title'>Issued By</div>
                        <div>PH Cinema</div>
                        </section>
                        <section>
                        <div class='title'>Seat Number</div>
                        <div>{db.seats.Find(item.seatID).seatName}</div>
                        </section>
                    </div>
                    </div>
                </div>";
            }
            return Encryptor.Base64Encode(respond);
        }

    }
}
