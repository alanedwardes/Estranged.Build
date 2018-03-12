# Estranged.Build
Tooling for the Estranged build.

## Example Usage
Example usage with an Unreal Engine 4 packaged game:
```bash
dotnet run --project "Depot\\Tools\\Estranged.Build.Symbols"
           --bucket my-bucket
           --symbols "Path\\To\\Binaries\\Win64"
           --symstore "C:\\Program Files (x86)\\Windows Kits\\10\\Debuggers\\x86\\symstore.exe"
           --properties:BUILD_LABEL "my build label"
```

Your build environment should set up the appropriate AWS credentials and region. This can be specified using the withCredential helper in Jenkins Pipelines:

```groovy
env.AWS_REGION = 'us-east-1'
withCredentials([[$class: 'AmazonWebServicesCredentialsBinding', credentialsId: '<my credential>']]) {
    // dotnet run ...
}
```