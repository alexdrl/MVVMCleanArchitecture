# ADR-001: Service Interface Design — Queries and Commands in a Single Interface

## Status

Accepted

## Date

2025-07-09

## Context

The Interface Segregation Principle (ISP) from SOLID suggests that interfaces should be kept small and focused. In CQRS-style architectures, it is common to separate read operations (queries) and write operations (commands) into distinct interfaces (e.g., `ICustomerQueryService` and `ICustomerCommandService`).

In this application, service interfaces like `ICustomerService` contain both query methods (e.g., `GetAllCustomersAsync`) and command methods (e.g., `AddCustomerAsync`, `DeleteCustomerAsync`).

## Decision

We intentionally keep queries and commands together in a single service interface per aggregate (e.g., `ICustomerService`).

**Reasons:**

- The application is a **WPF desktop app** with modest complexity — CQRS-style segregation adds indirection without a proportional benefit.
- ViewModels consume the full service via a single DI injection point, keeping the graph simpler.
- Splitting would require two separate registrations, two injected dependencies per ViewModel, and double the interface files with no measurable gain at this scale.
- Full CQRS can be adopted later (e.g., MediatR) if the domain grows to justify it — this decision does not block that path.

## Consequences

- A single interface per aggregate is the standard pattern in this codebase.
- Static analysis tools or code reviews flagging ISP violations on these interfaces should be treated as **acknowledged / accepted**, not as bugs.
- If the application grows significantly in complexity, revisit this decision and consider ADR-002 for a CQRS migration.

## References

- Martin Fowler — [CQRS](https://martinfowler.com/bliki/CQRS.html)
- Clean Architecture — Robert C. Martin, Chapter 20 (Interface Segregation Principle)
