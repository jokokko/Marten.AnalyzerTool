#r @"tools/FAKE/tools/FakeLib.dll"
open System.IO
open Fake
open Fake.AssemblyInfoFile
open Fake.Git.Information
open Fake.SemVerHelper
open Fake.Testing
open System

let buildMode = getBuildParamOrDefault "buildMode" "Release"
let buildArtifactPath = FullName "./artifacts"
let packagesPath = FullName "./tools"
let assemblyVersion = "1.2.0.0"
let baseVersion = "1.2.0"

let envVersion = (environVarOrDefault "APPVEYOR_BUILD_VERSION" (baseVersion + ".0"))
let buildVersion = (envVersion.Substring(0, envVersion.LastIndexOf('.')))

let semVersion : SemVerInfo = (parse buildVersion)

let Version = semVersion.ToString()

let branch = (fun _ ->
  (environVarOrDefault "APPVEYOR_REPO_BRANCH" (getBranchName "."))
)

let FileVersion = (environVarOrDefault "APPVEYOR_BUILD_VERSION" (Version + "." + "0"))

let informationalVersion = (fun _ ->
  let branchName = (branch ".")
  let label = if branchName="master" then "" else " (" + branchName + "/" + (getCurrentSHA1 ".").[0..7] + ")"
  (FileVersion + label)
)

let nugetVersion = (fun _ ->
  let branchName = (branch ".")
  let label = if branchName="master" then "" else "-" + branchName
  let version = if branchName="master" then Version else FileVersion
  (version + label)
)

let InfoVersion = informationalVersion()
let NuGetVersion = nugetVersion()

let versionArgs = [ @"/p:Version=""" + NuGetVersion + @""""; @"/p:AssemblyVersion=""" + FileVersion + @""""; @"/p:FileVersion=""" + FileVersion + @""""; @"/p:InformationalVersion=""" + InfoVersion + @"""" ]

printfn "Using version: %s" Version

Target "Clean" (fun _ ->
  ensureDirectory buildArtifactPath

  CleanDir buildArtifactPath
)

Target "RestorePackages" (fun _ -> 
     "./src/Marten.AnalyzerTool.sln"
     |> RestoreMSSolutionPackages (fun p ->
         { p with
             OutputPath = packagesPath
             Retries = 4 })
)

Target "Build" (fun _ ->

  CreateCSharpAssemblyInfo @".\src\Version.cs"
    [ Attribute.Title "Marten.AnalyzerTool"
      Attribute.Description "Catalog projections & projection wiring in codebases using Marten"
      Attribute.Product "Marten.AnalyzerTool"
      Attribute.Copyright "Joona-Pekka Kokko"
      Attribute.Version assemblyVersion
      Attribute.FileVersion FileVersion
      Attribute.InformationalVersion InfoVersion
    ]

  let setParams defaults = { 
    defaults with
        Verbosity = Some(Quiet)
        Targets = ["Clean"; "Build"]
        Properties =
            [
                "Optimize", "True"
                "DebugSymbols", "True"
                "RestorePackages", "True"
                "Configuration", buildMode
                "SignAssembly", "False"                
                "Platform", "Any CPU"                
            ]
  }
  build setParams @"./src/Marten.AnalyzerTool.sln"
      |> DoNothing
)

Target "RestoreXunit"  (fun _ ->
      let settings = { RestoreSinglePackageDefaults with                    
                        OutputPath = "src/packages"
                        ExcludeVersion = true
                  }
      RestorePackageId (fun p -> settings) "xunit.runner.console"
)

Target "Test"  (fun _ ->
  trace "Test..."

  let Error = Fake.UnitTestCommon.TestRunnerErrorLevel.Error

  !! (sprintf "./src/*.Tests/bin/%s/*.Tests.dll" buildMode)  
  |> xUnit2 (fun p -> 
     {p with 
          TimeOut = (TimeSpan.FromMinutes 2.0)
          ErrorLevel = Error
          HtmlOutputPath = Some (buildArtifactPath @@ "xunit.html")})
)

Target "Default" (fun _ ->
  trace "Build starting..."
)

Target "Package" (fun _ ->    
    let zipFiles = CreateZip "." (buildArtifactPath @@ ("Marten.AnalyzerTool." + Version + ".zip")) "" DefaultZipLevel true
    !! (sprintf "./src/Marten.AnalyzerTool/bin/%s/*.dll" buildMode)
    ++ (sprintf "./src/Marten.AnalyzerTool/bin/%s/*.exe" buildMode)
    ++ (sprintf "./src/Marten.AnalyzerTool/bin/%s/*.exe.config" buildMode)    
    |> Seq.toArray
    |> zipFiles
)

"RestoreXunit"
  ==> "Build"
  ==> "Test"

"Default"
  ==> "Package"

"Clean"
  ==> "RestorePackages"
  ==> "Build"
  ==> "Test"  
  ==> "Default"

RunTargetOrDefault "Default"