using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using DAL;
using DAL.Models;
using PagedList;

namespace SecSisBoletas.Areas.Administrador.Controllers
{
    [Authorize(Roles = "Admin, Fiscalizacion")]
    public class SueldosBasicosController : Controller
    {
        private SecModel db = new SecModel();

        // GET: Administrador/SueldosBasicos
        public ActionResult Index(string sortOrder, string currentFilter, DateTime? fecha, int? page)
        {
            ViewBag.NameSortParm = String.IsNullOrEmpty(sortOrder) ? "name_desc" : "";
            ViewBag.MontoSortParm = sortOrder == "Monto" ? "Monto_desc" : "Monto";
            ViewBag.CurrentSort = sortOrder;//Para mantener el orden(asc o desc)

            var sueldosBasicos = db.SueldoBasico.Include(t => t.Categoria).OrderByDescending( t => t.Desde);

            var fechas = sueldosBasicos.Select(x => x.Desde).Distinct().ToList();
            //DateTime hoy = DateTime.Today;
            //fechas.Add(hoy);

            if(fecha == null || fecha == DateTime.MinValue)
            {
                fecha = fechas.Last();
            }
            ViewBag.fecha = new SelectList(fechas);
            ViewBag.fecha = new SelectList(fechas, fecha);
            sueldosBasicos = sueldosBasicos.Where(x => x.Desde == fecha).Include(t => t.Categoria).OrderByDescending(t => t.Desde);

            var categorias = db.Categoria.ToList();
            List<Categoria> listaCategorias = new List<Categoria>();

            foreach (var cat in categorias)
            {
                if(sueldosBasicos.Where(x => x.IdCategoria == cat.IdCategoria).FirstOrDefault() == null)
                {
                    listaCategorias.Add(cat);
                }
            }

            ViewBag.FechaSeleccionada = fecha;
            ViewBag.IdCategoria = new SelectList(listaCategorias, "IdCategoria", "Descripcion");

            switch (sortOrder)
            {
                case "Monto_desc":
                    sueldosBasicos = sueldosBasicos.OrderByDescending(x => x.Monto);
                    break;
                case "Monto":
                    sueldosBasicos = sueldosBasicos.OrderBy(x => x.Monto);
                    break;
                case "name_desc":
                    sueldosBasicos = sueldosBasicos.OrderByDescending(x => x.Categoria.Descripcion);
                    break;
                default:
                    sueldosBasicos = sueldosBasicos.OrderBy(x => x.Categoria.Descripcion);
                    break;
            }
            int pageSize = (sueldosBasicos.Count() == 0) ? 1 : sueldosBasicos.Count();
            int pageNumber = (page ?? 1);

            return View(sueldosBasicos.ToPagedList(pageNumber, pageSize));
        }

        // GET: Administrador/SueldosBasicos/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            SueldoBasico sueldoBasico = db.SueldoBasico.Find(id);
            if (sueldoBasico == null)
            {
                return HttpNotFound();
            }
            return View(sueldoBasico);
        }

        // GET: Administrador/SueldosBasicos/Create
        public ActionResult Create()
        {
            ViewBag.IdCategoria = new SelectList(db.Categoria, "IdCategoria", "Descripcion");
            return View();
        }

        // POST: Administrador/SueldosBasicos/Create
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que desea enlazarse. Para obtener 
        // más información vea https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "IdSueldoBasico,IdCategoria,Monto,Desde,Hasta")] SueldoBasico sueldoBasico)
        {
            if (ModelState.IsValid)
            {
                db.SueldoBasico.Add(sueldoBasico);
                db.SaveChanges();
                return RedirectToAction("Index", new { fecha = sueldoBasico.Desde });
            }

            ViewBag.IdCategoria = new SelectList(db.Categoria, "IdCategoria", "Descripcion", sueldoBasico.IdCategoria);
            //return View(sueldoBasico);
            return RedirectToAction("Index", new { fecha = sueldoBasico.Desde });
        }

        // GET: Administrador/SueldosBasicos/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            SueldoBasico sueldoBasico = db.SueldoBasico.Find(id);
            if (sueldoBasico == null)
            {
                return HttpNotFound();
            }
            ViewBag.IdCategoria = new SelectList(db.Categoria, "IdCategoria", "Descripcion", sueldoBasico.IdCategoria);
            return View(sueldoBasico);
        }

        // POST: Administrador/SueldosBasicos/Edit/5
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que desea enlazarse. Para obtener 
        // más información vea https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "IdSueldoBasico,IdCategoria,Monto,Desde,Hasta")] SueldoBasico sueldoBasico)
        {
            if (ModelState.IsValid)
            {
                db.Entry(sueldoBasico).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.IdCategoria = new SelectList(db.Categoria, "IdCategoria", "Descripcion", sueldoBasico.IdCategoria);
            return View(sueldoBasico);
        }

        // GET: Administrador/SueldosBasicos/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            SueldoBasico sueldoBasico = db.SueldoBasico.Find(id);
            if (sueldoBasico == null)
            {
                return HttpNotFound();
            }
            return View(sueldoBasico);
        }

        // POST: Administrador/SueldosBasicos/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            SueldoBasico sueldoBasico = db.SueldoBasico.Find(id);
            db.SueldoBasico.Remove(sueldoBasico);
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
