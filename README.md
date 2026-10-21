# Solicitudes de pago — Angular · .NET 8 · Java 21

Aplicación web para registrar y consultar solicitudes de pago. Está formada por tres componentes:

```
Angular (4200) ──► API .NET (5080) ──► Servicio Java de cálculo (8081)
                        │
                        ▼
                Registro en memoria
```

Angular envía la solicitud a .NET. La API valida los datos, solicita el cálculo de la comisión a Java mediante HTTP y guarda la solicitud **solo si el cálculo termina correctamente**. Angular consume únicamente la API .NET.

| Directorio | Componente | Tecnología |
|---|---|---|
| `fee-service-java/` | Servicio de cálculo de comisión | Java 21, Spring Boot 4.1, Maven |
| `payment-api-dotnet/` | API principal | C#, .NET 8, ASP.NET Core Web API (controladores) |
| `payment-web-angular/` | Interfaz de usuario | Angular 21, TypeScript, SCSS |

---

## 1. Prerrequisitos y versiones utilizadas

| Herramienta | Versión utilizada | Notas |
|---|---|---|
| Java (JDK) | 21.0.10 | Se requiere Java 21 o superior. |
| Maven | 3.9.11 (vía Maven Wrapper) | **No** es necesario instalar Maven: se usa `mvnw` / `mvnw.cmd` incluido. |
| Spring Boot | 4.1.1 | |
| springdoc-openapi | 3.1.1 | Swagger UI del servicio Java. |
| .NET SDK | 8.0.426 | Fijado en `payment-api-dotnet/global.json` (acepta cualquier SDK 8.0.4xx o superior de la línea 8.0). |
| ASP.NET Core | 8.0 | Swagger con Swashbuckle (incluido en la plantilla). |
| xUnit | Plantilla `dotnet new xunit` de .NET 8 | |
| Node.js | 24.14.0 | |
| npm | 11.9.0 | |
| Angular / Angular CLI | 21.2.x / 21.2.17 | Aplicación standalone y *zoneless*, estado con *signals*. |
| TypeScript | 5.9 | |
| Vitest | 4.x | Ejecutor de pruebas por defecto de Angular 21. |

Los comandos de este documento se escribieron para Windows (PowerShell) y para Linux/macOS (bash) cuando difieren.

---

## 2. Orden de inicio, puertos y URLs

Iniciar **en este orden**, cada componente en su propia terminal:

| # | Componente | Puerto | URL |
|---|---|---|---|
| 1 | Servicio Java | 8081 | http://localhost:8081 · Swagger: http://localhost:8081/swagger-ui.html |
| 2 | API .NET | 5080 | http://localhost:5080 · Swagger: http://localhost:5080/swagger |
| 3 | Angular | 4200 | **http://localhost:4200** ← interfaz de usuario |

---

## 3. Servicio Java (`fee-service-java/`)

```bash
cd fee-service-java

# Compilar y ejecutar pruebas (Maven Wrapper descarga Maven la primera vez)
./mvnw clean test            # Windows: .\mvnw.cmd clean test

# Ejecutar
./mvnw spring-boot:run       # Windows: .\mvnw.cmd spring-boot:run

# (Opcional) Generar el JAR y ejecutarlo
./mvnw clean package         # Windows: .\mvnw.cmd clean package
java -jar target/fee-service-0.0.1-SNAPSHOT.jar
```

El puerto se configura en `src/main/resources/application.properties` (`server.port=8081`).

---

## 4. API .NET (`payment-api-dotnet/`)

```bash
cd payment-api-dotnet

dotnet restore
dotnet build
dotnet test
dotnet run --project src/PaymentApi
```

La URL del servicio Java y el timeout están externalizados en `src/PaymentApi/appsettings.json`:

```json
"FeeService": {
  "BaseUrl": "http://localhost:8081/",
  "TimeoutSeconds": 5
}
```

También se pueden sobrescribir con variables de entorno, sin modificar el código, por ejemplo `FeeService__BaseUrl=http://otro-host:8081/`. La configuración se valida al iniciar (`ValidateOnStart`), por lo que una URL inválida impide arrancar la API en lugar de fallar en la primera solicitud.

Swagger UI se habilita en el entorno `Development`, que es el que usa `dotnet run` por defecto mediante `Properties/launchSettings.json`.

---

## 5. Angular (`payment-web-angular/`)

