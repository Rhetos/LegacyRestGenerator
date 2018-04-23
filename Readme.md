# LegacyRestGenerator

LegacyRestGenerator is a plugin package for [Rhetos development platform](https://github.com/Rhetos/Rhetos).
It automatically generates REST interface for all data structures and actions that are defined in a Rhetos application.

See [rhetos.org](http://www.rhetos.org/) for more information on Rhetos.

## Features

LegacyRestGenerator provides DomainService.svc service that is single service with multiple operations.
Read/Insert/Update/Delete operations for all entities as well as Actions and other operations are available through this single service (single WSDL).

## Build

To build the package from source, run `Build.bat`.
The script will pause in case of an error.
The build output is a NuGet package in the "Install" subfolder.

## Installation

To install this package to a Rhetos server, add it to the Rhetos server's *RhetosPackages.config* file
and make sure the NuGet package location is listed in the *RhetosPackageSources.config* file.

* The package ID is "**Rhetos.LegacyRestGenerator**".
* For more information, see [Installing plugin packages](https://github.com/Rhetos/Rhetos/wiki/Installing-plugin-packages).
