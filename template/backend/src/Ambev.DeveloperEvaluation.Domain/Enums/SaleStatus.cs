namespace Ambev.DeveloperEvaluation.Domain.Enums;

/// <summary>
/// Represents the status of a sale
/// </summary>
public enum SaleStatus
{
    /// <summary>
    /// Sale is active and not cancelled
    /// </summary>
    Active = 1,

    /// <summary>
    /// Sale has been cancelled
    /// </summary>
    Cancelled = 2
}