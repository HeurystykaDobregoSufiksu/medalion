using Medalion.Data.Domain;
using Medalion.Data.DTOs;
using Medalion.Data.Repositories;
using Medalion.Services.Alpaca.Models;
using Medalion.Services.Polymarket.Models;
using Microsoft.Extensions.Logging;

namespace Medalion.Data.Services;

/// <summary>
/// Service implementation for market data operations
/// Connects external market data sources (Alpaca, Polymarket) to database via repositories
/// </summary>
public class MarketDataService : IMarketDataService
{
    private readonly IAssetRepository _assetRepository;
    private readonly IStockBarDataRepository _stockBarRepository;
    private readonly IStockQuoteSnapshotRepository _stockQuoteRepository;
    private readonly ICryptoBarDataRepository _cryptoBarRepository;
    private readonly IPolymarketEventRepository _polymarketEventRepository;
    private readonly IPolymarketSnapshotRepository _polymarketSnapshotRepository;
    private readonly IRepository<CryptoQuoteSnapshot> _cryptoQuoteRepository;
    private readonly IRepository<OptionContractData> _optionContractRepository;
    private readonly IRepository<OptionQuoteSnapshot> _optionQuoteRepository;
    private readonly IRepository<PolymarketMarketData> _polymarketMarketRepository;
    private readonly IRepository<PolymarketTradeData> _polymarketTradeRepository;
    private readonly IRepository<PolymarketOrderBookSnapshot> _polymarketOrderBookRepository;
    private readonly ILogger<MarketDataService> _logger;

    public MarketDataService(
        IAssetRepository assetRepository,
        IStockBarDataRepository stockBarRepository,
        IStockQuoteSnapshotRepository stockQuoteRepository,
        ICryptoBarDataRepository cryptoBarRepository,
        IPolymarketEventRepository polymarketEventRepository,
        IPolymarketSnapshotRepository polymarketSnapshotRepository,
        IRepository<CryptoQuoteSnapshot> cryptoQuoteRepository,
        IRepository<OptionContractData> optionContractRepository,
        IRepository<OptionQuoteSnapshot> optionQuoteRepository,
        IRepository<PolymarketMarketData> polymarketMarketRepository,
        IRepository<PolymarketTradeData> polymarketTradeRepository,
        IRepository<PolymarketOrderBookSnapshot> polymarketOrderBookRepository,
        ILogger<MarketDataService> logger)
    {
        _assetRepository = assetRepository;
        _stockBarRepository = stockBarRepository;
        _stockQuoteRepository = stockQuoteRepository;
        _cryptoBarRepository = cryptoBarRepository;
        _polymarketEventRepository = polymarketEventRepository;
        _polymarketSnapshotRepository = polymarketSnapshotRepository;
        _cryptoQuoteRepository = cryptoQuoteRepository;
        _optionContractRepository = optionContractRepository;
        _optionQuoteRepository = optionQuoteRepository;
        _polymarketMarketRepository = polymarketMarketRepository;
        _polymarketTradeRepository = polymarketTradeRepository;
        _polymarketOrderBookRepository = polymarketOrderBookRepository;
        _logger = logger;
    }

    #region Alpaca Stock Data

    public async Task<Guid> StoreStockQuoteAsync(
        Guid assetId,
        StockQuote quote,
        CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Storing stock quote for asset {AssetId}", assetId);

        var snapshot = new StockQuoteSnapshot
        {
            AssetId = assetId,
            QuoteTimestamp = quote.Timestamp,
            BidPrice = quote.BidPrice,
            BidSize = quote.BidSize,
            AskPrice = quote.AskPrice,
            AskSize = quote.AskSize,
        
        };

        var result = await _stockQuoteRepository.AddAsync(snapshot);
        return result.Id;
    }

    public async Task<Guid> StoreStockBarAsync(
        Guid assetId,
        StockBar bar,
        string timeframe,
        CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Storing stock bar for asset {AssetId}, timeframe {Timeframe}", assetId, timeframe);

        var barData = new StockBarData
        {
            AssetId = assetId,
            Timeframe = timeframe,
            BarTimestamp = bar.Timestamp,
            OpenPrice = bar.Open,
            HighPrice = bar.High,
            LowPrice = bar.Low,
            ClosePrice = bar.Close,
            Volume = bar.Volume,
            TradeCount = bar.TradeCount ?? 0,
            VWAP = bar.VWAP
        };

        var result = await _stockBarRepository.AddAsync(barData);
        return result.Id;
    }