```bash
cd payment-web-angular

npm ci                         # instala dependencias exactas de package-lock.json
npm start                      # ng serve con proxy de desarrollo → http://localhost:4200
npm test -- --watch=false      # pruebas con Vitest
npm run build                  # compilación de producción en dist/
```

Angular llama a URLs relativas (`/api/v1/payment-requests`). El **proxy de desarrollo** (`proxy.conf.json`) redirige `/api` hacia `http://localhost:5080`, por lo que no se requiere configurar CORS. El script `npm start` ya incluye `--proxy-config proxy.conf.json`.

---

## 6. Pruebas automatizadas

| Componente | Comando | Pruebas | Qué verifican |
|---|---|---|---|
| Java | `./mvnw clean test` | 12 | Cálculo de los tres canales y redondeo `HALF_UP` (1.00 MOBILE → 0.01 / 1.01) con prueba parametrizada; endpoint con respuestas 200 y 400 (monto cero, más de dos decimales, canal inválido, JSON mal formado). |
| .NET | `dotnet test` | 25 | **Falla del servicio Java sin guardar ninguna solicitud** (sin levantar Java); referencia duplicada (409, un solo registro, sin llamar a Java); 10 casos de datos inválidos que no llaman a Java; *trim* de la referencia; comparación sensible a mayúsculas; cliente HTTP ante error 5xx, sin conexión, timeout y resultados inválidos (total inconsistente, valores negativos, campos faltantes, JSON inválido). |
| Angular | `npm test -- --watch=false` | 7 | **Referencia vacía y monto ≤ 0 detectados por el formulario**; no se envía un formulario inválido; registro exitoso con recarga de la lista; **manejo de error HTTP 503** mostrando el mensaje y conservando los datos; lista vacía; filtro por canal. |

---

## 7. Ejemplos con curl

> En Windows PowerShell, `curl` es un alias de `Invoke-WebRequest`. Use `curl.exe` (incluido en Windows 10/11) o los ejemplos de PowerShell que aparecen más abajo.

### Servicio Java

```bash
curl -X POST http://localhost:8081/api/v1/fees/calculate \
  -H "Content-Type: application/json" \
  -d '{"amount": 1000.00, "channel": "WEB"}'
# 200 → {"fee":10.00,"total":1010.00}
```

### API .NET

```bash
# Crear una solicitud → 201 Created
curl -i -X POST http://localhost:5080/api/v1/payment-requests \
  -H "Content-Type: application/json" \
  -d '{"externalReference": "PAY-2026-0001", "amount": 1000.00, "channel": "WEB"}'

# Repetir la misma referencia → 409 Conflict (DUPLICATE_REFERENCE)

# Datos inválidos → 400 Bad Request (VALIDATION_ERROR)
curl -i -X POST http://localhost:5080/api/v1/payment-requests \
  -H "Content-Type: application/json" \
  -d '{"externalReference": "   ", "amount": 0, "channel": "ATM"}'

# Consultar todas → 200 OK ([] si no hay registros)
curl http://localhost:5080/api/v1/payment-requests

# Consultar filtrando por canal (opcional) → 200 OK
curl "http://localhost:5080/api/v1/payment-requests?channel=WEB"
```

Ejemplo de respuesta de creación:

```json
{
  "id": "cfce2175-7896-406b-89fa-870f9304f0cd",
  "externalReference": "PAY-2026-0001",
  "amount": 1000.00,
  "channel": "WEB",
  "fee": 10.00,
  "total": 1010.00,
  "status": "PENDING"
}
```

### Equivalentes en PowerShell

```powershell
Invoke-RestMethod -Method Post -Uri http://localhost:5080/api/v1/payment-requests `
  -ContentType "application/json" `
  -Body '{"externalReference":"PAY-2026-0001","amount":1000.00,"channel":"WEB"}'

(Invoke-WebRequest -Uri http://localhost:5080/api/v1/payment-requests -UseBasicParsing).Content
```

### Verificar el comportamiento con Java detenido

1. Detener el servicio Java.
2. Crear una solicitud nueva → **503 Service Unavailable**:
   ```json
   { "code": "CALCULATION_SERVICE_UNAVAILABLE", "message": "No fue posible calcular la comisión. Intente nuevamente.", "details": [] }
   ```
3. Consultar la lista → conserva los registros previos y no incluye la solicitud fallida.

---

## 8. Distribución de responsabilidades

