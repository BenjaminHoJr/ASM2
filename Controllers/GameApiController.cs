using Microsoft.AspNetCore.Mvc;
using System.Collections.Concurrent;

namespace WebApplication1.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class GameApiController : ControllerBase
    {
        // In-memory demo data. Replace with EF Core queries when real tables exist.
        private static readonly ConcurrentDictionary<int, PlayerDto> Players = new();
        private static readonly ConcurrentDictionary<int, ItemDto> Items = new();
        private static readonly ConcurrentDictionary<int, TransactionDto> Transactions = new();

        static GameApiController()
        {
            SeedPlayers();
            SeedItems();
            SeedTransactions();
        }

        // ==================== RESOURCE APIs ====================

        // 1) Get all resource types
        [HttpGet("resources")]
        public ActionResult<IEnumerable<ResourceDto>> GetResources()
        {
            var resources = new[]
            {
                new ResourceDto("Gold", "Currency used to buy basic items", 10000),
                new ResourceDto("Diamond", "Premium currency", 500),
                new ResourceDto("XP", "Experience points earned by playing", 25000),
                new ResourceDto("Energy", "Limits match attempts", 100)
            };
            return Ok(resources);
        }

        // ==================== PLAYER APIs ====================

        // Get all players
        [HttpGet("players")]
        public ActionResult<IEnumerable<PlayerDto>> GetAllPlayers()
        {
            return Ok(Players.Values.OrderBy(p => p.Id).ToList());
        }

        // Get player by ID
        [HttpGet("players/{playerId:int}")]
        public ActionResult<PlayerDto> GetPlayerById(int playerId)
        {
            if (!Players.TryGetValue(playerId, out var player))
                return NotFound("Player not found.");
            return Ok(player);
        }

        // Create new player
        [HttpPost("players")]
        public ActionResult<PlayerDto> CreatePlayer([FromBody] CreatePlayerRequest request)
        {
            if (!ModelState.IsValid) return ValidationProblem(ModelState);

            var id = Players.Keys.DefaultIfEmpty(0).Max() + 1;
            var player = new PlayerDto(id, request.Name, request.Mode, request.Experience, request.Password);
            Players[id] = player;
            return CreatedAtAction(nameof(GetPlayerById), new { playerId = id }, player);
        }

        // Update player
        [HttpPut("players/{playerId:int}")]
        public ActionResult<PlayerDto> UpdatePlayer(int playerId, [FromBody] UpdatePlayerRequest request)
        {
            if (!ModelState.IsValid) return ValidationProblem(ModelState);
            if (!Players.TryGetValue(playerId, out var existing))
                return NotFound("Player not found.");

            var updated = existing with
            {
                Name = request.Name ?? existing.Name,
                Mode = request.Mode ?? existing.Mode,
                Experience = request.Experience ?? existing.Experience
            };
            Players[playerId] = updated;
            return Ok(updated);
        }

        // Delete player
        [HttpDelete("players/{playerId:int}")]
        public ActionResult DeletePlayer(int playerId)
        {
            if (!Players.TryRemove(playerId, out _))
                return NotFound("Player not found.");
            return NoContent();
        }

        // 2) Get players by game mode
        [HttpGet("players/by-mode")]
        public ActionResult<IEnumerable<PlayerDto>> GetPlayersByMode([FromQuery] string mode)
        {
            if (string.IsNullOrWhiteSpace(mode))
                return Ok(Players.Values.OrderBy(p => p.Id).ToList());

            var result = Players.Values
                .Where(p => string.Equals(p.Mode, mode, StringComparison.OrdinalIgnoreCase))
                .ToList();
            return Ok(result);
        }

        // 3) Get weapons with XP cost > 100
        [HttpGet("weapons/over-100xp")]
        public ActionResult<IEnumerable<ItemDto>> GetWeaponsOver100Xp()
        {
            var weapons = Items.Values
                .Where(i => string.Equals(i.Category, "Weapon", StringComparison.OrdinalIgnoreCase) && i.XpCost > 100)
                .OrderByDescending(i => i.XpCost)
                .ToList();
            return Ok(weapons);
        }

        // ==================== ITEM APIs ====================

        // Get all items
        [HttpGet("items")]
        public ActionResult<IEnumerable<ItemDto>> GetAllItems()
        {
            return Ok(Items.Values.OrderBy(i => i.Id).ToList());
        }

        // Helper: get item by id
        [HttpGet("items/{id:int}")]
        public ActionResult<ItemDto> GetItemById(int id)
        {
            if (!Items.TryGetValue(id, out var item)) return NotFound();
            return Ok(item);
        }

        // 7) Add a new item
        [HttpPost("items")]
        public ActionResult<ItemDto> CreateItem([FromBody] CreateItemRequest request)
        {
            if (!ModelState.IsValid) return ValidationProblem(ModelState);

            var id = Items.Keys.DefaultIfEmpty(0).Max() + 1;
            var item = new ItemDto
            {
                Id = id,
                Name = request.Name,
                Category = request.Category,
                XpCost = request.XpCost,
                Description = request.Description
            };
            Items[id] = item;
            return CreatedAtAction(nameof(GetItemById), new { id }, item);
        }

        // Update item
        [HttpPut("items/{id:int}")]
        public ActionResult<ItemDto> UpdateItem(int id, [FromBody] UpdateItemRequest request)
        {
            if (!ModelState.IsValid) return ValidationProblem(ModelState);
            if (!Items.TryGetValue(id, out var existing))
                return NotFound("Item not found.");

            var updated = new ItemDto
            {
                Id = id,
                Name = request.Name ?? existing.Name,
                Category = request.Category ?? existing.Category,
                XpCost = request.XpCost ?? existing.XpCost,
                Description = request.Description ?? existing.Description
            };
            Items[id] = updated;
            return Ok(updated);
        }

        // Delete item
        [HttpDelete("items/{id:int}")]
        public ActionResult DeleteItem(int id)
        {
            if (!Items.TryRemove(id, out _))
                return NotFound("Item not found.");
            return NoContent();
        }

        // 4) Get items affordable for a player by XP
        [HttpGet("players/{playerId:int}/affordable-items")]
        public ActionResult<IEnumerable<ItemDto>> GetAffordableItems(int playerId)
        {
            if (!Players.TryGetValue(playerId, out var player)) return NotFound("Player not found.");

            var affordable = Items.Values
                .Where(i => i.XpCost <= player.Experience)
                .OrderBy(i => i.XpCost)
                .ToList();
            return Ok(affordable);
        }

        // 5) Items containing 'kim cương' and cost < 500 XP
        [HttpGet("items/kim-cuong-under-500xp")]
        public ActionResult<IEnumerable<ItemDto>> GetDiamondItemsUnder500()
        {
            var result = Items.Values
                .Where(i => i.Name.Contains("kim cương", StringComparison.OrdinalIgnoreCase) && i.XpCost < 500)
                .OrderBy(i => i.XpCost)
                .ToList();
            return Ok(result);
        }

        // 6) Transactions (item & vehicle) of a player, ordered by time
        [HttpGet("players/{playerId:int}/transactions")]
        public ActionResult<IEnumerable<TransactionDto>> GetPlayerTransactions(int playerId)
        {
            if (!Players.ContainsKey(playerId)) return NotFound("Player not found.");

            var result = Transactions.Values
                .Where(t => t.PlayerId == playerId)
                .OrderByDescending(t => t.OccurredAt)
                .ToList();
            return Ok(result);
        }

        // ==================== TRANSACTION APIs ====================

        // Get all transactions
        [HttpGet("transactions")]
        public ActionResult<IEnumerable<object>> GetAllTransactions()
        {
            var result = Transactions.Values
                .OrderByDescending(t => t.OccurredAt)
                .Select(t => new
                {
                    t.Id,
                    t.PlayerId,
                    PlayerName = Players.TryGetValue(t.PlayerId, out var p) ? p.Name : "Unknown",
                    t.ItemId,
                    ItemName = t.ItemId.HasValue && Items.TryGetValue(t.ItemId.Value, out var i) ? i.Name : "N/A",
                    t.Kind,
                    t.OccurredAt
                })
                .ToList();
            return Ok(result);
        }

        // Get transaction by ID
        [HttpGet("transactions/{id:int}")]
        public ActionResult<TransactionDto> GetTransactionById(int id)
        {
            if (!Transactions.TryGetValue(id, out var transaction))
                return NotFound("Transaction not found.");
            return Ok(transaction);
        }

        // Create new transaction (purchase)
        [HttpPost("transactions")]
        public ActionResult<TransactionDto> CreateTransaction([FromBody] CreateTransactionRequest request)
        {
            if (!ModelState.IsValid) return ValidationProblem(ModelState);
            if (!Players.ContainsKey(request.PlayerId))
                return BadRequest("Player not found.");
            if (request.ItemId.HasValue && !Items.ContainsKey(request.ItemId.Value))
                return BadRequest("Item not found.");

            var id = Transactions.Keys.DefaultIfEmpty(0).Max() + 1;
            var transaction = new TransactionDto(id, request.PlayerId, request.ItemId, request.Kind, DateTime.UtcNow);
            Transactions[id] = transaction;
            return CreatedAtAction(nameof(GetTransactionById), new { id }, transaction);
        }

        // Delete transaction
        [HttpDelete("transactions/{id:int}")]
        public ActionResult DeleteTransaction(int id)
        {
            if (!Transactions.TryRemove(id, out _))
                return NotFound("Transaction not found.");
            return NoContent();
        }

        // 8) Update player password
        [HttpPatch("players/{playerId:int}/password")]
        public ActionResult UpdatePassword(int playerId, [FromBody] UpdatePasswordRequest request)
        {
            if (!ModelState.IsValid) return ValidationProblem(ModelState);
            if (!Players.TryGetValue(playerId, out var player)) return NotFound("Player not found.");

            var updated = player with { Password = request.NewPassword };
            Players[playerId] = updated;
            return NoContent();
        }

        // 9) Most purchased items
        [HttpGet("items/top-purchased")]
        public ActionResult<IEnumerable<object>> GetTopPurchasedItems([FromQuery] int top = 5)
        {
            var result = Transactions.Values
                .GroupBy(t => t.ItemId)
                .Where(g => g.Key.HasValue)
                .Select(g =>
                {
                    var itemId = g.Key!.Value;
                    return new
                    {
                        ItemId = itemId,
                        Count = g.Count(),
                        Item = Items.TryGetValue(itemId, out var item) ? item : null
                    };
                })
                .OrderByDescending(x => x.Count)
                .Take(top)
                .ToList();
            return Ok(result);
        }

        // 10) Players and their purchase counts
        [HttpGet("players/purchase-counts")]
        public ActionResult<IEnumerable<object>> GetPlayersPurchaseCounts()
        {
            var counts = Transactions.Values
                .GroupBy(t => t.PlayerId)
                .Select(g => new
                {
                    PlayerId = g.Key,
                    Player = Players.TryGetValue(g.Key, out var p) ? p : null,
                    PurchaseCount = g.Count()
                })
                .OrderByDescending(x => x.PurchaseCount)
                .ToList();
            return Ok(counts);
        }

        // ==================== STATISTICS APIs ====================

        // Dashboard statistics
        [HttpGet("stats/dashboard")]
        public ActionResult<object> GetDashboardStats()
        {
            var totalPlayers = Players.Count;
            var totalItems = Items.Count;
            var totalTransactions = Transactions.Count;
            var totalXp = Players.Values.Sum(p => p.Experience);

            var playersByMode = Players.Values
                .GroupBy(p => p.Mode)
                .Select(g => new { Mode = g.Key, Count = g.Count() })
                .ToList();

            var itemsByCategory = Items.Values
                .GroupBy(i => i.Category)
                .Select(g => new { Category = g.Key, Count = g.Count() })
                .ToList();

            var recentTransactions = Transactions.Values
                .OrderByDescending(t => t.OccurredAt)
                .Take(5)
                .Select(t => new
                {
                    t.Id,
                    PlayerName = Players.TryGetValue(t.PlayerId, out var p) ? p.Name : "Unknown",
                    ItemName = t.ItemId.HasValue && Items.TryGetValue(t.ItemId.Value, out var i) ? i.Name : "N/A",
                    t.Kind,
                    t.OccurredAt
                })
                .ToList();

            return Ok(new
            {
                TotalPlayers = totalPlayers,
                TotalItems = totalItems,
                TotalTransactions = totalTransactions,
                TotalXp = totalXp,
                PlayersByMode = playersByMode,
                ItemsByCategory = itemsByCategory,
                RecentTransactions = recentTransactions
            });
        }

        // ==================== SEED DATA ====================
        private static void SeedPlayers()
        {
            Players[1] = new PlayerDto(1, "Alice", "Sinh tồn", 320, "pass1");
            Players[2] = new PlayerDto(2, "Bob", "Đối kháng", 180, "pass2");
            Players[3] = new PlayerDto(3, "Charlie", "Sinh tồn", 520, "pass3");
        }

        private static void SeedItems()
        {
            Items[1] = new ItemDto { Id = 1, Name = "Kiếm sắt", Category = "Weapon", XpCost = 120, Description = "Basic sword" };
            Items[2] = new ItemDto { Id = 2, Name = "Súng plasma", Category = "Weapon", XpCost = 260, Description = "Ranged weapon" };
            Items[3] = new ItemDto { Id = 3, Name = "Áo giáp kim cương", Category = "Armor", XpCost = 480, Description = "Diamond armor" };
            Items[4] = new ItemDto { Id = 4, Name = "Nhẫn kim cương", Category = "Accessory", XpCost = 300, Description = "Diamond ring" };
            Items[5] = new ItemDto { Id = 5, Name = "Xe địa hình", Category = "Vehicle", XpCost = 700, Description = "Off-road vehicle" };
            Items[6] = new ItemDto { Id = 6, Name = "Thuốc hồi phục", Category = "Consumable", XpCost = 60, Description = "Heal potion" };
        }

        private static void SeedTransactions()
        {
            Transactions[1] = new TransactionDto(1, 1, 1, "Item", DateTime.UtcNow.AddDays(-2));
            Transactions[2] = new TransactionDto(2, 1, 3, "Item", DateTime.UtcNow.AddDays(-1));
            Transactions[3] = new TransactionDto(3, 2, 1, "Item", DateTime.UtcNow.AddHours(-20));
            Transactions[4] = new TransactionDto(4, 2, 5, "Vehicle", DateTime.UtcNow.AddHours(-10));
            Transactions[5] = new TransactionDto(5, 3, 4, "Item", DateTime.UtcNow.AddHours(-5));
            Transactions[6] = new TransactionDto(6, 3, null, "Vehicle", DateTime.UtcNow.AddHours(-3));
        }

        // ==================== DTOs & REQUEST MODELS ====================

        public record ResourceDto(string Name, string Description, int Amount);

        public record PlayerDto(int Id, string Name, string Mode, int Experience, string Password);

        public class ItemDto
        {
            public int Id { get; set; }
            public string Name { get; set; } = string.Empty;
            public string Category { get; set; } = string.Empty;
            public int XpCost { get; set; }
            public string? Description { get; set; }
        }

        public record TransactionDto(int Id, int PlayerId, int? ItemId, string Kind, DateTime OccurredAt);

        // Request models
        public class CreatePlayerRequest
        {
            public string Name { get; set; } = string.Empty;
            public string Mode { get; set; } = "Sinh tồn";
            public int Experience { get; set; } = 0;
            public string Password { get; set; } = string.Empty;
        }

        public class UpdatePlayerRequest
        {
            public string? Name { get; set; }
            public string? Mode { get; set; }
            public int? Experience { get; set; }
        }

        public class CreateItemRequest
        {
            public string Name { get; set; } = string.Empty;
            public string Category { get; set; } = string.Empty;
            public int XpCost { get; set; }
            public string? Description { get; set; }
        }

        public class UpdateItemRequest
        {
            public string? Name { get; set; }
            public string? Category { get; set; }
            public int? XpCost { get; set; }
            public string? Description { get; set; }
        }

        public class UpdatePasswordRequest
        {
            public string NewPassword { get; set; } = string.Empty;
        }

        public class CreateTransactionRequest
        {
            public int PlayerId { get; set; }
            public int? ItemId { get; set; }
            public string Kind { get; set; } = "Item";
        }
    }
}
