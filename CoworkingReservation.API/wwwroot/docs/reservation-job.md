# Job de Actualización de Reservas Expiradas

## Descripción

Este job se encarga de **actualizar automáticamente** las reservas de coworking que han finalizado y que continúan en estado `Confirmed`. Al actualizar estas reservas a `Completed`, los usuarios pueden dejar sus reviews.

---

## Funcionamiento

* El job está implementado utilizando **Hangfire**.
* Se ejecuta de forma **recurrente todos los días a las 2:00 AM**.
* Además, el job se **dispara inmediatamente al iniciar la aplicación**.
* La lógica del job:

  * Busca reservas con:

    * `Status = Confirmed`
    * `EndDate < DateTime.Today`
  * Actualiza el estado de estas reservas a `Completed`.
  * Registra logs en consola sobre la ejecución.
  * Inserta registros en la tabla `AuditLogs` para cada reserva actualizada.

---

## Configuración de Hangfire

### Programación Recurrente

```csharp
RecurringJob.AddOrUpdate<IReservationJobService>(
    "complete-expired-reservations-job",
    job => job.CompleteExpiredReservationsAsync(),
    Cron.Daily(2)
);
```

### Ejecución Inmediata al Levantar la App

```csharp
RecurringJob.Trigger("complete-expired-reservations-job");
```

### Dashboard de Hangfire

Disponible en:

```
https://localhost:<puerto>/hangfire
```

---

## Auditoría

Por cada reserva actualizada, se registra una entrada en la tabla `AuditLogs` con la siguiente información:

* **Acción:** `ReservationCompletedByJob`
* **Descripción:** `Reserva {Id} completada automáticamente.`
* **Fecha:** Fecha y hora de ejecución del job.
* **UserId:** `null` (indica que es una acción automática del sistema).

---

## Beneficios

* Automatiza el cierre de reservas expiradas.
* Habilita la generación de reviews por parte de los usuarios.
* Mantiene la integridad y consistencia de los estados de reserva.
* Proporciona trazabilidad a través de logs y registros en base de datos.
* Permite monitorear ejecuciones y posibles errores desde el Dashboard de Hangfire.

---

## Consideraciones Futuras

* Agregar notificaciones automáticas a los usuarios para recordarles que pueden dejar una review.
* Registrar métricas en Application Insights para monitoreo avanzado.
* Posibilidad de agregar procesamiento por lotes si el volumen de reservas es muy alto.
