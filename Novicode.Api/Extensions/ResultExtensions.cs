using FluentResults;
using Microsoft.AspNetCore.Mvc;
using NoviCode.Core.Errors;

namespace NoviCode.Api.Extensions;

public static class ResultExtensions
{
    // holds your mappings from Error‐type → HTTP status code
    private static readonly Dictionary<Type,int> _errorStatusCodes = new();

    /// <summary>
    /// Call this at startup to register your Error→StatusCode mappings.
    /// </summary>
    public static void ConfigureErrorMappings(Action<ErrorMappingBuilder> configure)
    {
        var builder = new ErrorMappingBuilder(_errorStatusCodes);
        configure(builder);
    }

    public static ActionResult<T> ToActionResult<T>(this Result<T> result)
    {
        if (result.IsSuccess)
            return result.Value!;

        var err = result.Errors.First();

        if (err.Metadata.TryGetValue("StatusCode", out var mcode)
         && mcode is int code)
        {
            return new ObjectResult(err.Message) { StatusCode = code };
        }

        var t = err.GetType();
        if (_errorStatusCodes.TryGetValue(t, out var mappedCode))
        {
            return new ObjectResult(err.Message) { StatusCode = mappedCode };
        }

        
        if (err is NotFoundError)
            return new NotFoundObjectResult(err.Message);

        // ultimate fallback
        return new BadRequestObjectResult(err.Message);
    }

    public sealed class ErrorMappingBuilder
    {
        private readonly Dictionary<Type,int> _map;
        internal ErrorMappingBuilder(Dictionary<Type,int> map) => _map = map;

        /// <summary>
        /// Map a FluentResults Error subtype to an HTTP status code.
        /// </summary>
        public ErrorMappingBuilder Map<TError>(int statusCode)
            where TError : Error
        {
            _map[typeof(TError)] = statusCode;
            return this;
        }
    }
}