using FluentResults;

namespace NoviCode.Core.Errors;

public class NotFoundError(string message) : Error(message);