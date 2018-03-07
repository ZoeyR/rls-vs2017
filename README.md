# Rust support for Visual Studio 2017 Preview

[![Build status](https://ci.appveyor.com/api/projects/status/d2lxlincwninhsng?svg=true)](https://ci.appveyor.com/project/dgriffen/rls-vs2017)

Adds language support for Rust to Visual Studio 2017. Supports:

- Code completion
- Go to definition
- Find all references
- Code linting
- Code action (lightbulb)
- Rename

Rust support is powered by the [Rust Language Server](https://github.com/rust-lang-nursery/rls) (RLS). Language server support was recently added experimentally to Visual Studio, and is only available on preview builds of VS.

Please note that this extension is very early in development. At this time setup is more complicated than similar extensions for Visual Studio Code. This will be fixed in future versions.

## Quick Start

- Install a preview build of Visual Studio
- Install the preview LSP support from the [marketplace](https://marketplace.visualstudio.com/items?itemName=vsext.LanguageServerClientPreview)
- Install [rustup](https://www.rustup.rs/) (Rust toolchain manager).
- Install this extension from the [marketplace](https://marketplace.visualstudio.com/items?itemName=DanielGriffen.Rust)
- Open a Rust project (`File > Open Folder...`). Open the folder for the whole project (i.e., the folder containing 'Cargo.toml'), not the 'src' folder.
- The extension will start when you open a Rust file. You'll be prompted to install the RLS. Once installed, the RLS should start building your project.
