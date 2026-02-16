# Parbaudes darbs — ASP.NET Core Web API

Šis projekts demonstrē REST API ar:
- **Entity Framework Core** un relāciju datubāzi (SQLite),
- **GET/POST endpointiem** vairākām entītijām,
- **Swagger (Swashbuckle)** dokumentāciju un testēšanu,
- **JWT autentifikāciju/autorizāciju**.

## Domēna modelis (5 tabulas)

1. **Users**
   - Lietotāji ar `Email`, `PasswordHash`, `Role`.
2. **Categories**
   - Preču kategorijas.
3. **Products**
   - Preces ar cenu un sasaisti uz kategoriju (`CategoryId`).
4. **Orders**
   - Pasūtījumi ar sasaisti uz lietotāju (`UserId`).
5. **OrderItems**
   - Pasūtījuma rindas ar sasaisti uz pasūtījumu un preci (`OrderId`, `ProductId`).

## Relācijas
- `Category (1) -> (N) Product`
- `User (1) -> (N) Order`
- `Order (1) -> (N) OrderItem`
- `Product (1) -> (N) OrderItem`

## Galvenie endpointi

### Auth
- `POST /api/auth/register` — lietotāja reģistrācija.
- `POST /api/auth/login` — autentifikācija, JWT saņemšana.

### Categories
- `GET /api/categories` — visas kategorijas.
- `POST /api/categories` — jauna kategorija (**Admin**).

### Products
- `GET /api/products` — visas preces.
- `POST /api/products` — jauna prece (**Admin**).

### Orders
- `GET /api/orders` —
  - Admin redz visus pasūtījumus,
  - Customer redz tikai savus.
- `POST /api/orders` — jauna pasūtījuma izveide (autorizēts lietotājs).

## JWT darbības princips

JWT = `header.payload.signature`

1. Klients nosūta `email/parole` uz `POST /api/auth/login`.
2. API pārbauda lietotāju datubāzē.
3. API izveido tokenu ar claimiem (`sub`, `nameidentifier`, `email`, `role`) un paraksta to ar slepeno atslēgu.
4. Klients pievieno tokenu pie katra aizsargātā pieprasījuma:
   - `Authorization: Bearer <token>`
5. API validē tokenu (issuer, audience, signature, lifetime).

## Swagger
- Swagger UI pieejams `Development` vidē.
- Endpointi dokumentēti ar XML komentāriem (`summary`) un modeļi redzami kā `schemas`.
- Konfigurēta Bearer autentifikācija Swagger UI testēšanai.

## Lokāla palaišana

### Variants A (vienkāršākais)

```bash
dotnet restore
dotnet run --launch-profile "ParbaudesDarbs.Api"
```

Tas automātiski palaiž API uz `http://localhost:5164` un atver Swagger pārlūkprogrammā (`/swagger`).

### Variants B (ja nelieto launch profile)

```bash
ASPNETCORE_ENVIRONMENT=Development dotnet run --urls "http://localhost:5164"
```

Tad pārlūkprogrammā atver:

- `http://localhost:5164/swagger` — Swagger UI
- `http://localhost:5164/api/products` — piemēra GET endpoint

## Demo lietotāji (seed)
- Admin: `admin@example.com` / `Admin123!`
- Customer: `user@example.com` / `User123!`

## Arhitektūra
- `Models/` — EF Core entītijas.
- `Data/` — `AppDbContext` un sākuma datu seederis.
- `DTOs/` — ievades/izvades modeļi API līmenim.
- `Controllers/` — REST endpointi.
- `Services/` — paroles hash un JWT ģenerēšana.
