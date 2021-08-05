using PagedList;
using PagedList.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace ASP_MVCProject_NguyenVuongThienPhuc.Controllers
{
    public class MovieController : Controller
    {
        private readonly cinemaManagerEntities db = new cinemaManagerEntities();
        private const int pageSize = 10;

        public ActionResult Index(int? page)
        {
            if (page == null) page = 1;
            IEnumerable<movie> movies = db.movies.Where(movieInfo => movieInfo.publishDate < DateTime.Now).OrderBy(movieInfo => movieInfo.movieName);
            int pageNumber = page ?? 1;
            return View(movies.ToPagedList(pageNumber, pageSize));
        }

        public ActionResult ComingSoon(int? page)
        {
            if (page == null) page = 1;
            IEnumerable<movie> movies = db.movies.Where(movieInfo => movieInfo.publishDate > DateTime.Now).OrderBy(movieInfo => movieInfo.movieName);
            int pageNumber = page ?? 1;
            return View(movies.ToPagedList(pageNumber, pageSize));
        }

        public ActionResult Category(int ?id, int? page)
        {
            if(id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            category category = db.categories.Find(id);
            if(category == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ViewBag.categoryName = category.categoryName;

            if (page == null) page = 1;
            IEnumerable<movie> movies = db.movies.Where(movieInfo => movieInfo.categoryID == category.categoryID).OrderBy(movieInfo => movieInfo.movieName);
            int pageNumber = page ?? 1;
            return View(movies.ToPagedList(pageNumber, pageSize));
        }

        public ActionResult Detail(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            movie movieInfo = db.movies.Where(movie => movie.url == id).FirstOrDefault();

            if (movieInfo == null)
            {
                return HttpNotFound();
            }

            return View(model: movieInfo);
        }

        public string findMovie(string name)
        {
            if(name == "")
            {
                return Encryptor.Base64Encode(name);
            }

            StringBuilder responsive = new StringBuilder(
                @"<script>
                    $('#trailerViewModal').on('show.bs.modal', function (event) {
                        var button = $(event.relatedTarget)
                        var embedCode = button.data('embedcode')
                        var modal = $(this)
                        modal.find('.modal-body').html(embedCode)
                    });
                    $('#trailerViewModal').on('hide.bs.modal', function (event) {
                        var modal = $(this)
                        modal.find('.modal-body').html('')
                    });
                </script>");

            IEnumerable<movie> movies = db.movies.Where(movie => movie.movieName.Contains(name));
            
            if(movies == null)
            {
                return Encryptor.Base64Encode("We can't find anything, please try again and check the movie name");
            }

            foreach(movie item in movies)
            {
                responsive.Append($@"<div class='col-md-3 col-sm-6 col-6 card p-0'>
                    <img src='{item.imageLink}' class='card-img-top' />
                    <div class='details'>
                        <div class='center'>
                            <h1>{item.movieName}<br><span class='mt-2'>{item.category.categoryName} movie</span></h1>
                            <div class='mt-3'>
                                <button type='button' class='button-fill' data-toggle='modal' data-target='#trailerViewModal' data-embedcode='{item.trailerEmbedHTML}'>Trailer</button>
                                <div class='button button-2' onclick='window.open('/Movie/Detail/{item.url}', '_self');'>
                                    <div id='slide'></div>
                                    <a href='/Movie/Detail/{item.url}' style='color: #a61d2a'>Detail</a>
                                </div>
                            </div>
                        </div>
                    </div>
                </div>");
            }

            return Encryptor.Base64Encode(responsive.ToString());
        }

        public ActionResult Find()
        {
            return View();
        }
    }
}