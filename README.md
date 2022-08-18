# DEPRECATED

Please use [VS-RustAnalyzer](https://github.com/cchharris/VS-RustAnalyzer) instead.

# Rust support for Visual Studio 2017 Preview

[![Build status](https://ci.appveyor.com/api/projects/status/d2lxlincwninhsng?svg=true)](https://ci.appveyor.com/project/dgriffen/rls-vs2017)

Adds language support for Rust to Visual Studio 2017. Supports:

- code completion
- goto definition
- find all references
- error squiggles
- code action (lightbulb)
- hover
- formatting
- rename

Rust support is powered by the [Rust Language Server](https://github.com/rust-lang-nursery/rls) (RLS). Language server support was recently added experimentally to Visual Studio, and is only available on preview builds of VS.

Please note that this extension is very early in development, there may be bugs or rough edges.

## Quick Start

- Install a preview build of Visual Studio (15.8 preview 3 or newer).
- Install [rustup](https://www.rustup.rs/) (Rust toolchain manager).
- Install this extension from the [marketplace](https://marketplace.visualstudio.com/items?itemName=DanielGriffen.Rust)
- Open a Rust project (`File > Open Folder...`). Open the folder for the whole project (i.e., the folder containing 'Cargo.toml'), not the 'src' folder.
- The extension will start when you open a Rust file. You'll be prompted to install the RLS. Once installed, the RLS should start building your project.
