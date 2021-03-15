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

namespace SecSisBoletas.Areas.Administrador.Controllers
{
    public class BoletaAportesController : Controller
    {
        private SecModel db = new SecModel();

        // GET: Administrador/BoletaAportes
        public ActionResult Index()
        {
            var boletaAportes = db.BoletaAportes.Include(b => b.DeclaracionJurada).Where(x => x.DeBaja == false);
            return View(boletaAportes.ToList());
        }

        // GET: Administrador/BoletaAportes/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            BoletaAportes boletaAportes = db.BoletaAportes.Find(id);
            if (boletaAportes == null)
            {
                return HttpNotFound();
            }
            return View(boletaAportes);
        }

        // GET: Administrador/BoletaAportes/Create
        public ActionResult Create()
        {
            ViewBag.IdDeclaracionJurada = new SelectList(db.DeclaracionJurada.Where(x => x.DeBaja == false), "IdDeclaracionJurada", "IdDeclaracionJurada");
            return View();
        }

        // POST: Administrador/BoletaAportes/Create
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que desea enlazarse. Para obtener 
        // más información vea https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "IdBoleta,IdDeclaracionJurada,MesBoleta,AnioBoleta,FechaVencimiento,TotalSueldos2,TotalSueldos5,TotalPagado,RecargoMora,BoletaPagada,FechaPago,FechaBoleta")] BoletaAportes boletaAportes)
        {
            if (ModelState.IsValid)
            {
                db.BoletaAportes.Add(boletaAportes);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.IdDeclaracionJurada = new SelectList(db.DeclaracionJurada.Where(x => x.DeBaja == false), "IdDeclaracionJurada", "IdDeclaracionJurada", boletaAportes.IdDeclaracionJurada);
            return View(boletaAportes);
        }

        // GET: Administrador/BoletaAportes/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            BoletaAportes boletaAportes = db.BoletaAportes.Find(id);
            if (boletaAportes == null)
            {
                return HttpNotFound();
            }
            ViewBag.IdDeclaracionJurada = new SelectList(db.DeclaracionJurada.Where(x => x.DeBaja == false), "IdDeclaracionJurada", "IdDeclaracionJurada", boletaAportes.IdDeclaracionJurada);
            return View(boletaAportes);
        }

        // POST: Administrador/BoletaAportes/Edit/5
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que desea enlazarse. Para obtener 
        // más información vea https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "IdBoleta,IdDeclaracionJurada,MesBoleta,AnioBoleta,FechaVencimiento,TotalSueldos2,TotalSueldos5,TotalPagado,RecargoMora,BoletaPagada,FechaPago,FechaBoleta")] BoletaAportes boletaAportes)
        {
            if (ModelState.IsValid)
            {
                db.Entry(boletaAportes).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.IdDeclaracionJurada = new SelectList(db.DeclaracionJurada.Where(x => x.DeBaja == false), "IdDeclaracionJurada", "IdDeclaracionJurada", boletaAportes.IdDeclaracionJurada);
            return View(boletaAportes);
        }

        // GET: Administrador/BoletaAportes/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            BoletaAportes boletaAportes = db.BoletaAportes.Find(id);
            if (boletaAportes == null)
            {
                return HttpNotFound();
            }
            return View(boletaAportes);
        }

        // POST: Administrador/BoletaAportes/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            BoletaAportes boletaAportes = db.BoletaAportes.Find(id);
            db.BoletaAportes.Remove(boletaAportes);
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
