# 🎲 RandomGen v1.0.2!
<p align="center">
  <img src="https://github.com/ruslanlap/PowerToysRun-RandomGen/raw/master/assets/randomgen.logo.png" width="228" alt="RandomGen Logo"/>
</p>

## ✨ What's New
- 🌎 **Locale Selection** - Generate data for different regions with `rd locale [code]`
  <p align="center">
    <img src="https://github.com/ruslanlap/PowerToysRun-RandomGen/raw/master/assets/locale.png" width="600" alt="Locale Selection"/>
  </p>
- 🔐 **Enhanced Password Generator** (customizable options)
- Now supports multiple ways to specify options:

    Exclusion: -upper, -special, -numeric, -lower
    Inclusion: +upper, +special, +numeric, +lower
    Natural language: noupper, no-special, nonumbers

- 🚀 Turbocharged performance
- 🛡️ **Fixed all PTRUN validation errors**:
  - **PTRUN1301**: Corrected package naming format from "RandomGen-v{VERSION}-{platform}.zip" to "RandomGen-{VERSION}-{platform}.zip"
  - **PTRUN1303**: Added SHA256 checksum generation for both x64 and arm64 builds
  - **PTRUN1401**: Added step to update plugin.json version to match release tag
  - **PTRUN1402**: Removed unnecessary PowerToys DLLs that are provided by the host
- 🐛 Fixed syntax errors in Main.cs

## 🧙‍♂️ Install
1. Download ZIP for your architecture (x64 / ARM64)  
2. Extract to `%LOCALAPPDATA%\Microsoft\PowerToys\PowerToys Run\Plugins`   
3. Restart PowerToys → press `Alt+Space` → type `random`

## ⚡ Commands
| Incantation | Magic |
|-------------|-------|
| `rd password` | Forge a password |
| `rd pwd` | Password (alias) |
| `rd email` | Random email address |
| `rd name` | Random full name |
| `rd address` | Random address |
| `rd phone` | Random phone number |
| `rd company` | Random company name |
| `rd lorem` | Lorem ipsum text |
| `rd number` | Random number |
| `rd date` | Random date |
| `rd guid` | Fresh GUID/UUID |
| `rd color` | Random hex color |
| `rd url` | Random URL |
| `rd creditcard` | Random credit card number |
| `rd locale` | Change locale for generated data

## 💫 Contribute
Found a bug or have an idea? [Open an issue](https://github.com/ruslanlap/PowerToysRun-RandomGen/issues).

Crafted with ✨ cosmic energy ✨ by [ruslanlap](https://github.com/ruslanlap)