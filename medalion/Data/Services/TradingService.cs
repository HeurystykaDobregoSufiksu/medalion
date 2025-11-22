using Medalion.Data.Domain;
using Medalion.Data.DTOs;
using Medalion.Data.Repositories;

namespace Medalion.Data.Services;

/// <summary>
/// Service implementation for trading operations
/// </summary>
public class TradingService : ITradingService
{
    private readonly ITradeRepository _tradeRepository;
    private readonly IPositionRepository _positionRepository;
    private readonly IAssetRepository _assetRepository;
    private readonly IPolymarketEventRepository _eventRepository;

    public TradingService(
        ITradeRepository tradeRepository,
        IPositionRepository positionRepository,
        IAssetRepository assetRepository,
        IPolymarketEventRepository eventRepository)
    {
        _tradeRepository = tradeRepository ?? throw new ArgumentNullException(nameof(tradeRepository));
        _positionRepository = positionRepository ?? throw new ArgumentNullException(nameof(positionRepository));
        _assetRepository = assetRepository ?? throw new ArgumentNullException(nameof(assetRepository));
        _eventRepository = eventRepository ?? throw new ArgumentNullException(nameof(eventRepository));
    }

    #region Trade Operations

    public async Task<TradeDto> ExecuteMarketOrderAsync(
        CreateTradeRequest request,
        CancellationToken cancellationToken = default)
    {
        var trade = new Trade
        {
            StrategyId = request.StrategyId,
            SignalId = request.SignalId,
            AssetId = request.AssetId,
            PolymarketEventId = request.PolymarketEventId,
            Symbol = request.Symbol,
            TradeType = TradeType.Market,
            TradeSide = request.TradeSide,
            Quantity = request.Quantity,
            Price = request.LimitPrice ?? 0, // In real implementation, get current market price
            Status = TradeStatus.Filled,
            ExecutedAt = DateTime.UtcNow,
            AlpacaMarketDataSnapshot = request.AlpacaMarketDataSnapshot,
            PolymarketDataSnapshot = request.PolymarketDataSnapshot,
            Notes = request.Notes
        };

        trade.TotalValue = trade.Quantity * trade.Price;
        trade.Commission = CalculateCommission(trade.TotalValue);

        var savedTrade = await _tradeRepository.AddAsync(trade, cancellationToken);

        return MapToTradeDto(savedTrade);
    }

    public async Task<TradeDto> ExecuteLimitOrderAsync(
        CreateTradeRequest request,
        CancellationToken cancellationToken = default)
    {
        if (!request.LimitPrice.HasValue)
            throw new ArgumentException("Limit price is required for limit orders");

        var trade = new Trade
        {
            StrategyId = request.StrategyId,
            SignalId = request.SignalId,
            AssetId = request.AssetId,
            PolymarketEventId = request.PolymarketEventId,
            Symbol = request.Symbol,
            TradeType = TradeType.Limit,
            TradeSide = request.TradeSide,
            Quantity = request.Quantity,
            Price = request.LimitPrice.Value,
            Status = TradeStatus.Pending,
            ExecutedAt = DateTime.UtcNow,
            AlpacaMarketDataSnapshot = request.AlpacaMarketDataSnapshot,
            PolymarketDataSnapshot = request.PolymarketDataSnapshot,
            Notes = request.Notes
        };

        trade.TotalValue = trade.Quantity * trade.Price;
        trade.Commission = CalculateCommission(trade.TotalValue);

        var savedTrade = await _tradeRepository.AddAsync(trade, cancellationToken);

        return MapToTradeDto(savedTrade);
    }

    public async Task<TradeDto?> GetTradeByIdAsync(Guid tradeId, CancellationToken cancellationToken = default)
    {
        var trade = await _tradeRepository.GetByIdAsync(tradeId, cancellationToken);
        return trade != null ? MapToTradeDto(trade) : null;
    }

