using E_Commerce.Data;
using Google.GenAI;
using Microsoft.EntityFrameworkCore;

namespace E_Commerce.Services
{
    public class AIChatService
    {
        private readonly Client _client;
        private readonly ApplicationDbContext _context;

        public AIChatService(
            IConfiguration configuration,
            ApplicationDbContext context)
        {
            var apiKey = configuration["Gemini:ApiKey"];

            if (string.IsNullOrWhiteSpace(apiKey))
            {
                throw new Exception("Gemini API key is missing.");
            }

            _client = new Client(
                apiKey: apiKey
            );

            _context = context;
        }

        public async Task<string> GetResponseAsync(string message)
        {
            // ==========================================
            // Get products from database
            // ==========================================

            var products = await _context.Products
                .Include(p => p.Category)
                .ToListAsync();


            // ==========================================
            // Create store catalog
            // ==========================================

            var productData = string.Join("\n", products.Select(p =>
                $"""
                Product ID: {p.ProductId}
                Name: {p.Title}
                Description: {p.Description}
                Price: {p.Price}
                Quantity: {p.Quantity}
                Category: {p.Category?.Name ?? "Uncategorized"}
                """
            ));


            // ==========================================
            // AI Prompt
            // ==========================================

            var prompt = $"""
                You are the AI shopping assistant for an e-commerce
                website called E-Commerce.

                You MUST use the store catalog below when answering
                questions about products.

                IMPORTANT RULES:

                - Only recommend products that exist in the store catalog.
                - Never invent products.
                - Never invent prices.
                - Never invent stock information.
                - Never recommend Amazon, Walmart, Best Buy,
                  GameStop, or other external stores.
                - Do not provide generic internet shopping lists.
                - If no product matches the customer's request,
                  clearly say that no matching product was found
                  in our store.
                - Keep responses short and friendly.
                - Talk like an assistant inside an e-commerce website.
                - Use EGP when talking about prices.
                - If a product has Quantity = 0, say it is out of stock.
                - If the customer asks about a product, use the
                  product information from the catalog.
                - Do not expose database IDs unless necessary.

                STORE CATALOG:
                ----------------

                {productData}

                ----------------

                CUSTOMER MESSAGE:
                {message}

                Answer the customer using ONLY the store information
                above when talking about products.
                """;


            // ==========================================
            // Call Gemini
            // ==========================================

            const int maxAttempts = 3;

            for (int attempt = 1; attempt <= maxAttempts; attempt++)
            {
                try
                {
                    var response =
                        await _client.Models.GenerateContentAsync(
                            model: "gemini-3.6-flash",
                            contents: prompt
                        );

                    return response.Text?.Trim()
                        ?? "Sorry, I couldn't generate a response.";
                }
                catch (Exception)
                {
                    if (attempt == maxAttempts)
                    {
                        return "Sorry 😕 The AI assistant is temporarily busy. Please try again in a few seconds.";
                    }

                    await Task.Delay(attempt * 2000);
                }
            }

            return "Sorry 😕 Please try again.";
        }
    }
}