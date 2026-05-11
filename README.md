Since we configured your `.csproj` to require a `README.md`, the build will fail until this file exists. 

Create a new file named **`README.md`** inside your project folder (the same folder as your `.csproj`) and paste the following content into it. This is a professional-grade README that highlights your library's high-performance features.

---

### Copy this into `README.md`:

```markdown
# OpenMediation 🚀

**OpenMediation** is a high-performance, lightweight, and modern Mediator pattern implementation built specifically for **.NET 10**. It provides a compiled-expression based alternative to traditional mediation libraries, optimized for low-allocation and high-throughput environments.

[![.NET 10](https://img.shields.io/badge/.NET-10.0-blue.svg)](https://dotnet.microsoft.com/download/dotnet/10.0)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](LICENSE)

## 🌟 Why OpenMediation?

Traditional mediator libraries often rely on heavy reflection or `dynamic` dispatch, which can become a bottleneck in high-performance APIs. **OpenMediation** solves this by using:

- **Compiled Expression Trees:** Request and Notification execution plans are compiled into delegates and cached, offering near-native performance.
- **Zero `dynamic` Overhead:** Even notifications (one-to-many) are handled via type-safe compiled invokers, making it **Native AOT friendly**.
- **Correct Pipeline Ordering:** Guarantees that global behaviors (Middleware) execute in the exact order they are registered (Outermost to Innermost).
- **.NET 10 Optimized:** Uses modern C# features like `params ReadOnlySpan<Assembly>` for registration and collection expressions to minimize heap allocations.
- **Fluent Configuration:** A modern registration API that mirrors industry standards while providing more control.

---

## 📦 Installation

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
        return new UserResponse(request.Id, "Waleed Awn");
    }
}
```

### 3. Register in Program.cs (Fluent API)
```csharp
using OpenMediation.DependencyInjection;

builder.Services.AddOpenMediation(typeof(Program).Assembly);
```

### 4. Use it
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

### Pipeline Behaviors
Create cross-cutting concerns easily:
```csharp
public class LoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse> 
    where TRequest : IRequest<TResponse>
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken ct)
    {
        Console.WriteLine($"Handling {typeof(TRequest).Name}");
        return await next(ct);
    }
}
```

### Notifications (Events)
Broadcast events to multiple handlers without coupling:
```csharp
public record UserCreated(Guid Id) : INotification;

// Usage
await mediator.Publish(new UserCreated(Guid.NewGuid()));
```

---

## 🏗 Project Structure
- **Abstractions**: Clean interfaces for requests, handlers, and the mediator facade.
- **Pipeline**: Core middleware logic including pre/post processors.
- **Dispatching**: The high-performance engine using expression-tree caching.
- **DependencyInjection**: Fluent registration extensions for ASP.NET Core.

---

## 🤝 Contributing
OpenMediation is an open-source project by **SSFVision**. We welcome contributions! Feel free to open issues or submit pull requests on our [GitHub Repository](https://github.com/WaleedAwn/OpenMediation).

## 📄 License
Licensed under the **MIT License**.
```

---

### What to do now:

1. **Create the file:** Inside your `OpenMediation` folder, create `README.md` and paste the text above.
2. **Build the project:** Run `dotnet build`. The error **NU5039** should now disappear.
3. **Pack the project:** Run `dotnet pack -c Release`.
4. **Check the output:** Go to `bin/Release/net10.0/`. You will now see your **`OpenMediation.1.0.0.nupkg`**.

**Congratulations!** You have a fully documented, high-performance library package ready for use. 🚀