using FluentResults;

namespace NoviCode.Core.Errors;

public class ValidationError(string message) : Error(message);