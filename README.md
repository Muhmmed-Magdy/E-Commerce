# 🛒 E-Commerce Platform

A full-stack e-commerce web application built with **ASP.NET Core MVC**, **Entity Framework Core**, **SQL Server**, and **ASP.NET Core Identity**.

The platform provides product management, categories, authentication, wishlist, shopping cart, checkout, payments, and an AI-powered shopping assistant.

---

## 🚀 Features

### 👤 Authentication & Authorization
- User Registration & Login
- ASP.NET Core Identity
- Role-based authorization
- Admin and Customer roles
- Secure authentication
- Password recovery

### 🛍️ Products
- Browse products
- Product details
- Product search
- Category filtering
- Product quantity management
- Product image upload
- Admin product management

### 📂 Categories
- Create categories
- Edit categories
- Delete categories
- View categories
- Assign products to categories

### ❤️ Wishlist
- Add products to wishlist
- Remove products from wishlist
- View wishlist
- Add wishlist products directly to cart

### 🛒 Shopping Cart
- Add products to cart
- Update quantities
- Remove products
- Calculate total price

### 💳 Payments
- Stripe payment integration
- Secure checkout process
- Order processing

### 🤖 AI Shopping Assistant
- Integrated AI chatbot
- Helps users find products
- Uses the store's product catalog
- Provides product-related assistance
- Designed specifically for the E-Commerce platform

### 📧 Email
- Email service integration
- Account-related emails
- Password recovery emails

---

## 🛠️ Technologies

### Backend
- C#
- ASP.NET Core MVC
- Entity Framework Core
- ASP.NET Core Identity
- AutoMapper

### Database
- Microsoft SQL Server
- Entity Framework Core Migrations

### Frontend
- HTML5
- CSS3
- JavaScript
- Bootstrap
- Bootstrap Icons

### APIs & Services
- Stripe
- Google Gemini AI
- Email Service

### Development Tools
- Visual Studio
- SQL Server Management Studio
- Git
- GitHub

---

## 📁 Project Structure

```text
E-Commerce/
│
├── Controllers/
│   ├── AccountController.cs
│   ├── ProductController.cs
│   ├── CategoryController.cs
│   ├── CartController.cs
│   ├── WishlistController.cs
│   └── ChatController.cs
│
├── Models/
│   ├── Product.cs
│   ├── Category.cs
│   ├── Cart.cs
│   ├── Order.cs
│   └── ...
│
├── Data/
│   └── ApplicationDbContext.cs
│
├── Services/
│   ├── AIChatService.cs
│   ├── EmailSender.cs
│   └── ...
│
├── Views/
│   ├── Account/
│   ├── Product/
│   ├── Category/
│   ├── Cart/
│   ├── Wishlist/
│   └── Shared/
│
├── wwwroot/
│   ├── css/
│   ├── js/
│   └── images/
│
├── Migrations/
│
├── Program.cs
└── appsettings.json
