namespace Rework.Domain;

/// <summary>A business rule was violated. The API maps this to HTTP 409.</summary>
public class DomainRuleException(string message) : Exception(message) { }
