using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoworkingReservation.Application.DTOs.User
{
    /// <summary>
    /// DTO para manejar los datos de la solicitud de login.
    /// </summary>
    public class LoginRequest
    {
        /// <summary>
        /// Identificador del usuario (email o nombre de usuario).
        /// </summary>
        public string Identifier { get; set; }

        /// <summary>
        /// Contraseña del usuario.
        /// </summary>
        public string Password { get; set; }
    }
}