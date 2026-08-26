<#
.SYNOPSIS
    Scaffolds a new IDesign-compliant subsystem for the Viaduc .NET backend.

.PARAMETER Subsystem
    The subsystem name in PascalCase, e.g. "Notification", "Reporting".
    Used to derive all project names, namespaces, and class names.

.PARAMETER IncludeEngine
    When specified, also creates the CMI.Engine.<Subsystem> project.

.EXAMPLE
    .\New-Subsystem.ps1 -Subsystem "Reporting"
    .\New-Subsystem.ps1 -Subsystem "Reporting" -IncludeEngine
#>
param(
    [Parameter(Mandatory = $true)]
    [ValidatePattern('^[A-Z][A-Za-z]+$')]
    [string]$Subsystem,

    [switch]$IncludeEngine
)

$ErrorActionPreference = "Stop"

# ── Locate repo root ──────────────────────────────────────────────────────────
$repoRoot = git -C $PSScriptRoot rev-parse --show-toplevel 2>$null
if (-not $repoRoot) {
    Write-Error "Not inside a git repository."
    exit 1
}

# ── Guard: subsystem must not already exist ───────────────────────────────────
$existingPaths = @(
    (Join-Path $repoRoot "CMI\Contract\$Subsystem"),
    (Join-Path $repoRoot "CMI\Manager\$Subsystem")
)
foreach ($p in $existingPaths) {
    if (Test-Path $p) {
        Write-Error "Path already exists: $p`nA subsystem named '$Subsystem' may already exist."
        exit 1
    }
}

# ── Helpers ───────────────────────────────────────────────────────────────────
function New-Guid { [System.Guid]::NewGuid().ToString("B").ToUpper() }

function Write-File([string]$path, [string]$content) {
    $dir = Split-Path $path
    if (-not (Test-Path $dir)) { New-Item -ItemType Directory -Path $dir -Force | Out-Null }
    [System.IO.File]::WriteAllText($path, $content, [System.Text.Encoding]::UTF8)
    Write-Host "  created: $(($path -replace [regex]::Escape($repoRoot+'\')).Replace('\','/'))"
}

function New-AssemblyInfo([string]$dir, [string]$assemblyName, [string]$guid) {
    Write-File (Join-Path $dir "Properties\AssemblyInfo.cs") @"
using System.Reflection;
using System.Runtime.InteropServices;

[assembly: AssemblyTitle("$assemblyName")]
[assembly: AssemblyDescription("")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("")]
[assembly: AssemblyProduct("$assemblyName")]
[assembly: AssemblyCopyright("Copyright © $(Get-Date -Format yyyy)")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]
[assembly: ComVisible(false)]
[assembly: Guid("$($guid.Replace('{','').Replace('}','').ToLower())")]
"@
}

