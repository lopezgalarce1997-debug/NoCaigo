# NoCaigo – Detector de estafas con IA

## Objetivo
API REST de portafolio (autor: Bladimir). Una persona pega un mensaje sospechoso (SMS, WhatsApp o correo) y recibe:
veredicto (`Seguro` / `Sospechoso` / `Estafa`), nivel de riesgo 0–100, tipo de estafa y señales explicadas en lenguaje simple.
Los análisis se guardan y hay estadísticas (por tipo de estafa y tendencia semanal).

Demuestra: ASP.NET Core Web API, SQL Server + EF Core, xUnit + Moq, GitHub Actions e integración de IA.

## Forma de trabajo (obligatorio)
- Etapas pequeñas. **Detenerse al final de cada paso** para que Bladimir revise, pruebe y confirme.
- Explicar brevemente cada decisión de diseño importante; debe poder defender cada línea en una entrevista.
- Nunca generar todo el proyecto de una vez.
- Si hay varias alternativas razonables, presentarlas y dejarlo elegir.
- Código, nombres y comentarios en español cuando sea natural, con convenciones C# (PascalCase, etc.).
- Commits pequeños y descriptivos al final de cada paso.

## Stack
- ASP.NET Core Web API sobre .NET 10 (LTS), fijado con `global.json`
- SQL Server LocalDB (`(localdb)\MSSQLLocalDB`) + EF Core con migraciones
- xUnit + Moq, Swagger/OpenAPI
- IA detrás de interfaz, proveedor por configuración: Ollama (local) y Gemini o Groq (capa gratuita)
- GitHub (repo público `NoCaigo`), GitHub Projects (Kanban), GitHub Actions (`.github/workflows/ci.yml`)

## Arquitectura (Clean Architecture)
- **NoCaigo.Domain**: entidades, enums, reglas de negocio puras. Sin dependencias.
- **NoCaigo.Application**: casos de uso, DTOs, interfaces (`IServicioIA`, `IRepositorioAnalisis`, `IReglaDeteccion`, `IAnonimizador`), combinación de puntajes.
- **NoCaigo.Infrastructure**: EF Core, DbContext, repositorios, clientes de IA.
- **NoCaigo.Api**: controladores, DI, Swagger, manejo global de errores.
- **NoCaigo.Tests**: pruebas xUnit.

## Análisis híbrido
1. **Anonimizador**: reemplaza teléfonos, RUT, correos y tarjetas por `[TELEFONO]`, `[RUT]`, `[CORREO]`, `[TARJETA]` antes de guardar o enviar a la IA.
2. **Reglas (Strategy)**: cada regla implementa `IReglaDeteccion` → activada, puntos de riesgo, descripción. Iniciales: links acortados, dominios que imitan bancos/empresas chilenas, urgencia, pedido de claves/códigos/coordenadas/tarjeta, pedido de pago/transferencia, premios inesperados.
3. **IA**: recibe texto anonimizado, responde solo JSON fijo (tipo, riesgo, veredicto, señales, explicación). Parseo seguro.
4. **Combinación**: reglas + IA. Si la IA falla (error/timeout/cuota), se responde solo con reglas y se indica (`UsoIA = false`).

## Modelo de datos
- **Analisis**: Id, TextoAnonimizado, Canal (enum), NivelRiesgo, Veredicto (enum), TipoEstafaId, Explicacion, UsoIA, FechaCreacion
- **TipoEstafa** (semilla): Falso banco, Paquete retenido, Falso familiar, Premio falso, Falsa oferta de trabajo, Inversión falsa, Otro, Ninguno
- **Senal**: Id, AnalisisId, Descripcion, Origen (Regla / IA)

## Endpoints
- `POST /api/analisis`
- `GET /api/analisis/{id}`
- `GET /api/analisis?tipo=&canal=&desde=&hasta=&pagina=`
- `GET /api/estadisticas/por-tipo`
- `GET /api/estadisticas/tendencia`

## Seguridad
- Claves nunca en el repo: User Secrets (local), variables de entorno (servidor), GitHub Secrets (CI).
- `appsettings.json` solo con configuración de IA sin secretos.
- Validación de entrada: texto obligatorio, largo máximo.

## Pruebas
Cada regla (positivos/negativos), anonimizador, combinación de puntajes (incl. falla de IA con Moq), parseo JSON de IA (incl. mal formado).

## Plan
**Sprint 1**
1. Solución + 5 proyectos con referencias
2. Entidades, enums, DbContext, migración inicial y semilla
3. Anonimizador + pruebas
4. Reglas de detección + pruebas
5. Caso de uso solo con reglas + endpoints en Swagger

**Sprint 2**
6. `IServicioIA`: Ollama, luego Gemini o Groq, por configuración
7. Combinación de puntajes y fallas de IA + pruebas
8. Endpoints de estadísticas
9. GitHub Actions CI + badge en README
10. README (descripción, capturas, ejecución, decisiones, aviso de que el resultado es orientativo)

## Estado
- [x] Preparación: entorno (.NET 10, LocalDB, gh) y git local
- [x] Repo público: https://github.com/lopezgalarce1997-debug/NoCaigo (cuenta `gh`: lopezgalarce1997-debug)
- [x] Tablero Kanban: https://github.com/users/lopezgalarce1997-debug/projects/1 — un issue por paso (#1–#10). Los commits de cada paso cierran su issue con `Closes #N`
- [x] Paso 1: solución `NoCaigo.slnx` con 5 proyectos (`src/`, `tests/`) y referencias
- [x] Paso 2: entidades, enums, DbContext, migración `Inicial` y semilla (aplicada en LocalDB)
- [x] Paso 3: `IAnonimizador` + `Anonimizador` (GeneratedRegex) con pruebas
- [x] Paso 4: 6 reglas `IReglaDeteccion` (Strategy + base Template Method `ReglaPorPalabrasClave`) con pruebas
- [x] Paso 5: `ServicioAnalisis` (solo reglas), `RepositorioAnalisis`, `AnalisisController`, Swagger UI, manejo global de errores
- [x] Paso 6: `IServicioIA` + `ClienteIACompatibleOpenAI` (un cliente para Ollama y Groq, `IA:ProveedorActivo`), `ParserRespuestaIA`, pruebas con HttpMessageHandler falso. Aún NO conectado a `ServicioAnalisis` (paso 7)
- [x] Paso 7: `CombinadorPuntajes` → `final = max(reglas, PesoReglas·reglas + PesoIA·IA)` (sección `Combinacion`, 0.4/0.6); IA falla → solo reglas + `UsoIA=false` + aviso. Regla 7 `ReglaIntentoManipulacion` (40 pts). Mensaje a la IA como JSON `{"mensaje": ...}` separado del prompt system

## Comandos útiles
- Ejecutar API: `dotnet run --project src/NoCaigo.Api` (Swagger en http://localhost:5224/swagger)
- Pruebas: `dotnet test`
- Migración nueva: `dotnet ef migrations add <Nombre> -p src/NoCaigo.Infrastructure -s src/NoCaigo.Api -o Persistencia/Migraciones`
- Aplicar: `dotnet ef database update -p src/NoCaigo.Infrastructure -s src/NoCaigo.Api`
- IA local: Ollama con `llama3.2:3b` en http://localhost:11434 (primera llamada ~30 s mientras carga el modelo)
- Groq: `dotnet user-secrets set "IA:Proveedores:Groq:ApiKey" "<clave>" --project src/NoCaigo.Api` y `IA:ProveedorActivo=Groq`
- `dotnet-ef` es herramienta local (`dotnet-tools.json`): `dotnet tool restore`
