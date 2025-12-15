using Microsoft.AspNetCore.Mvc;

namespace WebApplication1.Controllers
{
    public class ShopController : Controller
    {
        // Simple in-memory demo items. Replace with DB/EF as needed.
        private static readonly List<ShopItemVm> Items = new()
        {
            new(1, "Kiếm sắt", "Weapon", 120, "Basic sword"),
            new(2, "Súng plasma", "Weapon", 260, "Ranged weapon"),
            new(3, "Áo giáp kim cương", "Armor", 480, "Diamond armor"),
            new(4, "Nhẫn kim cương", "Accessory", 300, "Diamond ring"),
            new(5, "Xe địa hình", "Vehicle", 700, "Off-road vehicle"),
            new(6, "Thuốc hồi phục", "Consumable", 60, "Heal potion"),
        };

        public IActionResult Index()
        {
            return View(Items);
        }

        public record ShopItemVm(int Id, string Name, string Category, int XpCost, string? Description);
    }
}
