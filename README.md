# 💼 Personal Finance App - Backend

📦 **A sophisticated personal finance management platform with intelligent transaction processing**

This project is a full-stack personal finance application designed to help users track, categorize, and analyze their financial transactions with AI-powered insights. It provides a complete enterprise-grade solution from frontend UI to backend APIs, intelligent processing, and secure data management.

### 🎯 Current Features

✅ **User Management** - Registration, email confirmation, password reset, user base currency  
✅ **Authentication** - JWT-based auth with token invalidation  
✅ **Transaction CRUD** - Create, read, update, delete transactions  
✅ **Transaction Groups** - Organize transactions into pre defined or custom categories  
✅ **AI Integration** - Async LLM processing via RabbitMQ for matching transactions and transaction groups  
✅ **Currency Exchange** - Recurring background job querying live exchange rates for multi-currency support  
✅ **Caching System** - Redis-based token and data caching  
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
- **Service Layer Pattern** - Business logic encapsulation in dedicated services
- **Client Pattern** - External API integration through dedicated client classes
- **Background Jobs** - Scheduled and recurring tasks for maintenance operations
- **Dependency Injection** throughout all layers
- **Async/Await** for non-blocking operations
- **Event-Driven Architecture** with RabbitMQ

## 🚀 Tech Stack

### **Backend (.NET 8)**

- **ASP.NET Core 8** - Web API framework
- **Entity Framework Core** - ORM with MSSQL/SQLite support
- **AutoMapper** - Object-to-object mapping
- **FluentValidation** - Input validation
- **MediatR** - CQRS implementation
- **JWT Bearer** - Authentication & authorization
- **Swagger/OpenAPI** - API documentation

### **Infrastructure & Data Layer**

- **Entity Framework Core** - ORM with Code-First migrations
- **Microsoft SQL Server** - Primary production database with full ACID compliance
- **SQLite** - Lightweight database for development and testing environments
- **Redis** - High-performance in-memory caching and session management
- **Connection Pooling** - Optimized database connection management
- **Data Seeding** - Automated initial data setup for development

### **Message Queuing & Communication**

- **RabbitMQ** - Reliable message broker for async communication
- **Event-driven processing** for transaction analysis and AI integration
- **Message Persistence** - Ensures reliability during service restarts

### **Background Services**

- **Exchange Rate Jobs** - Scheduled tasks fetching live currency exchange rates
- **RabbitMQ Message Processing** - Background consumers handling async AI processing queues
- **Health Monitoring** - Periodic service health checks and alerting

### **External Services & APIs**

- **FinanceApp.LLMProcessor** - AI-powered transaction and group matching service
- **SMTP Services** - Email delivery for notifications and user communications
- **Exchange Rate APIs** - Live currency conversion data providers
- **Logging Services** - Centralized application logging and monitoring

### **Frontend**

- **Angular 19** - Modern TypeScript framework with latest features
- **Angular CLI** - Development tooling and build optimization
- **RxJS** - Reactive programming for async data handling
- **Angular Material** - Professional UI component library
- **TypeScript** - Strong typing for better code quality and maintainability

## 🔧 Features Deep Dive

### **👤 User Management**

- **Registration Flow** with email verification
- **Password Reset** with secure token-based recovery
- **Profile Management** with account settings
- **Session Management** with token invalidation

### **💰 Transaction Management**

- **CRUD Operations** - Full transaction lifecycle
- **Bulk Operations** - CSV import/export capabilities
- **Transaction Grouping** - Organize by categories
- **Real-time Validation** - Immediate feedback on data entry

### **🤖 AI-Powered Features**

- **Transaction Matching** - AI-powered matching of transactions to appropriate categories
- **Transaction Group Analysis** - Intelligent grouping of related transactions
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
Node.js 18+ (for frontend)
```

### **Backend Setup**

```bash
# Clone the repository
git clone https://github.com/Sziszka90/FinanceApp.Backend.git
cd FinanceApp.Backend

# Restore dependencies
dotnet restore

# Update database
dotnet ef database update --project FinanceApp.Backend.Infrastructure.EntityFramework.Mssql

# Run the application
dotnet run --project FinanceApp.Backend.Presentation.WebApi
```

### **Configuration**

```json
// appsettings.json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=FinanceApp;Trusted_Connection=true;"
  },
  "Redis": {
    "ConnectionString": "localhost:6379"
  },
  "RabbitMQ": {
    "HostName": "localhost",
    "QueueName": "llm-processing"
  },
  "Smtp": {
    "Host": "smtp.gmail.com",
    "Port": 587
  }
}
```

## 📋 API Documentation

### **Authentication Endpoints**

```http
POST /api/auth/login             # User login
POST /api/auth/validate-token    # Validate JWT token
```

### **User Management Endpoints**

```http
GET    /api/users                    # Get current active user (requires auth)
GET    /api/users/{id}               # Get user by ID (requires auth)
POST   /api/users                    # Create new user (registration)
PUT    /api/users                    # Update current user (requires auth)
DELETE /api/users/{id}               # Delete user by ID (requires auth)
GET    /api/users/{id}/confirm-email # Confirm user email with token
POST   /api/users/resend-confirmation-email  # Resend email confirmation
POST   /api/users/forgot-password    # Request password reset
POST   /api/users/update-password    # Update password with reset token
```

### **Transaction Endpoints**

```http
GET    /api/transactions             # Get all transactions with optional filters (requires auth)
GET    /api/transactions/{id}        # Get transaction by ID (requires auth)
POST   /api/transactions             # Create new transaction (requires auth)
PUT    /api/transactions/{id}        # Update transaction (requires auth)
DELETE /api/transactions/{id}        # Delete transaction (requires auth)
GET    /api/transactions/summary     # Get transaction summary/analytics (requires auth)
POST   /api/transactions/upload-csv  # Bulk CSV upload (requires auth)
```

### **Transaction Group Endpoints**

```http
GET    /api/transactiongroups           # Get all transaction groups (requires auth)
GET    /api/transactiongroups/{id}      # Get transaction group by ID (requires auth)
POST   /api/transactiongroups           # Create new transaction group (requires auth)
PUT    /api/transactiongroups/{id}      # Update transaction group (requires auth)
DELETE /api/transactiongroups/{id}      # Delete transaction group (requires auth)
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

```bash
# Run unit tests
dotnet test FinanceApp.Backend.Testing.Unit

# Run integration tests
dotnet test FinanceApp.Backend.Testing.Integration

# Generate coverage report
dotnet test --collect:"XPlat Code Coverage"
```

## � Docker & Deployment

### **Docker Support**

```dockerfile
# Build and run with Docker
docker build -t financeapp-backend .
docker run -p 5000:80 financeapp-backend
```

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
2. **Build & Test** → Runs automated test suite
3. **Containerize** → Creates optimized Docker images
4. **Deploy** → Updates Azure Container Apps with zero downtime
5. **Verify** → Automated health checks ensure successful deployment

## �📊 Monitoring & Logging

- **Health Checks** for database and external services
- **Performance Metrics** with Application Insights
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