    public async Task<int> StoreStockBarsAsync(
        Guid assetId,
        IEnumerable<StockBar> bars,
        string timeframe,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Storing {Count} stock bars for asset {AssetId}", bars.Count(), assetId);

        var count = 0;
        foreach (var bar in bars)
        {
            await StoreStockBarAsync(assetId, bar, timeframe, cancellationToken);
            count++;
        }

        return count;
    }

    public async Task<StockQuote?> GetLatestStockQuoteAsync(
        Guid assetId,
        CancellationToken cancellationToken = default)
    {
        var snapshot = await _stockQuoteRepository.GetLatestQuoteAsync(assetId, cancellationToken);
        if (snapshot == null) return null;

        return new StockQuote
        {
            Timestamp = snapshot.QuoteTimestamp,
            BidPrice = snapshot.BidPrice,
            BidSize = snapshot.BidSize,
            AskPrice = snapshot.AskPrice,
            AskSize = snapshot.AskSize,
            BidExchange = snapshot.BidExchange,
            AskExchange = snapshot.AskExchange,
            Conditions = snapshot.Conditions?.Split(',').ToList(),
            Tape = snapshot.Tape
        };
    }

    public async Task<IEnumerable<MarketBarDto>> GetStockBarsAsync(
        Guid assetId,
        string timeframe,
        DateTime startDate,
        DateTime endDate,
        CancellationToken cancellationToken = default)
    {
        var bars = await _stockBarRepository.GetBarsByAssetAndTimeframeAsync(
            assetId, timeframe, startDate, endDate, cancellationToken);

        return bars.Select(b => new MarketBarDto
        {
            Timestamp = b.BarTimestamp,
            Open = b.OpenPrice,
            High = b.HighPrice,
            Low = b.LowPrice,
            Close = b.ClosePrice,
            Volume = b.Volume,
            VWAP = b.VWAP
        });
    }

    #endregion

    #region Alpaca Crypto Data

    public async Task<Guid> StoreCryptoQuoteAsync(
        Guid assetId,
        CryptoQuote quote,
        CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Storing crypto quote for asset {AssetId}", assetId);

        var snapshot = new CryptoQuoteSnapshot
        {
            AssetId = assetId,
            QuoteTimestamp = quote.Timestamp,
            BidPrice = quote.BidPrice,
            BidSize = quote.BidSize,
            AskPrice = quote.AskPrice,
            AskSize = quote.AskSize,
            Exchange = quote.Exchange ?? string.Empty
        };

        var result = await _cryptoQuoteRepository.AddAsync(snapshot);
        return result.Id;
    }

    public async Task<Guid> StoreCryptoBarAsync(
        Guid assetId,
        CryptoBar bar,
        string timeframe,
        CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Storing crypto bar for asset {AssetId}, timeframe {Timeframe}", assetId, timeframe);

        var barData = new CryptoBarData
        {
            AssetId = assetId,
            Timeframe = timeframe,
            BarTimestamp = bar.Timestamp,
            OpenPrice = bar.Open,
            HighPrice = bar.High,
            LowPrice = bar.Low,
            ClosePrice = bar.Close,
            Volume = bar.Volume,
            TradeCount = bar.TradeCount ?? 0,
            VWAP = bar.VWAP
        };

        var result = await _cryptoBarRepository.AddAsync(barData);
        return result.Id;
    }

    public async Task<int> StoreCryptoBarsAsync(
        Guid assetId,
        IEnumerable<CryptoBar> bars,
        string timeframe,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Storing {Count} crypto bars for asset {AssetId}", bars.Count(), assetId);

        var count = 0;
        foreach (var bar in bars)
        {
            await StoreCryptoBarAsync(assetId, bar, timeframe, cancellationToken);
            count++;
        }

        return count;
    }

    public async Task<CryptoQuote?> GetLatestCryptoQuoteAsync(
        Guid assetId,
        CancellationToken cancellationToken = default)
    {
        var snapshots = await _cryptoQuoteRepository.FindAsync(s => s.AssetId == assetId);
        var latest = snapshots.OrderByDescending(s => s.QuoteTimestamp).FirstOrDefault();

        if (latest == null) return null;

        return new CryptoQuote
        {
            Timestamp = latest.QuoteTimestamp,
            BidPrice = latest.BidPrice,
            BidSize = latest.BidSize,
            AskPrice = latest.AskPrice,
            AskSize = latest.AskSize,
            Exchange = latest.Exchange
        };
    }

