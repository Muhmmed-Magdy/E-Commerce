using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using E_Commerce.Data;
using E_Commerce.Models;
using E_Commerce.ViewModels;

namespace E_Commerce.Controllers;

[Authorize(Roles = "Admin")]
public class AdminController : Controller
{
    private readonly ApplicationDbContext db;
    private readonly UserManager<ApplicationUser> userManager;

    public AdminController(
        ApplicationDbContext db,
        UserManager<ApplicationUser> userManager)
    {
        this.db = db;
        this.userManager = userManager;
    }

    // GET: /Admin/Dashboard
    public async Task<IActionResult> Dashboard()
    {
        var customers = await userManager.GetUsersInRoleAsync("Customer");

        var dashboardData = new AdminDashboardViewModel
        {
            TotalProducts = await db.Products.CountAsync(),

            TotalOrders = await db.Orders.CountAsync(),

            TotalCustomers = customers.Count,

            TotalRevenue = await db.Orders
                .Select(o => (decimal?)o.TotalAmount)
                .SumAsync() ?? 0
        };

        return View(dashboardData);
    }
}