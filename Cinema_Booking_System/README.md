# 🎬 CineMax — Cinema Booking System

A full-featured cinema ticket booking system built with **.NET 10 ASP.NET Core MVC**, **Entity Framework Core**, and **SQL Server Express (LocalDB)**. Supports two roles — **Admin** and **Member** — with a complete back-office management panel and a public-facing movie catalog with cart and checkout flow.

---

## 📋 Table of Contents

- [Features](#features)
- [Tech Stack](#tech-stack)
- [Prerequisites](#prerequisites)
- [Getting Started](#getting-started)
- [Project Structure](#project-structure)
- [Default Credentials](#default-credentials)
- [URL Reference](#url-reference)
- [Image Upload](#image-upload)
- [Database Schema](#database-schema)

---

## ✨ Features

### 🛡️ Security & Authentication
- ASP.NET Core Identity with **Admin** and **Member** roles
- Cookie-based authentication (8-hour session)
- Role-based route authorization
- Anti-forgery token protection on all POST forms
- Inactive member login prevention

### 👤 Member Features
| Feature | Description |
|---|---|
| **Register / Login** | Create an account and sign in |
| **Profile Management** | Update name, phone, address |
| **Movie Catalog** | Browse all active movies with search & category filter |
| **Movie Detail** | View movie info and grouped showtimes by date |
| **Shopping Cart** | Add tickets, update quantity, remove items |
| **Checkout** | Enter contact info and choose payment method |
| **Order Confirmation** | See a summary of the placed order |
| **Order History** | View all past orders with status badges |
| **Order Detail** | Full breakdown of any individual order |

### 🔧 Admin Features
| Feature | Description |
|---|---|
| **Dashboard** | Stats cards (members, movies, orders, revenue) + recent orders |
| **Member Maintenance** | List, search, view order history, activate/deactivate, delete |
| **Category Maintenance** | Full CRUD — create, edit, delete, toggle active |
| **Movie/Product Maintenance** | Full CRUD + **poster image upload** with live preview |
| **Showtime Management** | Schedule movie screenings with hall, seats, time, price |
| **Order Maintenance** | View all orders, filter by status, update order & payment status |

### 💳 Payment Methods (Simulated)
- 💵 Cash — marked Unpaid until admin confirms
- 💳 Credit Card — auto-marked as Paid
- 🏦 Online Banking
- 📱 E-Wallet

---

## 🛠️ Tech Stack

| Layer | Technology |
|---|---|
| Framework | .NET 10 ASP.NET Core MVC |
| Language | C# |
| ORM | Entity Framework Core 10 |
| Database | SQL Server Express (LocalDB — file-based, no install needed) |
| Authentication | ASP.NET Core Identity |
| UI | Bootstrap 5.3 + Bootstrap Icons 1.11 |
| Image Upload | IFormFile → wwwroot/uploads/movies/ |

---

## ✅ Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download) (10.0.x)
- SQL Server Express LocalDB (bundled with Visual Studio, or install via SQL Server Express)
- (Optional) Visual Studio 2022+ or VS Code with C# extension

Verify your setup:
```
dotnet --version   # should show 10.x.x
```

---

## 🚀 Getting Started

### 1. Open the Project Folder

```
cd "c:\Users\User\OneDrive\桌面\Cinema_Booking_System"
```

### 2. Restore Dependencies

```
dotnet restore
```

### 3. Run the Application

```
dotnet run
```

> On first launch, the app **automatically**:
> - Creates the SQL Server LocalDB database
> - Applies EF Core migrations
> - Seeds: Admin & Member roles, default admin account, 6 categories, 6 movies, 84 showtimes

### 4. Open in Browser

```
http://localhost:5189
```

> The exact port may vary. Check the terminal for the `Now listening on:` line.

---

## ⚙️ Configuration

Connection string in `appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=CinemaBookingSystem;Trusted_Connection=True;TrustServerCertificate=True"
  }
}
```

---

## 🔑 Default Credentials

| Role | Email | Password |
|---|---|---|
| **Admin** | admin@cinema.com | Admin@123 |
| **Member** | Register at /Account/Register | Your choice |

> The admin credentials are shown on the login page during development.

---

## 📁 Project Structure

```
Cinema_Booking_System/
│
├── Controllers/
│   ├── AccountController.cs        # Login, Register, Logout, Profile
│   ├── HomeController.cs           # Landing page
│   ├── MovieController.cs          # Public catalog & detail
│   ├── CartController.cs           # Shopping cart
│   ├── OrderController.cs          # Checkout, confirmation, history
│   ├── DashboardController.cs      # Admin dashboard
│   ├── MembersController.cs        # Admin member management
│   ├── CategoriesController.cs     # Admin category CRUD
│   ├── MoviesController.cs         # Admin movie CRUD + image upload
│   ├── ShowtimesController.cs      # Admin showtime management
│   └── OrdersController.cs         # Admin order management
│
├── Data/
│   ├── ApplicationDbContext.cs     # EF Core DbContext
│   └── SeedData.cs                 # Startup seeder
│
├── Models/
│   ├── ApplicationUser.cs
│   ├── Category.cs
│   ├── Movie.cs                    # With ImagePath for poster
│   ├── Showtime.cs
│   ├── Cart.cs / CartItem.cs
│   ├── Order.cs / OrderItem.cs
│   └── ViewModels/ViewModels.cs
│
├── Views/
│   ├── Shared/
│   │   ├── _Layout.cshtml          # Public navbar + footer
│   │   └── _AdminLayout.cshtml     # Admin sidebar layout
│   ├── Account/                    # Login, Register, Profile
│   ├── Home/                       # Landing page
│   ├── Movie/                      # Catalog + Detail
│   ├── Cart/                       # Shopping cart
│   ├── Order/                      # Checkout, Confirmation, History
│   ├── Dashboard/                  # Admin dashboard
│   ├── Members/                    # Admin member views
│   ├── Categories/                 # Admin category views
│   ├── Movies/                     # Admin movie views + image upload
│   ├── Showtimes/                  # Admin showtime views
│   └── Orders/                     # Admin order views
│
├── wwwroot/
│   ├── css/site.css                # Public styles
│   ├── css/admin.css               # Admin styles
│   ├── js/site.js
│   └── uploads/movies/             # Uploaded movie posters
│
├── Migrations/                     # EF Core migrations
├── appsettings.json
└── Program.cs
```

---

## 🌐 URL Reference

### Public Routes
| Route | Description |
|---|---|
| `/` | Home — featured movies |
| `/Movie` | Movie catalog (search, filter, paginate) |
| `/Movie/Detail/{id}` | Movie + showtimes |
| `/Account/Login` | Sign in |
| `/Account/Register` | Sign up |

### Member Routes *(login required)*
| Route | Description |
|---|---|
| `/Account/Profile` | Edit profile |
| `/Cart` | Shopping cart |
| `/Order/Checkout` | Checkout |
| `/Order/Confirmation/{id}` | Booking confirmed |
| `/Order/History` | My orders |
| `/Order/Detail/{id}` | Order detail |

### Admin Routes *(Admin role required)*
| Route | Description |
|---|---|
| `/Admin` | Dashboard |
| `/Admin/Members` | Member list |
| `/Admin/Members/Detail/{id}` | Member detail |
| `/Admin/Categories` | Category list |
| `/Admin/Categories/Create` | Add category |
| `/Admin/Movies` | Movie list |
| `/Admin/Movies/Create` | Add movie + poster |
| `/Admin/Movies/Edit/{id}` | Edit movie + poster |
| `/Admin/Showtimes` | Showtime list |
| `/Admin/Showtimes/Create` | Add showtime |
| `/Admin/Orders` | All orders |
| `/Admin/Orders/Detail/{id}` | Order + update status |

---

## 🖼️ Image Upload

- **Accepted formats:** JPG, JPEG, PNG, GIF, WEBP
- **Storage:** `wwwroot/uploads/movies/{guid}.ext`
- **Live preview** shown in the form before saving
- **Old image deleted** automatically when replaced
- Film icon placeholder shown when no image is uploaded

---

## 🗄️ Database Schema

| Table | Description |
|---|---|
| `Users` | Members and Admins (extends Identity) |
| `AspNetRoles` | Admin, Member roles |
| `Categories` | Movie genres/categories |
| `Movies` | Movie catalog with poster image path |
| `Showtimes` | Individual screening sessions |
| `Carts` | One cart per user |
| `CartItems` | Tickets in cart |
| `Orders` | Placed orders |
| `OrderItems` | Snapshot of booked tickets per order |

---

## 🎭 Seeded Sample Data

| Type | Count | Details |
|---|---|---|
| Categories | 6 | Action, Comedy, Drama, Horror, Sci-Fi, Animation |
| Movies | 6 | One per category |
| Showtimes | 84 | 2 per day × 7 days × 6 movies |

---

> Built with ❤️ using .NET 10 · Bootstrap 5 · Entity Framework Core
