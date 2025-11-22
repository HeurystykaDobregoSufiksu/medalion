using System.Collections.Concurrent;
using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;
using Medalion.Services.Alpaca.Interfaces;
using Medalion.Services.Alpaca.Models;
using Microsoft.Extensions.Logging;

namespace Medalion.Services.Alpaca;

/// <summary>
/// Alpaca Markets REST API client implementation
/// Provides comprehensive access to stock, crypto, and options data with IV support
/// </summary>
public class AlpacaApiClient : IAlpacaApiClient
{
    private readonly HttpClient _httpClient;
    private readonly HttpClient _dataHttpClient;
    private readonly AlpacaApiConfiguration _config;
    private readonly ILogger<AlpacaApiClient> _logger;
    private readonly SemaphoreSlim _rateLimitSemaphore;
    private readonly ConcurrentQueue<DateTime> _requestTimestamps;
    private readonly Timer _rateLimitTimer;

    private int _rateLimitLimit = 200;
    private int _rateLimitRemaining = 200;
    private DateTime _rateLimitResetTime = DateTime.UtcNow.AddMinutes(1);

    private bool _disposed;

    public AlpacaApiClient(
        AlpacaApiConfiguration config,
        ILogger<AlpacaApiClient> logger,
        IHttpClientFactory? httpClientFactory = null)
    {
        _config = config ?? throw new ArgumentNullException(nameof(config));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        ValidateConfiguration();

        // Initialize HTTP clients
        if (httpClientFactory != null)
        {
            _httpClient = httpClientFactory.CreateClient("AlpacaApi");
            _dataHttpClient = httpClientFactory.CreateClient("AlpacaDataApi");
        }
        else
        {
            _httpClient = new HttpClient();
            _dataHttpClient = new HttpClient();
        }

        ConfigureHttpClient(_httpClient, _config.BaseUrl);
        ConfigureHttpClient(_dataHttpClient, _config.DataBaseUrl);

        // Initialize rate limiting
        _rateLimitSemaphore = new SemaphoreSlim(1, 1);
        _requestTimestamps = new ConcurrentQueue<DateTime>();
        _rateLimitTimer = new Timer(CleanupOldTimestamps, null, TimeSpan.Zero, TimeSpan.FromSeconds(10));

        _logger.LogInformation("AlpacaApiClient initialized with base URL: {BaseUrl}", _config.BaseUrl);
    }

    #region Configuration and Setup

    private void ValidateConfiguration()
    {
        if (string.IsNullOrWhiteSpace(_config.ApiKeyId))
            throw new ArgumentException("API Key ID is required", nameof(_config.ApiKeyId));

        if (string.IsNullOrWhiteSpace(_config.ApiSecretKey))
            throw new ArgumentException("API Secret Key is required", nameof(_config.ApiSecretKey));
    }

    private void ConfigureHttpClient(HttpClient client, string baseUrl)
    {
        client.BaseAddress = new Uri(baseUrl);
        client.Timeout = TimeSpan.FromSeconds(_config.TimeoutSeconds);
        client.DefaultRequestHeaders.Clear();
        client.DefaultRequestHeaders.Add("APCA-API-KEY-ID", _config.ApiKeyId);
        client.DefaultRequestHeaders.Add("APCA-API-SECRET-KEY", _config.ApiSecretKey);
        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
    }

    #endregion

    #region Stock Data Methods

    public async Task<StockQuote> GetStockQuoteAsync(
        string symbol,
        Feed feed = Feed.SIP,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting stock quote for {Symbol}", symbol);

        var feedParam = feed.ToString().ToLower();
        var endpoint = $"/v2/stocks/{symbol}/quotes/latest?feed={feedParam}";

        var response = await SendRequestAsync<Dictionary<string, StockQuote>>(
            _dataHttpClient,
            endpoint,
            cancellationToken);

        if (response.TryGetValue("quote", out var quote))
        {
            quote.Symbol = symbol;
            return quote;
        }

        throw new InvalidOperationException($"No quote data found for symbol {symbol}");
    }

