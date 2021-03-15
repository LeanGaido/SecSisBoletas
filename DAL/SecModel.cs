using DAL.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Conventions;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL
{
    public class SecModel: DbContext
    {
        public SecModel() : base("SecModel")
        {
        }

        public DbSet<Actividad> Actividad { get; set; }
        public DbSet<Afiliado> Afiliado { get; set; }
        public DbSet<Categoria> Categoria { get; set; }
        public DbSet<DeclaracionJurada> DeclaracionJurada { get; set; }
        public DbSet<DetalleDeclaracionJurada> DetalleDeclaracionJurada { get; set; }
        public DbSet<Empleado> Empleado { get; set; }
        public DbSet<EmpleadoEmpresa> EmpleadoEmpresa { get; set; }
        public DbSet<Empresa> Empresa { get; set; }
        public DbSet<Jornada> Jornada { get; set; }
        public DbSet<LicenciaEmpleado> LicenciaEmpleado { get; set; }
        public DbSet<LicenciaLaboral> LicenciaLaboral { get; set; }
        public DbSet<LiquidacionProporcional> LiquidacionProporcional { get; set; }
        public DbSet<LiquidacionProporcionalEmpleado> LiquidacionProporcionalEmpleado { get; set; }
        public DbSet<Localidad> Localidad { get; set; }
        public DbSet<Provincia> Provincia { get; set; }
        public DbSet<SueldoBasico> SueldoBasico { get; set; }
        public DbSet<BoletaAportes> BoletaAportes { get; set; }
        public DbSet<FechaVencimiento> FechaVencimiento { get; set; }
        public DbSet<Reclamo> Reclamo { get; set; }
        public DbSet<TurnoEmpleado> TurnoEmpleado { get; set; }
        public DbSet<HorarioEmpleado> HorarioEmpleado { get; set; }
        public DbSet<BoletaAportesEspecial> BoletaAportesEspeciales { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Conventions.Remove<PluralizingTableNameConvention>();
        }
    }
}
