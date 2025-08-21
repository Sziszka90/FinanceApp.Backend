# 💼 Personal Finance App - Backend

📦 **A sophisticated personal finance management platform with intelligent transaction processing**

This project is a full-stack personal finance application designed to help users track, categorize, and analyze their financial transactions with AI-powered insights. It provides a complete solution from frontend UI to backend APIs, intelligent processing, and secure data management.

### 🎯 Current Features

✅ **User Management** - Registration, email confirmation, password reset, user base currency

✅ **Authentication** - JWT-based auth with token caching and invalidation

✅ **Transactions CRUD** - Create, read, update, delete transactions

✅ **Transaction Groups CRUD** - Create, read, update, delete transaction groups

✅ **AI Integration** - Async LLM processing via RabbitMQ for matching transactions and transaction groups

✅ **Currency Exchange** - Recurring background job querying live exchange rates for multi-currency support

✅ **Caching System** - Redis-based token caching

✅ **Email Services** - SMTP integration for notifications

### 🔮 Upcoming Features

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

## 🚀 Tech Stack

- **ASP.NET Core 8** - Web API framework
- **AutoMapper** - Object-to-object mapping
- **FluentValidation** - Input validation
- **MediatR** - CQRS implementation
- **JWT Bearer** - Authentication & authorization
- **Swagger/OpenAPI** - API documentation
- **SignalR** - Push notifications
- **Entity Framework Core** - ORM with Code-First migrations
- **Microsoft SQL Server** - Primary production database with full ACID compliance
- **SQLite** - Lightweight database for development and testing environments
- **Redis** - High-performance in-memory caching and session management
- **RabbitMQ** - Reliable message broker for async communication

## 🔧 Features Deep Dive

### **👤 User Management**

- **Registration Flow** with email verification
- **Password Reset** with secure token-based recovery
- **Profile Management** with account settings
- **Session Management** with token invalidation

### **💰 Transaction and Transaction Group Management**

- **CRUD Operations** - Full transaction and transaction group lifecycle
- **Bulk Operations** - CSV import/export capabilities
- **Transaction Grouping** - Organize by categories
- **Real-time Validation** - Immediate feedback on data entry

### **🤖 AI-Powered Features**

- **Transaction Matching** - AI-powered matching of transactions to appropriate categories
- **Async Processing** - Non-blocking AI analysis via RabbitMQ message queues

### **⚡ Performance & Scalability**

- **Redis Caching** - Token management
- **Async Operations** - Non-blocking database and external service calls
- **Connection Pooling** - Optimized database connections

### **🔒 Security Features**

- **Token Invalidation** - Secure logout and session management
- **Input Validation** - Protection against malicious data
- **CORS Configuration** - Cross-origin request security

## 🚦 Getting Started

### **Prerequisites**

```bash
# Required software
.NET 8 SDK
SQL Server (or SQLite for development)
Redis Server
RabbitMQ Server
```

### **Configuration**

The application uses **layered configuration** with multiple sources:

- **appsettings.json** - Development defaults and structure
- **appsettings.Development.json** - Local development overrides
- **appsettings.Database.json** - Database specific settings
- **appsettings.Messaging.json** - Messaging specific settings
- **appsettings.Testing.json** - Testing specific settings

**Key Configuration Areas:**

- Database connections (SQL Server/SQLite)
- Redis caching settings
- RabbitMQ message queuing
- SMTP email services
- JWT authentication
- External API integrations
- Logging and monitoring

Production configuration is managed through **GitHub Actions env variables** and **GitHub Actions secrets** for security and deployment automation.

## 📋 API Documentation

### **Authentication Endpoints**

```http
POST   /api/v1/auth/login                   # User login
POST   /api/v1/auth/logout                  # User logout
```

### **Token Endpoints**

```http
POST   /api/v1/token/validate               # Validate token
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

### **Response Formats**

All endpoints use standardized **Result objects** for consistent response handling. The API returns JSON responses with the following structure:

```json
// Success Response
{
  "data": { /* response data */ },
  "isSuccess": true,
  "message": "Operation completed successfully"
}

// Error Response
{
  "errors": ["Error message"],
  "isSuccess": false,
  "message": "Operation failed"
}
```

**Result Pattern Benefits:**

- **Consistent error handling** across all endpoints
- **Type-safe responses** with clear success/failure indicators
- **Standardized messaging** for better client-side integration
- **Multiple error support** for comprehensive validation feedback

## 🧪 Testing

All unit and integration tests are run automatically in the CI/CD pipeline.  
Test coverage is checked in each build and is consistently above **80%**.

## 🚀 Deployment

### **Azure Container Apps**

The application is deployed as **containerized microservices** on **Azure Container Apps**, providing:

- **Auto-scaling** based on demand
- **Container orchestration** with built-in load balancing
- **Blue-green deployments** for zero-downtime updates
- **Integrated monitoring** and logging

### **CI/CD Pipeline**

**GitHub Actions** handles the complete CI/CD workflow:

```yaml
# Automated pipeline includes:
✅ Code quality checks and linting
✅ Unit and integration test execution
✅ Docker image building and optimization
✅ Container registry push (Azure Container Registry)
✅ Automated deployment to Azure Container Apps
✅ Health checks and rollback capabilities
```

**Deployment Flow:**

1. **Push to main** → Triggers GitHub Actions workflow
2. **Code quality** → Runs code quality che
2. **Build & Test** → Runs automated test suite
3. **Containerize** → Creates optimized Docker images
4. **Deploy** → Updates Azure Container Apps with zero downtime
5. **Verify** → Automated health checks ensure successful deployment

## 📊 Monitoring & Logging

- **Health Checks** for database and external services
- **Logging** for easy debugging
- **Error Tracking** with custom exception handling

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
