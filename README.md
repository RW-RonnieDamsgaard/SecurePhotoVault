# SecurePhotoVaultMAUI

**SecurePhotoVaultMAUI** er en .NET MAUI-app, der giver brugeren mulighed for at oprette en adgangskodebeskyttet fotovalv, hvor billeder kan gemmes krypteret og dekrypteres efter behov. Appen fungerer på Android, iOS, Windows og Mac Catalyst.

## Funktioner

- Opret og bekræft adgangskode ved første opstart
- Login med adgangskode ved efterfølgende opstart
- Tilføj billeder til valvet (ukrypteret ved tilføjelse)
- Krypter og dekrypter billeder med ét klik
- Automatisk session-timeout og re-kryptering af billeder ved inaktivitet
- Sikker lagring af adgangskode-hash og salt i SecureStorage
- Mulighed for at nulstille appen (rydder brugerdata)

## Teknologi

- .NET 8 & .NET MAUI
- Argon2 til password hashing
- SecureStorage til sikker lagring af credentials
- MVVM-arkitektur

## Installation

1. **Krav:**  
   - .NET 8 SDK  
   - Visual Studio 2022 med MAUI workload

2. **Klon repoet:**

`git clone <repo-url> cd SecurePhotoVaultMAUI`

3. **Åbn løsningen i Visual Studio 2022.**

4. **Vælg ønsket platform (Android, iOS, Windows, MacCatalyst) og kør projektet.**

## Brug

1. Ved første opstart skal du oprette en adgangskode og bekræfte den.
2. Tilføj billeder via "Tilføj billede"-knappen.
3. Krypter/dekrypter billeder direkte fra billedlisten.
4. Log ud eller vent på session-timeout for automatisk re-kryptering og logout.

## Projektstruktur

- **Views/**: XAML-sider (UI)
- **ViewModels/**: MVVM ViewModels
- **Services/**: AuthService, billedkryptering mv.
- **Helper/**: Session management og sikkerhedshjælpere

## Sikkerhed

- Adgangskode hashes med Argon2 og unik salt
- Hash og salt gemmes i SecureStorage
- Billeder krypteres/dekrypteres kun lokalt
- Automatisk session-timeout beskytter mod uautoriseret adgang


   
