using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace E_Commerce.Controllers;

[Authorize(Roles = "Admin")]
public class AdminController : Controller
{
    // GET: /Admin/Dashboard
    public IActionResult Dashboard()
    {
        return View();
    }
}
//ahmed test