    public async Task<StockBarsResponse> GetStockBarsAsync(
        string symbol,
        Timeframe timeframe,
        DateTime start,
        DateTime end,
        int limit = 1000,
        Feed feed = Feed.SIP,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting stock bars for {Symbol} from {Start} to {End}", symbol, start, end);

        var timeframeStr = ConvertTimeframeToString(timeframe);
        var feedParam = feed.ToString().ToLower();
        var startStr = start.ToString("yyyy-MM-ddTHH:mm:ssZ");
        var endStr = end.ToString("yyyy-MM-ddTHH:mm:ssZ");

        var endpoint = $"/v2/stocks/{symbol}/bars?timeframe={timeframeStr}&start={startStr}&end={endStr}&limit={limit}&feed={feedParam}";

        var response = await SendRequestAsync<StockBarsResponse>(_dataHttpClient, endpoint, cancellationToken);
        response.Symbol = symbol;
        return response;
    }

    public async Task<StockSnapshot> GetStockSnapshotAsync(
        string symbol,
        Feed feed = Feed.SIP,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting stock snapshot for {Symbol}", symbol);

        var feedParam = feed.ToString().ToLower();
        var endpoint = $"/v2/stocks/{symbol}/snapshot?feed={feedParam}";

        var response = await SendRequestAsync<Dictionary<string, StockSnapshot>>(
            _dataHttpClient,
            endpoint,
            cancellationToken);

        if (response.TryGetValue("snapshot", out var snapshot))
        {
            snapshot.Symbol = symbol;
            return snapshot;
        }

        throw new InvalidOperationException($"No snapshot data found for symbol {symbol}");
    }

    public async Task<StockTrade> GetStockLatestTradeAsync(
        string symbol,
        Feed feed = Feed.SIP,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting latest stock trade for {Symbol}", symbol);

        var feedParam = feed.ToString().ToLower();
        var endpoint = $"/v2/stocks/{symbol}/trades/latest?feed={feedParam}";

        var response = await SendRequestAsync<Dictionary<string, StockTrade>>(
            _dataHttpClient,
            endpoint,
            cancellationToken);

        if (response.TryGetValue("trade", out var trade))
        {
            return trade;
        }

        throw new InvalidOperationException($"No trade data found for symbol {symbol}");
    }

    #endregion

    #region Crypto Data Methods

    public async Task<CryptoQuote> GetCryptoQuoteAsync(
        string symbol,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting crypto quote for {Symbol}", symbol);

        var endpoint = $"/v1beta3/crypto/us/latest/quotes?symbols={symbol}";

        var response = await SendRequestAsync<Dictionary<string, Dictionary<string, CryptoQuote>>>(
            _dataHttpClient,
            endpoint,
            cancellationToken);

        if (response.TryGetValue("quotes", out var quotes) && quotes.TryGetValue(symbol, out var quote))
        {
            quote.Symbol = symbol;
            return quote;
        }

        throw new InvalidOperationException($"No crypto quote found for symbol {symbol}");
    }

    public async Task<CryptoBarsResponse> GetCryptoBarsAsync(
        string symbol,
        Timeframe timeframe,
        DateTime start,
        DateTime end,
        int limit = 1000,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting crypto bars for {Symbol} from {Start} to {End}", symbol, start, end);

        var timeframeStr = ConvertTimeframeToString(timeframe);
        var startStr = start.ToString("yyyy-MM-ddTHH:mm:ssZ");
        var endStr = end.ToString("yyyy-MM-ddTHH:mm:ssZ");

        var endpoint = $"/v1beta3/crypto/us/bars?symbols={symbol}&timeframe={timeframeStr}&start={startStr}&end={endStr}&limit={limit}";

        var response = await SendRequestAsync<Dictionary<string, List<CryptoBar>>>(
            _dataHttpClient,
            endpoint,
            cancellationToken);

        if (response.TryGetValue(symbol, out var bars))
        {
            return new CryptoBarsResponse
            {
                Symbol = symbol,
                Bars = bars
            };
        }

        throw new InvalidOperationException($"No crypto bars found for symbol {symbol}");
    }

