@echo off

set platform=Win32
set configuration=Release

set nuget=%~dp0nuget
set lib=%~dp0lib
set nupkg=%~dp0ht4o

for %%p in (%*) do (
	if /i "%%~p" == "x86" (
		set platform=Win32
	) else (
		if /i "%%~p" == "win32" (
			set platform=Win32
		) else (
			if /i "%%~p" == "x64" (
				set platform=x64
			) else (
				if /i "%%~p" == "win64" (
					set platform=x64
				) else (
					if /i "%%~p" == "release" (
						set configuration=Release
					) else (
						if /i "%%~p" == "debug" (
							set configuration=Debug
						)
					)
				)
			)
		)
	)
)


if exist %lib% rmdir /S /Q %lib%
mkdir %lib%

mkdir %lib%\net40
set bin=%~dp0..\dist\10.0\%platform%\%configuration%
if not exist %bin%\ht4o.dll goto :missing_ht4o
xcopy /Q %bin%\ht4o.dll %lib%\net40\ > nul

mkdir %lib%\net45
set bin=%~dp0..\dist\11.0\%platform%\%configuration%
if not exist %bin%\ht4o.dll goto :missing_ht4o
xcopy /Q %bin%\ht4o.dll %lib%\net45\ > nul

%nuget% update -self

set target=
if /i "%platform%"=="Win32" (
	set target=x86
) else (
	set target=x64
)

if exist %nupkg%.%target%.*.nupkg del /Q %nupkg%.%target%.*.nupkg
cd %~dp0
%nuget% pack %nupkg%.%target%.nuspec

if exist %lib% rmdir /S /Q %lib%
if exist %~dp0*.%target%.nuspec del %~dp0*.%target%.nuspec

goto :done

:missing_ht4o
echo %bin%\ht4o.dll does not exists
goto done:

:done

