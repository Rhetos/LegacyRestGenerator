ECHO Target folder = [%1]
ECHO $(ConfigurationName) = [%2]

REM "%~dp0" is this script's folder.

XCOPY /Y/D/R "%~dp0"Plugins\Rhetos.LegacyRestGenerator\bin\%2\Rhetos.LegacyRestGenerator.dll %1 || EXIT /B 1
XCOPY /Y/D/R "%~dp0"Plugins\Rhetos.LegacyRestGenerator\bin\%2\Rhetos.LegacyRestGenerator.pdb %1 || EXIT /B 1
XCOPY /Y/D/R "%~dp0"Plugins\Rhetos.LegacyRestGenerator.DefaultConcepts\bin\%2\Rhetos.LegacyRestGenerator.DefaultConcepts.dll %1 || EXIT /B 1
XCOPY /Y/D/R "%~dp0"Plugins\Rhetos.LegacyRestGenerator.DefaultConcepts\bin\%2\Rhetos.LegacyRestGenerator.DefaultConcepts.pdb %1 || EXIT /B 1

EXIT /B 0
