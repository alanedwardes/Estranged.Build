# Estranged.Build
Jenkins tooling for the Estranged build:
* Estranged.Build.Notarizer - Completely automates the macOS `codesign` and Notarization process for an app bundle
* Estranged.Build.Symbols - Extracts symbols with `symstore` and uploads them to Amazon S3, so that they can be served for Visual Studio and other debugging tools

## Estranged.Build.Notarizer
Completely automates the macOS `codesign` and Notarization process for an app bundle.

### Requirements
* At least .NET Core 2.1 SDK
* A Mac running macOS with Xcode
* The `codesign` and `xcrun` commands available on the $PATH
* A valid Apple Developer account with an application password
* A Developer ID certificate available in the keychain certificate store of the macOS installation

### Example Usage
Example usage with an Unreal Engine 4 packaged game:
```bash
dotnet run --project "depot/Tools/Estranged.Build.Notarizer"
           --appPath "Path/To/My/AppBundle.app"
           --certificateId "My Certificate ID"
           --entitlements "AppBundle.app=<key>com.apple.security.cs.allow-dyld-environment-variables</key><true/>"
           --developerUsername $USERNAME --developerPassword $PASSWORD
```
* Certificate ID is the name of your Developer ID certificate in KeyChain to be passed to `codesign` - see https://developer.apple.com/support/developer-id/
* Entitlements that your app needs in the hardened run time - see https://developer.apple.com/documentation/security/hardened_runtime_entitlements
* The developer username is usually an email address, and the password is an app-password generated in your Apple ID settings

This process will:
1. Recursively sign each library and binary in your app bundle
2. Sign the app bundle itself
3. Put all binaries into a separate zip folder for notarization
4. Submit the zip folder for notarization
5. Wait for the notarization process to complete
6. Staple the result of the notarization process to the app bundle

To specify multiple entitlements for the bundle, use a semicolon:
```
--entitlements "AppBundle.app=com.apple.security.cs.allow-dyld-environment-variables;com.apple.security.app-sandbox"
```

## Estranged.Build.Symbols
Extracts symbols with `symstore` and uploads them to Amazon S3, so that they can be served for Visual Studio and other debugging tools.

### Requirements
* A pre-built Windows executable with .pdb symbol files
* At least .NET Core 2.1 SDK
* The Windows SDK

### Example Usage
Example usage with an Unreal Engine 4 packaged game:
```bash
dotnet run --project "Depot\\Tools\\Estranged.Build.Symbols"
           --bucket my-bucket
           --symbols "Path\\To\\Binaries\\Win64"
           --symstore "C:\\Program Files (x86)\\Windows Kits\\10\\Debuggers\\x86\\symstore.exe"
           --properties:BUILD_LABEL "my build label"
```

Your build environment should set up the appropriate AWS credentials and region. This can be specified using the `withCredentials` helper in Jenkins Pipelines:

```groovy
env.AWS_REGION = 'us-east-1'
withCredentials([[$class: 'AmazonWebServicesCredentialsBinding', credentialsId: '<my credential>']]) {
    // dotnet run ...
}
```