    public async Task<IEnumerable<TradeDto>> GetTradesByPositionAsync(
        Guid positionId,
        CancellationToken cancellationToken = default)
    {
        var trades = await _tradeRepository.FindAsync(t => t.PositionId == positionId, cancellationToken);
        return trades.Select(MapToTradeDto);
    }

    public async Task<IEnumerable<TradeDto>> GetTradesByDateRangeAsync(
        DateTime startDate,
        DateTime endDate,
        CancellationToken cancellationToken = default)
    {
        var trades = await _tradeRepository.GetTradesByDateRangeAsync(startDate, endDate, cancellationToken);
        return trades.Select(MapToTradeDto);
    }

    public async Task<bool> CancelTradeAsync(Guid tradeId, CancellationToken cancellationToken = default)
    {
        var trade = await _tradeRepository.GetByIdAsync(tradeId, cancellationToken);
        if (trade == null || trade.Status != TradeStatus.Pending)
            return false;

        trade.Status = TradeStatus.Cancelled;
        await _tradeRepository.UpdateAsync(trade, cancellationToken);
        return true;
    }

    #endregion

    #region Position Operations

    public async Task<PositionDto> OpenPositionAsync(
        OpenPositionRequest request,
        CancellationToken cancellationToken = default)
    {
        var position = new Position
        {
            AssetId = request.AssetId,
            PolymarketEventId = request.PolymarketEventId,
            Symbol = request.Symbol,
            PositionSide = request.PositionSide,
            Status = PositionStatus.Open,
            Quantity = request.Quantity,
            RemainingQuantity = request.Quantity,
            AverageEntryPrice = request.EntryPrice,
            CurrentPrice = request.EntryPrice,
            CostBasis = request.Quantity * request.EntryPrice,
            MarketValue = request.Quantity * request.EntryPrice,
            UnrealizedPnL = 0,
            TotalCommissions = CalculateCommission(request.Quantity * request.EntryPrice),
            OpenedAt = DateTime.UtcNow,
            StopLoss = request.StopLoss,
            TakeProfit = request.TakeProfit,
            AlpacaOpenSnapshot = request.AlpacaMarketDataSnapshot,
            PolymarketOpenSnapshot = request.PolymarketDataSnapshot,
            Notes = request.Notes
        };

        var savedPosition = await _positionRepository.AddAsync(position, cancellationToken);

        return MapToPositionDto(savedPosition);
    }

    public async Task<PositionDto> ClosePositionAsync(
        Guid positionId,
        decimal? closePrice = null,
        CancellationToken cancellationToken = default)
    {
        var position = await _positionRepository.GetByIdAsync(positionId, cancellationToken);
        if (position == null)
            throw new InvalidOperationException($"Position {positionId} not found");

        if (position.Status != PositionStatus.Open)
            throw new InvalidOperationException($"Position {positionId} is not open");

        var effectiveClosePrice = closePrice ?? position.CurrentPrice ?? position.AverageEntryPrice;

        position.Status = PositionStatus.Closed;
        position.ClosedAt = DateTime.UtcNow;
        position.CurrentPrice = effectiveClosePrice;
        position.RemainingQuantity = 0;

        // Calculate realized P&L
        if (position.PositionSide == PositionSide.Long)
        {
            position.RealizedPnL = (effectiveClosePrice - position.AverageEntryPrice) * position.Quantity;
        }
        else // Short
        {
            position.RealizedPnL = (position.AverageEntryPrice - effectiveClosePrice) * position.Quantity;
        }

        position.RealizedPnL -= position.TotalCommissions;

        await _positionRepository.UpdateAsync(position, cancellationToken);

        return MapToPositionDto(position);
    }