    public async Task<CryptoSnapshot> GetCryptoSnapshotAsync(
        string symbol,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting crypto snapshot for {Symbol}", symbol);

        var endpoint = $"/v1beta3/crypto/us/snapshots?symbols={symbol}";

        var response = await SendRequestAsync<Dictionary<string, Dictionary<string, CryptoSnapshot>>>(
            _dataHttpClient,
            endpoint,
            cancellationToken);

        if (response.TryGetValue("snapshots", out var snapshots) && snapshots.TryGetValue(symbol, out var snapshot))
        {
            snapshot.Symbol = symbol;
            return snapshot;
        }

        throw new InvalidOperationException($"No crypto snapshot found for symbol {symbol}");
    }

    public async Task<CryptoTrade> GetCryptoLatestTradeAsync(
        string symbol,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting latest crypto trade for {Symbol}", symbol);

        var endpoint = $"/v1beta3/crypto/us/latest/trades?symbols={symbol}";

        var response = await SendRequestAsync<Dictionary<string, Dictionary<string, CryptoTrade>>>(
            _dataHttpClient,
            endpoint,
            cancellationToken);

        if (response.TryGetValue("trades", out var trades) && trades.TryGetValue(symbol, out var trade))
        {
            return trade;
        }

        throw new InvalidOperationException($"No crypto trade found for symbol {symbol}");
    }

    #endregion

    #region Options Data Methods

    public async Task<OptionsChainResponse> GetOptionsChainAsync(
        string underlyingSymbol,
        DateTime? expirationDate = null,
        decimal? strikePrice = null,
        OptionType? optionType = null,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting options chain for {UnderlyingSymbol}", underlyingSymbol);

        var queryParams = new List<string>
        {
            $"underlying_symbols={underlyingSymbol}"
        };

        if (expirationDate.HasValue)
        {
            queryParams.Add($"expiration_date={expirationDate.Value:yyyy-MM-dd}");
        }

        if (strikePrice.HasValue)
        {
            queryParams.Add($"strike_price={strikePrice.Value}");
        }

        if (optionType.HasValue)
        {
            queryParams.Add($"type={optionType.Value.ToString().ToLower()}");
        }

        var endpoint = $"/v1beta1/options/contracts?{string.Join("&", queryParams)}";

        var response = await SendRequestAsync<OptionsChainResponse>(_dataHttpClient, endpoint, cancellationToken);
        return response;
    }

    public async Task<OptionContract> GetOptionContractAsync(
        string optionSymbol,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting option contract for {OptionSymbol}", optionSymbol);

        var endpoint = $"/v1beta1/options/contracts/{optionSymbol}";

        var contract = await SendRequestAsync<OptionContract>(_dataHttpClient, endpoint, cancellationToken);
        return contract;
    }

    public async Task<OptionQuote> GetOptionQuoteAsync(
        string optionSymbol,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting option quote for {OptionSymbol}", optionSymbol);

        var endpoint = $"/v1beta1/options/quotes/latest?symbols={optionSymbol}";

        var response = await SendRequestAsync<Dictionary<string, Dictionary<string, OptionQuote>>>(
            _dataHttpClient,
            endpoint,
            cancellationToken);

        if (response.TryGetValue("quotes", out var quotes) && quotes.TryGetValue(optionSymbol, out var quote))
        {
            quote.Symbol = optionSymbol;
            return quote;
        }

        throw new InvalidOperationException($"No option quote found for symbol {optionSymbol}");
    }

    public async Task<OptionSnapshot> GetOptionSnapshotAsync(
        string optionSymbol,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting option snapshot for {OptionSymbol}", optionSymbol);

        var endpoint = $"/v1beta1/options/snapshots/{optionSymbol}";

        var response = await SendRequestAsync<Dictionary<string, OptionSnapshot>>(
            _dataHttpClient,
            endpoint,
            cancellationToken);

        if (response.TryGetValue("snapshot", out var snapshot))
        {
            snapshot.Symbol = optionSymbol;
            return snapshot;
        }

        throw new InvalidOperationException($"No option snapshot found for symbol {optionSymbol}");
    }

    #endregion

    #region Implied Volatility Methods (PRIMARY FOCUS)

