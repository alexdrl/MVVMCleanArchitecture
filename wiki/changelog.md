# Changelog

Aquí encontrarás un resumen de los cambios de cada versión explicados de forma clara y sin tecnicismos.

---

## [Sin publicar]

> Mejoras y correcciones que están en camino pero todavía no forman parte de ninguna versión oficial.

- *(Próximamente)*

---

## [1.1.0] — 2026-04-27

### ✨ Novedades
- Los clientes ahora tienen **nombre y apellido** por separado. Antes solo se registraba el nombre.
- Al crear o editar un cliente, el formulario incluye un campo específico para el apellido.
- No es posible guardar un cliente si el apellido está en blanco — la aplicación avisa al instante.

### 🔄 Cambios
- El listado de clientes muestra ahora nombre y apellido de forma diferenciada.
- Los clientes ya existentes en la base de datos conservan sus datos; el apellido se ha completado automáticamente.

---

## [1.0.0] — 2025-06-19

### ✨ Novedades
- Primera versión de la aplicación.
- Pantalla principal con el listado completo de clientes.
- Posibilidad de **añadir**, **editar** y **eliminar** clientes desde la pantalla principal.
- Cada cliente puede tener uno o varios **pedidos** asociados.
- Al eliminar un cliente, la aplicación avisa si tiene pedidos vinculados y evita borrados accidentales.
- Los datos se guardan de forma automática en local; no es necesario ningún servidor externo.
- La aplicación incluye clientes y pedidos de ejemplo para facilitar las primeras pruebas.

---

## Guía para añadir una nueva entrada

Cuando se publique una nueva versión, añade una sección con este formato **encima** de la anterior:

```markdown
## [X.Y.Z] — AAAA-MM-DD

### ✨ Novedades
- Qué puede hacer el usuario ahora que antes no podía.

### 🔄 Cambios
- Qué ha cambiado en algo que ya existía.

### 🐛 Correcciones
- Qué problema o comportamiento incorrecto se ha resuelto.

### 🗑️ Eliminado
- Qué ha dejado de estar disponible y por qué.
```

> **Tipos de cambio:** `✨ Novedades` · `🔄 Cambios` · `🐛 Correcciones` · `🗑️ Eliminado` · `⚠️ Obsoleto` · `🔒 Seguridad`
