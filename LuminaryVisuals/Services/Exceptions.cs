using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using System.Reflection;

namespace LuminaryVisuals.Services
{
    public class ProjectReorderingServiceException : Exception {
        public ProjectReorderingServiceException() { }

        public ProjectReorderingServiceException(string message)
            : base(message) { }

        public ProjectReorderingServiceException(string message, Exception inner)
            : base(message, inner) { }
    }
    public class ProjectReorderingNotFoundException : ProjectReorderingServiceException
    {
        public ProjectReorderingNotFoundException(string message) : base(message) { }
        public ProjectReorderingNotFoundException(string message, Exception innerException) : base(message, innerException) { }
    }

    public class InvalidProjectOrderException : ProjectReorderingServiceException
    {
        public InvalidProjectOrderException(string message) : base(message) { }
        public InvalidProjectOrderException(string message, Exception innerException) : base(message, innerException) { }
    }

}