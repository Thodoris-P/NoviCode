using FluentResults;

namespace NoviCode.Core.Errors;

public class BusinessError(string message) : Error(message);