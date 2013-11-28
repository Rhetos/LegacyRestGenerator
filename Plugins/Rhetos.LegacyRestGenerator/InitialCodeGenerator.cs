/*
    Copyright (C) 2013 Omega software d.o.o.

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
using System.ServiceModel;
using System.ServiceModel.Web;
using Rhetos;
using Rhetos.Utilities;
using Rhetos.Compiler;
using Rhetos.Dom;
using Rhetos.Dsl;
using Rhetos.Logging;
using Rhetos.Processing;
using Rhetos.Security;
using System.IO;
using System.Web.Routing;

namespace Rhetos.LegacyRestGenerator
{
    public class InitialCodeGenerator : ILegacyRestGeneratorPlugin
    {
        public const string UsingTag = "/*using*/";
        public const string ImplementationMembersTag = "/*implementation*/";
        public const string NamespaceMembersTag = "/*body*/";

        private const string CodeSnippet =
@"
using System;
using System.Collections.Generic;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.ServiceModel.Channels;
using System.ServiceModel.Configuration;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;
using System.Web;
using System.IO;
using System.Text;
using System.Net;
using Rhetos;
using Rhetos.Processing;
using Rhetos.Logging;
using Autofac;
using Rhetos.XmlSerialization;
using Rhetos.Dom.DefaultConcepts;
using System.Runtime.Serialization.Json;
using Rhetos.Processing.DefaultCommands;
using System.Web.Routing;
using Module = Autofac.Module;

" + UsingTag + @"

namespace Rhetos
{
    public class RestServiceHostFactory : Autofac.Integration.Wcf.AutofacServiceHostFactory
    {
        protected override ServiceHost CreateServiceHost(Type serviceType, Uri[] baseAddresses)
        {
            RestServiceHost host = new RestServiceHost(serviceType, baseAddresses);

            return host;
        }
    }

    public class RestServiceHost : ServiceHost
    {
        private Type _serviceType;

        public RestServiceHost(Type serviceType, Uri[] baseAddresses)
            : base(serviceType, baseAddresses)
        {
            _serviceType = serviceType;
        }

        protected override void OnOpening()
        {
            base.OnOpening();
            this.AddServiceEndpoint(_serviceType, new WebHttpBinding(""rhetosWebHttpBinding""), string.Empty);
            this.AddServiceEndpoint(_serviceType, new BasicHttpBinding(""rhetosBasicHttpBinding""), ""SOAP"");

            ((ServiceEndpoint)(Description.Endpoints.Where(e => e.Binding is WebHttpBinding).Single())).Behaviors.Add(new WebHttpBehavior()); 
            if (Description.Behaviors.Find<Rhetos.JsonErrorServiceBehavior>() == null)
                Description.Behaviors.Add(new Rhetos.JsonErrorServiceBehavior());
        }
    }

    [System.ComponentModel.Composition.Export(typeof(Module))]
    public class LegacyRestServiceModuleConfiguration : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<DomainService>().InstancePerLifetimeScope();
            base.Load(builder);
        }
    }

    [System.ComponentModel.Composition.Export(typeof(Rhetos.IService))]
    public class LegacyRestServiceInitializer : Rhetos.IService
    {
        public void Initialize()
        {
            System.Web.Routing.RouteTable.Routes.Add(new System.ServiceModel.Activation.ServiceRoute(""DomainService.svc"", 
                          new RestServiceHostFactory(), typeof(DomainService)));
        }
    }

