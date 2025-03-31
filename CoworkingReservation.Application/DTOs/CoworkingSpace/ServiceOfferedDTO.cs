using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoworkingReservation.Application.DTOs.CoworkingSpace
{
    /// <summary>
    /// DTO para la respuesta de un servicio ofrecido por un coworking.
    /// </summary>
    public class ServiceOfferedDTO
    {
        #region Properties

        /// <summary>
        /// Identificador del servicio.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Nombre del servicio ofrecido.
        /// </summary>
        public string Name { get; set; }

        #endregion
    }
}
