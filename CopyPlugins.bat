ECHO Target folder = [%1]
ECHO $(ConfigurationName) = [%2]

SET ThisScriptFolder=%~dp0

XCOPY /Y/D/R %ThisScriptFolder%Plugins\Rhetos.LegacyRestGenerator\bin\%2\Rhetos.LegacyRestGenerator.dll %1 || EXIT /B 1
XCOPY /Y/D/R %ThisScriptFolder%Plugins\Rhetos.LegacyRestGenerator\bin\%2\Rhetos.LegacyRestGenerator.pdb %1 || EXIT /B 1
XCOPY /Y/D/R %ThisScriptFolder%Plugins\Rhetos.LegacyRestGenerator.DefaultConcepts\bin\%2\Rhetos.LegacyRestGenerator.DefaultConcepts.dll %1 || EXIT /B 1
XCOPY /Y/D/R %ThisScriptFolder%Plugins\Rhetos.LegacyRestGenerator.DefaultConcepts\bin\%2\Rhetos.LegacyRestGenerator.DefaultConcepts.pdb %1 || EXIT /B 1

EXIT /B 0
