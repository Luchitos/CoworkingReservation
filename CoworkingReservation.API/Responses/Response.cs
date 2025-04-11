namespace CoworkingReservation.API.Responses
{
   /// <summary>
    /// Clase genérica para estandarizar las respuestas de la API.
    /// </summary>
    public class Response
    {
        /// <summary>
        /// Código de estado de la operación.
        /// </summary>
        public int Status { get; set; }

        /// <summary>
        /// Datos devueltos por la operación.
        /// </summary>
        public object? Data { get; set; }

        /// <summary>
        /// Mensaje de error en caso de fallo.
        /// </summary>
        public string? Error { get; set; }

        /// <summary>
        /// Constructor para una respuesta exitosa.
        /// </summary>
        public static Response Success(object data, int status = 200)
        {
            return new Response
            {
                Status = status,
                Data = data,
                Error = null
            };
        }

        /// <summary>
        /// Constructor para una respuesta con error.
        /// </summary>
        public static Response Failure(string error, int status = 400)
        {
            return new Response
            {
                Status = status,
                Data = null,
                Error = error
            };
        }
    }
}
