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
        private static readonly ConcurrentBag<TransactionDto> Transactions = new();

        static GameApiController()
        {
            SeedPlayers();
            SeedItems();
            SeedTransactions();
        }

        // 1) Get all resource types
        [HttpGet("resources")]
        public ActionResult<IEnumerable<ResourceDto>> GetResources()
        {
            var resources = new[]
            {
                new ResourceDto("Gold", "Currency used to buy basic items"),
                new ResourceDto("Diamond", "Premium currency"),
                new ResourceDto("XP", "Experience points earned by playing"),
                new ResourceDto("Energy", "Limits match attempts")
            };
            return Ok(resources);
        }

        // 2) Get players by game mode
        [HttpGet("players/by-mode")]
        public ActionResult<IEnumerable<PlayerDto>> GetPlayersByMode([FromQuery] string mode)
        {
            if (string.IsNullOrWhiteSpace(mode)) return BadRequest("Mode is required.");

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
        public ActionResult<IEnumerable<TransactionDto>> GetTransactions(int playerId)
        {
            if (!Players.ContainsKey(playerId)) return NotFound("Player not found.");

            var result = Transactions
                .Where(t => t.PlayerId == playerId)
                .OrderByDescending(t => t.OccurredAt)
                .ToList();
            return Ok(result);
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

        // Helper: get item by id
        [HttpGet("items/{id:int}")]
        public ActionResult<ItemDto> GetItemById(int id)
        {
            if (!Items.TryGetValue(id, out var item)) return NotFound();
            return Ok(item);
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
            var result = Transactions
                .GroupBy(t => t.ItemId)
                .Where(g => g.Key.HasValue)
                .Select(g =>
                {
                    var itemId = g.Key!.Value; // safe due to HasValue filter
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
            var counts = Transactions
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

        // ---------- Seed data ----------
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
            Transactions.Add(new TransactionDto(1, 1, 1, "Item", DateTime.UtcNow.AddDays(-2)));
            Transactions.Add(new TransactionDto(2, 1, 3, "Item", DateTime.UtcNow.AddDays(-1)));
            Transactions.Add(new TransactionDto(3, 2, 1, "Item", DateTime.UtcNow.AddHours(-20)));
            Transactions.Add(new TransactionDto(4, 2, 5, "Vehicle", DateTime.UtcNow.AddHours(-10)));
            Transactions.Add(new TransactionDto(5, 3, 4, "Item", DateTime.UtcNow.AddHours(-5)));
            Transactions.Add(new TransactionDto(6, 3, null, "Vehicle", DateTime.UtcNow.AddHours(-3))); // vehicle without item mapping
        }

        // ---------- DTOs & requests ----------
        public record ResourceDto(string Name, string Description);

        public record PlayerDto(int Id, string Name, string Mode, int Experience, string Password);

        public class ItemDto
        {
            public int Id { get; set; }
            public string Name { get; set; } = string.Empty;
            public string Category { get; set; } = string.Empty; // Weapon, Armor, Vehicle, etc.
            public int XpCost { get; set; }
            public string? Description { get; set; }
        }

        public record TransactionDto(int Id, int PlayerId, int? ItemId, string Kind, DateTime OccurredAt);

        public class CreateItemRequest
        {
            public string Name { get; set; } = string.Empty;
            public string Category { get; set; } = string.Empty;
            public int XpCost { get; set; }
            public string? Description { get; set; }
        }

        public class UpdatePasswordRequest
        {
            public string NewPassword { get; set; } = string.Empty;
        }
    }
}