function New-LibraryCsproj([string]$dir, [string]$name, [string]$guid, [string[]]$projectRefs) {
    $refs = ""
    foreach ($r in $projectRefs) {
        $refName = [System.IO.Path]::GetFileNameWithoutExtension($r)
        $refGuid = $projectGuids[$refName]
        $refs += @"

  <ItemGroup>
    <ProjectReference Include="$r">
      <Project>$refGuid</Project>
      <Name>$refName</Name>
    </ProjectReference>
  </ItemGroup>
"@
    }

    Write-File (Join-Path $dir "$name.csproj") @"
<?xml version="1.0" encoding="utf-8"?>
<Project ToolsVersion="15.0" xmlns="http://schemas.microsoft.com/developer/msbuild/2003">
  <Import Project="`$(MSBuildExtensionsPath)\`$(MSBuildToolsVersion)\Microsoft.Common.props" Condition="Exists('`$(MSBuildExtensionsPath)\`$(MSBuildToolsVersion)\Microsoft.Common.props')" />
  <PropertyGroup>
    <Configuration Condition=" '`$(Configuration)' == '' ">Debug</Configuration>
    <Platform Condition=" '`$(Platform)' == '' ">x64</Platform>
    <ProjectGuid>$guid</ProjectGuid>
    <OutputType>Library</OutputType>
    <AppDesignerFolder>Properties</AppDesignerFolder>
    <RootNamespace>$name</RootNamespace>
    <AssemblyName>$name</AssemblyName>
    <TargetFrameworkVersion>v4.8.1</TargetFrameworkVersion>
    <FileAlignment>512</FileAlignment>
    <TargetFrameworkProfile />
    <NuGetPackageImportStamp></NuGetPackageImportStamp>
    <AutoGenerateBindingRedirects>true</AutoGenerateBindingRedirects>
    <GenerateBindingRedirectsOutputType>true</GenerateBindingRedirectsOutputType>
  </PropertyGroup>
  <PropertyGroup Condition=" '`$(Configuration)|`$(Platform)' == 'Debug|x64' ">
    <DebugSymbols>true</DebugSymbols>
    <DebugType>full</DebugType>
    <Optimize>false</Optimize>
    <OutputPath>bin\Debug\</OutputPath>
    <DefineConstants>DEBUG;TRACE</DefineConstants>
    <ErrorReport>prompt</ErrorReport>
    <WarningLevel>4</WarningLevel>
    <TreatWarningsAsErrors>true</TreatWarningsAsErrors>
  </PropertyGroup>
  <PropertyGroup Condition=" '`$(Configuration)|`$(Platform)' == 'Release|x64' ">
    <DebugType>pdbonly</DebugType>
    <Optimize>true</Optimize>
    <OutputPath>bin\Release\</OutputPath>
    <DefineConstants>TRACE</DefineConstants>
    <ErrorReport>prompt</ErrorReport>
    <WarningLevel>4</WarningLevel>
  </PropertyGroup>
  <ItemGroup>
    <Reference Include="System" />
    <Reference Include="System.Core" />
    <Reference Include="Microsoft.CSharp" />
    <Reference Include="System.Data" />
    <Reference Include="System.Net.Http" />
    <Reference Include="System.Xml" />
  </ItemGroup>
  <ItemGroup>
    <Compile Include="Properties\AssemblyInfo.cs" />
  </ItemGroup>$refs
  <ItemGroup>
    <PackageReference Include="Nerdbank.GitVersioning">
      <Version>3.10.91</Version>
      <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
      <PrivateAssets>all</PrivateAssets>
    </PackageReference>
  </ItemGroup>
  <Import Project="`$(MSBuildToolsPath)\Microsoft.CSharp.targets" />
</Project>
"@
}

# ── Generate GUIDs for all projects upfront (needed for cross-references) ────
$projectGuids = @{
    "CMI.Contract.$Subsystem" = New-Guid
    "CMI.Access.$Subsystem"   = New-Guid
    "CMI.Manager.$Subsystem"  = New-Guid
    "CMI.Host.$Subsystem"     = New-Guid
}
if ($IncludeEngine) {
    $projectGuids["CMI.Engine.$Subsystem"] = New-Guid
}

# ── Paths ─────────────────────────────────────────────────────────────────────
$contractDir = Join-Path $repoRoot "CMI\Contract\$Subsystem"
$accessDir   = Join-Path $repoRoot "CMI\Access\$Subsystem"
$engineDir   = Join-Path $repoRoot "CMI\Engine\$Subsystem"
$managerDir  = Join-Path $repoRoot "CMI\Manager\$Subsystem"
$hostDir     = Join-Path $repoRoot "CMI\Host\$Subsystem"

Write-Host "`nScaffolding subsystem '$Subsystem'...`n"

# ══════════════════════════════════════════════════════════════════════════════
# CONTRACT
# ══════════════════════════════════════════════════════════════════════════════
Write-Host "── CMI.Contract.$Subsystem"
$contractName = "CMI.Contract.$Subsystem"
$contractGuid = $projectGuids[$contractName]

New-LibraryCsproj $contractDir $contractName $contractGuid @()
New-AssemblyInfo  $contractDir $contractName $contractGuid

Write-File (Join-Path $contractDir "I${Subsystem}Manager.cs") @"
namespace $contractName
{
    /// <summary>
    /// Public contract for the $Subsystem Manager.
    /// Only add operations that cross subsystem boundaries here.
    /// Keep internal Engine and Access interfaces in the Manager project.
    /// </summary>
    public interface I${Subsystem}Manager
    {
        // TODO: Add public manager operations
        // Follow IDesign contract rules:
        //   - 3-5 operations per contract (optimal)
        //   - Expose atomic business verbs, not CRUD
        //   - Parameters: primitives and plain DTOs only
    }
}
"@

# ══════════════════════════════════════════════════════════════════════════════
# ACCESS
# ══════════════════════════════════════════════════════════════════════════════
Write-Host "── CMI.Access.$Subsystem"
$accessName = "CMI.Access.$Subsystem"
$accessGuid = $projectGuids[$accessName]
$contractRelPath = "..\..\Contract\$Subsystem\$contractName.csproj"

