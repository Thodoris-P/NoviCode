using FluentResults;

namespace NoviCode.Core.Errors;

public class ExchangeProviderError(string message) : Error(message);