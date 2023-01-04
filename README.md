# 基础的代码封装库

1：包含Nuget打包测试

2：多框架构建

3：常见的代码库封装


持续性构建
```xml
<PropertyGroup>
  <EmbedUntrackedSources>true</EmbedUntrackedSources>
  <ContinuousIntegrationBuild>true</ContinuousIntegrationBuild>
</PropertyGroup>
```

多SDK

```xml
<Project>
  <PropertyGroup>
    <TargetFrameworkMonikerAssemblyAttributesPath>$([System.IO.Path]::Combine('$(IntermediateOutputPath)','$(TargetFrameworkMoniker).AssemblyAttributes$(DefaultLanguageSourceExtension)'))</TargetFrameworkMonikerAssemblyAttributesPath>
  </PropertyGroup>
  <ItemGroup>
    <EmbeddedFiles Include="$(GeneratedAssemblyInfoFile)"/>
  </ItemGroup>
</Project>

```