### Servicio Java

| Paquete | Responsabilidad |
|---|---|
| `domain.Channel` | **Regla de negocio**: cada canal con su tasa (WEB 1 %, MOBILE 0,5 %, BRANCH 1,5 %). Para modificar una tasa o agregar un canal se cambia solo este enum. |
| `service.FeeCalculatorService` | Cálculo con `BigDecimal`: `fee = amount × tasa` redondeado a 2 decimales con `RoundingMode.HALF_UP`; `total = amount + fee`. |
| `api.FeeController` + `api.dto` | Endpoint `POST /api/v1/fees/calculate` y validación de entrada con Bean Validation (monto > 0, máximo 2 decimales, canal válido). |
| `error.GlobalExceptionHandler` | Errores con estructura `{ code, message, details }`, sin trazas internas. |

### API .NET

| Carpeta | Responsabilidad |
|---|---|
| `Controllers/` | Controlador delgado: recibe la solicitud y delega en el servicio. No contiene consumo HTTP ni almacenamiento. |
| `Services/` | Lógica de negocio: validación y normalización (`PaymentRequestValidator`) y orquestación del flujo (`PaymentRequestService`): validar → verificar duplicado → calcular en Java → guardar. |
| `Integration/` | Cliente HTTP tipado hacia Java (`HttpFeeCalculationClient`) y validación del resultado recibido. |
| `Persistence/` | Almacenamiento en memoria detrás de la interfaz `IPaymentRequestRepository`, seguro ante concurrencia. |
| `Domain/` | Entidad `PaymentRequest`, canales y estados. El identificador y el estado `PENDING` los asigna el backend. |
| `Contracts/` | DTOs de entrada y salida, incluida la estructura de error. |
| `Errors/` | Excepciones de negocio y su traducción a HTTP (400, 409, 503, 500), además del formato común para 404/405/415 y JSON mal formado. |
| `Options/` | Configuración tipada y validada del servicio Java (URL y timeout). |

Todas las dependencias se registran mediante inyección de dependencias en `Program.cs`.

### Angular

| Archivo | Responsabilidad |
|---|---|
| `payment-request.service.ts` | Todas las llamadas HTTP a la API .NET. |
| `payment-request.model.ts` | Contratos con interfaces TypeScript, sin `any` en el flujo principal. |
| `payment-request.validators.ts` | Validadores del formulario reactivo (referencia no vacía y ≤ 50 caracteres tras *trim*; monto > 0 con hasta 2 decimales). |
| `api-error.ts` | Conversión de errores HTTP a mensajes comprensibles para el usuario. |
| `payment-requests-page.*` | Pantalla única: formulario reactivo, tabla, estados de carga, lista vacía y error, filtro por canal. El botón se deshabilita mientras se registra, y el formulario conserva los datos si la creación falla. |

---

## 9. Manejo de fallas de integración

- La URL base del servicio Java y un **timeout finito** (5 s por defecto) se configuran en `appsettings.json` y se aplican al `HttpClient` tipado registrado con `AddHttpClient`.
- `HttpFeeCalculationClient` transforma en `FeeServiceUnavailableException` cualquiera de estos casos:
  - servicio no disponible (error de conexión),
  - timeout vencido,
  - respuesta distinta de **HTTP 200**,
  - cuerpo con formato inválido,
  - `fee` o `total` ausentes o negativos,
  - `total` distinto de `amount + fee`.
- La API responde **503 `CALCULATION_SERVICE_UNAVAILABLE`** con un mensaje genérico. El motivo técnico se registra en el log como *warning* y **no se expone al usuario**.
- La solicitud se guarda **solo después** de recibir un cálculo válido, así que una falla nunca deja registros incompletos.
- No se implementan reintentos automáticos ni *circuit breaker*, de acuerdo con el enunciado.

### Formato de error común (ambas APIs)

```json
{ "code": "VALIDATION_ERROR", "message": "La solicitud contiene datos inválidos.", "details": ["amount: debe ser mayor que cero"] }
```

