# Wiki — MVVMCleanArchitecture

Bienvenido a la documentación técnica de **MVVMCleanArchitecture**, una aplicación de escritorio WPF construida sobre los principios de Clean Architecture y el patrón MVVM.

## Contenidos

| Documento | Descripción |
|---|---|
| [Funcionalidades](./funcionalidades.md) | Resumen de las capacidades de la aplicación y sus flujos principales |
| [Glosario](./glosario.md) | Definición de conceptos arquitectónicos, patrones y entidades del dominio |
| [Changelog](./changelog.md) | Historial de cambios entre versiones de la aplicación |

---

## Arquitectura general

El proyecto sigue una separación estricta en capas inspirada en **Clean Architecture**:

```
SharedKernel/               → Abstracciones compartidas (IAggregateRoot)
Wpf.Domain/                 → Entidades, lógica de dominio, Guard clauses
Wpf.Application/            → DTOs, interfaces de servicio, validadores FluentValidation
Wpf.Infrastructure/         → EF Core, SQLite, mappers Mapperly, implementaciones de servicio
WpfAppCleanArchitecture/    → Interfaz WPF, ViewModels (MVVM), diálogos
```

**Flujo de dependencias**: UI → Infrastructure → Application → Domain → SharedKernel

> Ninguna capa interna conoce las capas externas. El dominio no tiene referencias a EF Core ni a WPF.

---

## Tecnologías principales

| Tecnología | Uso |
|---|---|
| .NET 8 / C# 12 | Plataforma base |
| WPF | Interfaz de usuario |
| CommunityToolkit.Mvvm | Implementación MVVM (`ObservableObject`, `[RelayCommand]`) |
| Entity Framework Core | Acceso a datos |
| SQLite | Base de datos local |
| Mapperly | Mapeo entidad ↔ DTO en tiempo de compilación |
| FluentValidation | Validación de DTOs |
| Microsoft.Extensions.Hosting | Inyección de dependencias y ciclo de vida |
