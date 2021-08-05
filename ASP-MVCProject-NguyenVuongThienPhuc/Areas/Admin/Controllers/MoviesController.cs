using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using ASP_MVCProject_NguyenVuongThienPhuc;

namespace ASP_MVCProject_NguyenVuongThienPhuc.Areas.Admin.Controllers
{
    public class MoviesController : Controller
    {
        private readonly cinemaManagerEntities db = new cinemaManagerEntities();

        // GET: Admin/Movies
        public ActionResult Index()
        {
            var movies = db.movies.Include(m => m.category).Include(m => m.movieRated);
            return View(movies.ToList());
        }

        // GET: Admin/Movies/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            movie movie = db.movies.Find(id);
            if (movie == null)
            {
                return HttpNotFound();
            }
            return View(movie);
        }

        // GET: Admin/Movies/Create
        public ActionResult Create()
        {
            ViewBag.categoryID = new SelectList(db.categories, "categoryID", "categoryName");
            ViewBag.ratedID = new SelectList(db.movieRateds, "ratedID", "ratedName");
            return View();
        }

        // POST: Admin/Movies/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "movieID,movieName,categoryID,publishDate,price,trailerEmbedHTML,length,imageLink,description,ratedID,language,url")] movie movie)
        {
            if (ModelState.IsValid)
            {
                db.movies.Add(movie);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.categoryID = new SelectList(db.categories, "categoryID", "categoryName", movie.categoryID);
            ViewBag.ratedID = new SelectList(db.movieRateds, "ratedID", "ratedName", movie.ratedID);
            return View(movie);
        }

        // GET: Admin/Movies/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            movie movie = db.movies.Find(id);
            if (movie == null)
            {
                return HttpNotFound();
            }
            ViewBag.categoryID = new SelectList(db.categories, "categoryID", "categoryName", movie.categoryID);
            ViewBag.ratedID = new SelectList(db.movieRateds, "ratedID", "ratedName", movie.ratedID);
            return View(movie);
        }

        // POST: Admin/Movies/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "movieID,movieName,categoryID,publishDate,price,trailerEmbedHTML,length,imageLink,description,ratedID,language,url")] movie movie)
        {
            if (ModelState.IsValid)
            {
                db.Entry(movie).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.categoryID = new SelectList(db.categories, "categoryID", "categoryName", movie.categoryID);
            ViewBag.ratedID = new SelectList(db.movieRateds, "ratedID", "ratedName", movie.ratedID);
            return View(movie);
        }

        // GET: Admin/Movies/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            movie movie = db.movies.Find(id);
            if (movie == null)
            {
                return HttpNotFound();
            }
            return View(movie);
        }

        // POST: Admin/Movies/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            movie movie = db.movies.Find(id);
            db.movies.Remove(movie);
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
    }
}
