[![Build status](https://ci.appveyor.com/api/projects/status/wryhq70j3cfxca8v?svg=true)](https://ci.appveyor.com/project/DaveJohnson8080/feliz-msal)


# Feliz.MSAL

[Feliz](https://github.com/Zaid-Ajaj/Feliz)-style Fable React bindings for [MSAL-React](http://aka.ms/aadv2).

Contributing
------------

This project uses `fake`, `paket`, and `femto` as .NET Core 3 local tools. Therefore, run `dotnet tool restore` to restore the necessary CLI tools before doing anything else.

To run targets using Fake: `dotnet fake build -t TargetName`

### Regular maintenance

1. Run the `CiBuild` target to check that everything compiles
8. Commit and tag the commit (this is what triggers deployment from  AppVeyor). For consistency, the tag should be identical to the version (e.g. `1.2.3`).
