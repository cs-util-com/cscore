:: This batch script can be used to link the folder its contained in
:: into a target Unity project directory. The target directory must contain an 
:: Assets folder as a savety guard to prevent linking mistakes
:: https://en.wikibooks.org/wiki/Windows_Batch_Scripting

	@echo off
	cls
	@setlocal enabledelayedexpansion
	@cd /d "%~dp0"
	set "sourceFiles=%~dp0."

	set UserInput=%1
	set "targetFiles=!UserInput!\Assets"
	IF EXIST "!targetFiles!" (
		echo passed parameter used as folder
	) else (
		set /p UserInput= "Copy+Paste the target Unity project path where I should be linked into:"
	)
	set "targetFiles=!UserInput!\Assets"
	IF EXIST "!targetFiles!" (
		set "targetFiles=!targetFiles!" 
		call :traverceFolders2 "!sourceFiles!" "!targetFiles!" ""
	) else (
		echo Could not find targetFiles=!targetFiles!
	)
	echo The linking finished successfully :)
	pause
	:: exit script
	goto :eof

:traverceFolders2
SETLOCAL
	set "sourceBaseFolder=%1"
	:: remove all " from the string:
	set sourceBaseFolder=%sourceBaseFolder:"=%
	set "targetBaseFolder=%2"
	:: remove all " from the string:
	set targetBaseFolder=%targetBaseFolder:"=%
	set "currentFolder=%3"
	:: remove all " from the string:
	set currentFolder=%currentFolder:"=%

	set "sourceFolder=!sourceBaseFolder!!currentFolder!"
	::set "targetFolder=!targetBaseFolder!!currentFolder!"


	echo Now searching in folder !currentFolder!
	:: echo    - sourceBaseFolder    =!sourceBaseFolder!
	echo    - sourceFolder=!sourceFolder!
	echo    - targetBaseFolder=!targetBaseFolder!
	::echo    - targetFolder=!targetFolder!

	for /f "delims=" %%d in ('dir /b /ad-h-s') do (
		set "subfolder=%%d"
		set "newCurrentFolder=!currentFolder!\!subfolder!"
		cd !subfolder!
		echo cd !subfolder!
		call :traverceFolders "!sourceBaseFolder!" "!targetBaseFolder!" "!newCurrentFolder!"	
		cd ..
		echo .
	)
ENDLOCAL
goto :eof

:traverceFolders
SETLOCAL
	:: 1. parameter:
	set "sourceBaseFolder=%1"
	:: remove all " from the string:
	set sourceBaseFolder=%sourceBaseFolder:"=%

	:: 2. parameter:
	set "targetBaseFolder=%2"
	:: remove all " from the string:
	set targetBaseFolder=%targetBaseFolder:"=%

	:: 3. parameter:
	set "currentFolder=%3"
	:: remove all " from the string:
	set currentFolder=%currentFolder:"=%

	set "targetFolder=!targetBaseFolder!!currentFolder!"
	echo now mkdir !targetFolder!
	mkdir "!targetFolder!"

	echo Now searching in folder !currentFolder!
	:: echo    - sourceBaseFolder    =!sourceBaseFolder!
	:: echo    - targetBaseFolder=!targetBaseFolder!
	echo    - targetFolder=!targetFolder!
		
	for /f "delims=" %%d in ('dir /b /ad-h-s') do (
		set "subfolder=%%d"
		echo now subfolder  =!subfolder!
		set "sourceFolder=!sourceBaseFolder!!currentFolder!\!subfolder!"	
		echo  - sourceFolder=!sourceFolder!
		IF !subfolder!==Android (
			echo Found Android folder
			set "sourceFolder=!sourceBaseFolder!!currentFolder!"
			cd !subfolder!
			echo cd !subfolder!
			call :traverceFolders "!sourceFolder!" "!targetFolder!" "\!subfolder!"
			cd ..
			echo .
		) ELSE (
			set "backupCurrentDir=%cd%"
			cd !targetFolder!
			echo executing mklink /d "subfolder" "sourceFolder"
			mklink /d "!subfolder!" "!sourceFolder!"
			cd !backupCurrentDir!
			echo .
		)
	)
ENDLOCAL
exit /b