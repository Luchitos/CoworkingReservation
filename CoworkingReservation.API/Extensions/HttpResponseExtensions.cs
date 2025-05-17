using CoworkingReservation.API.Responses;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace CoworkingReservation.API.Extensions
{
    public static class HttpResponseExtensions
    {
        public static async Task Success(this HttpResponse response, object data, int statusCode = 200)
        {
            response.StatusCode = statusCode;
            await response.WriteAsJsonAsync(Response.Success(data, statusCode));
        }

        public static async Task Failure(this HttpResponse response, string error, int statusCode = 400)
        {
            response.StatusCode = statusCode;
            await response.WriteAsJsonAsync(Response.Failure(error, statusCode));
        }
    }
} 