| HTTP | `code` | Cuándo |
|---|---|---|
| 400 | `VALIDATION_ERROR` | Datos inválidos (Java y .NET). |
| 400 | `MALFORMED_REQUEST` | JSON mal formado o con tipos incorrectos (Java y .NET). |
| 409 | `DUPLICATE_REFERENCE` | Referencia ya registrada (.NET). |
| 503 | `CALCULATION_SERVICE_UNAVAILABLE` | Java no disponible, timeout o resultado inválido (.NET). |
| 404 / 405 / 415 | `NOT_FOUND` / `METHOD_NOT_ALLOWED` / `UNSUPPORTED_MEDIA_TYPE` | Ruta, método o Content-Type no soportados (Java y .NET). |
| 500 | `INTERNAL_ERROR` | Error inesperado, sin exponer trazas (Java y .NET). |

---

## 10. Puntos opcionales implementados

- ✅ **OpenAPI y Swagger UI** en ambas APIs, con los códigos 201/400/409/503 documentados en .NET.
- ✅ **Prueba adicional de referencia duplicada** en .NET.
- ✅ **Prueba Angular del manejo de errores HTTP** (503).
- ✅ **Filtro por canal** en la consulta (`GET /api/v1/payment-requests?channel=WEB`) y en la interfaz.

---

## 11. Supuestos, limitaciones y requisitos no completados

**Requisitos no completados:** ninguno. Se implementaron todos los requisitos obligatorios y los cuatro puntos opcionales.

**Supuestos**

- Los canales se aceptan exactamente en mayúsculas (`WEB`, `MOBILE`, `BRANCH`); `web` se rechaza con 400.
- La referencia se guarda normalizada (sin espacios al inicio ni al final), y la unicidad se evalúa sobre ese valor normalizado distinguiendo mayúsculas y minúsculas.
- Un monto con ceros adicionales a la derecha (por ejemplo `10.100`) se considera equivalente a dos decimales. `10.123` se rechaza.
- El monto máximo es de 15 dígitos enteros y 2 decimales, con el mismo límite en Java y en .NET. Así, un monto aceptado por .NET nunca es rechazado por Java, lo que de otro modo se informaría erróneamente como 503.
- La consulta devuelve las solicitudes en orden de registro.
- La respuesta 201 no incluye la cabecera `Location`, porque el enunciado no define un endpoint de consulta por identificador.
- En la interfaz, el monto acepta punto o coma como separador decimal, y los montos se muestran con formato chileno (`1.010,00`).

**Limitaciones**

- Los datos se almacenan en memoria y se pierden al reiniciar la API .NET, según lo permitido por el enunciado.
- El almacenamiento en memoria evita duplicados concurrentes **dentro de una sola instancia**. Con varias instancias se requeriría una base de datos (ver la nota técnica 1).
- El proxy de Angular es solo para desarrollo. En un despliegue se usaría un *reverse proxy* o se configuraría la URL de la API por entorno junto con CORS.
- Swagger UI de .NET se expone solo en el entorno `Development`.
- En Angular el monto se envía como `number` de JavaScript. Para montos extremadamente grandes (más de 15 dígitos significativos) convendría enviarlo como texto.
- **.NET 8 finaliza su soporte el 10 de noviembre de 2026.** Se usó porque el enunciado lo exige; para producción se recomienda planificar la migración a .NET 10 (LTS).

---

## 12. Notas de criterio técnico

### 1. Registros duplicados ante solicitudes concurrentes y reemplazo de la memoria por una base de datos

**Situación actual.** El repositorio en memoria usa un `lock` y una operación `TryAdd` atómica. El servicio consulta primero si la referencia existe, para no llamar a Java en vano, y luego vuelve a validar al guardar. Si dos solicitudes con la misma referencia llegan al mismo tiempo, ambas pueden calcular en Java, pero **solo una se guarda** y la otra recibe 409.

**Con base de datos.**

- Crear una **restricción única** sobre `external_reference`, con una *collation* sensible a mayúsculas, para que coincida con la regla del enunciado. La base de datos pasa a ser la garantía final de unicidad aunque existan varias instancias de la API.
- Insertar y capturar la violación de unicidad para traducirla a **409**, en lugar de depender solo de una consulta previa, que tiene condición de carrera.
- Si se quiere evitar también la llamada duplicada a Java, reservar primero la referencia (insertar un registro en estado `CALCULATING`), calcular, y luego actualizarlo a `PENDING` o eliminarlo si el cálculo falla. Otra opción es aceptar una cabecera `Idempotency-Key`.

**Cómo se reemplaza.** Gracias a la interfaz `IPaymentRequestRepository`, basta con:

