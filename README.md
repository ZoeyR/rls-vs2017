# Rust support for Visual Studio 2017 Preview

[![Build status](https://ci.appveyor.com/api/projects/status/var20s6rdqf8bfv5/branch/master?svg=true)](https://ci.appveyor.com/project/dgriffen/whackwhackterminal/branch/master)

Adds language support for Rust to Visual Studio 2017. Supports:

- code completion
- goto definition
- find all references
- error squiggles

Rust support is powered by the [Rust Language Server](https://github.com/rust-lang-nursery/rls) (RLS). Language server support was recently added experimentally to Visual Studio, and is only available on preview builds of VS.

Please note that this extension is very early in development. At this time setup is more complicated than similar extensions for Visual Studio Code. This will be fixed in future versions.

## Quick Start

- Install a preview build of Visual Studio
- Install [rustup](https://www.rustup.rs/) (Rust toolchain manager).
- Install rls-preview, rust-analysis, and rust-src
    - `rustup component add rls-preview rust-analysis rust-src --toolchain {toolchain}`
    - replace toolchain with the toolchain that you wish to use for the rls. Note that some toolchains may be missing the rls.
- Install the preview LSP support from the [marketplace](https://marketplace.visualstudio.com/items?itemName=vsext.LanguageServerClientPreview)
- Download and run the extension .vsix from the [releases](https://github.com/dgriffen/rls-vs2017/releases) page.
- Go to the options for the extension (`Tools > Options > Rust`) and set the toolchain to the one you installed the components to.
- Open a Rust project (`File > Open Folder...`). Open the folder for the whole project (i.e., the folder containing 'Cargo.toml'), not the 'src' folder.
- The extension will start when you open a Rust file. You'll be notified if you are missing any 