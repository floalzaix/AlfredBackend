/**
 * Base exception for expected application errors.
 *
 * Carries an HTTP status and a short title so the API
 * layer can map them to ProblemDetails without treating
 * them as unexpected 500 failures.
 */

//
//  Imports
//

namespace Backend.Models.Exceptions;

//
//  AppException
//  

public class AppException : Exception {
    public int StatusCode { get; }

    public string Title { get; }

    public AppException(
        string message,
        int statusCode,
        string title
    ) : base(message) {
        StatusCode = statusCode;
        Title = title;
    }
}

//
//  UnauthorizedException
//

public class UnauthorizedException : AppException {
    public UnauthorizedException(
        string message = "Unauthorized."
    ) : base(message, 401, "Unauthorized") {
    }
}

//
//  NotFoundException
//

public class NotFoundException : AppException {
    public NotFoundException(
        string message = "Resource not found."
    ) : base(message, 404, "Not Found") {
    }
}

//
//  ConflictException
//

public class ConflictException : AppException {
    public ConflictException(
        string message = "Conflict."
    ) : base(message, 409, "Conflict") {
    }
}