    public async Task<IEnumerable<MarketBarDto>> GetCryptoBarsAsync(
        Guid assetId,
        string timeframe,
        DateTime startDate,
        DateTime endDate,
        CancellationToken cancellationToken = default)
    {
        var bars = await _cryptoBarRepository.GetBarsByAssetAndTimeframeAsync(
            assetId, timeframe, startDate, endDate, cancellationToken);

        return bars.Select(b => new MarketBarDto
        {
            Timestamp = b.BarTimestamp,
            Open = b.OpenPrice,
            High = b.HighPrice,
            Low = b.LowPrice,
            Close = b.ClosePrice,
            Volume = b.Volume,
            VWAP = b.VWAP
        });
    }

    #endregion

    #region Alpaca Options Data

    public async Task<Guid> StoreOptionContractAsync(
        Guid underlyingAssetId,
        OptionContract contract,
        CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Storing option contract {Symbol}", contract.Symbol);

        // Check if contract already exists
        var existing = (await _optionContractRepository.FindAsync(
            c => c.OptionSymbol == contract.Symbol)).FirstOrDefault();

        if (existing != null)
        {
            // Update existing contract
            existing.StrikePrice = contract.StrikePrice;
            existing.ExpirationDate = contract.ExpirationDate;
            existing.ContractType = contract.Type == OptionType.Call ? "Call" : "Put";
            existing.OpenInterest = contract.OpenInterest ?? 0;
            existing.ContractSize = contract.Size ?? 100;

            await _optionContractRepository.UpdateAsync(existing);
            return existing.Id;
        }

        // Create new contract
        var contractData = new OptionContractData
        {
            UnderlyingAssetId = underlyingAssetId,
            OptionSymbol = contract.Symbol,
            StrikePrice = contract.StrikePrice,
            ExpirationDate = contract.ExpirationDate,
            ContractType = contract.Type == OptionType.Call ? "Call" : "Put",
            OpenInterest = contract.OpenInterest ?? 0,
            ContractSize = contract.Size ?? 100
        };

        var result = await _optionContractRepository.AddAsync(contractData);
        return result.Id;
    }

    public async Task<Guid> StoreOptionQuoteAsync(
        Guid optionContractId,
        OptionQuote quote,
        decimal? impliedVolatility = null,
        OptionGreeks? greeks = null,
        CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Storing option quote for contract {ContractId}", optionContractId);

        var quoteSnapshot = new OptionQuoteSnapshot
        {
            OptionContractId = optionContractId,
            QuoteTimestamp = quote.Timestamp,
            BidPrice = quote.BidPrice ?? 0,
            BidSize = quote.BidSize ?? 0,
            AskPrice = quote.AskPrice ?? 0,
            AskSize = quote.AskSize ?? 0,
            ImpliedVolatility = impliedVolatility,
            Delta = greeks?.Delta,
            Gamma = greeks?.Gamma,
            Theta = greeks?.Theta,
            Vega = greeks?.Vega,
            Rho = greeks?.Rho
        };

        var result = await _optionQuoteRepository.AddAsync(quoteSnapshot);
        return result.Id;
    }

    #endregion

    #region Polymarket Data

    public async Task<Guid> StorePolymarketEventAsync(
        PolymarketEvent polymarketEvent,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Storing Polymarket event {EventId} - {Title}",
            polymarketEvent.Id, polymarketEvent.Title);

        // Check if event already exists
        var existing = await _polymarketEventRepository.GetByEventIdAsync(
            polymarketEvent.Id, cancellationToken);

        if (existing != null)
        {
            // Update existing event
            existing.Title = polymarketEvent.Title;
            existing.Description = polymarketEvent.Description ?? string.Empty;
            existing.Category = polymarketEvent.Category ?? string.Empty;
            existing.IsActive = polymarketEvent.Active;
            existing.IsClosed = polymarketEvent.Closed;
            existing.Tags = string.Join(",", polymarketEvent.Tags ?? new List<string>());

            await _polymarketEventRepository.UpdateAsync(existing);

            // Store/update markets
            foreach (var market in polymarketEvent.Markets)
            {
                await StorePolymarketMarketAsync(existing.Id, market, cancellationToken);
            }

            return existing.Id;
        }

