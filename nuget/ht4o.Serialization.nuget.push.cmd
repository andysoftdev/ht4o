@echo off

set configuration=Release
set version=

set nuget=%~dp0nuget
set nupkg=%~dp0ht4o.Serialization

for %%p in (%*) do (
	if /i "%%~p" == "release" (
		set configuration=Release
	) else (
		if /i "%%~p" == "debug" (
			set configuration=Debug
		) else (
			set version=%%p
		)
	)
)

%nuget% update -self

if not exist %nupkg%.%version%.nupkg goto :missing_nupkg
%nuget% push %nupkg%.%version%.nupkg

goto :done

:missing_nupkg
echo %nupkg%.%version%.nupkg does not exists
goto done:

:done

