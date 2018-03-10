# Estranged.Build
Tooling for the Estranged build.

## Example Usage
Example usage with an Unreal Engine 4 packaged game:
```
dotnet run --project Depot\\Tools\\Estranged.Build.Symbols
 --accessKey %ACCESS_KEY%
 --secret %SECRET_KEY%
 --region eu-west-1
 --bucket my-bucket
 --symbols Path\\To\\Binaries\\Win64
 --symstore "C:\\Program Files (x86)\\Windows Kits\\10\\Debuggers\\x86\\symstore.exe"
 --properties:BUILD_LABEL "my build label"
```