        // Create new event
        var eventData = new PolymarketEventData
        {
            EventId = polymarketEvent.Id,
            Slug = polymarketEvent.Slug ?? string.Empty,
            Title = polymarketEvent.Title,
            Description = polymarketEvent.Description ?? string.Empty,
            Category = polymarketEvent.Category ?? string.Empty,
            IsActive = polymarketEvent.Active,
            IsClosed = polymarketEvent.Closed,
            IsMonitored = true,
            EventCreatedAt = polymarketEvent.CreationTime ?? DateTime.UtcNow,
            Tags = string.Join(",", polymarketEvent.Tags ?? new List<string>())
        };

        var result = await _polymarketEventRepository.AddAsync(eventData);

        // Store markets
        foreach (var market in polymarketEvent.Markets)
        {
            await StorePolymarketMarketAsync(result.Id, market, cancellationToken);
        }

        return result.Id;
    }

    private async Task<Guid> StorePolymarketMarketAsync(
        Guid eventId,
        Market market,
        CancellationToken cancellationToken)
    {
        // Check if market already exists
        var existing = (await _polymarketMarketRepository.FindAsync(
            m => m.MarketId == market.Id)).FirstOrDefault();

        if (existing != null)
        {
            // Update existing market
            existing.Question = market.Question ?? string.Empty;
            existing.Volume = market.Volume;
            existing.Liquidity = market.Liquidity;
            existing.IsActive = market.Active;

            await _polymarketMarketRepository.UpdateAsync(existing);
            return existing.Id;
        }

        // Create new market
        var marketData = new PolymarketMarketData
        {
            PolymarketEventId = eventId,
            MarketId = market.Id,
            Question = market.Question ?? string.Empty,
            Outcomes = string.Join(",", market.Outcomes),
            Volume = market.Volume,
            Liquidity = market.Liquidity,
            IsActive = market.Active
        };

        var result = await _polymarketMarketRepository.AddAsync(marketData);
        return result.Id;
    }

    public async Task<Guid> StorePolymarketSnapshotAsync(
        Guid marketId,
        MarketDataUpdate snapshot,
        CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Storing Polymarket snapshot for market {MarketId}", marketId);

        // Find the market data entity by MarketId string
        var market = (await _polymarketMarketRepository.FindAsync(
            m => m.MarketId == snapshot.MarketId)).FirstOrDefault();

        if (market == null)
        {
            throw new InvalidOperationException($"Market {snapshot.MarketId} not found in database");
        }

        var snapshotData = new PolymarketSnapshot
        {
            PolymarketMarketId = market.Id,
            SnapshotTimestamp = snapshot.Timestamp,
            Price = snapshot.Price,
            ImpliedVolatility = snapshot.ImpliedVolatility,
            Volume24h = snapshot.Volume24h,
            Liquidity = snapshot.Liquidity
        };

        var result = await _polymarketSnapshotRepository.AddAsync(snapshotData);
        return result.Id;
    }

    public async Task<Guid> StorePolymarketTradeAsync(
        TradeUpdate trade,
        CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Storing Polymarket trade for market {MarketId}", trade.MarketId);

        // Find the market
        var market = (await _polymarketMarketRepository.FindAsync(
            m => m.MarketId == trade.MarketId)).FirstOrDefault();

        if (market == null)
        {
            throw new InvalidOperationException($"Market {trade.MarketId} not found in database");
        }

        var tradeData = new PolymarketTradeData
        {
            PolymarketMarketId = market.Id,
            TradeId = trade.TradeId ?? Guid.NewGuid().ToString(),
            TradeTimestamp = trade.Timestamp,
            Price = trade.Price,
            Size = trade.Size,
            Side = trade.Side ?? string.Empty,
            Outcome = trade.Outcome ?? string.Empty
        };

        var result = await _polymarketTradeRepository.AddAsync(tradeData);
        return result.Id;
    }

    public async Task<Guid> StorePolymarketOrderBookAsync(
        OrderBookUpdate orderBook,
        CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Storing Polymarket order book for market {MarketId}", orderBook.MarketId);

        // Find the market
        var market = (await _polymarketMarketRepository.FindAsync(
            m => m.MarketId == orderBook.MarketId)).FirstOrDefault();

        if (market == null)
        {
            throw new InvalidOperationException($"Market {orderBook.MarketId} not found in database");
        }

        var orderBookData = new PolymarketOrderBookSnapshot
        {
            PolymarketMarketId = market.Id,
            SnapshotTimestamp = orderBook.Timestamp,
            BidsJson = System.Text.Json.JsonSerializer.Serialize(orderBook.Bids),
            AsksJson = System.Text.Json.JsonSerializer.Serialize(orderBook.Asks),
            Hash = orderBook.Hash ?? string.Empty
        };

        var result = await _polymarketOrderBookRepository.AddAsync(orderBookData);
        return result.Id;
    }

    public async Task<MarketDataUpdate?> GetLatestPolymarketSnapshotAsync(
        Guid marketId,
        CancellationToken cancellationToken = default)
    {
        // Find the market by Guid
        var market = await _polymarketMarketRepository.GetByIdAsync(marketId);
        if (market == null) return null;

        var snapshot = await _polymarketSnapshotRepository.GetLatestSnapshotAsync(
            market.Id, cancellationToken);

        if (snapshot == null) return null;

        return new MarketDataUpdate
        {
            MarketId = market.MarketId,
            Timestamp = snapshot.SnapshotTimestamp,
            Price = snapshot.Price,
            ImpliedVolatility = snapshot.ImpliedVolatility,
            Volume24h = snapshot.Volume24h,
            Liquidity = snapshot.Liquidity
        };
    }

    public async Task<IEnumerable<MarketDataUpdate>> GetPolymarketSnapshotsAsync(
        Guid marketId,
        DateTime startDate,
        DateTime endDate,
        CancellationToken cancellationToken = default)
    {
        // Find the market by Guid
        var market = await _polymarketMarketRepository.GetByIdAsync(marketId);
        if (market == null) return Enumerable.Empty<MarketDataUpdate>();

        var snapshots = await _polymarketSnapshotRepository.GetSnapshotsByDateRangeAsync(
            market.Id, startDate, endDate, cancellationToken);

        return snapshots.Select(s => new MarketDataUpdate
        {
            MarketId = market.MarketId,
            Timestamp = s.SnapshotTimestamp,
            Price = s.Price,
            ImpliedVolatility = s.ImpliedVolatility,
            Volume24h = s.Volume24h,
            Liquidity = s.Liquidity
        });
    }

    #endregion

    #region Asset Management

    public async Task<AssetDto> CreateOrUpdateAssetAsync(
        string symbol,
        string name,
        AssetType assetType,
        string assetClass,
        string exchange,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Creating/updating asset {Symbol}", symbol);

        var existing = await _assetRepository.GetBySymbolAsync(symbol, cancellationToken);

        if (existing != null)
        {
            // Update existing asset
            existing.Name = name;
            existing.AssetClass = assetClass;
            existing.Exchange = exchange;
            existing.IsActive = true;

            await _assetRepository.UpdateAsync(existing);

            return new AssetDto
            {
                Id = existing.Id,
                Symbol = existing.Symbol,
                Name = existing.Name,
                AssetType = existing.AssetType,
                AssetClass = existing.AssetClass,
                Exchange = existing.Exchange,
                IsActive = existing.IsActive,
                IsTradable = existing.IsTradable
            };
        }

        // Create new asset
        var asset = new Asset
        {
            Symbol = symbol,
            Name = name,
            AssetType = assetType,
            AssetClass = assetClass,
            Exchange = exchange,
            IsActive = true,
            IsTradable = true
        };

        var result = await _assetRepository.AddAsync(asset);

        return new AssetDto
        {
            Id = result.Id,
            Symbol = result.Symbol,
            Name = result.Name,
            AssetType = result.AssetType,
            AssetClass = result.AssetClass,
            Exchange = result.Exchange,
            IsActive = result.IsActive,
            IsTradable = result.IsTradable
        };
    }

    public async Task<AssetDto?> GetAssetBySymbolAsync(
        string symbol,
        CancellationToken cancellationToken = default)
    {
        var asset = await _assetRepository.GetBySymbolAsync(symbol, cancellationToken);
        if (asset == null) return null;

        return new AssetDto
        {
            Id = asset.Id,
            Symbol = asset.Symbol,
            Name = asset.Name,
            AssetType = asset.AssetType,
            AssetClass = asset.AssetClass,
            Exchange = asset.Exchange,
            IsActive = asset.IsActive,
            IsTradable = asset.IsTradable
        };
    }

    public async Task<IEnumerable<AssetDto>> GetActiveAssetsAsync(
        CancellationToken cancellationToken = default)
    {
        var assets = await _assetRepository.GetActiveAssetsAsync(cancellationToken);

        return assets.Select(a => new AssetDto
        {
            Id = a.Id,
            Symbol = a.Symbol,
            Name = a.Name,
            AssetType = a.AssetType,
            AssetClass = a.AssetClass,
            Exchange = a.Exchange,
            IsActive = a.IsActive,
            IsTradable = a.IsTradable
        });
    }

    #endregion
}
