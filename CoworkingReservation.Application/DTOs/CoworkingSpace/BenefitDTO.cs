using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoworkingReservation.Application.DTOs.CoworkingSpace
{
    /// <summary>
    /// DTO para la respuesta de un beneficio ofrecido por un coworking.
    /// </summary>
    public class BenefitDTO
    {
        #region Properties

        /// <summary>
        /// Identificador del beneficio.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Nombre del beneficio.
        /// </summary>
        public string Name { get; set; }

        #endregion
    }
}
