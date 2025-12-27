# 🌸 Florist E-Commerce Web Application

A professional **Florist E-Commerce Web Application** built using **ASP.NET Core MVC** and **Entity Framework Core**.  
This project represents a real-world online flower shop where customers can browse products and admins can manage inventory and orders.

The application follows clean MVC architecture and modern database practices.

---

## 🚀 Project Overview

This application is designed for:
- Online flower shops
- Small e-commerce businesses
- Learning real-world ASP.NET Core MVC workflows

It demonstrates **backend logic, database handling, and e-commerce concepts** in a structured and scalable way.

---

## ✨ Features

### 🛍 Customer Side
- Browse flowers and bouquets
- View product details with pricing
- Add products to cart
- Simple checkout flow
- Responsive user interface

### 🛠 Admin Panel
- Manage products (Add / Edit / Delete)
- Manage categories
- View and manage orders
- Basic inventory handling

### ⚙ Technical Features
- ASP.NET Core MVC architecture
- Entity Framework Core (Code-First)
- SQL Server / LocalDB support
- Razor Views
- Clean folder structure
- Secure database configuration

---

## 🛠 Tech Stack

| Category | Technology |
|--------|------------|
| Backend | ASP.NET Core MVC |
| Language | C# |
| ORM | Entity Framework Core |
| Database | SQL Server / LocalDB |
| Frontend | Razor Views, HTML, CSS, JavaScript |
| Tools | .NET SDK, GitHub |

---

## 📂 Project Structure

FloristEcommerceApp
│
├── Controllers/
│ ├── HomeController.cs
│ ├── ProductsController.cs
│ ├── CartController.cs
│ └── AdminController.cs
│
├── Models/
│ ├── Product.cs
│ ├── Category.cs
│ ├── Order.cs
│ └── CartItem.cs
│
├── Data/
│ └── ApplicationDbContext.cs
│
├── Migrations/
│ └── Entity Framework Core Migrations
│
├── Views/
│ ├── Home/
│ ├── Products/
│ ├── Cart/
│ ├── Admin/
│ └── Shared/
│
├── wwwroot/
│ └── css / js (essential assets only)
│
├── Program.cs
├── appsettings.json
└── README.md


---

## 🗄 Database Setup

-This project uses **Entity Framework Core (Code-First)**.  
-Actual database files are **not included** for security reasons.

### Sample Connection String

`json
"ConnectionStrings": {
  "DefaultConnection": "Server=.;Database=FloristEcommerceDb;Trusted_Connection=True;TrustServerCertificate=True;"
}

-git clone https://github.com/AliyanGhani/florist-ecommerce-app.git

-cd florist-ecommerce-app

Apply Entity Framework Core migrations

This command will automatically create the database and tables:

Update-Database


Run the application

dotnet run


The application will be available at:

http://localhost:5000


or

https://localhost:5001


## ⚙ Getting Started

Follow the steps below to run this project locally.

---

### 🔹 Prerequisites

Make sure you have the following installed:

- .NET SDK **6.0 or later**
- SQL Server or LocalDB
- Any code editor (VS Code recommended)

Check .NET installation:
`bash
dotnet --version

## 📦 Static Assets Note

Due to GitHub file size limitations, **heavy images and media files are not included** in this repository.

Only essential **CSS, JavaScript, and placeholder assets** are provided to demonstrate layout, styling, and functionality.

Product images, banners, and other large media files can be:
- Added locally during development
- Served via CDN or cloud storage in production

This approach follows standard industry practices and keeps the repository lightweight and secure.

---

## 🚧 Future Enhancements

- 🔐 User authentication & role-based authorization
- 💳 Online payment gateway integration
- 📦 Order tracking and order history
- 📧 Email notifications for orders
- ⭐ Product reviews and ratings
- 🌐 REST API for mobile or third-party integration

---

## 👨‍💻 Author

**Aliyan Ghani**  
Full Stack & ASP.NET Developer  
📍 Pakistan  

GitHub: https://github.com/AliyanGhani  

---

⭐ If you find this project useful, please give it a star  
🤝 Open to freelance, job, and collaboration opportunities