" + NamespaceMembersTag + @"

    public class MessagesResult 
    {
        public string SystemMessage;
        public string UserMessage;

        public override string ToString() 
        {
            return ""SystemMessage: "" + (SystemMessage ?? ""<null>"") + "", UserMessage: "" + (UserMessage ?? ""<null>"");
        }
    } 

    [ServiceContract]
	[System.ServiceModel.Activation.AspNetCompatibilityRequirements(RequirementsMode = System.ServiceModel.Activation.AspNetCompatibilityRequirementsMode.Required)]
	public class DomainService
	{

        private readonly IServerApplication _serverApplication;

        public DomainService(
            IServerApplication serverApplication,
            Rhetos.Dom.IDomainObjectModel domainObjectModel,
            ILogProvider logProvider)
        {
            ILogger logger = logProvider.GetLogger(""RestService"");
            logger.Trace(""Service initialization."");

            _serverApplication = serverApplication;

            if (Rhetos.Utilities.XmlUtility.Dom == null)
                lock(Rhetos.Utilities.XmlUtility.DomLock)
                    if (Rhetos.Utilities.XmlUtility.Dom == null)
                    {
                        Rhetos.Utilities.XmlUtility.Dom = domainObjectModel.ObjectModel;
                        logger.Trace(""Domain object model initialized."");
                    }
        }

        private static ServerCommandInfo ToServerCommand(ICommandInfo commandInfo)
        {
            return new ServerCommandInfo
            {
                CommandName = commandInfo.GetType().Name,
                Data = Rhetos.Utilities.XmlUtility.SerializeToXml(commandInfo)
            };
        }

        private static void CheckForErrors(ServerProcessingResult result)
        {
            if (!result.Success)
                throw new WebFaultException<MessagesResult>(
                        new MessagesResult { SystemMessage = result.SystemMessage, UserMessage = result.UserMessage }, 
                        string.IsNullOrEmpty(result.UserMessage) ? HttpStatusCode.InternalServerError : HttpStatusCode.BadRequest
                );
        }

" + ImplementationMembersTag + @"
    
    }
}
";

        public void GenerateCode(IConceptInfo conceptInfo, ICodeBuilder codeBuilder)
        {
            codeBuilder.InsertCode(CodeSnippet);

            codeBuilder.AddReferencesFromDependency(typeof(IServerApplication));
            codeBuilder.AddReferencesFromDependency(typeof(ServiceContractAttribute));
            codeBuilder.AddReferencesFromDependency(typeof(ICommandInfo));
            codeBuilder.AddReferencesFromDependency(typeof(XmlUtility));
            codeBuilder.AddReferencesFromDependency(typeof(Guid));
            codeBuilder.AddReferencesFromDependency(typeof(WebFaultException));
            codeBuilder.AddReferencesFromDependency(typeof(System.Linq.Enumerable));
            codeBuilder.AddReferencesFromDependency(typeof(System.Net.HttpStatusCode));
            codeBuilder.AddReferencesFromDependency(typeof(Rhetos.Utilities.XmlUtility));
            codeBuilder.AddReferencesFromDependency(typeof(Rhetos.Dom.IDomainObjectModel));
            codeBuilder.AddReferencesFromDependency(typeof(Rhetos.ErrorServiceBehavior));
            codeBuilder.AddReferencesFromDependency(typeof(ILogProvider));
            codeBuilder.AddReferencesFromDependency(typeof(Rhetos.JsonErrorServiceBehavior));

            // registration
            codeBuilder.AddReferencesFromDependency(typeof(System.ComponentModel.Composition.ExportAttribute));
            codeBuilder.AddReferencesFromDependency(typeof(Autofac.Integration.Wcf.AutofacServiceHostFactory));

            // wcf dataservices
            codeBuilder.AddReferencesFromDependency(typeof(System.ServiceModel.ServiceContractAttribute));
            codeBuilder.AddReferencesFromDependency(typeof(System.ServiceModel.Activation.AspNetCompatibilityRequirementsAttribute));
            codeBuilder.AddReferencesFromDependency(typeof(System.ServiceModel.Web.WebServiceHost));
            codeBuilder.AddReferencesFromDependency(typeof(System.Uri));
            codeBuilder.AddReferencesFromDependency(typeof(System.Web.Routing.RouteTable));
            codeBuilder.AddReferencesFromDependency(typeof(System.ServiceModel.Activation.ServiceHostFactory));
            codeBuilder.AddReferencesFromDependency(typeof(Route));

            codeBuilder.AddReference(Path.Combine(_rootPath, "ServerDom.dll"));
            codeBuilder.AddReference(Path.Combine(_rootPath, "Autofac.dll"));
        }

        private static readonly string _rootPath = AppDomain.CurrentDomain.BaseDirectory;
    }
}