New-LibraryCsproj $accessDir $accessName $accessGuid @($contractRelPath)
New-AssemblyInfo  $accessDir $accessName $accessGuid

Write-File (Join-Path $accessDir "${Subsystem}Access.cs") @"
using $contractName;

namespace $accessName
{
    /// <summary>
    /// Resource access for the $Subsystem subsystem.
    /// Expose atomic business verbs — never CRUD (Select/Insert/Delete).
    /// This class stays internal to the subsystem; its interface is not public.
    /// </summary>
    public class ${Subsystem}Access
    {
        // TODO: Inject dependencies (e.g. connection string) via constructor
        // TODO: Implement atomic business verb methods
    }
}
"@

# ══════════════════════════════════════════════════════════════════════════════
# ENGINE (optional)
# ══════════════════════════════════════════════════════════════════════════════
if ($IncludeEngine) {
    Write-Host "── CMI.Engine.$Subsystem"
    $engineName = "CMI.Engine.$Subsystem"
    $engineGuid = $projectGuids[$engineName]
    $accessRelFromEngine = "..\..\Access\$Subsystem\$accessName.csproj"

    New-LibraryCsproj $engineDir $engineName $engineGuid @($accessRelFromEngine)
    New-AssemblyInfo  $engineDir $engineName $engineGuid

    Write-File (Join-Path $engineDir "PlaceholderEngine.cs") @"
namespace $engineName
{
    /// <summary>
    /// Engine for the $Subsystem subsystem.
    ///
    /// IMPORTANT — Rename this class with a Gerund prefix (verb+ing), e.g.:
    ///   ProcessingEngine, CalculatingEngine, ValidatingEngine
    ///
    /// An Engine encapsulates HOW a specific business activity is performed.
    /// It knows business rules and algorithms, but NOT when it is called or
    /// what use case triggered it.
    ///
    /// This class stays internal to the subsystem; its interface is not public.
    /// </summary>
    public class PlaceholderEngine
    {
        // TODO: Rename class with gerund prefix
        // TODO: Inject Access dependencies via constructor if needed
        // TODO: Implement business logic methods
    }
}
"@
}

# ══════════════════════════════════════════════════════════════════════════════
# MANAGER
# ══════════════════════════════════════════════════════════════════════════════
Write-Host "── CMI.Manager.$Subsystem"
$managerName = "CMI.Manager.$Subsystem"
$managerGuid = $projectGuids[$managerName]

$managerRefs = @(
    "..\..\Contract\$Subsystem\$contractName.csproj",
    "..\..\Access\$Subsystem\$accessName.csproj"
)
if ($IncludeEngine) {
    $managerRefs += "..\..\Engine\$Subsystem\CMI.Engine.$Subsystem.csproj"
}

New-LibraryCsproj $managerDir $managerName $managerGuid $managerRefs
New-AssemblyInfo  $managerDir $managerName $managerGuid

Write-File (Join-Path $managerDir "${Subsystem}Manager.cs") @"
using $contractName;

namespace $managerName
{
    /// <summary>
    /// Manager for the $Subsystem subsystem.
    /// Orchestrates the workflow — knows WHEN to call Engines and Access.
    /// Does NOT know HOW Engines or Access work internally.
    /// Communicates with other Managers only via the MassTransit message bus.
    /// </summary>
    public class ${Subsystem}Manager : I${Subsystem}Manager
    {
        private readonly ${Subsystem}Access _access;

        public ${Subsystem}Manager(${Subsystem}Access access)
        {
            _access = access;
        }

        // TODO: Implement I${Subsystem}Manager operations
    }
}
"@

Write-File (Join-Path $managerDir "${Subsystem}Service.cs") @"
using System.Reflection;
using Autofac;
using CMI.Contract.Messaging;
using CMI.Contract.Monitoring;
using CMI.Manager.$Subsystem.Infrastructure;
using CMI.Utilities.Bus.Configuration;
using CMI.Utilities.Logging.Configurator;
using MassTransit;
using Serilog;

namespace $managerName
{
    public class ${Subsystem}Service
    {
        private readonly ContainerBuilder _containerBuilder;
        private IBusControl _bus;

        public ${Subsystem}Service()
        {
            _containerBuilder = ContainerConfigurator.Configure();
            LogConfigurator.ConfigureForService();
        }

