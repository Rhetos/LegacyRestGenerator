LegacyRestGenerator
=================

LegacyRestGenerator is a DSL package (a plugin module) for [Rhetos development platform](https://github.com/Rhetos/Rhetos).

LegacyRestGenerator automatically generates REST interface for all data structures and actions that are defined in a Rhetos application.

See [rhetos.org](http://www.rhetos.org/) for more information on Rhetos.

Features
========

LegacyRestGenerator provides DomainService.svc service that is single service with multiple operations.
Read/Insert/Update/Delete operations for all entities as well as Actions and other operations are available through this single service (single WSDL).

Prerequisites
=============

Utilities in this project are based on relative path to Rhetos repository. [Rhetos source](https://github.com/Rhetos/Rhetos) must be downloaded to a folder with relative path "..\..\Rhetos". 

Sample folder structure:
 
	\ROOT
		\Rhetos
		\RhetosPackages
			\LegacyRestGenerator


Build and Installation
======================

Build package with Build.bat. Check BuildError.log for errors.

Instalation package creation:

1. Set the new version number in "ChangeVersion.bat" and start it.
2. Start "CreatePackage.bat". Instalation package (.zip) is going to be created in parent directory of LegacyRestGenerator.