    /// <summary>
    /// CRITICAL METHOD: Retrieves Implied Volatility for a specific option
    ///
    /// How Alpaca provides IV:
    /// 1. IV is included in the option snapshot endpoint
    /// 2. It's calculated based on the Black-Scholes model
    /// 3. Greeks are also provided in the same response
    ///
    /// The IV value represents annualized volatility as a decimal (e.g., 0.25 = 25%)
    /// </summary>
    public async Task<ImpliedVolatilityData> GetImpliedVolatilityAsync(
        string optionSymbol,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting implied volatility for {OptionSymbol}", optionSymbol);

        // Step 1: Get the option snapshot which includes IV and Greeks
        var snapshot = await GetOptionSnapshotAsync(optionSymbol, cancellationToken);

        // Step 2: Get the option contract details for strike, expiration, etc.
        var contract = await GetOptionContractAsync(optionSymbol, cancellationToken);

        // Step 3: Get the underlying stock price
        var underlyingQuote = await GetStockQuoteAsync(contract.UnderlyingSymbol, Feed.SIP, cancellationToken);

        // Step 4: Calculate days to expiration
        var daysToExpiration = (contract.ExpirationDate - DateTime.UtcNow).Days;

        // Step 5: Build the comprehensive IV data model
        var ivData = new ImpliedVolatilityData
        {
            Symbol = optionSymbol,
            UnderlyingSymbol = contract.UnderlyingSymbol,
            ImpliedVolatility = snapshot.ImpliedVolatility ?? 0m,
            Timestamp = DateTime.UtcNow,
            StrikePrice = contract.StrikePrice,
            ExpirationDate = contract.ExpirationDate,
            OptionType = contract.Type,
            OptionPrice = snapshot.LatestQuote?.MidPrice,
            UnderlyingPrice = underlyingQuote.MidPrice,
            DaysToExpiration = daysToExpiration,
            Greeks = snapshot.Greeks,
            OpenInterest = contract.OpenInterest,
            Volume = snapshot.LatestTrade != null ? snapshot.LatestTrade.Size : null
        };

        _logger.LogInformation(
            "Retrieved IV for {Symbol}: {IV:P2} (Strike: {Strike}, Expiration: {Expiration}, Underlying: {UnderlyingPrice:C2})",
            optionSymbol,
            ivData.ImpliedVolatility,
            ivData.StrikePrice,
            ivData.ExpirationDate.ToString("yyyy-MM-dd"),
            ivData.UnderlyingPrice);

        return ivData;
    }

    /// <summary>
    /// Get IV for an entire options chain
    /// This is useful for building volatility surfaces and analyzing volatility skew
    /// </summary>
    public async Task<ImpliedVolatilityChainResponse> GetImpliedVolatilityChainAsync(
        string underlyingSymbol,
        DateTime? expirationDate = null,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting IV chain for {UnderlyingSymbol}", underlyingSymbol);

        // Step 1: Get the options chain
        var optionsChain = await GetOptionsChainAsync(
            underlyingSymbol,
            expirationDate,
            null,
            null,
            cancellationToken);

        // Step 2: Get underlying price once (for efficiency)
        var underlyingQuote = await GetStockQuoteAsync(underlyingSymbol, Feed.SIP, cancellationToken);

        // Step 3: Get IV for each option in parallel (with rate limiting)
        var ivDataList = new List<ImpliedVolatilityData>();
        var semaphore = new SemaphoreSlim(10, 10); // Limit concurrent requests to 10

        var tasks = optionsChain.Options.Select(async contract =>
        {
            await semaphore.WaitAsync(cancellationToken);
            try
            {
                var ivData = await GetImpliedVolatilityAsync(contract.Symbol, cancellationToken);
                return ivData;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to get IV for {Symbol}", contract.Symbol);
                return null;
            }
            finally
            {
                semaphore.Release();
            }
        });

        var results = await Task.WhenAll(tasks);
        ivDataList.AddRange(results.Where(r => r != null)!);

        // Step 4: Calculate average IV
        var averageIV = ivDataList.Any() ? ivDataList.Average(iv => iv.ImpliedVolatility) : 0m;

        var response = new ImpliedVolatilityChainResponse
        {
            UnderlyingSymbol = underlyingSymbol,
            Options = ivDataList,
            AverageImpliedVolatility = averageIV,
            Timestamp = DateTime.UtcNow,
            UnderlyingPrice = underlyingQuote.MidPrice
        };

        _logger.LogInformation(
            "Retrieved IV chain for {Symbol}: {Count} options, Average IV: {AverageIV:P2}",
            underlyingSymbol,
            ivDataList.Count,
            averageIV);

        return response;
    }