1. Implementar `EfCorePaymentRequestRepository` con un `DbContext`, la tabla, el índice único y migraciones.
2. Cambiar en `Program.cs` el registro `AddSingleton<IPaymentRequestRepository, InMemoryPaymentRequestRepository>()` por el `DbContext` y `AddScoped<IPaymentRequestRepository, EfCorePaymentRequestRepository>()`.
3. Mantener sin cambios el controlador, el servicio y las pruebas de negocio, y agregar pruebas de integración del repositorio (por ejemplo, con Testcontainers).

### 2. Qué revisaría primero si Angular recibe errores al registrar, pero la consulta funciona

Que la consulta funcione descarta que Angular, el proxy o la API .NET estén caídos. El problema está en el camino **exclusivo del POST**, y lo revisaría en este orden:

1. **Pestaña Network del navegador**: el código HTTP y el cuerpo de la respuesta del `POST`, porque el `code` indica la causa.
   - **503 `CALCULATION_SERVICE_UNAVAILABLE`**: es la causa más probable, porque el POST es la única operación que depende de Java. Verificaría que Java esté levantado en el puerto 8081, el valor de `FeeService:BaseUrl`, y los logs de .NET (*warning* "Fallo en la integración con el servicio de cálculo"), que indican si fue conexión, timeout o un resultado inválido.
   - **400 `VALIDATION_ERROR` / `MALFORMED_REQUEST`**: diferencias entre el cuerpo enviado y el contrato (nombres de campos, tipos o el canal en minúsculas).
   - **409**: la referencia ya existe.
   - **404 / 405 / 415**: URL, método o `Content-Type` incorrectos, o una regla de proxy distinta para el POST.
2. **Request payload** en Network: comparar el JSON enviado con el contrato.
3. **Logs de .NET y de Java**, correlacionando por hora y referencia.
4. Reproducir el POST con curl directamente contra .NET y luego contra Java, para aislar el componente que falla.

### 3. Precauciones al actualizar de .NET 5 a .NET 8 y cómo comunicarlo

**Contexto.** .NET 5 no tiene soporte desde mayo de 2022, así que la actualización reduce un riesgo de seguridad. A la vez, atraviesa tres versiones (6, 7 y 8) con cambios incompatibles.

**Precauciones**

1. **Inventario previo**: proyectos, paquetes NuGet (especialmente EF Core, autenticación, Swashbuckle y librerías de terceros), versiones de SDK en CI/CD, imágenes Docker y entornos de despliegue.
2. **Revisar los *breaking changes*** oficiales de .NET 6, 7 y 8 y de ASP.NET Core y EF Core en cada versión. Usar .NET Upgrade Assistant como apoyo, no como garantía.
3. **Actualizar de forma incremental** en una rama: `TargetFramework` a `net8.0`, un `global.json` con el SDK fijado, y los paquetes a versiones compatibles. La migración de `Startup.cs` al modelo de *minimal hosting* es opcional y se puede hacer en un segundo paso.
4. **Puntos que suelen romperse**:
   - comportamiento de serialización con `System.Text.Json`,
   - cambios de EF Core (consultas, migraciones, conversiones),
   - autenticación y autorización,
   - activación de *nullable reference types*,
   - nuevas advertencias de analizadores,
   - las imágenes Docker de .NET 8, que escuchan por defecto en el **puerto 8080** (antes 80) y se ejecutan como usuario no root.
5. **Validación**:
   - compilar sin nuevas advertencias críticas,
   - ejecutar las pruebas unitarias y de integración (agregando pruebas de caracterización donde falte cobertura),
   - pruebas de humo en un entorno de *staging* equivalente a producción,
   - comparar métricas de rendimiento y errores antes y después.
6. **Despliegue con plan de reversa**: mantener disponible la versión anterior y, si es posible, liberar de forma gradual (*canary* o *blue-green*).

**Comunicación al equipo**

- Un documento breve con el **motivo** (soporte y seguridad), el **alcance** (proyectos y dependencias afectados), los **riesgos identificados** con su probabilidad e impacto, y el **plan por etapas** con responsables y fechas.
- **Criterios de aceptación** explícitos: pruebas en verde, sin regresiones en *staging* y métricas estables.
- **Ventana de despliegue** acordada, **plan de reversa** definido y comunicación del resultado después de liberar.
- Advertir que .NET 8 termina su soporte el 10 de noviembre de 2026, por lo que conviene planificar desde ya el paso a .NET 10 (LTS) y no repetir la situación.