        public void Start()
        {
            Log.Information("$Subsystem service is starting");

            var helper = new ParameterBusHelper();
            BusConfigurator.ConfigureBus(_containerBuilder, MonitoredServices.${Subsystem}Service, (cfg, ctx) =>
            {
                // TODO: Register MassTransit receive endpoints / consumers
                // Example:
                // cfg.ReceiveEndpoint(BusConstants.${Subsystem}SomeQueue, ec =>
                // {
                //     ec.Consumer(ctx.Resolve<IConsumer<SomeRequest>>);
                // });

                cfg.UseNewtonsoftJsonSerializer();
                helper.SubscribeAllSettingsInAssembly(Assembly.GetExecutingAssembly(), cfg);
            });

            var container = _containerBuilder.Build();
            _bus = container.Resolve<IBusControl>();
            _bus.Start();

            Log.Information("$Subsystem service started");
        }

        public void Stop()
        {
            Log.Information("$Subsystem service is stopping");
            _bus.Stop();
            Log.Information("$Subsystem service stopped");
            Log.CloseAndFlush();
        }
    }
}
"@

New-Item -ItemType Directory -Path (Join-Path $managerDir "Infrastructure") -Force | Out-Null
Write-File (Join-Path $managerDir "Infrastructure\ContainerConfigurator.cs") @"
using Autofac;

namespace $managerName.Infrastructure
{
    public static class ContainerConfigurator
    {
        public static ContainerBuilder Configure()
        {
            var builder = new ContainerBuilder();

            // TODO: Register consumers
            // builder.RegisterType<SomeRequestConsumer>();

            // TODO: Register Manager and its dependencies
            builder.RegisterType<${Subsystem}Access>().AsSelf();
            builder.RegisterType<${Subsystem}Manager>().As<I${Subsystem}Manager>();

            return builder;
        }
    }
}
"@

# ══════════════════════════════════════════════════════════════════════════════
# HOST
# ══════════════════════════════════════════════════════════════════════════════
Write-Host "── CMI.Host.$Subsystem"
$hostName = "CMI.Host.$Subsystem"
$hostGuid = $projectGuids[$hostName]
$managerRelFromHost = "..\..\Manager\$Subsystem\$managerName.csproj"

# Host .csproj (Exe, with Topshelf)
$managerRef = @"

  <ItemGroup>
    <ProjectReference Include="$managerRelFromHost">
      <Project>$managerGuid</Project>
      <Name>$managerName</Name>
    </ProjectReference>
  </ItemGroup>
"@

Write-File (Join-Path $hostDir "$hostName.csproj") @"
<?xml version="1.0" encoding="utf-8"?>
<Project ToolsVersion="15.0" xmlns="http://schemas.microsoft.com/developer/msbuild/2003">
  <Import Project="`$(MSBuildExtensionsPath)\`$(MSBuildToolsVersion)\Microsoft.Common.props" Condition="Exists('`$(MSBuildExtensionsPath)\`$(MSBuildToolsVersion)\Microsoft.Common.props')" />
  <PropertyGroup>
    <Configuration Condition=" '`$(Configuration)' == '' ">Debug</Configuration>
    <Platform Condition=" '`$(Platform)' == '' ">x64</Platform>
    <ProjectGuid>$hostGuid</ProjectGuid>
    <OutputType>Exe</OutputType>
    <RootNamespace>$hostName</RootNamespace>
    <AssemblyName>$hostName</AssemblyName>
    <TargetFrameworkVersion>v4.8.1</TargetFrameworkVersion>
    <FileAlignment>512</FileAlignment>
    <AutoGenerateBindingRedirects>true</AutoGenerateBindingRedirects>
    <TargetFrameworkProfile />
    <NuGetPackageImportStamp></NuGetPackageImportStamp>
    <GenerateBindingRedirectsOutputType>true</GenerateBindingRedirectsOutputType>
  </PropertyGroup>
  <PropertyGroup Condition=" '`$(Configuration)|`$(Platform)' == 'Debug|x64' ">
    <PlatformTarget>x64</PlatformTarget>
    <DebugSymbols>true</DebugSymbols>
    <DebugType>full</DebugType>
    <Optimize>false</Optimize>
    <OutputPath>bin\Debug\</OutputPath>
    <DefineConstants>DEBUG;TRACE</DefineConstants>
    <ErrorReport>prompt</ErrorReport>
    <WarningLevel>4</WarningLevel>
    <Prefer32Bit>false</Prefer32Bit>
  </PropertyGroup>
  <PropertyGroup Condition=" '`$(Configuration)|`$(Platform)' == 'Release|x64' ">
    <PlatformTarget>x64</PlatformTarget>
    <DebugType>pdbonly</DebugType>
    <Optimize>true</Optimize>
    <OutputPath>bin\Release\</OutputPath>
    <DefineConstants>TRACE</DefineConstants>
    <ErrorReport>prompt</ErrorReport>
    <WarningLevel>4</WarningLevel>
    <Prefer32Bit>false</Prefer32Bit>
  </PropertyGroup>
  <ItemGroup>
    <Reference Include="System" />
    <Reference Include="System.Configuration.Install" />
    <Reference Include="System.Core" />
    <Reference Include="System.ServiceProcess" />
  </ItemGroup>
  <ItemGroup>
    <Compile Include="Program.cs" />
    <Compile Include="Properties\AssemblyInfo.cs" />
  </ItemGroup>$managerRef
  <ItemGroup>
    <PackageReference Include="Nerdbank.GitVersioning">
      <Version>3.10.91</Version>
      <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
      <PrivateAssets>all</PrivateAssets>
    </PackageReference>
    <PackageReference Include="Topshelf">
      <Version>4.3.0</Version>
    </PackageReference>
  </ItemGroup>
  <Import Project="`$(MSBuildToolsPath)\Microsoft.CSharp.targets" />
  <PropertyGroup>
    <PostBuildEvent>IF NOT '%25ON_BUILDSERVER%25'=='True' "`$(SolutionDir)CMI\Tools\UrbanCode\bin\`$(ConfigurationName)\CMI.Tools.UrbanCode.exe" e "`$(TargetDir) " "`$(SolutionDir)..\Credentials for develop.json"</PostBuildEvent>
  </PropertyGroup>
