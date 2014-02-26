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

using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using Rhetos.Compiler;
using Rhetos.Dsl;
using Rhetos.Dsl.DefaultConcepts;
using Rhetos.Extensibility;
using Rhetos.LegacyRestGenerator;

namespace Rhetos.LegacyRestGenerator.DefaultConcepts
{
    [Export(typeof(ILegacyRestGeneratorPlugin))]
    [ExportMetadata(MefProvider.Implements, typeof(ActionInfo))]
    public class ActionCodeGenerator : ILegacyRestGeneratorPlugin
    {
        private static string ImplementationCodeSnippet(ActionInfo info)
        {
            return String.Format(
@"
        [OperationContract]
        [WebInvoke(Method = ""POST"", UriTemplate = ""/{0}/{1}"", BodyStyle = WebMessageBodyStyle.Bare, RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        public void Execute{0}{1}({0}.{1} action)
        {{
            var commandInfo = new ExecuteActionCommandInfo {{ Action = action }};
            var result = _serverApplication.Execute(ToServerCommand(commandInfo));
            CheckForErrors(result);
        }}
", info.Module.Name, info.Name);
        }

        public void GenerateCode(IConceptInfo conceptInfo, ICodeBuilder codeBuilder)
        {
            var info = (ActionInfo)conceptInfo;

            codeBuilder.InsertCode(ImplementationCodeSnippet(info), InitialCodeGenerator.ImplementationMembersTag);
        }
    }
}
