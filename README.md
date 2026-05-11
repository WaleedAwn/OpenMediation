# OpenMediation 🚀

**OpenMediation** is a high-performance, lightweight, and modern Mediator pattern implementation engineered specifically for **.NET 10**. It provides a compiled-expression-based alternative to traditional mediation libraries, optimized for low-allocation and high-throughput environments.

[![.NET 10](https://img.shields.io/badge/.NET-10.0-blue.svg)](https://dotnet.microsoft.com/download/dotnet/10.0)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](LICENSE)

## 🌟 Why OpenMediation?

Traditional mediator libraries often rely on heavy reflection or `dynamic` dispatch, which can become a performance bottleneck in high-load APIs. **OpenMediation** solves this by leveraging:

- **Compiled Expression Trees:** Request and Notification execution plans are compiled into delegates and cached, offering near-native performance after the first call.
- **Zero `dynamic` Overhead:** Unlike other implementations, notifications (one-to-many) are handled via type-safe compiled invokers, making the library **Native AOT friendly**.
- **Correct Pipeline Ordering:** Guarantees that global behaviors (Middleware) execute in the exact order they are registered (Outermost to Innermost).
- **.NET 10 Optimized:** Fully utilizes modern C# features such as `params ReadOnlySpan<Assembly>` for registration and collection expressions to minimize heap allocations.
- **Fluent Configuration:** A modern, intuitive registration API that mirrors industry standards while providing granular control.

---

## 📦 Installation

Install the package via the .NET CLI:

```bash
dotnet add package OpenMediation
```

---

## 🚀 Quick Start

### 1. Define a Request and Response
```csharp
public record GetUserQuery(Guid Id) : IRequest<UserResponse>;
public record UserResponse(Guid Id, string Name);
```

### 2. Create a Handler
```csharp
public class GetUserHandler : IRequestHandler<GetUserQuery, UserResponse>
{
    public async Task<UserResponse> Handle(GetUserQuery request, CancellationToken ct)
    {
        // Business logic here
        return new UserResponse(request.Id, "Waleed Awn");
    }
}
```

### 3. Register in Program.cs (Fluent API)
```csharp
using OpenMediation.DependencyInjection;

builder.Services.AddOpenMediation(typeof(Program).Assembly);
```

### 4. Inject and Use
```csharp
public class UserController(IMediator mediator) : ControllerBase
{
    [HttpGet("{id}")]
    public async Task<IActionResult> Get(Guid id)
    {
        var response = await mediator.Send(new GetUserQuery(id));
        return Ok(response);
    }
}
```

---

## 🛠 Advanced Features

### Pipeline Behaviors (Middleware)
Easily implement cross-cutting concerns that wrap your handlers:

```csharp
public class LoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse> 
    where TRequest : IRequest<TResponse>
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken ct)
    {
        Console.WriteLine($"[LOG] Handling {typeof(TRequest).Name}");
        var response = await next(ct);
        Console.WriteLine($"[LOG] Finished {typeof(TRequest).Name}");
        return response;
    }
}
```

### Notifications (Events)
Broadcast events to multiple independent handlers without coupling your code:

```csharp
public record UserCreated(Guid Id) : INotification;

// Usage inside a service
await mediator.Publish(new UserCreated(Guid.NewGuid()));
```

---

## 🏗 Project Structure

- **Abstractions**: Clean, decoupled interfaces for requests, handlers, and the mediator facade.
- **Pipeline**: Core middleware logic including support for Behaviors and Pre/Post Processors.
- **Dispatching**: The high-performance engine using expression-tree caching and optimized execution plans.
- **DependencyInjection**: Fluent registration extensions and assembly scanning for ASP.NET Core.

---

## 🤝 Contributing

**OpenMediation** is an open-source project maintained by **SSFVision**. We are committed to high-performance .NET engineering and welcome contributions from the community!

- **Found a bug?** Open an [issue](https://github.com/WaleedAwn/OpenMediation/issues).
- **Have a feature idea?** Submit a [Pull Request](https://github.com/WaleedAwn/OpenMediation/pulls).

---

## 📄 License

This project is licensed under the **MIT License**. See the `LICENSE` file for more details.
