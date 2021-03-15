using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SecSisBoletas.Models.Utilities
{
    public class RespuestaJson
    {
        public bool resultado;
        public string mensaje;

        public RespuestaJson(bool _Resultado, string _Mensaje)
        {
            resultado = _Resultado;
            mensaje = _Mensaje;
        }
    }
}