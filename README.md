# Manufacturing Execution System (MES)

[![.NET](https://img.shields.io/badge/.NET-9.0-512BD4?logo=dotnet)](https://dotnet.microsoft.com/)
[![License](https://img.shields.io/badge/License-MIT-blue.svg)](LICENSE)

A comprehensive Manufacturing Execution System (MES) built with WPF and .NET 9.0, designed to streamline manufacturing operations, track production, and manage resources efficiently.

## ✨ Features

- **Dashboard** - Real-time overview of production metrics and KPIs
- **Order Management** - Create, track, and manage production orders
- **Inventory Control** - Track raw materials and finished goods
- **Employee Management** - Manage workforce and assign tasks
- **Maintenance Tracking** - Schedule and monitor equipment maintenance
- **Reporting** - Generate production and performance reports

## 🚀 Getting Started

### Prerequisites

- [.NET 9.0 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- SQL Server (LocalDB or full version)
- Visual Studio 2022 or later (recommended)

### Installation

1. Clone the repository:
   ```bash
   git clone https://github.com/letrongduc41203/MES.git
   cd MES
   ```

2. Restore NuGet packages:
   ```bash
   dotnet restore
   ```

3. Update the connection string in `appsettings.json` to point to your SQL Server instance.

4. Run database migrations:
   ```bash
   dotnet ef database update
   ```

5. Build and run the application:
   ```bash
   dotnet build
   dotnet run
   ```

## 🛠️ Project Structure

```
MES/
├── Data/               # Database context and configurations
├── Helpers/            # Utility classes and converters
├── Models/             # Domain models and entities
├── Services/           # Business logic and data access
├── ViewModels/         # MVVM ViewModels
├── Views/              # XAML views
├── App.xaml            # Application entry point
├── App.xaml.cs         # Application code-behind
├── MainWindow.xaml     # Main application window
└── appsettings.json    # Application configuration
```

## 📊 Technologies Used

- **Frontend**: WPF, XAML, MVVM Pattern
- **Backend**: .NET 9.0, C#
- **Database**: SQL Server with Entity Framework Core
- **Dependency Injection**: Built-in .NET DI Container
- **UI/UX**: Modern WPF with Material Design principles

## 📸 Screenshots

### Dashboard
![Dashboard](Img/Screenshot%202025-09-05%20021456.png)

### Materials Overview
![Materials Overview](Img/Screenshot%202025-09-05%20021510.png)

### Order Management
![Order Management](Img/Screenshot%202025-09-05%20021505.png)

### Employee Management
![Employee Management](Img/Screenshot%202025-09-05%20021952.png)

### Machine Status
![Machine Status](Img/Screenshot%202025-09-05%20021625.png)

