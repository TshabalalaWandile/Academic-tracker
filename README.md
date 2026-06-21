<div align="center">

# 📘 Academic Tracker

### A cross-platform .NET MAUI app for tracking module marks and academic progress

[![.NET](https://img.shields.io/badge/.NET-9.0-512BD4?style=for-the-badge&logo=dotnet)](https://dotnet.microsoft.com/)
[![MAUI](https://img.shields.io/badge/.NET%20MAUI-Multi--Target-512BD4?style=for-the-badge&logo=dotnet)](https://learn.microsoft.com/dotnet/maui/)
[![C#](https://img.shields.io/badge/Language-C%23-239120?style=for-the-badge&logo=csharp)](https://learn.microsoft.com/dotnet/csharp/)
[![SQLite](https://img.shields.io/badge/Database-SQLite-07405E?style=for-the-badge&logo=sqlite)](https://www.sqlite.org/)
[![License](https://img.shields.io/badge/License-MIT-yellow?style=for-the-badge)](#license)


</div>

<br>

## Table of Contents

- [About The Project](#about-the-project)
  - [Built With](#built-with)
  - [Data Model](#data-model)
- [Features](#features)
- [Getting Started](#getting-started)
  - [Prerequisites](#prerequisites)
  - [Installation](#installation)
- [Usage](#usage)
- [Project Structure](#project-structure)
- [How the Running Mark Is Calculated](#how-the-running-mark-is-calculated)
- [Roadmap](#roadmap)
- [Team](#team)
- [Contributing](#contributing)
- [License](#license)
- [Contact](#contact)
- [Acknowledgments](#acknowledgments)

<br>

## About The Project

**Academic Tracker** is a mobile and desktop app that lets students log their modules, record assessment marks as they come in, and instantly see whether they're on pace to hit their target grade for each module — without doing the weighted-average math by hand every time a new mark comes in.

Each module carries a target mark. Every assessment recorded against that module (tests, assignments, practicals, exams) carries its own weighting and score. The app rolls these up into a live **running mark** and flags the module as:

- ✅ **On Track** — running mark meets or exceeds the target
- ⚠️ **At Risk** — within 80% of the target, but not there yet
- ❌ **Off Track** — below 80% of the target

The goal is simple: turn "how am I actually doing in this module?" into a number you can check in two taps, instead of a mental calculation you put off until results day.

### Built With

This project is built entirely on the **.NET MAUI** stack for true single-codebase, multi-platform delivery:

* [![.NET MAUI](https://img.shields.io/badge/.NET_MAUI-512BD4?style=flat-square&logo=dotnet&logoColor=white)](https://learn.microsoft.com/dotnet/maui/) — single project targeting Android, iOS, Mac Catalyst, and Windows
* **C# / XAML** — application logic and UI markup
* [![SQLite](https://img.shields.io/badge/SQLite-07405E?style=flat-square&logo=sqlite&logoColor=white)](https://www.sqlite.org/) via `sqlite-net-pcl` — local, offline-first persistence
* **BCrypt.Net-Next** — salted password hashing for account security
* `Microsoft.Extensions.Logging.Debug` — debug-time diagnostics

### Data Model

```
User
 ├─ UserID        (PK)
 ├─ Username
 ├─ Email
 └─ PasswordHash

Module
 ├─ ModuleID      (PK)
 ├─ UserID        (FK → User)
 ├─ ModuleName
 ├─ ModuleCode
 └─ TargetMark

Assessment
 ├─ AssessmentID  (PK)
 ├─ ModuleID      (FK → Module)
 ├─ AssessmentName
 ├─ Weighting
 ├─ MarkObtained
 └─ TotalMark
```

One user → many modules → many assessments. Deleting a module cascades (in application code) to remove its assessments first, avoiding orphaned rows.

<br>

## Features

- 🔐 **Secure authentication** — registration and login with BCrypt-hashed passwords; session persisted via `Preferences` so users stay logged in between launches
- 🔑 **Password recovery** — dedicated forgot-password flow
- 📚 **Module management** — add, edit, and delete modules, each with a unique module code per user and a target mark (0–100%)
- 📝 **Assessment tracking** — record assessments with name, weighting, mark obtained, and total mark
- 📊 **Live running mark calculation** — weighted average computed on the fly from all recorded assessments
- 🚦 **Status indicators** — On Track / At Risk / Off Track, color-coded for quick scanning
- ✅ **Weighting integrity checks** — prevents total assessment weighting for a module from exceeding 100%
- 📱 **Multi-target project** — one codebase configured for Android, iOS, Mac Catalyst, and Windows (currently verified working on **Android only** — see [Roadmap](#roadmap))

<br>

## Getting Started

### Prerequisites

- [Visual Studio 2022](https://visualstudio.microsoft.com/) (17.8+) with the **.NET MAUI** workload installed
- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- A target emulator/simulator (Android Emulator, iOS Simulator via a Mac build host, or Windows directly)

### Installation

1. Clone the repository
   ```sh
   git clone https://github.com/TshabalalaWandile/Academic-tracker.git
   ```
2. Open `Academic tracker.sln` in Visual Studio
3. Restore NuGet packages (Visual Studio does this automatically on open, or run):
   ```sh
   dotnet restore
   ```
4. Select your target framework/device (Android, Windows, etc.) from the run dropdown
5. Run the app — the SQLite database (`trackwise.db3`) is created automatically on first launch inside the platform's app data directory

<br>

## Usage

1. **Register** a new account with a username, email, and password
2. **Log in** — your session is remembered until you explicitly log out
3. From the **Dashboard**, tap **Add Module** and enter the module name, code (e.g. `COMP301`), and target mark
4. Tap into a module to open its detail page, then **Add Assessment** for each test/assignment, entering its weighting and mark
5. Watch the **running mark** and status badge update automatically as you add results

<br>

## Project Structure

```
Academic tracker/
├── Models/
│   ├── User.cs
│   ├── Module.cs
│   └── Assessment.cs
├── ViewModels/
│   └── ModuleViewModel.cs
├── Services/
│   └── DBServices.cs          # SQLite data access layer
├── Pages/
│   ├── LoginPage.xaml(.cs)
│   ├── RegisterPage.xaml(.cs)
│   ├── ForgotPasswordPage.xaml(.cs)
│   ├── Dashboard.xaml(.cs)
│   ├── ModuleDetailPage.xaml(.cs)
│   └── AddAssessmentPage.xaml(.cs)
├── Platforms/                 # Android / iOS / MacCatalyst / Windows / Tizen heads
├── Resources/                 # Fonts, images, styles, app icon, splash
├── App.xaml(.cs)
├── AppShell.xaml(.cs)
└── MauiProgram.cs
```

<br>

## How the Running Mark Is Calculated

For each assessment under a module:

```
Assessment % = (MarkObtained / TotalMark) × 100
Contribution  = Assessment % × (Weighting / 100)

Running Mark  = Σ Contribution, across all assessments in the module
```

**Example** — two assessments, each weighted 50%:

| Assessment | Mark Obtained | Total Mark | Weighting | Contribution |
|---|---|---|---|---|
| Assignment 1 | 90 | 100 | 50% | 45% |
| Test 1 | 80 | 100 | 50% | 40% |
| **Running Mark** | | | | **85%** |

The app also tracks total weighting per module so it can warn you before the assessments you've entered add up to more than 100%.

<br>

## Roadmap

- [ ] **Fix iOS / Mac Catalyst builds** — app currently only runs correctly on Android despite the project being configured to multi-target iOS, Mac Catalyst, and Windows
- [ ] Bring edit-assessment validation in line with add-assessment (bounds checks on weighting, total mark, and mark-vs-total are currently only enforced when *adding* an assessment, not editing one)
- [ ] Extract the On Track / At Risk / Off Track threshold logic into a single shared helper instead of duplicating it across the Dashboard view model and the module detail page
- [ ] Add automated tests around `DBServices` (running mark calculation, weighting cap, cascade delete)
- [ ] Replace `DisplayPromptAsync` dialogs with dedicated entry forms for a smoother UX
- [ ] Add a LICENSE file
- [ ] Add CI (GitHub Actions) to build all target frameworks on push

See the [open issues](../../issues) for a full list of proposed features and known issues.

<br>

## Team

This project was built collaboratively. Based on commit history:

| Member | Role | GitHub |
| --- | --- | --- |
| **Wandile Tshabalala** | Repository owner / Contributor | [@TshabalalaWandile](https://github.com/TshabalalaWandile) |
| **Tebogo Jr Mabuza** | Contributor | [@Tebogomabuzaa](https://github.com/Tebogomabuzaa) |
| **Tiago Martins** | Contributor | [@TiagoMartins300905](https://github.com/TiagoMartins300905) |

<br>

## Contributing

Contributions make the open-source community a great place to learn and build. Any contributions you make are greatly appreciated.

1. Fork the project
2. Create your feature branch (`git checkout -b feature/AmazingFeature`)
3. Commit your changes (`git commit -m 'Add some AmazingFeature'`)
4. Push to the branch (`git push origin feature/AmazingFeature`)
5. Open a Pull Request

<br>

## License

Distributed under the MIT License. See `LICENSE` for more information.

<br>

## Contact

**TshabalalaWandile** — [GitHub Profile](https://github.com/TshabalalaWandile)

Project Link: [https://github.com/TshabalalaWandile/Academic-tracker](https://github.com/TshabalalaWandile/Academic-tracker)

<br>

## Acknowledgments

* [.NET MAUI Documentation](https://learn.microsoft.com/dotnet/maui/)
* [sqlite-net](https://github.com/praeclarum/sqlite-net)
* [BCrypt.Net-Next](https://github.com/BcryptNet/bcrypt.net)
* [Best-README-Template](https://github.com/othneildrew/Best-README-Template) — structural inspiration for this document