</Project>
"@

New-AssemblyInfo $hostDir $hostName $hostGuid

Write-File (Join-Path $hostDir "Program.cs") @"
using $managerName;
using Topshelf;

namespace $hostName
{
    internal class Program
    {
        // ReSharper disable once UnusedParameter.Local
        private static void Main(string[] args)
        {
            HostFactory.Run(x =>
            {
                x.Service<${Subsystem}Service>(s =>
                {
                    s.ConstructUsing(name => new ${Subsystem}Service());
                    s.WhenStarted(tc => tc.Start());
                    s.WhenStopped(tc => tc.Stop());
                });
                x.RunAsLocalSystem();

                x.SetDescription("TODO: Describe what this service does");
                x.SetDisplayName("CMI Viaduc $Subsystem Service");
                x.SetServiceName("CMI${Subsystem}Service");
            });
        }
    }
}
"@

# ══════════════════════════════════════════════════════════════════════════════
# Summary
# ══════════════════════════════════════════════════════════════════════════════
Write-Host ""
Write-Host "Subsystem '$Subsystem' scaffolded successfully."
Write-Host ""
Write-Host "Generated projects:"
Write-Host "  CMI/Contract/$Subsystem/  (CMI.Contract.$Subsystem - Library)"
Write-Host "  CMI/Access/$Subsystem/    (CMI.Access.$Subsystem   - Library)"
if ($IncludeEngine) {
Write-Host "  CMI/Engine/$Subsystem/    (CMI.Engine.$Subsystem   - Library)"
}
Write-Host "  CMI/Manager/$Subsystem/   (CMI.Manager.$Subsystem  - Library)"
Write-Host "  CMI/Host/$Subsystem/      (CMI.Host.$Subsystem     - Exe)"
Write-Host ""
Write-Host "Required manual steps:"
$engineStep = if ($IncludeEngine) { " > Engine" } else { "" }
Write-Host "  1. Add all projects to Viaduc.sln in Visual Studio (Add > Existing Project)"
Write-Host "     Solution folder order: Contract > Access$engineStep > Manager > Host"
Write-Host "  2. Add '${Subsystem}Service' to MonitoredServices enum in CMI.Contract.Monitoring"
Write-Host "  3. Add queue name constants to BusConstants in CMI.Contract.Messaging"
Write-Host "  4. Register consumers in ContainerConfigurator.cs and wire them in ${Subsystem}Service.cs"
if ($IncludeEngine) {
Write-Host "  5. Rename PlaceholderEngine.cs with a gerund prefix (e.g. ProcessingEngine)"
}
Write-Host "  6. Restore NuGet packages: nuget restore Viaduc.sln"
Write-Host "  7. Build: msbuild Viaduc.sln /p:Configuration=Release /p:Platform=x64"
