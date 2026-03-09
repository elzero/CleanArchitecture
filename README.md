# Clean Architecture Demo (.NET 8/9+)

本示例项目是对 .NET 简洁架构（Clean Architecture）模式在现代企业级应用中的一套完整、可运行的最佳实践演示。

## 🎯 核心特性 (Key Features)

本架构设计采用了强领域驱动（DDD）色彩，内置了大量生产环境常用的高级架构模式：

- **Clean Architecture 基础分层**
  - **Domain**: 放下最纯粹的业务实体（Entity, AggregateRoot）、结果对象（Result Pattern）领域事件契约与自定义错误模型（Error）。
  - **Application**: 引入 CQRS 切分读写操作，利用 `MediatR` 和 `FluentValidation` 搭建出带有校验和日志能力的中间件管道体系。
  - **Infrastructure**: 连接底层的基础设施设施。本例中将 EF Core 迁移至 **MySQL (Pomelo)** 核心；使用具有 `IHostedService` 特性的 `Quartz` 构建后台 Job 调度器。
  - **Presentation (Api)**: 依靠 `Carter` 管理的现代 Minimal APIs 抛弃笨重的 Controller，依靠全局异常处理器 (`IExceptionHandler`) 规范错误出口。
- **Domain Events (领域事件)**
  - 核心实体状态变更仅在内部记录事件 `RaiseDomainEvent`，保证领域侧纯洁。
- **Outbox Pattern (发件箱模式) + 面向并发安全的后台排队**
  - `InsertOutboxMessagesInterceptor`: 通过 EF Core SaveChanges 拦截器拦截事件持久化存入 Outbox，保障原子提交不丢单。
  - 分布式节点并发竞态保障: 搭配 MySQL `FOR UPDATE SKIP LOCKED` 悲观锁与排他查询，配合 `Quartz` 后台调度实现微服务环境下多节点并发安全削峰消费。
- **CQRS 混合隔离读取 (Dapper + EF Core)**
  - **写 (Commands)** 跑在 `EF Core` 和它的 ChangeTracker 机制上，享受最佳数据完整性和领域规则安全。
  - **读 (Queries)** 使用 `Dapper` 直接越界通过 `MySqlConnection` 到数据库拿 Flat Model 数据，最大限度避免 ORM 对象跟踪导致的巨额开销。

## 📁 目录结构 (Project Structure)

```text
src/
├── CleanArchitecture.Domain/         [核心]：实体、值对象、领域事件与契约抽象
├── CleanArchitecture.Application/    [应用]：中介请求 (CQRS)、行为验证管道
├── CleanArchitecture.Infrastructure/ [基建]：EF Core(MySQL)、Outbox Interceptor、Quartz 调度、外部 API 调用
└── CleanArchitecture.Api/            [展示]：Minimal APIs、中间件注册、全局容灾
```

## 🚀 快速启动 (Getting Started)

### 前置条件
- .NET 8.0 或更新版本的 SDK
- MySQL Server (版本 > 8.0)

### 操作步骤
1. 打开 `CleanArchitecture.Api` 项目下的 `appsettings.json`。
2. 找到 `ConnectionStrings:Database`，将其中的服务器地址、根账号与密码替换为您本地或远端的 MySQL 连接。
3. **初始化数据库表结构** (执行 EF Core 迁移)：
    在终端（解决方案根目录）中运行 EF Core CLI 迁移命令，在数据库中创建相关的业务表与发件箱表：
    ```bash
    # 1. 确保您的环境已全局安装 dotnet-ef 命令行工具 (若未安装)
    dotnet tool install --global dotnet-ef

    # 2. 生成初始迁移记录 (此步骤中指定 Infrastructure 为模型宿主，API 负责提供 appsettings 配置)
    dotnet ef migrations add InitialCreate --project CleanArchitecture.Infrastructure --startup-project CleanArchitecture.Api

    # 3. 将刚刚生成的表结构直接推进到真实 MySQL 数据库中
    dotnet ef database update --project CleanArchitecture.Infrastructure --startup-project CleanArchitecture.Api
    ```
4. 在解决方案的根目录执行测试/运行：
    ```bash
    dotnet build
    dotnet run --project CleanArchitecture.Api
    ```
5. Swagger 调试面板通常会在 `http://localhost:<Port>/swagger`。可查阅对外暴露的轻量级 API 端口。

## 🛠 技术栈 (Technology Stack)

| 组件类别 | 具体使用的技术栈库 |
| --- | --- |
| 基础框架 | .NET 8/9, ASP.NET Core Minimal APIs |
| 架构消息中介 | MediatR |
| 验证机制 | FluentValidation |
| 模块化 API | Carter |
| ORM (读写持久化) | Entity Framework Core, Pomelo.EntityFrameworkCore.MySql |
| 轻量级反序列化读取 | Dapper, MySqlConnector |
| 调度系统 (发件箱消费) | Quartz.Extensions.Hosting |
| 数据序列化交换 | Newtonsoft.Json / System.Text.Json |

---
**Disclaimer**: This project structure is mainly for demonstrative purposes showing how to adapt advanced distributed system patterns (like Outbox and CQRS) into standard Clean Architecture.
