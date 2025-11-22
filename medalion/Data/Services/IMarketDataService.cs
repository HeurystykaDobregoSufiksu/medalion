using Medalion.Data.Domain;
using Medalion.Data.DTOs;
using Medalion.Services.Alpaca.Models;
using Medalion.Services.Polymarket.Models;

namespace Medalion.Data.Services;

/// <summary>
/// Service interface for market data operations
/// </summary>
public interface IMarketDataService
{
    #region Alpaca Stock Data

    /// <summary>
    /// Store stock quote snapshot
    /// </summary>
    Task<Guid> StoreStockQuoteAsync(
        Guid assetId,
        StockQuote quote,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Store stock bar data
    /// </summary>
    Task<Guid> StoreStockBarAsync(
        Guid assetId,
        StockBar bar,
        string timeframe,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Store multiple stock bars (bulk operation)
    /// </summary>
    Task<int> StoreStockBarsAsync(
        Guid assetId,
        IEnumerable<StockBar> bars,
        string timeframe,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get latest stock quote
    /// </summary>
    Task<StockQuote?> GetLatestStockQuoteAsync(
        Guid assetId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get stock bars for a date range
    /// </summary>
    Task<IEnumerable<MarketBarDto>> GetStockBarsAsync(
        Guid assetId,
        string timeframe,
        DateTime startDate,
        DateTime endDate,
        CancellationToken cancellationToken = default);

    #endregion

    #region Alpaca Crypto Data

    /// <summary>
    /// Store crypto quote snapshot
    /// </summary>
    Task<Guid> StoreCryptoQuoteAsync(
        Guid assetId,
        CryptoQuote quote,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Store crypto bar data
    /// </summary>
    Task<Guid> StoreCryptoBarAsync(
        Guid assetId,
        CryptoBar bar,
        string timeframe,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Store multiple crypto bars (bulk operation)
    /// </summary>
    Task<int> StoreCryptoBarsAsync(
        Guid assetId,
        IEnumerable<CryptoBar> bars,
        string timeframe,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get latest crypto quote
    /// </summary>
    Task<CryptoQuote?> GetLatestCryptoQuoteAsync(
        Guid assetId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get crypto bars for a date range
    /// </summary>
    Task<IEnumerable<MarketBarDto>> GetCryptoBarsAsync(
        Guid assetId,
        string timeframe,
        DateTime startDate,
        DateTime endDate,
        CancellationToken cancellationToken = default);

    #endregion

    #region Alpaca Options Data

    /// <summary>
    /// Store or update option contract
    /// </summary>
    Task<Guid> StoreOptionContractAsync(
        Guid underlyingAssetId,
        OptionContract contract,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Store option quote with Greeks and IV
    /// </summary>
    Task<Guid> StoreOptionQuoteAsync(
        Guid optionContractId,
        OptionQuote quote,
        decimal? impliedVolatility = null,
        OptionGreeks? greeks = null,
        CancellationToken cancellationToken = default);

    #endregion

    #region Polymarket Data

    /// <summary>
    /// Store or update Polymarket event
    /// </summary>
    Task<Guid> StorePolymarketEventAsync(
        PolymarketEvent polymarketEvent,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Store Polymarket market data snapshot
    /// </summary>
    Task<Guid> StorePolymarketSnapshotAsync(
        Guid marketId,
        MarketDataUpdate snapshot,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Store Polymarket trade data
    /// </summary>
    Task<Guid> StorePolymarketTradeAsync(
        TradeUpdate trade,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Store Polymarket order book snapshot
    /// </summary>
    Task<Guid> StorePolymarketOrderBookAsync(
        OrderBookUpdate orderBook,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get latest Polymarket market snapshot
    /// </summary>
    Task<MarketDataUpdate?> GetLatestPolymarketSnapshotAsync(
        Guid marketId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get Polymarket snapshots for a date range
    /// </summary>
    Task<IEnumerable<MarketDataUpdate>> GetPolymarketSnapshotsAsync(
        Guid marketId,
        DateTime startDate,
        DateTime endDate,
        CancellationToken cancellationToken = default);

    #endregion

    #region Asset Management

    /// <summary>
    /// Create or update asset
    /// </summary>
    Task<AssetDto> CreateOrUpdateAssetAsync(
        string symbol,
        string name,
        AssetType assetType,
        string assetClass,
        string exchange,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get asset by symbol
    /// </summary>
    Task<AssetDto?> GetAssetBySymbolAsync(
        string symbol,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get all active assets
    /// </summary>
    Task<IEnumerable<AssetDto>> GetActiveAssetsAsync(
        CancellationToken cancellationToken = default);

    #endregion
}
