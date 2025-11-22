using Medalion.Services.Alpaca.Models;

namespace Medalion.Services.Alpaca.Interfaces;

/// <summary>
/// Interface for Alpaca Markets REST API client
/// Provides access to stock, crypto, and options data including implied volatility
/// </summary>
public interface IAlpacaApiClient : IDisposable
{
    #region Stock Data Methods

    /// <summary>
    /// Get the latest quote for a stock symbol
    /// </summary>
    /// <param name="symbol">Stock symbol (e.g., "AAPL")</param>
    /// <param name="feed">Data feed to use (default: SIP for all exchanges)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Latest stock quote</returns>
    Task<StockQuote> GetStockQuoteAsync(
        string symbol,
        Feed feed = Feed.SIP,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get historical bars/candles for a stock
    /// </summary>
    /// <param name="symbol">Stock symbol</param>
    /// <param name="timeframe">Bar timeframe (1Min, 1Hour, 1Day, etc.)</param>
    /// <param name="start">Start time (UTC)</param>
    /// <param name="end">End time (UTC)</param>
    /// <param name="limit">Max number of bars to return (default: 1000)</param>
    /// <param name="feed">Data feed to use</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Historical bars</returns>
    Task<StockBarsResponse> GetStockBarsAsync(
        string symbol,
        Timeframe timeframe,
        DateTime start,
        DateTime end,
        int limit = 1000,
        Feed feed = Feed.SIP,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get a snapshot of current stock data (quote, trade, bars)
    /// </summary>
    /// <param name="symbol">Stock symbol</param>
    /// <param name="feed">Data feed to use</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Stock snapshot</returns>
    Task<StockSnapshot> GetStockSnapshotAsync(
        string symbol,
        Feed feed = Feed.SIP,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get latest trade for a stock
    /// </summary>
    /// <param name="symbol">Stock symbol</param>
    /// <param name="feed">Data feed to use</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Latest stock trade</returns>
    Task<StockTrade> GetStockLatestTradeAsync(
        string symbol,
        Feed feed = Feed.SIP,
        CancellationToken cancellationToken = default);

    #endregion

    #region Crypto Data Methods

    /// <summary>
    /// Get the latest quote for a crypto symbol
    /// </summary>
    /// <param name="symbol">Crypto symbol (e.g., "BTCUSD", "ETHUSD")</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Latest crypto quote</returns>
    Task<CryptoQuote> GetCryptoQuoteAsync(
        string symbol,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get historical bars for a crypto symbol
    /// </summary>
    /// <param name="symbol">Crypto symbol</param>
    /// <param name="timeframe">Bar timeframe</param>
    /// <param name="start">Start time (UTC)</param>
    /// <param name="end">End time (UTC)</param>
    /// <param name="limit">Max number of bars (default: 1000)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Historical crypto bars</returns>
    Task<CryptoBarsResponse> GetCryptoBarsAsync(
        string symbol,
        Timeframe timeframe,
        DateTime start,
        DateTime end,
        int limit = 1000,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get a snapshot of current crypto data
    /// </summary>
    /// <param name="symbol">Crypto symbol</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Crypto snapshot</returns>
    Task<CryptoSnapshot> GetCryptoSnapshotAsync(
        string symbol,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get latest trade for a crypto symbol
    /// </summary>
    /// <param name="symbol">Crypto symbol</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Latest crypto trade</returns>
    Task<CryptoTrade> GetCryptoLatestTradeAsync(
        string symbol,
        CancellationToken cancellationToken = default);

    #endregion

    #region Options Data Methods

    /// <summary>
    /// Get options chain for an underlying symbol
    /// Returns all available option contracts
    /// </summary>
    /// <param name="underlyingSymbol">Underlying stock symbol (e.g., "AAPL")</param>
    /// <param name="expirationDate">Filter by expiration date (optional)</param>
    /// <param name="strikePrice">Filter by strike price (optional)</param>
    /// <param name="optionType">Filter by call/put (optional)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Options chain</returns>
    Task<OptionsChainResponse> GetOptionsChainAsync(
        string underlyingSymbol,
        DateTime? expirationDate = null,
        decimal? strikePrice = null,
        OptionType? optionType = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get a specific option contract by symbol
    /// </summary>
    /// <param name="optionSymbol">Option symbol (e.g., "AAPL230120C00150000")</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Option contract details</returns>
    Task<OptionContract> GetOptionContractAsync(
        string optionSymbol,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get latest quote for an option
    /// </summary>
    /// <param name="optionSymbol">Option symbol</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Option quote</returns>
    Task<OptionQuote> GetOptionQuoteAsync(
        string optionSymbol,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get snapshot for an option (quote, trade, Greeks, IV)
    /// </summary>
    /// <param name="optionSymbol">Option symbol</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Option snapshot with Greeks and IV</returns>
    Task<OptionSnapshot> GetOptionSnapshotAsync(
        string optionSymbol,
        CancellationToken cancellationToken = default);

    #endregion

    #region Implied Volatility Methods (PRIMARY FOCUS)

    /// <summary>
    /// Get implied volatility for a specific option contract
    /// This is the PRIMARY method for retrieving IV data
    /// </summary>
    /// <param name="optionSymbol">Option symbol (e.g., "AAPL230120C00150000")</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Implied volatility data with Greeks</returns>
    Task<ImpliedVolatilityData> GetImpliedVolatilityAsync(
        string optionSymbol,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get implied volatility for an entire options chain
    /// Useful for building volatility surfaces and analyzing skew
    /// </summary>
    /// <param name="underlyingSymbol">Underlying stock symbol</param>
    /// <param name="expirationDate">Filter by expiration date (optional)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>IV data for all options in the chain</returns>
    Task<ImpliedVolatilityChainResponse> GetImpliedVolatilityChainAsync(
        string underlyingSymbol,
        DateTime? expirationDate = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get implied volatility for multiple option symbols at once
    /// Batch operation for efficiency
    /// </summary>
    /// <param name="optionSymbols">List of option symbols</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Dictionary of symbol to IV data</returns>
    Task<Dictionary<string, ImpliedVolatilityData>> GetImpliedVolatilityBatchAsync(
        IEnumerable<string> optionSymbols,
        CancellationToken cancellationToken = default);

    #endregion

    #region Utility Methods

    /// <summary>
    /// Get current rate limit information
    /// </summary>
    /// <returns>Rate limit info</returns>
    RateLimitInfo GetRateLimitInfo();

    /// <summary>
    /// Check if the API client is healthy and authenticated
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if healthy</returns>
    Task<bool> HealthCheckAsync(CancellationToken cancellationToken = default);

    #endregion
}
