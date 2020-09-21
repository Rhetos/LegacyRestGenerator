/*
    Copyright (C) 2014 Omega software d.o.o.

    This file is part of Rhetos.

    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU Affero General Public License as
    published by the Free Software Foundation, either version 3 of the
    License, or (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU Affero General Public License for more details.

    You should have received a copy of the GNU Affero General Public License
    along with this program.  If not, see <http://www.gnu.org/licenses/>.
*/

using Rhetos.Compiler;
using Rhetos.Dsl.DefaultConcepts;
using System;
using System.Linq;

namespace Rhetos.LegacyRestGenerator.DefaultConcepts
{
    public static class ReadParametersHelper
    {
        public static void GenerateFilterParametersIfSupported(
            ICodeBuilder codeBuilder,
            DataStructureInfo dataStructure,
            string parameterType)
        {
            if (DataStructureCodeGenerator.IsTypeSupported(dataStructure))
                codeBuilder.InsertCode(CodeSnippet(dataStructure, parameterType), DataStructureCodeGenerator.FilterTypesTag, dataStructure);
        }

        private static string CodeSnippet(DataStructureInfo dataStructure, string parameterType)
        {
            string fullTypeName = System.CodeDom.Compiler.CodeGenerator.IsValidLanguageIndependentIdentifier(parameterType)
                ? dataStructure.Module.Name + "." + parameterType
                : parameterType;

            string result = $@"Tuple.Create(""{fullTypeName}"", typeof({fullTypeName})),
                ";

            var shortName = TryExtractShortName(fullTypeName);
            if (shortName != null)
                result += $@"Tuple.Create(""{shortName}"", typeof({fullTypeName})),
                ";

            return result;
        }

        private static string TryExtractShortName(string typeName)
        {
            if (typeName.Contains('.'))
            {
                var shortName = typeName.Split('.').Last();
                if (System.CodeDom.Compiler.CodeGenerator.IsValidLanguageIndependentIdentifier(shortName))
                    return shortName;
            }
            return null;
        }
    }
}
