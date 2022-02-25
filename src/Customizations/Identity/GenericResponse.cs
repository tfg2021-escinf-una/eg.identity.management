using System.Net;

namespace EG.IdentityManagement.Microservice.Entities
{
    public class GenericResponse
    {
        public object Data { set; get; } = new object();
        public object Errors { set; get; } = new object[] { };
        public HttpStatusCode StatusCode { set; get; }
    }
}