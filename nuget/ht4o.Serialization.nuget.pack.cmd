@echo off

set configuration=Release

set nuget=%~dp0nuget
set lib=%~dp0lib
set nupkg=%~dp0ht4o.Serialization

for %%p in (%*) do (
	if /i "%%~p" == "release" (
		set configuration=Release
	) else (
		if /i "%%~p" == "debug" (
			set configuration=Debug
		)
	)
)

set bin=%~dp0..\dist\AnyCPU\%configuration%
if not exist %bin%\ht4o.Serialization.dll goto :missing_ht4o_serialization

if exist %lib% rmdir /S /Q %lib%
mkdir %lib%
mkdir %lib%\net40

xcopy /Q %bin%\ht4o.Serialization.dll %lib%\net40\ > nul

%nuget% update -self

if exist %nupkg%.*.nupkg del /Q %nupkg%.*.nupkg
cd %~dp0
%nuget% pack %nupkg%.nuspec

if exist %lib% rmdir /S /Q %lib%
if exist %~dp0*.nuspec del %~dp0*.nuspec

goto :done

:missing_ht4o_serialization
echo %bin%\ht4o.Serialization.dll does not exists
goto done:

:done

