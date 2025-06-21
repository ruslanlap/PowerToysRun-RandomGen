<div align="center">
  <img src="assets/randomgen.logo.png" width="200" alt="RandomGen Logo" style="margin-bottom: 20px;">
  <h1>🎲 RandomGen for PowerToys Run</h1>
  <h3>Generate random data instantly with a single keystroke</h3>

  <!-- Badges -->
  <div style="margin: 20px 0;">
    <a href="https://github.com/ruslanlap/RandomGen/releases/latest">
      <img src="https://img.shields.io/github/v/release/ruslanlap/RandomGen?style=for-the-badge&label=Latest%20Version" alt="Latest Release">
    </a>
    <a href="https://github.com/ruslanlap/RandomGen/actions/workflows/build-and-release.yml">
      <img src="https://github.com/ruslanlap/RandomGen/actions/workflows/build-and-release.yml/badge.svg?style=for-the-badge" alt="Build Status">
    </a>
    <a href="https://github.com/ruslanlap/RandomGen/releases">
      <img src="https://img.shields.io/github/downloads/ruslanlap/RandomGen/total?style=for-the-badge" alt="Total Downloads">
    </a>
    <a href="https://github.com/ruslanlap/RandomGen/blob/main/LICENSE">
      <img src="https://img.shields.io/github/license/ruslanlap/RandomGen?style=for-the-badge" alt="License">
    </a>
  </div>

  <div>
    <img src="https://img.shields.io/badge/Supported%20Data%20Types-14%2B-success?style=flat-square" alt="Supported Data Types">
    <img src="https://img.shields.io/badge/Platform-Windows%2010%2B-0078d7?style=flat-square" alt="Windows 10+">
    <img src="https://img.shields.io/badge/PowerToys-v0.75%2B-0078d7?style=flat-square" alt="PowerToys v0.75+">
    <img src="https://img.shields.io/badge/.NET-9.0-512BD4?style=flat-square" alt=".NET 9.0">
    <img src="https://img.shields.io/badge/Arch-x64%20%7C%20ARM64-0078d7?style=flat-square" alt="x64 | ARM64">
    <img src="https://img.shields.io/badge/Code%20Quality-A-brightgreen?style=flat-square" alt="Code Quality">
    <img src="https://img.shields.io/badge/Windows%2011-Compatible-0078d7?style=flat-square&logo=windows" alt="Windows 11 Compatible">
    <img src="https://img.shields.io/badge/Automated%20Builds-CI%2FCD-2088FF?style=flat-square&logo=github-actions" alt="Automated Builds">
    <img src="https://img.shields.io/badge/PRs-welcome-brightgreen.svg?style=flat-square" alt="PRs Welcome">
  </div>

  <div style="margin: 20px 0;">
    <a href="https://github.com/ruslanlap/RandomGen/releases/latest">
      <img src="https://img.shields.io/badge/Download_Now-2088FF?style=for-the-badge&logo=github&logoColor=white&labelColor=24292f" alt="Download Now">
    </a>
  </div>
</div>

## 🎥 Demo

<div align="center">
  <img src="assets/demo-randomgen.gif" width="80%" alt="Main Demo" style="margin-bottom: 15px;">
  <div>
    <img src="assets/demo-randomgen-password.gif" width="48%" alt="Password Generation">
    <img src="assets/demo-randomgen-pin-color.gif" width="48%" alt="PIN and Color Generation">
  </div>
  <div style="margin-top: 15px;">
    <img src="assets/demo-randomgen-address-date.gif" width="48%" alt="Address and Date Generation">
    <img src="assets/demo-randomgen-guid.gif" width="48%" alt="GUID Generation">
  </div>
  <div style="margin-top: 15px;">
    <img src="assets/demo-randomgen-name.gif" width="48%" alt="Name Generation">
    <img src="assets/demo-randomgen-num.gif" width="48%" alt="Number Generation">
  </div>
</div>

## 📋 Table of Contents