    /// <summary>
    /// Batch retrieval of IV for multiple option symbols
    /// More efficient than calling GetImpliedVolatilityAsync multiple times
    /// </summary>
    public async Task<Dictionary<string, ImpliedVolatilityData>> GetImpliedVolatilityBatchAsync(
        IEnumerable<string> optionSymbols,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting IV for {Count} option symbols", optionSymbols.Count());

        var result = new Dictionary<string, ImpliedVolatilityData>();
        var semaphore = new SemaphoreSlim(10, 10); // Limit concurrent requests

        var tasks = optionSymbols.Select(async symbol =>
        {
            await semaphore.WaitAsync(cancellationToken);
            try
            {
                var ivData = await GetImpliedVolatilityAsync(symbol, cancellationToken);
                return (symbol, ivData);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to get IV for {Symbol}", symbol);
                return (symbol, null);
            }
            finally
            {
                semaphore.Release();
            }
        });

        var results = await Task.WhenAll(tasks);

        foreach (var (symbol, ivData) in results)
        {
            if (ivData != null)
            {
                result[symbol] = ivData;
            }
        }

        return result;
    }

    #endregion

    #region HTTP Request Handling with Retry and Rate Limiting

    private async Task<T> SendRequestAsync<T>(
        HttpClient client,
        string endpoint,
        CancellationToken cancellationToken)
    {
        var attempt = 0;
        var delay = _config.InitialRetryDelayMs;

        while (attempt <= _config.MaxRetryAttempts)
        {
            try
            {
                // Rate limiting
                await WaitForRateLimitAsync(cancellationToken);

                // Send request
                var response = await client.GetAsync(endpoint, cancellationToken);

                // Update rate limit info from headers
                UpdateRateLimitInfo(response.Headers);

                // Handle response
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync(cancellationToken);
                    var result = JsonSerializer.Deserialize<T>(content, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    if (result == null)
                    {
                        throw new InvalidOperationException("Failed to deserialize response");
                    }

                    return result;
                }

                // Handle specific error codes
                if (response.StatusCode == HttpStatusCode.TooManyRequests)
                {
                    _logger.LogWarning("Rate limit exceeded, waiting before retry...");
                    await Task.Delay(TimeSpan.FromSeconds(60), cancellationToken);
                    attempt++;
                    continue;
                }

                if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    throw new UnauthorizedAccessException("Invalid API credentials");
                }

                // Read error response
                var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogError("API request failed: {StatusCode} - {Error}", response.StatusCode, errorContent);

                // Retry on server errors (5xx)
                if ((int)response.StatusCode >= 500)
                {
                    attempt++;
                    if (attempt <= _config.MaxRetryAttempts)
                    {
                        _logger.LogWarning("Retrying request (attempt {Attempt}/{MaxAttempts}) after {Delay}ms",
                            attempt, _config.MaxRetryAttempts, delay);
                        await Task.Delay(delay, cancellationToken);
                        delay *= 2; // Exponential backoff
                        continue;
                    }
                }

                throw new HttpRequestException($"API request failed: {response.StatusCode} - {errorContent}");
            }
            catch (Exception ex) when (ex is not UnauthorizedAccessException && attempt < _config.MaxRetryAttempts)
            {
                attempt++;
                _logger.LogWarning(ex, "Request failed, retrying (attempt {Attempt}/{MaxAttempts}) after {Delay}ms",
                    attempt, _config.MaxRetryAttempts, delay);
                await Task.Delay(delay, cancellationToken);
                delay *= 2; // Exponential backoff
            }
        }

