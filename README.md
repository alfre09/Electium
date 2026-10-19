# Electium — Plataforma de votación electrónica

Mini proyecto ITLA · ASP.NET Core MVC (.NET 9) · EF Core Code First (SQLite) · Arquitectura Onion

## Cómo ejecutarlo

1. Instala el **SDK de .NET 9** y abre `Electium.sln` en Visual Studio 2022 (17.12 o superior).
2. Marca **Electium.Web** como proyecto de inicio y presiona **F5**.
3. La primera vez se crea `electium.db` (SQLite) con datos de ejemplo.

> Si tu Visual Studio solo tiene .NET 10, cambia `net9.0` por `net10.0` en los cinco `.csproj`
> (el documento acepta 9 o 10; usa la misma versión en todos los proyectos).

### Cuentas de prueba

| Rol | Correo | Contraseña |
|---|---|---|
| Administrador | `admin@electium.local` | `Admin123*` |
| Dirigente político (Partido Horizonte) | `dirigente@electium.local` | `Lider123*` |

Cambia estas credenciales antes de cualquier uso real (`Persistence/Seed/DbSeeder.cs`).

### Probar el flujo completo

1. Entra como **admin** → *Elecciones*. Existe "Elecciones Generales" en **borrador** con 6 candidatos.
2. Pulsa **Abrir**. (Una elección sin candidatos activos no se puede abrir.)
3. Cierra sesión y entra a **Votar** como elector (sin login). Documentos de prueba, del 1.º al 12.º ciudadano:
   `00100000011`, `00100000022`, `00100000033`, … `00100000132` (el patrón es 001 + n×11 con 8 dígitos).
   Los ciudadanos 7 al 12 no son candidatos.
4. Vota. Vuelve como admin o dirigente → *Resultados*.
5. **Cerrar** la elección cuando termine la votación.

## Interfaz

El diseño institucional (barra tricolor, navegación, portada y pie) está en `Views/Shared/_Layout.cshtml`,
`Views/Home/Index.cshtml` y `wwwroot/css/site.css` (clases con prefijo `dr-`). La portada muestra los módulos
según el rol (administrador, dirigente o visitante).

La imagen de fondo de la portada va en `src/Electium.Web/wwwroot/imaghine/bandera-dominicana-hero.jpg`.
Si no existe, se ve un degradado azul. Las fuentes, los iconos y la bandera se cargan desde internet (CDN).

## Arquitectura (Onion)

```
Electium.Domain        Entidades y enums. No depende de nadie.
Electium.Application   Interfaces, DTOs, ViewModels, AutoMapper, servicios con reglas de negocio.
Electium.Persistence   DbContext (EF Core + SQLite), repositorio genérico, Identity, semillas.
Electium.Shared        Envío de correo (MailKit) y OCR (Tesseract).
Electium.Web           MVC: controladores y vistas (Bootstrap 5).
```

- **Repositorio genérico:** `IGenericRepository<T>` / `GenericRepository<T>`.
- **Servicio genérico:** `IGenericService<TDto,TEntity>` / `GenericService<TDto,TEntity>`.
- **Controlador genérico:** `CrudController<TViewModel,TDto,TEntity>` (ciudadanos, partidos, puestos y elecciones).
- **DTOs vs ViewModels:** los servicios hablan en DTOs; las vistas en ViewModels; AutoMapper convierte (`Mappings/GeneralProfile.cs`).

## Roles

| Rol | Acceso |
|---|---|
| **Elector** (sin login) | Verifica documento + foto (OCR), vota una vez por elección, recibe correo de confirmación. |
| **Administrador** | CRUD de ciudadanos, partidos, puestos electivos, elecciones, candidatos y dirigentes; abre/cierra elecciones; ve resultados. |
| **Dirigente político** | Gestiona solo los candidatos de **su** partido y ve resultados de su partido. |

## Reglas de negocio implementadas

- Documento de identidad único, 11 dígitos (se guarda sin guiones).
- Solo ciudadanos **activos** pueden votar o ser candidatos.
- Un ciudadano solo puede ser candidato una vez por elección; un partido, un candidato por puesto y elección.
- Los candidatos solo se crean/editan/eliminan con la elección en **borrador**.
- Solo una elección abierta a la vez; una elección cerrada no se reabre.
- **Un voto por ciudadano y elección** (validado en el servicio y con índice único en la base de datos).
- El voto es secreto: el correo de confirmación no dice por quién votó.
- No se pueden borrar registros con información relacionada (mensaje claro en lugar de error).

## OCR (Tesseract)

La carpeta `src/Electium.Web/tessdata` trae `eng.traineddata`, suficiente para leer los dígitos del documento.
Para mejorar la lectura en español descarga `spa.traineddata` desde
<https://github.com/tesseract-ocr/tessdata_fast>, cópialo a esa carpeta y cambia `"Language"` a `"spa"` en `appsettings.json`.

- Requiere el **Visual C++ Redistributable x64** en Windows.
- Para saltarte el OCR en desarrollo: `"Ocr": { "Enabled": false }`.
- La imagen no se guarda; solo se procesa en memoria.

## Correo (SMTP)

Por defecto el envío está **deshabilitado** (solo se escribe un log). Para activarlo, en `appsettings.json`
(o mejor con *User Secrets*):

```json
"Email": {
  "Enabled": true,
  "Host": "smtp.gmail.com",
  "Port": 587,
  "User": "tu-correo@gmail.com",
  "Password": "contraseña-de-aplicación",
  "From": "tu-correo@gmail.com",
  "FromName": "Electium"
}
```

Con Gmail debes usar una *contraseña de aplicación*, no la contraseña normal.

## Migraciones de EF Core (Code First)

Sin migraciones, la app crea la base con `EnsureCreated`. Para usar migraciones reales:

```bash
dotnet tool install --global dotnet-ef
# borra primero electium.db si ya existe
dotnet ef migrations add Initial --project src/Electium.Persistence --startup-project src/Electium.Web
dotnet ef database update      --project src/Electium.Persistence --startup-project src/Electium.Web
```

Desde ese momento la app aplica las migraciones automáticamente al arrancar.

## Supuestos (ajústalos al documento funcional v1.1)

Esta solución se armó sin tener el documento funcional completo, a partir de los requisitos generales
(roles, Onion, EF Core, AutoMapper, OCR, correo). Revisa contra el documento: nombres de pantallas,
campos exactos de cada entidad, si el voto en blanco debe registrarse, y si el dirigente debe ver
resultados mientras la elección está abierta.

Se descartó `IMacroIndicator` del proyecto Domain original porque no parecía formar parte de Electium.
