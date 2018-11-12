# Lumi

**Abandon hope all ye who enter here.**

Lumi is a shell for Windows, written in C# because command prompt sucks and PowerShell ~~is even worse~~ leaves much to be desired. Anyone is free to use, abuse, toy with, tear apart, or set fire to this project as they please.
However, **this is a personal project intended solely for my own use**. Do not expect quality, support for anything but Windows and the full .NET Framework (no Mono, no .NET Core), or documentation. The code may be ugly, janky, and confusing; there will most definitely be bugs.

You have been warned.

# Projects

- **Lumi**: The CLI app itself.
  - **Lumi.Core**: Some core functionality that is common between other projects.
  - **Lumi.Parsing**: Basic framework for implementing lexing, parsing, and other related fun stuff.
  - **Lumi.Shell**: Stuff related to parsing and executing command line text.