    public async Task<PositionDto> PartiallyClosePositionAsync(
        Guid positionId,
        decimal quantity,
        decimal? closePrice = null,
        CancellationToken cancellationToken = default)
    {
        var position = await _positionRepository.GetByIdAsync(positionId, cancellationToken);
        if (position == null)
            throw new InvalidOperationException($"Position {positionId} not found");

        if (position.Status != PositionStatus.Open)
            throw new InvalidOperationException($"Position {positionId} is not open");

        if (quantity > position.RemainingQuantity)
            throw new InvalidOperationException("Cannot close more than remaining quantity");

        var effectiveClosePrice = closePrice ?? position.CurrentPrice ?? position.AverageEntryPrice;

        // Calculate partial realized P&L
        decimal partialPnL;
        if (position.PositionSide == PositionSide.Long)
        {
            partialPnL = (effectiveClosePrice - position.AverageEntryPrice) * quantity;
        }
        else // Short
        {
            partialPnL = (position.AverageEntryPrice - effectiveClosePrice) * quantity;
        }

        var partialCommission = CalculateCommission(quantity * effectiveClosePrice);
        partialPnL -= partialCommission;

        position.RemainingQuantity -= quantity;
        position.RealizedPnL = (position.RealizedPnL ?? 0) + partialPnL;
        position.TotalCommissions += partialCommission;

        if (position.RemainingQuantity == 0)
        {
            position.Status = PositionStatus.Closed;
            position.ClosedAt = DateTime.UtcNow;
        }
        else
        {
            position.Status = PositionStatus.PartiallyFilled;
        }

        await _positionRepository.UpdateAsync(position, cancellationToken);

        return MapToPositionDto(position);
    }

    public async Task<PositionDto?> GetPositionByIdAsync(
        Guid positionId,
        CancellationToken cancellationToken = default)
    {
        var position = await _positionRepository.GetByIdAsync(positionId, cancellationToken);
        return position != null ? MapToPositionDto(position) : null;
    }

    public async Task<IEnumerable<PositionDto>> GetOpenPositionsAsync(
        CancellationToken cancellationToken = default)
    {
        var positions = await _positionRepository.GetOpenPositionsAsync(cancellationToken);
        return positions.Select(MapToPositionDto);
    }

    public async Task<IEnumerable<PositionDto>> GetPositionsByAssetAsync(
        Guid assetId,
        CancellationToken cancellationToken = default)
    {
        var positions = await _positionRepository.GetPositionsByAssetAsync(assetId, cancellationToken);
        return positions.Select(MapToPositionDto);
    }

    public async Task<bool> UpdatePositionRiskParametersAsync(
        Guid positionId,
        decimal? stopLoss,
        decimal? takeProfit,
        CancellationToken cancellationToken = default)
    {
        var position = await _positionRepository.GetByIdAsync(positionId, cancellationToken);
        if (position == null)
            return false;

        position.StopLoss = stopLoss;
        position.TakeProfit = takeProfit;

        await _positionRepository.UpdateAsync(position, cancellationToken);
        return true;
    }

    public async Task<bool> UpdatePositionMarketDataAsync(
        Guid positionId,
        decimal currentPrice,
        CancellationToken cancellationToken = default)
    {
        await _positionRepository.UpdatePositionMarketValuesAsync(positionId, currentPrice, cancellationToken);
        return true;
    }

    #endregion

    #region Portfolio Operations

    public async Task<PortfolioSummaryDto> GetPortfolioSummaryAsync(
        CancellationToken cancellationToken = default)
    {
        var openPositions = await _positionRepository.GetOpenPositionsAsync(cancellationToken);
        var openPositionsList = openPositions.ToList();

        var totalPositions = await _positionRepository.CountAsync(cancellationToken: cancellationToken);

        var summary = new PortfolioSummaryDto
        {
            OpenPositionsCount = openPositionsList.Count,
            TotalPositionsCount = totalPositions,
            TotalMarketValue = openPositionsList.Sum(p => p.MarketValue ?? 0),
            TotalCostBasis = openPositionsList.Sum(p => p.CostBasis),
            TotalUnrealizedPnL = openPositionsList.Sum(p => p.UnrealizedPnL ?? 0),
            TotalRealizedPnL = openPositionsList.Sum(p => p.RealizedPnL ?? 0),
            TotalCommissions = openPositionsList.Sum(p => p.TotalCommissions),
            OpenPositions = openPositionsList.Select(MapToPositionDto).ToList()
        };

        summary.TotalPnL = summary.TotalUnrealizedPnL + summary.TotalRealizedPnL;
        summary.NetPnL = summary.TotalPnL - summary.TotalCommissions;
        summary.ReturnPercentage = summary.TotalCostBasis > 0 ?
            (summary.NetPnL / summary.TotalCostBasis) * 100 : 0;

        return summary;
    }

