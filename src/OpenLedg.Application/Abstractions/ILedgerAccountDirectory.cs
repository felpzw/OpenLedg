namespace OpenLedg.Application.Abstractions;

// Future TigerBeetle adapter must validate account existence before a route is activated.
public interface ILedgerAccountDirectory
{
    Task<bool> ExistsAsync(UInt128 accountId, CancellationToken cancellationToken = default);
}
