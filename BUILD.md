# Building RedisMemoryCacheInvalidationCore

This project has been consolidated into a single project that can produce both signed and unsigned NuGet packages.

## Project Structure

- **Main Project**: `src/RedisMemoryCacheInvalidation/RedisMemoryCacheInvalidation.csproj`
- **Tests**: `tests/RedisMemoryCacheInvalidation.Tests/`
- **Sample**: `samples/SampleInvalidationEmitter/`

## Building Packages

### Option 1: Using Build Scripts

#### Build Both Versions
```bash
build-both.bat
```

#### Build Unsigned Version Only
```bash
build-unsigned.bat
```

#### Build Signed Version Only
```bash
build-signed.bat
```

### Option 2: Using dotnet CLI

#### Build Unsigned Version
```bash
dotnet build src\RedisMemoryCacheInvalidation\RedisMemoryCacheInvalidation.csproj -c Release
dotnet pack src\RedisMemoryCacheInvalidation\RedisMemoryCacheInvalidation.csproj -c Release --no-build -o artifacts
```

#### Build Signed Version
```bash
dotnet build src\RedisMemoryCacheInvalidation\RedisMemoryCacheInvalidation.csproj -c Release -p:SignAssembly=true
dotnet pack src\RedisMemoryCacheInvalidation\RedisMemoryCacheInvalidation.csproj -c Release -p:SignAssembly=true --no-build -o artifacts
```

### Option 3: Using the Updated pack.bat

```bash
pack.bat [version]
```

If no version is specified, it defaults to `2.5.0-local`.

## Package Output

Both build approaches will create packages in the `artifacts/` folder:

- **Unsigned**: `RedisMemoryCacheInvalidationCore.{version}.nupkg`
- **Signed**: `RedisMemoryCacheInvalidationCoreStrong.{version}.nupkg`
- **Symbols**: Both versions include corresponding `.symbols.nupkg` files

## Key Changes

1. **Consolidated Projects**: Removed `RedisMemoryCacheInvalidationCore` and `RedisMemoryCacheInvalidationCore.Strong` projects
2. **Conditional Signing**: Uses MSBuild conditions to enable/disable assembly signing
3. **Package ID Switching**: Automatically switches package ID based on signing configuration
4. **Test Compatibility**: Updated `AssemblyInfo.cs` to handle both signed and unsigned test scenarios

## Technical Details

The project uses MSBuild conditions to:
- Enable assembly signing when `SignAssembly=true` property is set
- Switch package ID between `RedisMemoryCacheInvalidationCore` and `RedisMemoryCacheInvalidationCoreStrong`
- Define `SIGNED` preprocessor symbol for conditional compilation
- Include the correct public key in `InternalsVisibleTo` attributes for test access