        throw new InvalidOperationException($"Request failed after {_config.MaxRetryAttempts} attempts");
    }

    private async Task WaitForRateLimitAsync(CancellationToken cancellationToken)
    {
        await _rateLimitSemaphore.WaitAsync(cancellationToken);
        try
        {
            // Clean up old timestamps
            var oneMinuteAgo = DateTime.UtcNow.AddMinutes(-1);
            while (_requestTimestamps.TryPeek(out var timestamp) && timestamp < oneMinuteAgo)
            {
                _requestTimestamps.TryDequeue(out _);
            }

            // Check if we've hit the rate limit
            if (_requestTimestamps.Count >= _config.MaxRequestsPerMinute)
            {
                var oldestTimestamp = _requestTimestamps.TryPeek(out var ts) ? ts : DateTime.UtcNow;
                var waitTime = oldestTimestamp.AddMinutes(1) - DateTime.UtcNow;

                if (waitTime > TimeSpan.Zero)
                {
                    _logger.LogWarning("Rate limit reached, waiting {WaitTime} seconds", waitTime.TotalSeconds);
                    await Task.Delay(waitTime, cancellationToken);
                }
            }

            // Add current request timestamp
            _requestTimestamps.Enqueue(DateTime.UtcNow);
        }
        finally
        {
            _rateLimitSemaphore.Release();
        }
    }

    private void UpdateRateLimitInfo(HttpResponseHeaders headers)
    {
        if (headers.TryGetValues("X-RateLimit-Limit", out var limitValues))
        {
            if (int.TryParse(limitValues.FirstOrDefault(), out var limit))
            {
                _rateLimitLimit = limit;
            }
        }

        if (headers.TryGetValues("X-RateLimit-Remaining", out var remainingValues))
        {
            if (int.TryParse(remainingValues.FirstOrDefault(), out var remaining))
            {
                _rateLimitRemaining = remaining;
            }
        }

        if (headers.TryGetValues("X-RateLimit-Reset", out var resetValues))
        {
            if (long.TryParse(resetValues.FirstOrDefault(), out var resetEpoch))
            {
                _rateLimitResetTime = DateTimeOffset.FromUnixTimeSeconds(resetEpoch).UtcDateTime;
            }
        }
    }

    private void CleanupOldTimestamps(object? state)
    {
        var oneMinuteAgo = DateTime.UtcNow.AddMinutes(-1);
        while (_requestTimestamps.TryPeek(out var timestamp) && timestamp < oneMinuteAgo)
        {
            _requestTimestamps.TryDequeue(out _);
        }
    }

    #endregion

    #region Utility Methods

    public RateLimitInfo GetRateLimitInfo()
    {
        return new RateLimitInfo
        {
            Limit = _rateLimitLimit,
            Remaining = _rateLimitRemaining,
            ResetTime = _rateLimitResetTime
        };
    }

    public async Task<bool> HealthCheckAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Performing health check");

            // Try to get a simple quote to verify authentication and connectivity
            var endpoint = "/v2/stocks/AAPL/quotes/latest?feed=iex";
            await SendRequestAsync<Dictionary<string, StockQuote>>(_dataHttpClient, endpoint, cancellationToken);

            _logger.LogInformation("Health check passed");
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Health check failed");
            return false;
        }
    }

    private string ConvertTimeframeToString(Timeframe timeframe)
    {
        return timeframe switch
        {
            Timeframe.OneMinute => "1Min",
            Timeframe.FiveMinutes => "5Min",
            Timeframe.FifteenMinutes => "15Min",
            Timeframe.ThirtyMinutes => "30Min",
            Timeframe.OneHour => "1Hour",
            Timeframe.FourHours => "4Hour",
            Timeframe.OneDay => "1Day",
            Timeframe.OneWeek => "1Week",
            Timeframe.OneMonth => "1Month",
            _ => "1Min"
        };
    }

    #endregion

    #region IDisposable

    public void Dispose()
    {
        if (_disposed)
            return;

        _rateLimitTimer?.Dispose();
        _rateLimitSemaphore?.Dispose();
        _httpClient?.Dispose();
        _dataHttpClient?.Dispose();

        _disposed = true;
        GC.SuppressFinalize(this);
    }

    #endregion
}
