using Domain;
using Domain.Errors;
using Microsoft.AspNetCore.Mvc;

namespace Api.Extensions;

public static class ResultExtensions
{
    public static IActionResult ToActionResult(this Result result)
    {
        return result.IsSuccess
            ? new NoContentResult()
            : result.Error!.ToActionResult();
    }

    public static IActionResult ToActionResult<TValue>(this Result<TValue> result)
    {
        return result.IsSuccess
            ? new OkObjectResult(result.Value)
            : result.Error!.ToActionResult();
    }

    private static IActionResult ToActionResult(this DomainError error)
    {
        var statusCode = error.ErrorType switch
        {
            ErrorType.NotFound   => StatusCodes.Status404NotFound,
            ErrorType.Conflict   => StatusCodes.Status409Conflict,
            ErrorType.Validation => StatusCodes.Status422UnprocessableEntity,
            _                    => StatusCodes.Status500InternalServerError
        };

        var problemDetails = new ProblemDetails
        {
            Title  = error.Code,
            Detail = error.Description,
            Status = statusCode
        };

        if (error.Metadata is not null)
            foreach (var (key, value) in error.Metadata)
                problemDetails.Extensions[key] = value;

        return new ObjectResult(problemDetails) { StatusCode = statusCode };
    }
}