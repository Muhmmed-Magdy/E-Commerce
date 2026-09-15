using E_Commerce.Data;
using E_Commerce.Models;
using E_Commerce.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace E_Commerce.Controllers
{
    public class CategoryController : Controller
    {
        private readonly ApplicationDbContext context;

        public CategoryController(ApplicationDbContext context)
        {
            this.context = context;
        }

        public IActionResult Index()
        {
            var categories = context.Categories
                .Include(c => c.Products)
                .ToList();

            return View(categories);
        }

        public IActionResult Details(int id)
        {
            var category = context.Categories
                .Include(c => c.Products)
                .FirstOrDefault(c => c.CategoryId == id);

            if (category == null)
                return NotFound();

            return View(category);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Create(CreateCategoryViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var category = new Category
            {
                Name = model.Name,
                Description = model.Description
            };

            context.Categories.Add(category);
            context.SaveChanges();

            return RedirectToAction("Index");
        }

        public IActionResult Edit(int id)
        {
            var category = context.Categories.Find(id);

            if (category == null)
                return NotFound();

            var model = new EditCategoryViewModel
            {
                CategoryId = category.CategoryId,
                Name = category.Name,
                Description = category.Description
            };

            return View(model);
        }

        [HttpPost]
        public IActionResult Edit(EditCategoryViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var category = context.Categories.Find(model.CategoryId);

            if (category == null)
                return NotFound();

            category.Name = model.Name;
            category.Description = model.Description;

            context.SaveChanges();

            return RedirectToAction("Index");
        }

        public IActionResult Delete(int id)
        {
            var category = context.Categories.Find(id);

            if (category == null)
                return NotFound();

            return View(category);
        }

        [HttpPost, ActionName("Delete")]
        public IActionResult DeleteConfirmed(int id)
        {
            var category = context.Categories.Find(id);

            if (category == null)
                return NotFound();

            context.Categories.Remove(category);
            context.SaveChanges();

            return RedirectToAction("Index");
        }
    }
}