- [✨ Features](#-features)
- [🚀 Quick Start](#-quick-start)
- [🔍 Usage](#-usage)
- [⚙️ Installation](#️-installation)
- [🧩 Requirements](#-requirements)
- [🛠️ Building from Source](#️-building-from-source)
- [🤝 Contributing](#-contributing)
- [📄 License](#-license)
- [🙏 Acknowledgements](#-acknowledgements)
- [💬 Feedback and Support](#-feedback-and-support)
- [❓ FAQ](#-faq)
- [🔒 Security Note](#-security-note)

## ✨ Features

RandomGen is a powerful PowerToys Run plugin that generates various types of random data with a single keystroke. Perfect for developers, testers, and anyone who needs quick access to random data.

### Key Features

- 🔐 **Cryptographically Secure Passwords** - Generate strong passwords with mixed case, numbers, and symbols
- 📍 **Secure PIN Codes** - Create numeric PINs with smart pattern detection to avoid weak PINs like 1234 or 0000
- 👤 **Personal Data** - Generate realistic names, emails, phone numbers, and addresses
- 🏢 **Business Data** - Company names and other business-related information
- 📅 **Date Generation** - Random dates within customizable ranges
- 🔢 **Number Generation** - Random numbers with custom min-max ranges
- 🆔 **Unique Identifiers** - GUIDs/UUIDs for development and testing
- 🎨 **Color Codes** - Random HEX colors for design and testing
- 🌐 **Web Data** - URLs and domains for testing
- 💳 **Payment Testing** - Credit card numbers (fake, for testing only)
- 📝 **Lorem Ipsum** - Placeholder text with customizable word count

### Technical Highlights

- **Plugin ID:** `EFADBA167C1B41D8A7426A7DF808D28E`
- **Action Keyword:** `rnd`
- **Thread-Safe Design** - Optimized for performance with thread-local Faker instances
- **Smart Caching** - Improved performance with intelligent result caching
- **Cryptographically Secure** - Uses `System.Security.Cryptography.RandomNumberGenerator` for secure random generation
- **Intelligent Suggestions** - Smart autocomplete for commands
- **Context Menu Support** - Right-click options for copying and regenerating values

## 🚀 Quick Start

1. **Download** the latest release from the [Releases page](https://github.com/ruslanlap/RandomGen/releases/latest)
2. **Extract** the ZIP file to your PowerToys Run plugins directory:
   ```
   %LOCALAPPDATA%\Microsoft\PowerToys\PowerToys Run\Plugins\RandomGen\
   ```
3. **Restart PowerToys**
4. Press `Alt+Space` to open PowerToys Run
5. Type `rnd` followed by what you want to generate (e.g., `rnd password`)

## 🔍 Usage

### Basic Commands

Use the activation keyword `rnd` followed by the data type you want to generate:

| Command | Example | Description |
|---------|---------|-------------|
| `rnd password [length]` | `rnd password 16` | Generate a secure password (default: 12 chars) |
| `rnd pwd [length]` | `rnd pwd 16` | Alias for password command |
| `rnd pin [length]` | `rnd pin 6` | Generate a numeric PIN (default: 4 digits) |
| `rnd email` | `rnd email` | Generate a random email address |
| `rnd name` | `rnd name` | Generate a random full name |
| `rnd address` | `rnd address` | Generate a random address |
| `rnd phone` | `rnd phone` | Generate a random phone number |
| `rnd company` | `rnd company` | Generate a random company name |
| `rnd lorem [count]` | `rnd lorem 25` | Generate lorem ipsum text (default: 10 words) |
| `rnd number [min-max]` | `rnd number 1-1000` | Generate a random number (default: 1-100) |
| `rnd num [min-max]` | `rnd num 1-1000` | Alias for number command |
| `rnd date` | `rnd date` | Generate a random date |
| `rnd guid` | `rnd guid` | Generate a random GUID/UUID |
| `rnd uuid` | `rnd uuid` | Alias for guid command |
| `rnd color` | `rnd color` | Generate a random hex color |
| `rnd url` | `rnd url` | Generate a random URL |
| `rnd creditcard` | `rnd creditcard` | Generate a random credit card number (test use only) |
| `rnd credit` | `rnd credit` | Alias for creditcard command |

### Examples

```bash
# Generate a 16-character password
rnd password 16

# Generate a 6-digit PIN
rnd pin 6

# Generate a random email address
rnd email

# Generate a random number between 1 and 1000
rnd number 1-1000

# Generate 25 words of lorem ipsum text
rnd lorem 25
```

### Advanced Features

- **Context Menu** - Right-click on any result to:
  - Copy to clipboard (Ctrl+C)
  - Generate a new value (F5)

- **Smart Autocomplete** - Type part of a command to see suggestions

- **Intelligent Caching** - Recently generated values are cached for improved performance

## ⚙️ Installation

### Prerequisites
- [PowerToys](https://github.com/microsoft/PowerToys) (v0.75 or later)
- Windows 10/11 (x64 or ARM64)
- .NET 9.0 Runtime (included with PowerToys)
- Approximately 2MB of disk space

### Installation Steps

#### Method 1: Using the Release Package
1. Download the latest release from the [Releases page](https://github.com/ruslanlap/RandomGen/releases/latest)
2. Extract the ZIP file to your PowerToys Run plugins directory:
   ```
   %LOCALAPPDATA%\Microsoft\PowerToys\PowerToys Run\Plugins\RandomGen\
   ```
3. Restart PowerToys
4. Enable the plugin in PowerToys Settings → PowerToys Run → Plugin Manager

#### Method 2: Manual Installation from Build
1. Build the project (see [Building from Source](#️-building-from-source))
2. Copy all files from the `RandomGen\Publish\` folder to your PowerToys Run plugins directory:
   ```
   %LOCALAPPDATA%\Microsoft\PowerToys\PowerToys Run\Plugins\RandomGen\
   ```
3. Restart PowerToys
4. Enable the plugin in PowerToys Settings → PowerToys Run → Plugin Manager

### Verifying Installation
After installation, press `Alt+Space` to open PowerToys Run, then type `rnd` to see if the plugin is working correctly.

## 🧩 Requirements

- Windows 10/11 (x64 or ARM64)
- PowerToys v0.75 or later
- .NET 9.0 Runtime (included with PowerToys)
- Approximately 2MB of disk space

## 🛠️ Building from Source

### Prerequisites
- [.NET 9.0 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- [PowerToys](https://github.com/microsoft/PowerToys) (for testing)
- [Visual Studio 2022](https://visualstudio.microsoft.com/) or [Visual Studio Code](https://code.visualstudio.com/)
- [Git](https://git-scm.com/)

### Build Steps

```bash
# Clone the repository
git clone https://github.com/ruslanlap/RandomGen.git
cd RandomGen

# Restore dependencies
dotnet restore

# Build the solution
dotnet build -c Release

# Create a publishable version (optional)
dotnet publish -c Release -o ./Publish
```

The built plugin will be in `RandomGen\bin\Release\net9.0-windows10.0.22621.0` or in the `Publish` directory if you ran the publish command.

### Project Structure

- `RandomGen/Community.PowerToys.Run.Plugin.RandomGen/` - Main plugin code
  - `Main.cs` - Core plugin implementation
  - `Images/` - Plugin icons
  - `plugin.json` - Plugin metadata
- `RandomGen/Community.PowerToys.Run.Plugin.RandomGen.UnitTests/` - Unit tests

### Dependencies

- [Bogus](https://github.com/bchavez/Bogus) v35.6.3 - For generating realistic fake data
- [Community.PowerToys.Run.Plugin.Dependencies](https://www.nuget.org/packages/Community.PowerToys.Run.Plugin.Dependencies/) v0.91.0 - PowerToys Run plugin dependencies

## 🤝 Contributing

Contributions are welcome! Here's how you can help:

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/AmazingFeature`)
3. Commit your changes (`git commit -m 'Add some AmazingFeature'`)
4. Push to the branch (`git push origin feature/AmazingFeature`)
5. Open a Pull Request

### Ideas for Contribution

- **New Data Types** - Add more types of random data generation
- **Localization** - Add support for more languages and region-specific data formats
- **Settings UI** - Create a settings page for customizing default values
- **Performance Optimizations** - Improve caching and thread safety
- **Enhanced Validation** - Add more validation for generated data
- **Unit Tests** - Expand test coverage
- **Documentation** - Improve inline code documentation and user guides

### Development Guidelines

- Follow the existing code style and patterns
- Ensure all new features have appropriate unit tests
- Use thread-safe practices for all data generation
- Document any new commands or features
- Test on both x64 and ARM64 architectures if possible

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## 💬 Feedback and Support

If you encounter any issues or have suggestions for improvement, please [open an issue](https://github.com/ruslanlap/RandomGen/issues) on GitHub.

## ❓ FAQ

<details>
<summary><b>How do I change the default action keyword?</b></summary>
<p>You can change the action keyword in PowerToys Settings → PowerToys Run → Plugin Manager → RandomGen → Action Keyword.</p>
</details>

<details>
<summary><b>Are the generated passwords secure?</b></summary>
<p>Yes, passwords are generated using cryptographically secure random number generation methods from the .NET framework, ensuring high entropy and unpredictability.</p>
</details>

<details>
<summary><b>Can I use this data in production?</b></summary>
<p>The generated data is intended for testing, development, and demonstration purposes only. While passwords are cryptographically secure, other data like names, addresses, and credit card numbers are fictional.</p>
</details>

<details>
<summary><b>Does this plugin work offline?</b></summary>
<p>Yes, RandomGen works completely offline and doesn't require an internet connection.</p>
</details>

<details>
<summary><b>How can I add a new data type?</b></summary>
<p>To add a new data type, you would need to modify the Main.cs file, add a new generator method, and update the command handling logic. See the Contributing section for more details.</p>
</details>

## 🔒 Security Note

- **Secure Password Generation** - Passwords are generated using `System.Security.Cryptography.RandomNumberGenerator` for cryptographically secure randomness
- **PIN Security** - PINs are checked against common patterns (like 1234, 0000) to avoid weak PINs
- **Fake Data Only** - All generated data (names, addresses, credit cards, etc.) is completely fictional and suitable for testing only
- **Privacy** - No personal data is collected, stored, or transmitted by this plugin
- **Local Processing** - All data generation happens locally on your machine

> ⚠️ **Note:** While passwords generated by this tool use cryptographically secure methods, always follow your organization's security policies for production passwords.

## 🙏 Acknowledgements

- [Bogus](https://github.com/bchavez/Bogus) - For generating realistic fake data
- [Microsoft PowerToys](https://github.com/microsoft/PowerToys) - For the amazing PowerToys Run platform
- [All Contributors](https://github.com/ruslanlap/RandomGen/graphs/contributors) - For their valuable contributions
- Credit card numbers are fake and generated using standard test patterns
- Do not use generated personal information for malicious purposes