    public async Task<PortfolioPerformanceDto> GetPortfolioPerformanceAsync(
        DateTime startDate,
        DateTime endDate,
        CancellationToken cancellationToken = default)
    {
        var tradeStats = await _tradeRepository.GetTradeStatisticsAsync(startDate, endDate, cancellationToken);
        var positionMetrics = await _positionRepository.GetPerformanceMetricsAsync(startDate, endDate, cancellationToken);

        return new PortfolioPerformanceDto
        {
            StartDate = startDate,
            EndDate = endDate,
            TotalTrades = tradeStats.TotalTrades,
            WinningTrades = tradeStats.WinningTrades,
            LosingTrades = tradeStats.LosingTrades,
            WinRate = tradeStats.WinRate,
            TotalProfit = tradeStats.TotalProfit,
            TotalLoss = tradeStats.TotalLoss,
            NetProfit = tradeStats.NetProfit,
            AverageProfit = tradeStats.AverageProfit,
            AverageLoss = tradeStats.AverageLoss,
            ProfitFactor = tradeStats.TotalLoss > 0 ? tradeStats.TotalProfit / tradeStats.TotalLoss : 0,
            TotalCommissions = tradeStats.TotalCommissions,
            ReturnPercentage = 0, // Calculate based on initial capital
            OpenPositionsCount = positionMetrics.OpenPositions,
            ClosedPositionsCount = positionMetrics.ClosedPositions
        };
    }

    #endregion

    #region Helper Methods

    private decimal CalculateCommission(decimal tradeValue)
    {
        // Simple commission calculation - customize based on broker
        return tradeValue * 0.001m; // 0.1%
    }

    private TradeDto MapToTradeDto(Trade trade)
    {
        return new TradeDto
        {
            Id = trade.Id,
            ExternalTradeId = trade.ExternalTradeId,
            StrategyId = trade.StrategyId,
            SignalId = trade.SignalId,
            AssetId = trade.AssetId,
            PolymarketEventId = trade.PolymarketEventId,
            Symbol = trade.Symbol,
            TradeType = trade.TradeType.ToString(),
            TradeSide = trade.TradeSide.ToString(),
            Quantity = trade.Quantity,
            Price = trade.Price,
            TotalValue = trade.TotalValue,
            Commission = trade.Commission,
            Status = trade.Status.ToString(),
            ExecutedAt = trade.ExecutedAt,
            PositionId = trade.PositionId,
            Notes = trade.Notes,
            CreatedAt = trade.CreatedAt
        };
    }

    private PositionDto MapToPositionDto(Position position)
    {
        return new PositionDto
        {
            Id = position.Id,
            AssetId = position.AssetId,
            PolymarketEventId = position.PolymarketEventId,
            Symbol = position.Symbol,
            PositionSide = position.PositionSide.ToString(),
            Status = position.Status.ToString(),
            Quantity = position.Quantity,
            RemainingQuantity = position.RemainingQuantity,
            AverageEntryPrice = position.AverageEntryPrice,
            CurrentPrice = position.CurrentPrice,
            CostBasis = position.CostBasis,
            MarketValue = position.MarketValue,
            UnrealizedPnL = position.UnrealizedPnL,
            RealizedPnL = position.RealizedPnL,
            TotalCommissions = position.TotalCommissions,
            OpenedAt = position.OpenedAt,
            ClosedAt = position.ClosedAt,
            StopLoss = position.StopLoss,
            TakeProfit = position.TakeProfit,
            Notes = position.Notes,
            TradeCount = position.Trades?.Count ?? 0
        };
    }

    #endregion
}
