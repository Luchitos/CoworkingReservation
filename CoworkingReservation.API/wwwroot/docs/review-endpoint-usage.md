# Endpoint de Reseñas - Documentación de Uso

## Crear Reseña

### URL
```
POST /api/Review/create
```

### Headers Requeridos
```
Content-Type: application/json
Authorization: Bearer {token}
```

### Payload
```json
{
  "reservationId": 123,
  "rating": 5,
  "comment": "Excelente espacio de trabajo, muy cómodo y bien equipado!"
}
```

### Validaciones
- `reservationId`: Obligatorio, debe ser mayor a 0
- `rating`: Obligatorio, debe estar entre 1 y 5
- `comment`: Opcional, máximo 1000 caracteres

### Responses

#### ✅ Éxito (201 Created)
```json
{
  "status": 201,
  "data": {
    "id": 456,
    "message": "Reseña creada exitosamente"
  },
  "error": null
}
```

#### ❌ Error - Reserva ya reseñada (400 Bad Request)
```json
{
  "status": 400,
  "data": null,
  "error": "Ya existe una reseña para esta reserva."
}
```

#### ❌ Error - Reserva no completada (400 Bad Request)
```json
{
  "status": 400,
  "data": null,
  "error": "Solo se pueden reseñar reservas completadas."
}
```

#### ❌ Error - Reserva no encontrada (404 Not Found)
```json
{
  "status": 404,
  "data": null,
  "error": "La reserva especificada no existe."
}
```

#### ❌ Error - No autorizado (403 Forbidden)
```json
{
  "status": 403,
  "data": null,
  "error": "No tienes autorización para reseñar esta reserva."
}
```

#### ❌ Error - Validación (422 Unprocessable Entity)
```json
{
  "status": 422,
  "data": null,
  "error": "Datos de validación incorrectos: La calificación debe estar entre 1 y 5 estrellas"
}
```

## Verificar Elegibilidad

### URL
```
GET /api/Review/eligibility?reservationId=123
```

### Headers Requeridos
```
Authorization: Bearer {token}
```

### Response
```json
{
  "status": 200,
  "data": {
    "canReview": true,
    "reason": "Puede crear reseña"
  },
  "error": null
}
```

## Reglas de Negocio

1. **Autenticación**: El usuario debe estar autenticado
2. **Autorización**: Solo el usuario que hizo la reserva puede reseñarla
3. **Estado de reserva**: Solo se pueden reseñar reservas con estado `COMPLETED` (4)
4. **Fecha**: Solo se pueden reseñar reservas cuya fecha de finalización ya haya pasado
5. **Unicidad**: Una reserva solo puede ser reseñada una vez
6. **Calificación**: Debe estar entre 1 y 5 estrellas
7. **Comentario**: Máximo 1000 caracteres, es opcional

## Ejemplo de Uso en JavaScript

```javascript
// Verificar elegibilidad
const checkEligibility = async (reservationId, token) => {
  const response = await fetch(`/api/Review/eligibility?reservationId=${reservationId}`, {
    headers: {
      'Authorization': `Bearer ${token}`
    }
  });
  return response.json();
};

// Crear reseña
const createReview = async (reviewData, token) => {
  const response = await fetch('/api/Review/create', {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json',
      'Authorization': `Bearer ${token}`
    },
    body: JSON.stringify(reviewData)
  });
  return response.json();
};

// Uso completo
const handleCreateReview = async () => {
  try {
    // 1. Verificar elegibilidad
    const eligibility = await checkEligibility(123, userToken);
    
    if (!eligibility.data.canReview) {
      alert(`No se puede crear la reseña: ${eligibility.data.reason}`);
      return;
    }

    // 2. Crear reseña
    const reviewData = {
      reservationId: 123,
      rating: 5,
      comment: "Excelente experiencia!"
    };

    const result = await createReview(reviewData, userToken);
    
    if (result.status === 201) {
      alert('Reseña creada exitosamente!');
      // Actualizar UI
    } else {
      alert(`Error: ${result.error}`);
    }
  } catch (error) {
    console.error('Error al crear reseña:', error);
    alert('Error interno del servidor');
  }
};
``` 