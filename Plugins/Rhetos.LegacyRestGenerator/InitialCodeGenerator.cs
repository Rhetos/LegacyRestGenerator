﻿/*
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
using Rhetos.Dsl;
using Rhetos.Logging;
using Rhetos.Processing;
using System;
using System.IO;
using System.ServiceModel;
using System.ServiceModel.Web;
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
using Autofac;
using Rhetos.Dom;
using Rhetos.Dom.DefaultConcepts;
using Rhetos.Logging;
using Rhetos.Processing;
using Rhetos.Processing.DefaultCommands;
using Rhetos.Web;
using Rhetos.XmlSerialization;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Serialization.Json;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Configuration;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;
using System.ServiceModel.Web;
using System.Text;
using System.Web;
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
            if (Description.Behaviors.Find<Rhetos.Web.JsonErrorServiceBehavior>() == null)
                Description.Behaviors.Add(new Rhetos.Web.JsonErrorServiceBehavior());
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

        public void InitializeApplicationInstance(System.Web.HttpApplication context)
        {
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
        private readonly Rhetos.Utilities.XmlUtility _xmlUtility;
        private readonly Rhetos.Dom.IDomainObjectModel _dom;

        public DomainService(
            IServerApplication serverApplication,
            ILogProvider logProvider,
            Rhetos.Utilities.XmlUtility xmlUtility,
            Rhetos.Dom.IDomainObjectModel dom)
        {
            ILogger logger = logProvider.GetLogger(""RestService"");
            logger.Trace(""Service initialization."");

            _serverApplication = serverApplication;
            _xmlUtility = xmlUtility;
            _dom = dom;
        }

        private ServerCommandInfo ToServerCommand(ICommandInfo commandInfo)
        {
            return new ServerCommandInfo
            {
                CommandName = commandInfo.GetType().Name,
                Data = _xmlUtility.SerializeToXml(commandInfo)
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
            codeBuilder.AddReferencesFromDependency(typeof(Guid));
            codeBuilder.AddReferencesFromDependency(typeof(WebFaultException));
            codeBuilder.AddReferencesFromDependency(typeof(System.Linq.Enumerable));
            codeBuilder.AddReferencesFromDependency(typeof(System.Net.HttpStatusCode));
            codeBuilder.AddReferencesFromDependency(typeof(Rhetos.Utilities.XmlUtility));
            codeBuilder.AddReferencesFromDependency(typeof(Rhetos.Web.ErrorServiceBehavior));
            codeBuilder.AddReferencesFromDependency(typeof(ILogProvider));
            codeBuilder.AddReferencesFromDependency(typeof(Rhetos.Web.JsonErrorServiceBehavior));
            codeBuilder.AddReferencesFromDependency(typeof(Rhetos.Dom.IDomainObjectModel));

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

            foreach (var file in Directory.GetFiles(_rootPath, "ServerDom*.dll", SearchOption.AllDirectories))
                codeBuilder.AddReference(file);
            codeBuilder.AddReference(Path.Combine(_rootPath, "Autofac.dll"));
        }

        private static readonly string _rootPath = AppDomain.CurrentDomain.BaseDirectory;
    }
}