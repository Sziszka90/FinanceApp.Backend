# 💼 Finance App - Backend

## 📦 A sophisticated personal finance management platform with intelligent transaction processing

This project is a full-stack personal finance application designed to help users track, categorize, and analyze their financial transactions with AI-powered insights. It provides a complete solution from frontend UI to backend APIs, intelligent processing, and secure data management.

## 🎯 Current Features

✅ **User Management** 
  - Login, registration, password reset with JWT integration and email confirmation
  - User profile where users can set their preferred currency
  - JWT-based authentication with token caching and invalidation, using HTTP-only cookies for security reasons
  - Separated tokens for password reset, login and email confirmation

✅ **Transactions** 
  - Create, read, update, delete transactions with input validation
  - Import transactions from CSV files with automatic, asynchronous transaction group matching powered by AI, RabbitMQ and SignalR

✅ **Transaction Groups** 
  - Create, read, update, delete transaction groups with input validation

✅ **MCP endpoint** 
  - Standardized MCP endpoint to call backend tools to provide data to user via LLMProcessor Service

✅ **Currency Exchange** 
  - Recurring background job querying live exchange rates for multi-currency support. Stored in internal database
  - Exchange rate cache ensures that all transactions use historically accurate exchange rates

✅ **Email Services** 
  - SMTP integration for notifications

## 🔮 Upcoming Features

For detailed upcoming features and development progress, please check our [GitHub Issues](https://github.com/Sziszka90/FinanceApp.Backend/issues).

## 🏗️ Architecture

### **Clean Architecture Principles**

```
📁 FinanceApp.Backend.Presentation.WebApi              # Controllers, middleware, API endpoints
📁 FinanceApp.Backend.Application                      # Business logic, CQRS, DTOs
📁 FinanceApp.Backend.Domain                           # Entities, interfaces, domain rules
📁 FinanceApp.Backend.Infrastructure                   # Core infrastructure services
📁 FinanceApp.Backend.Infrastructure.Cache             # Redis caching implementation
📁 FinanceApp.Backend.Infrastructure.EntityFramework   # Base EF Core abstractions
    └── .Common                                        # Shared EF configurations & context
    └── .Mssql                                         # SQL Server specific implementation
    └── .Sqlite                                        # SQLite specific implementation
📁 FinanceApp.Backend.Infrastructure.RabbitMq          # Message queuing & async communication
```

### **Key Architectural Patterns**

- **CQRS** (Command Query Responsibility Segregation)
- **Repository Pattern** with Entity Framework
- **Domain Layer** Domain objects enforce self-mutation constraints
- **Service Layer Pattern** - Business logic encapsulation in dedicated services
- **Client Pattern** - External API integration through dedicated client classes
- **Background Jobs** - Scheduled and recurring tasks for operations
- **Dependency Injection** throughout all layers
- **Async/Await** for non-blocking operations
- **Event-Driven Architecture** with RabbitMQ

## 💻 Tech Stack

- **ASP.NET Core 8** - Web API framework
- **AutoMapper** - Object-to-object mapping
- **FluentValidation** - Input validation
- **MediatR** - CQRS implementation
- **JWT & HTTP Only Cookies** - Authentication & authorization
- **Swagger/OpenAPI** - API documentation
- **SignalR** - Push notifications
- **Entity Framework Core** - ORM with Code-First migrations
- **Microsoft SQL Server** - Primary production database with full ACID compliance
- **SQLite** - Lightweight database for development and testing environments
- **Redis** - High-performance in-memory caching and session management
- **RabbitMQ** - Reliable message broker for async communication

## 📋 API Documentation

### **Authentication Endpoints**

```http
POST   /api/v1/auth/login                   # User login
POST   /api/v1/auth/logout                  # User logout
GET.   /api/v1/auth/check                   # Check if user authenticated
```

### **Token Endpoints**

```http
POST   /api/v1/token/validate               # Validate token
POST   /api/v1/token/refresh                # Refresh token endpoint
```

### **MCP Endpoints**

```http
POST   /api/v1/mcp                          # MCP endpoint for tool call
```

### **User Management Endpoints**

```http
GET    /api/v1/users/{id}/email-confirmation    # Confirm user email with token
POST   /api/v1/users/email-confirmation         # Resend email confirmation
POST   /api/v1/users/password-reset             # Request password reset email
PATCH  /api/v1/users/password                   # Update password
GET    /api/v1/users/{id}                       # Get user by ID
GET    /api/v1/users                            # Get current active user (requires auth)
POST   /api/v1/users                            # Register new user
PUT    /api/v1/users                            # Update current user (requires auth)
DELETE /api/v1/users/{id}                       # Delete user by ID (requires auth)
```

### **Transaction Endpoints**

```http
GET    /api/v1/transactions                 # Get all transactions (requires auth)
GET    /api/v1/transactions/{id}            # Get transaction by ID (requires auth)
POST   /api/v1/transactions                 # Create new transaction (requires auth)
PUT    /api/v1/transactions/{id}            # Update transaction (requires auth)
DELETE /api/v1/transactions/{id}            # Delete transaction (requires auth)
GET    /api/v1/transactions/summary         # Get transaction summary/analytics (requires auth)
POST   /api/v1/transactions/import          # Bulk CSV upload (requires auth)
```

### **Transaction Group Endpoints**

```http
GET    /api/v1/transactiongroups            # Get all transaction groups (requires auth)
GET    /api/v1/transactiongroups/{id}       # Get transaction group by ID (requires auth)
POST   /api/v1/transactiongroups            # Create new transaction group (requires auth)
PUT    /api/v1/transactiongroups/{id}       # Update transaction group (requires auth)
DELETE /api/v1/transactiongroups/{id}       # Delete transaction group (requires auth)
GET    /api/v1/transactiongroups/top        # Get top transaction groups (requires auth)
```

### **Wakeup Endpoints**

```http
POST   /api/v1/wakeup                       # Wakeup endpoint to wakeup cloud services
```

## 🧪 Testing and Quality

All unit and integration tests are run automatically in the CI/CD pipeline.  
Test coverage is checked in each build and is consistently above **80%**.
Code quality is enforced using linting tools to ensure consistent style and catch potential errors.

## 🚀 Deployment

### **Azure Container Apps**

The application is deployed as **containerized microservices** on **Azure Container Apps** using GitHub Actions.

**Deployment Flow:**

1. **Push to main** → Triggers GitHub Actions workflow
2. **Build & Test** → Runs automated test suite and linting
3. **Bundle** → Creates production build
4. **Deploy** → Updates hosting platform with new version
5. **Verify** → Automated health checks ensure successful deployment

## 📊 Monitoring & Logging

- **Health Checks** for database and external services
- **Logging** for easy debugging

## 🤝 Contributing

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/amazing-feature`)
3. Commit your changes (`git commit -m 'Add amazing feature'`)
4. Push to the branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request

## 👤 Author

**Szilard Ferencz**  
🌐 [szilardferencz.dev](https://www.szilardferencz.dev)  
💼 [LinkedIn](https://www.linkedin.com/in/szilard-ferencz/)  
🐙 [GitHub](https://github.com/Sziszka90)

## 📄 License

Distributed under the MIT License. See `LICENSE` for more information.

---

⭐ **Star this repo if you find it helpful!** ⭐
