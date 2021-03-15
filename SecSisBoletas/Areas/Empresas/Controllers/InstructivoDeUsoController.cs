using DAL;
using System;
using System.Data;
using System.Data.Entity;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using PagedList;

namespace SecSisBoletas.Areas.Empresas.Controllers
{
    [Authorize (Roles = "Empresa")]
    public class InstructivoDeUsoController : Controller
    {
        private SecModel db = new SecModel();

        // GET: Empresas/InstructivoDeUso/IntructivoImportacionEmpleados
        public ActionResult IntructivoImportacionEmpleados()
        {
            return View();
        }
        // GET: Empresas/InstructivoDeUso/IntructivoImportacionDDJJ
        public ActionResult IntructivoImportacionDDJJ()
        {
            return View();
        }

        // GET: Empresas/InstructivoDeUso/IndexLocalidades
        public ActionResult IndexLocalidades(string sortOrder, string currentFilter, string searchString, int? page, int idProvincia = 0)
        {
            ViewBag.NameSortParm = String.IsNullOrEmpty(sortOrder) ? "name_desc" : "";
            ViewBag.CurrentSort = sortOrder;//Para mantener el orden(asc o desc)
            ViewBag.idEmpresa = new SelectList(db.Empresa.Where(x => x.FechaBajaEmpresa == null), "IdEmpresa", "RazonSocial");
            ViewBag.idProvincia = new SelectList(db.Provincia, "IdProvincia", "Nombre", idProvincia);
            ViewBag.idProvinciaSeleccionada = idProvincia;

            if (searchString != null)
            {
                page = 1;
            }
            else
            {
                searchString = currentFilter;
            }

            ViewBag.CurrentFilter = searchString;

            var localidades = db.Localidad.Include(t => t.Provincia);

            if (idProvincia != 0)
            {
                localidades = localidades.Where(x => x.IdProvincia == idProvincia);
            }

            if (!string.IsNullOrEmpty(searchString))
            {
                localidades = localidades.Where(x => x.Nombre.Contains(searchString));
            }

            switch (sortOrder)
            {
                case "name_desc":
                    localidades = localidades.OrderByDescending(x => x.Nombre);
                    break;
                default:
                    localidades = localidades.OrderBy(x => x.Nombre);
                    break;
            }
            int pageSize = 7;
            int pageNumber = (page ?? 1);

            return View(localidades.ToPagedList(pageNumber, pageSize));
        }

        // GET: Empresas/InstructivoDeUso/IndexProvincias
        public ActionResult IndexProvincias(string sortOrder, string currentFilter, string searchString, int? page)
        {
            ViewBag.NameSortParm = String.IsNullOrEmpty(sortOrder) ? "name_desc" : "";
            ViewBag.CurrentSort = sortOrder;//Para mantener el orden(asc o desc)

            if (searchString != null)
            {
                page = 1;
            }
            else
            {
                searchString = currentFilter;
            }

            ViewBag.CurrentFilter = searchString;

            var provincias = from oProvincia in db.Provincia
                             select oProvincia;

            if (!string.IsNullOrEmpty(searchString))
            {
                provincias = provincias.Where(x => x.Nombre.Contains(searchString));
            }

            switch (sortOrder)
            {
                case "name_desc":
                    provincias = provincias.OrderByDescending(x => x.Nombre);
                    break;
                default:
                    provincias = provincias.OrderBy(x => x.Nombre);
                    break;
            }
            int pageSize = 7;
            int pageNumber = (page ?? 1);

            return View(provincias.ToPagedList(pageNumber, pageSize));
        }

        // GET: Empresas/InstructivoDeUso/IndexCategorias
        public ActionResult IndexCategorias(string sortOrder, string currentFilter, string searchString, int? page)
        {
            ViewBag.NameSortParm = String.IsNullOrEmpty(sortOrder) ? "name_desc" : "";
            ViewBag.CurrentSort = sortOrder;//Para mantener el orden(asc o desc)

            if (searchString != null)
            {
                page = 1;
            }
            else
            {
                searchString = currentFilter;
            }

            ViewBag.CurrentFilter = searchString;

            var categorias = from oCategorias in db.Categoria
                             select oCategorias;

            if (!string.IsNullOrEmpty(searchString))
            {
                categorias = categorias.Where(x => x.Descripcion.Contains(searchString));
            }

            switch (sortOrder)
            {
                case "name_desc":
                    categorias = categorias.OrderByDescending(x => x.Descripcion);
                    break;
                default:
                    categorias = categorias.OrderBy(x => x.Descripcion);
                    break;
            }
            int pageSize = 7;
            int pageNumber = (page ?? 1);

            return View(categorias.ToPagedList(pageNumber, pageSize));
        }

        // GET: Empresas/InstructivoDeUso/IndexJornadas
        public ActionResult IndexJornadas(string sortOrder, string currentFilter, string searchString, int? page)
        {
            ViewBag.NameSortParm = String.IsNullOrEmpty(sortOrder) ? "name_desc" : "";
            ViewBag.CurrentSort = sortOrder;//Para mantener el orden(asc o desc)

            if (searchString != null)
            {
                page = 1;
            }
            else
            {
                searchString = currentFilter;
            }

            ViewBag.CurrentFilter = searchString;

            var jornadas = from oJornadas in db.Jornada
                             select oJornadas;

            if (!string.IsNullOrEmpty(searchString))
            {
                jornadas = jornadas.Where(x => x.Descripcion.Contains(searchString));
            }

            switch (sortOrder)
            {
                case "name_desc":
                    jornadas = jornadas.OrderByDescending(x => x.Descripcion);
                    break;
                default:
                    jornadas = jornadas.OrderBy(x => x.Descripcion);
                    break;
            }
            int pageSize = 7;
            int pageNumber = (page ?? 1);

            return View(jornadas.ToPagedList(pageNumber, pageSize));
        }

        // GET: Empresas/InstructivoDeUso/IndexLiquidacionesProporcionales
        public ActionResult IndexLiquidacionesProporcionales(string sortOrder, string currentFilter, string searchString, int? page)
        {
            ViewBag.NameSortParm = String.IsNullOrEmpty(sortOrder) ? "name_desc" : "";
            ViewBag.CurrentSort = sortOrder;//Para mantener el orden(asc o desc)

            if (searchString != null)
            {
                page = 1;
            }
            else
            {
                searchString = currentFilter;
            }

            ViewBag.CurrentFilter = searchString;

            var liquidaciones = from oLiquidaciones in db.LiquidacionProporcional
                             select oLiquidaciones;

            if (!string.IsNullOrEmpty(searchString))
            {
                liquidaciones = liquidaciones.Where(x => x.Descripcion.Contains(searchString));
            }

            switch (sortOrder)
            {
                case "name_desc":
                    liquidaciones = liquidaciones.OrderByDescending(x => x.Descripcion);
                    break;
                default:
                    liquidaciones = liquidaciones.OrderBy(x => x.Descripcion);
                    break;
            }
            int pageSize = 7;
            int pageNumber = (page ?? 1);

            return View(liquidaciones.ToPagedList(pageNumber, pageSize));
        }
    }

}