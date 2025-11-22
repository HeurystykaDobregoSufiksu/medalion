using System.Net.Http.Headers;
using System.Numerics;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Medalion.Services.Polymarket.Models;
using Nethereum.Signer;
using Nethereum.Util;

namespace Medalion.Services.Polymarket;

/// <summary>
/// Trading client for Polymarket CLOB (Central Limit Order Book) API
/// Handles order creation, signing, execution, and management
/// Requires Nethereum.Signer package for Ethereum cryptographic operations
/// </summary>
public class PolymarketTradingClient
{
    private const string ClobApiUrl = "https://clob.polymarket.com";
    private const int DefaultFeeRateBps = 0; // Fee in basis points (0 = 0%)
    private const long DefaultExpirationDays = 30; // Default order expiration

    private readonly ILogger<PolymarketTradingClient> _logger;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly JsonSerializerOptions _jsonOptions;
    private readonly TradingConfig _config;
    private readonly EthECKey _ethKey;

    public PolymarketTradingClient(
        ILogger<PolymarketTradingClient> logger,
        IHttpClientFactory httpClientFactory,
        TradingConfig config)
    {
        _logger = logger;
        _httpClientFactory = httpClientFactory;
        _config = config;

        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            WriteIndented = false
        };

        // Initialize Ethereum key for signing
        try
        {
            _ethKey = new EthECKey(_config.PrivateKey);
            _config.WalletAddress = _ethKey.GetPublicAddress();
            _logger.LogInformation("Trading client initialized for wallet: {Address}", _config.WalletAddress);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to initialize Ethereum key from private key");
            throw new InvalidOperationException("Invalid private key provided", ex);
        }
    }

    /// <summary>
    /// Places a market order (Fill or Kill)
    /// Market orders execute immediately at best available price
    /// </summary>
    /// <param name="request">Market order parameters</param>
    /// <returns>Order response with order ID and status</returns>
    public async Task<OrderResponse> PlaceMarketOrderAsync(MarketOrderRequest request)
    {
        _logger.LogInformation("Placing market order: {Side} {Amount} of token {TokenId}",
            request.Side, request.Amount, request.TokenId);

        try
        {
            // Get current market price to calculate size
            var price = await GetBestPriceAsync(request.TokenId, request.Side);

            if (price <= 0)
            {
                throw new InvalidOperationException($"Unable to get valid price for token {request.TokenId}");
            }

            // Calculate size from amount and price
            var size = request.Amount / price;

            // Create signed order with FOK type
            var signedOrder = CreateAndSignOrder(
                tokenId: request.TokenId,
                price: price,
                size: size,
                side: request.Side,
                orderType: OrderType.FOK
            );

            // Post order to CLOB
            return await PostOrderAsync(signedOrder);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to place market order");
            return new OrderResponse
            {
                Success = false,
                ErrorMsg = ex.Message
            };
        }
    }

    /// <summary>
    /// Places a limit order (Good Till Cancel)
    /// Limit orders rest on the order book at specified price until filled or cancelled
    /// </summary>
    /// <param name="request">Limit order parameters</param>
    /// <returns>Order response with order ID and status</returns>
    public async Task<OrderResponse> PlaceLimitOrderAsync(LimitOrderRequest request)
    {
        _logger.LogInformation("Placing limit order: {Side} {Size} @ {Price} of token {TokenId}",
            request.Side, request.Size, request.Price, request.TokenId);

        try
        {
            // Validate price range for binary markets (0.01 to 0.99)
            if (request.Price < 0.01m || request.Price > 0.99m)
            {
                throw new ArgumentException("Price must be between 0.01 and 0.99 for binary markets");
            }

            // Create signed order with GTC type
            var orderType = request.Expiration.HasValue ? OrderType.GTD : OrderType.GTC;
            var signedOrder = CreateAndSignOrder(
                tokenId: request.TokenId,
                price: request.Price,
                size: request.Size,
                side: request.Side,
                orderType: orderType,
                expiration: request.Expiration
            );

            // Post order to CLOB
            return await PostOrderAsync(signedOrder);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to place limit order");
            return new OrderResponse
            {
                Success = false,
                ErrorMsg = ex.Message
            };
        }
    }

    /// <summary>
    /// Cancels a specific order by ID
    /// </summary>
    /// <param name="orderId">Order ID to cancel</param>
    /// <returns>True if cancelled successfully</returns>
    public async Task<bool> CancelOrderAsync(string orderId)
    {
        _logger.LogInformation("Cancelling order: {OrderId}", orderId);

        try
        {
            var httpClient = _httpClientFactory.CreateClient();
            var url = $"{ClobApiUrl}/order/{orderId}";

            var response = await httpClient.DeleteAsync(url);

            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("Order {OrderId} cancelled successfully", orderId);
                return true;
            }

            var error = await response.Content.ReadAsStringAsync();
            _logger.LogError("Failed to cancel order {OrderId}: {Error}", orderId, error);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cancelling order {OrderId}", orderId);
            return false;
        }
    }

    /// <summary>
    /// Cancels all open orders for the current wallet
    /// </summary>
    /// <returns>Number of orders cancelled</returns>
    public async Task<int> CancelAllOrdersAsync()
    {
        _logger.LogInformation("Cancelling all orders for wallet {Address}", _config.WalletAddress);

        try
        {
            var openOrders = await GetOpenOrdersAsync(new OpenOrderParams());
            var cancelTasks = openOrders.Select(o => CancelOrderAsync(o.Id)).ToList();
            var results = await Task.WhenAll(cancelTasks);

            var cancelledCount = results.Count(r => r);
            _logger.LogInformation("Cancelled {Count} out of {Total} orders", cancelledCount, openOrders.Count);

            return cancelledCount;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cancelling all orders");
            return 0;
        }
    }

    /// <summary>
    /// Gets all open orders for the current wallet
    /// </summary>
    /// <param name="params">Optional filter parameters</param>
    /// <returns>List of open orders</returns>
    public async Task<List<OpenOrder>> GetOpenOrdersAsync(OpenOrderParams? params = null)
    {
        try
        {
            var httpClient = _httpClientFactory.CreateClient();
            var url = $"{ClobApiUrl}/orders?owner={_config.WalletAddress}";

            if (params?.Market != null)
                url += $"&market={params.Market}";

            if (params?.AssetId != null)
                url += $"&asset_id={params.AssetId}";

            var response = await httpClient.GetStringAsync(url);
            var orders = JsonSerializer.Deserialize<List<OpenOrder>>(response, _jsonOptions) ?? new();

            _logger.LogDebug("Retrieved {Count} open orders", orders.Count);
            return orders;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get open orders");
            return new List<OpenOrder>();
        }
    }

    /// <summary>
    /// Gets the best available price for a token on a given side
    /// </summary>
    /// <param name="tokenId">Token ID to query</param>
    /// <param name="side">Buy or Sell</param>
    /// <returns>Best available price</returns>
    public async Task<decimal> GetBestPriceAsync(string tokenId, OrderSide side)
    {
        try
        {
            var httpClient = _httpClientFactory.CreateClient();
            var sideStr = side == OrderSide.Buy ? "BUY" : "SELL";
            var url = $"{ClobApiUrl}/price/{sideStr}/{tokenId}";

            var response = await httpClient.GetStringAsync(url);
            var priceInfo = JsonSerializer.Deserialize<PriceInfo>(response, _jsonOptions);

            return priceInfo?.Price ?? 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get best price for token {TokenId}", tokenId);
            return 0;
        }
    }

    /// <summary>
    /// Gets the midpoint price for a token
    /// </summary>
    /// <param name="tokenId">Token ID to query</param>
    /// <returns>Midpoint price between bid and ask</returns>
    public async Task<decimal> GetMidpointPriceAsync(string tokenId)
    {
        try
        {
            var httpClient = _httpClientFactory.CreateClient();
            var url = $"{ClobApiUrl}/midpoint/{tokenId}";

            var response = await httpClient.GetStringAsync(url);
            var priceInfo = JsonSerializer.Deserialize<PriceInfo>(response, _jsonOptions);

            return priceInfo?.Price ?? 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get midpoint price for token {TokenId}", tokenId);
            return 0;
        }
    }

    /// <summary>
    /// Creates and signs an order according to Polymarket's protocol
    /// </summary>
    private SignedOrder CreateAndSignOrder(
        string tokenId,
        decimal price,
        decimal size,
        OrderSide side,
        OrderType orderType,
        long? expiration = null)
    {
        // Generate unique salt for this order
        var salt = GenerateSalt();

        // Calculate expiration timestamp (default 30 days from now)
        var expirationTimestamp = expiration ?? DateTimeOffset.UtcNow.AddDays(DefaultExpirationDays).ToUnixTimeSeconds();

        // Generate nonce (current timestamp in milliseconds)
        var nonce = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString();

        // Calculate maker and taker amounts based on side
        // For BUY: maker gives USDC, taker gives shares
        // For SELL: maker gives shares, taker gives USDC
        var makerAmount = side == OrderSide.Buy
            ? (size * price).ToString("F6")  // USDC amount
            : size.ToString("F6");           // Share amount

        var takerAmount = side == OrderSide.Buy
            ? size.ToString("F6")            // Share amount
            : (size * price).ToString("F6"); // USDC amount

        var order = new SignedOrder
        {
            OrderType = orderType.ToString(),
            Salt = salt,
            Maker = _config.WalletAddress,
            Signer = _config.WalletAddress,
            Taker = "0x0000000000000000000000000000000000000000", // Zero address for open orders
            TokenId = tokenId,
            MakerAmount = makerAmount,
            TakerAmount = takerAmount,
            Side = side == OrderSide.Buy ? "BUY" : "SELL",
            Expiration = expirationTimestamp.ToString(),
            Nonce = nonce,
            FeeRateBps = DefaultFeeRateBps.ToString(),
            SignatureType = _config.SignatureType
        };

        // Sign the order
        order.Signature = SignOrder(order);

        return order;
    }

    /// <summary>
    /// Signs an order using EIP-712 typed data signing
    /// </summary>
    private string SignOrder(SignedOrder order)
    {
        try
        {
            // Create the order hash according to Polymarket's specification
            // This follows the EIP-712 typed data hashing standard
            var orderHash = HashOrder(order);

            // Sign with the private key
            var signature = _ethKey.Sign(orderHash);

            // Return signature in hex format with 0x prefix
            return $"0x{signature.R.ToHex()}{signature.S.ToHex()}{signature.V.ToByteArray().ToHex()}";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to sign order");
            throw;
        }
    }

    /// <summary>
    /// Hashes an order according to EIP-712 specification
    /// Note: This is a simplified version. Production code should use proper EIP-712 domain separator
    /// </summary>
    private byte[] HashOrder(SignedOrder order)
    {
        // Concatenate order fields for hashing
        var orderData = $"{order.Salt}{order.Maker}{order.Signer}{order.Taker}" +
                       $"{order.TokenId}{order.MakerAmount}{order.TakerAmount}" +
                       $"{order.Side}{order.Expiration}{order.Nonce}{order.FeeRateBps}";

        // Hash using Keccak256 (Ethereum standard)
        using var sha3 = new Sha3Keccak();
        return sha3.CalculateHash(Encoding.UTF8.GetBytes(orderData));
    }

    /// <summary>
    /// Posts a signed order to the CLOB API
    /// </summary>
    private async Task<OrderResponse> PostOrderAsync(SignedOrder signedOrder)
    {
        try
        {
            var httpClient = _httpClientFactory.CreateClient();
            var url = $"{ClobApiUrl}/order";

            var json = JsonSerializer.Serialize(signedOrder, _jsonOptions);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await httpClient.PostAsync(url, content);
            var responseBody = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                var orderResponse = JsonSerializer.Deserialize<OrderResponse>(responseBody, _jsonOptions);
                _logger.LogInformation("Order placed successfully: {OrderId}", orderResponse?.OrderId);
                return orderResponse ?? new OrderResponse { Success = false, ErrorMsg = "Empty response" };
            }
            else
            {
                _logger.LogError("Failed to post order: {StatusCode} - {Response}",
                    response.StatusCode, responseBody);
                return new OrderResponse
                {
                    Success = false,
                    ErrorMsg = $"HTTP {response.StatusCode}: {responseBody}"
                };
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception while posting order");
            return new OrderResponse
            {
                Success = false,
                ErrorMsg = ex.Message
            };
        }
    }

    /// <summary>
    /// Generates a random salt for order uniqueness
    /// </summary>
    private string GenerateSalt()
    {
        var random = new byte[32];
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(random);
        }
        return new BigInteger(random).ToString();
    }
}
