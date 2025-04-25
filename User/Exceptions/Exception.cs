using System;
using System.Net;

namespace MODSI_SQLRestAPI.UserAuth
{
    public class HttpException : Exception
    {
        public HttpStatusCode StatusCode { get; }

        public HttpException(HttpStatusCode statusCode, string message) : base(message)
        {
            StatusCode = statusCode;
        }
    }

    public class NotFoundException : HttpException
    {
        public NotFoundException(string message)
            : base(HttpStatusCode.NoContent, message) { }
    }

    public class BadRequestException : HttpException
    {
        public BadRequestException(string message)
            : base(HttpStatusCode.BadRequest, message) { }
    }

}
