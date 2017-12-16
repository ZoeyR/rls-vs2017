# Changelog
All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](http://keepachangelog.com/en/1.0.0/).

## [Unreleased]

## [0.1.2] - 2017-12-15
### Added
- detect if rustup is installed and display error message if not
- option page for setting rustup path

### Fixed
- RUST\_SRC\_PATH now gets set correctly

## [0.1.1] - 2017-12-11
### Added
- task status for component and toolchain installation

### Changed
- missing component notifications now have an install button

## 0.1.0 - 2017-12-06
### Added
- basic language support
    - code completion
    - error diagnostics
    - goto definition
    - find all references
- support for toolchains other than the default nightly
- error notifications

[Unreleased]: https://github.com/olivierlacan/keep-a-changelog/compare/v0.1.2...HEAD
[0.1.1]: https://github.com/olivierlacan/keep-a-changelog/compare/v0.1-announce...v0.1.1
[0.1.2]: https://github.com/olivierlacan/keep-a-changelog/compare/v0.1.1...v0.1.2