﻿//-----------------------------------------------------------------------
// <copyright file="RepoService.cs" company="">
// Copyright © 
// </copyright>
//-----------------------------------------------------------------------

namespace Navertica.SharePoint.RepoService.Service
{
    using System;
    using System.ComponentModel;
    using System.Web;
    using Microsoft.SharePoint.Administration;
    using Microsoft.SharePoint.Utilities;


    /// <summary>
    /// The Service. This is registered once per farm.
    /// </summary>
    [System.Runtime.InteropServices.Guid("d6bd012a-6feb-4baf-b399-035b9a3c76f8")]
    internal sealed class RepoService : SPIisWebService, IServiceAdministration
    {
        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="RepoService"/> class. Default constructor (required for SPPersistedObject serialization). Never call this directly.
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public RepoService()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RepoService"/> class. Use this constructor to install the service in the farm if it doesn't exist.
        /// </summary>
        /// <param name="farm">The <see cref="Microsoft.SharePoint.Administration.SPFarm"/> that this service will be installed in.</param>
        internal RepoService(SPFarm farm)
            : base(farm)
        {
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the name of the service. This will display on the Services on Server screen. You can localize this value.
        /// </summary>
        public override string TypeName
        {
            get
            {
                return SPUtility.GetLocalizedString("$Resources:ServiceName", "NVRRepoService.ServiceResources", (uint)System.Threading.Thread.CurrentThread.CurrentCulture.LCID);
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Returns the link to an ASPX page that can be used to create new Service Applications. Without this, you will not be able to create new service applications on the Manage Service Applications screen.
        /// </summary>
        /// <param name="serviceApplicationType">The <see cref="System.Type" /> of service application.</param>
        /// <returns>A <see cref="Microsoft.SharePoint.Administration.SPAdministrationLink" />.</returns>
        public override SPAdministrationLink GetCreateApplicationLink(Type serviceApplicationType)
        {
            if (serviceApplicationType != typeof(RepoServiceApplication))
            {
                throw new NotSupportedException();
            }

            return new SPAdministrationLink("/_admin/NVRRepoService/CreateApplication.aspx");
        }

        /// <summary>
        /// Gets the options for creating a new service application. Used to explicitly determine how/if the Farm Configuration Wizard can be used to provision this service application.
        /// </summary>
        /// <param name="serviceApplicationType">The <see cref="System.Type" /> of service application.</param>
        /// <returns>An <see cref="Microsoft.SharePoint.Administration.SPCreateApplicationOptions" /> value.</returns>
        public override SPCreateApplicationOptions GetCreateApplicationOptions(Type serviceApplicationType)
        {
            if (serviceApplicationType != typeof(RepoServiceApplication))
            {
                throw new NotSupportedException();
            }

            return SPCreateApplicationOptions.None;
        }

        /// <summary>
        /// Used for the Farm Configuration Wizard. Create the service application programmatically here if you want to support Single-Click install.
        /// </summary>
        /// <param name="name">The name of the service application.</param>
        /// <param name="serviceApplicationType">The <see cref="System.Type" /> of service application.</param>
        /// <param name="provisioningContext">The SPServiceProvisioningContext (will be passed in by the farm configuration wizard).</param>
        /// <returns>A reference to a ServiceApplication.</returns>
        public SPServiceApplication CreateApplication(string name, Type serviceApplicationType, SPServiceProvisioningContext provisioningContext)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Used for the Farm Configuration Wizard. Create the service application proxy programmatically here if you want to support Single-Click install.
        /// </summary>
        /// <param name="name">The name of the Service Application Proxy.</param>
        /// <param name="serviceApplication">The Service Application to associate with this proxy (will be passed in automatically by the configuration wizard).</param>
        /// <param name="provisioningContext">The SPServiceProvisioningContext (will be passed in by the farm configuration wizard).</param>
        /// <returns>A reference to a ServiceApplicationProxy.</returns>
        public SPServiceApplicationProxy CreateProxy(string name, SPServiceApplication serviceApplication, SPServiceProvisioningContext provisioningContext)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Gets a friendly name and description for this service application for display in the farm configuration wizard. You can localize these values.
        /// </summary>
        /// <param name="serviceApplicationType">The <see cref="System.Type" /> of service application.</param>
        /// <returns>An <see cref="SPPersistedTypeDescription"/> containing the name and description of the service application.</returns>
        public SPPersistedTypeDescription GetApplicationTypeDescription(Type serviceApplicationType)
        {
            if (serviceApplicationType != typeof(RepoServiceApplication))
            {
                throw new NotSupportedException();
            }

            return new SPPersistedTypeDescription(
                SPUtility.GetLocalizedString("$Resources:ServiceApplicationName", "NVRRepoService.ServiceResources", (uint)System.Threading.Thread.CurrentThread.CurrentCulture.LCID),
                SPUtility.GetLocalizedString("$Resources:ServiceApplicationDescription", "NVRRepoService.ServiceResources", (uint)System.Threading.Thread.CurrentThread.CurrentCulture.LCID));
        }

        /// <summary>
        /// Gets an array of the service application types supported by the service.
        /// </summary>
        /// <returns>An array of supported service application types.</returns>
        public Type[] GetApplicationTypes()
        {
            return new Type[] { typeof(RepoServiceApplication) };
        }

        /// <summary>
        /// Gets an existing service or creates it if it doesn't exist.
        /// </summary>
        /// <returns>An instance of the Service.</returns>
        internal static RepoService GetOrCreateService()
        {
            RepoService service = SPFarm.Local.Services.GetValue<RepoService>();
            if (service == null)
            {
                service = new RepoService(SPFarm.Local);
                service.Status = SPObjectStatus.Online;
                service.Update();
            }

            return service;
        }

        /// <summary>
        /// Removes the service and components from the farm.
        /// </summary>
        internal static void RemoveService()
        {
            RepoService service = SPFarm.Local.Services.GetValue<RepoService>();
            RepoServiceProxy serviceProxy = SPFarm.Local.ServiceProxies.GetValue<RepoServiceProxy>();

            // Uninstall any service applications          
            if (service != null)
            {
                foreach (SPServiceApplication app in service.Applications)
                {
                    app.Unprovision(true);
                    app.Delete();
                }
            }

            // Uninstall any remaining service application proxies (e.g. any connections to other farms)          
            if (serviceProxy != null)
            {
                foreach (SPServiceApplicationProxy proxy in serviceProxy.ApplicationProxies)
                {
                    proxy.Unprovision(true);
                    proxy.Delete();
                }
            }

            // Uninstall the instances
            foreach (SPServer server in SPFarm.Local.Servers)
            {
                RepoServiceInstance serviceInstance = server.ServiceInstances.GetValue<RepoServiceInstance>();
                while (serviceInstance != null)
                {
                    server.ServiceInstances.Remove(serviceInstance.Id);
                    serviceInstance = server.ServiceInstances.GetValue<RepoServiceInstance>();
                }
            }

            // Uninstall the service proxy
            if (serviceProxy != null)
            {
                SPFarm.Local.ServiceProxies.Remove(serviceProxy.Id);
            }

            // Uninstall the service
            if (service != null)
            {
                SPFarm.Local.Services.Remove(service.Id);
            }
        }

        #endregion
    }
}