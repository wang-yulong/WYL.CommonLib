# �����Ĵ����װ��

1������Nuget�������

2�����ܹ���

3�������Ĵ�����װ


�����Թ���
```xml
<PropertyGroup>
  <EmbedUntrackedSources>true</EmbedUntrackedSources>
  <ContinuousIntegrationBuild>true</ContinuousIntegrationBuild>
</PropertyGroup>
```

��SDK

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