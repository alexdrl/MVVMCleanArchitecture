# Glosario de conceptos y entidades

Este glosario reúne los términos arquitectónicos, patrones de diseño y entidades del dominio presentes en el proyecto **MVVMCleanArchitecture**. Está pensado como referencia rápida para desarrolladores que se incorporan al proyecto.

---

## Entidades del dominio

### `Customer` (Cliente)
Raíz de agregado principal. Representa a una persona registrada en el sistema.

| Propiedad | Tipo | Descripción |
|---|---|---|
| `Id` | `int` | Identificador único (generado por la base de datos) |
| `Name` | `string` | Nombre del cliente |
| `LastName` | `string` | Apellido del cliente |
| `Orders` | `ICollection<Order>` | Colección de pedidos asociados |

**Comportamientos:**
- `Rename(name, lastName)` — actualiza el nombre y apellido validando que no estén vacíos.
- `AddOrder(description)` — crea y añade un nuevo pedido garantizando que la descripción no sea vacía.

> `Customer` implementa `IAggregateRoot`, lo que indica que es el punto de entrada para modificar su grafo de objetos relacionados.

---

### `Order` (Pedido)
Entidad subordinada que pertenece a un `Customer`. No puede existir de forma independiente.

| Propiedad | Tipo | Descripción |
|---|---|---|
| `Id` | `int` | Identificador único |
| `Description` | `string` | Descripción del pedido |
| `CustomerId` | `int` | Clave foránea al cliente propietario |
| `Customer` | `Customer?` | Navegación al cliente (opcional en lecturas) |

---

## DTOs (Data Transfer Objects)

### `CustomerDto`
Representación plana de un cliente, usada para transferir datos entre capas sin exponer las entidades del dominio.

| Propiedad | Tipo |
|---|---|
| `Id` | `int` |
| `Name` | `string` |
| `LastName` | `string` |

### `OrderDto`
Representación plana de un pedido para consumo en la capa de presentación.

---

## Patrones y conceptos arquitectónicos

### Clean Architecture
Organización del código en capas concéntricas donde las dependencias **solo apuntan hacia el interior**. El dominio y la aplicación no conocen EF Core, SQLite ni WPF. Esto permite sustituir la base de datos o la interfaz de usuario sin modificar la lógica de negocio.

### MVVM (Model-View-ViewModel)
Patrón de presentación empleado en WPF. Separa la lógica de presentación (ViewModel) de la interfaz visual (View), enlazadas mediante *data binding*. Los ViewModels usan `CommunityToolkit.Mvvm` para reducir el código repetitivo.

### Aggregate Root
Una entidad que actúa como punto de acceso único a un grupo de objetos relacionados (agregado). En este proyecto, `Customer` es el agregado raíz y es la única vía para crear o modificar sus `Order`s.

### `BaseEntity<TId>`
Clase abstracta de la que heredan todas las entidades. Centraliza la propiedad `Id` con un tipo genérico, permitiendo usar `int`, `Guid` u otros como clave primaria.

### `IAggregateRoot`
Interfaz marcadora (sin miembros) del `SharedKernel`. Su presencia en una entidad señala que dicha entidad es una raíz de agregado y puede ser consultada directamente desde el repositorio o servicio.

### Guard Clauses
Método estático `Guard` en `Wpf.Domain.Common` que valida precondiciones al inicio de constructores y métodos de dominio, lanzando excepciones descriptivas si se violan invariantes (p. ej., nombre vacío).

```csharp
Guard.AgainstNullOrWhiteSpace(name, nameof(name));
```

### `Result<T>`
Tipo genérico en `Wpf.Application.Common` que encapsula el resultado de una operación que puede fallar por reglas de negocio. Evita el uso de excepciones para flujos esperados.

| Propiedad | Descripción |
|---|---|
| `IsSuccess` | `true` si la operación tuvo éxito |
| `Value` | Valor de retorno cuando `IsSuccess` es `true` |
| `ErrorMessage` | Mensaje de error cuando `IsSuccess` es `false` |

### `ICustomerService`
Interfaz de la capa de aplicación que define el contrato de operaciones sobre clientes. La implementación vive en la capa de infraestructura (`CustomerService`), siguiendo el principio de inversión de dependencias.

### FluentValidation / `CustomerValidator`
Librería de validación de entrada. Los validadores se registran como singletons en el contenedor DI y se invocan desde los servicios de infraestructura antes de persistir datos.

### Mapperly / `CustomerMapper`, `OrderMapper`
Librería de mapeo en tiempo de compilación. Genera código de mapeo eficiente entre entidades y DTOs sin reflexión en tiempo de ejecución.

### `IDbContextFactory<AppDbContext>`
Patrón de EF Core para crear instancias de `DbContext` bajo demanda. Cada operación de servicio abre su propio contexto con `using var db = await _dbContextFactory.CreateDbContextAsync(token)` y lo libera al finalizar, evitando problemas de concurrencia en aplicaciones de escritorio.

### `AppDbSeeder`
Clase estática de infraestructura que inserta datos de ejemplo en la primera ejecución de la aplicación, únicamente si la base de datos está vacía.

### Migrations (EF Core)
Historial versionado de cambios en el esquema de base de datos. Se aplican automáticamente al arrancar la aplicación mediante `db.Database.Migrate()`.

| Migración | Descripción |
|---|---|
| `InitialCreate` | Creación inicial de las tablas `Customers` y `Orders` |
| `AddCustomerLastName` | Añade la columna `LastName` a la tabla `Customers` |
| `AddCustomerLastNameValue` | Rellena el valor de `LastName` en registros existentes |

---

## Acrónimos de referencia rápida

| Acrónimo | Significado |
|---|---|
| DI | Dependency Injection (Inyección de dependencias) |
| DTO | Data Transfer Object |
| MVVM | Model-View-ViewModel |
| ORM | Object-Relational Mapper (EF Core) |
| SOLID | Single responsibility, Open/closed, Liskov, Interface segregation, Dependency inversion |
| TDD | Test-Driven Development |
