﻿/*  Copyright (C) 2015 NAVERTICA a.s. http://www.navertica.com 

    This program is free software; you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation; either version 2 of the License, or
    (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License along
    with this program; if not, write to the Free Software Foundation, Inc.,
    51 Franklin Street, Fifth Floor, Boston, MA 02110-1301 USA.  */

using Navertica.SharePoint.ConfigService.Administration;
using Navertica.SharePoint.ConfigService.Service;

namespace Navertica.SharePoint.ConfigService.PowerShell
{
    using System;
    using System.Management.Automation;
    using Microsoft.SharePoint.Administration;
    using Microsoft.SharePoint.PowerShell;
    using ConfigService;

    /// <summary>
    /// Creates a new Service Application Proxy.
    /// </summary>
    [Cmdlet(VerbsCommon.New, "ConfigServiceApplicationProxy", SupportsShouldProcess = true)]
    [SPCmdlet(RequireLocalFarmExist = true, RequireUserFarmAdmin = true)]
    [System.Runtime.InteropServices.Guid("a8806e71-a98c-40e2-8e60-4aec0af994a3")]
    public class NewConfigServiceApplicationProxy : SPCmdlet
    {
        #region Fields

        /// <summary>
        /// The service application.
        /// </summary>
        private SPServiceApplicationPipeBind application;

        /// <summary>
        /// The name of the proxy.
        /// </summary>
        private string name;

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the service application to associate this proxy with.
        /// </summary>
        [Parameter(Mandatory = true, Position = 0, ParameterSetName = "Application", ValueFromPipeline = true)]
        [ValidateNotNullOrEmpty]
        public SPServiceApplicationPipeBind ServiceApplication
        {
            get
            {
                return this.application;
            }

            set
            {
                this.application = value;
            }
        }

        /// <summary>
        /// Gets or sets the URI to a published service application.
        /// </summary>
        [Parameter(Mandatory = true, Position = 0, ParameterSetName = "URI")]
        [ValidateNotNullOrEmpty]
        public string Uri
        {
            get;

            set;
        }

        /// <summary>
        /// Gets or sets whether the proxy is added to the default proxy group for the farm.
        /// </summary>
        [Parameter(Mandatory = false)]
        public SwitchParameter DefaultProxyGroup
        {
            get;

            set;
        }

        /// <summary>
        /// Gets or sets the name of the proxy
        /// </summary>
        [Parameter(Mandatory = false, ParameterSetName = "Application"), Parameter(Mandatory = true, ParameterSetName = "Uri")]
        public string Name
        {
            get
            {
                return this.name;
            }

            set
            {
                this.name = value;
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// This method gets invoked when the command is called
        /// </summary>
        protected override void InternalProcessRecord()
        {
            Uri applicationUri = null;
            SPServiceApplication resolvedApplication = null;
            ConfigServiceApplication castedApplication = null;

            if (this.ServiceApplication == null && string.IsNullOrEmpty(this.Uri))
            {
                this.ThrowTerminatingError(new InvalidOperationException("No service application or Uri was provided."), ErrorCategory.InvalidOperation, this);
            }

            if (this.ServiceApplication != null)
            {
                resolvedApplication = this.ServiceApplication.Read();

                if (resolvedApplication == null)
                {
                    this.ThrowTerminatingError(new InvalidOperationException("Service application not found."), ErrorCategory.InvalidOperation, this);
                }

                castedApplication = resolvedApplication as ConfigServiceApplication;

                if (castedApplication == null)
                {
                    this.ThrowTerminatingError(new InvalidOperationException("Service application was not of the correct type."), ErrorCategory.InvalidOperation, this);
                }

                applicationUri = castedApplication.Uri;

                if (string.IsNullOrEmpty(this.Name))
                {
                    this.Name = castedApplication.Name + " Proxy";
                }
            }
            else
            {
                applicationUri = new Uri(this.Uri);
            }

            if (this.ShouldProcess(this.Name))
            {
                // Ensure the service exists
                NVRConfigService.GetOrCreateService();

                // Ensure the proxy exists
                ConfigServiceProxy serviceProxy = ConfigServiceProxy.GetOrCreateServiceProxy();

                // Create the service application proxy
                ConfigServiceApplicationProxy proxy = new ConfigServiceApplicationProxy(this.Name, serviceProxy, applicationUri);
                proxy.Update();
                proxy.Provision();

                if (this.DefaultProxyGroup.ToBool())
                {
                    SPServiceApplicationProxyGroup group = SPServiceApplicationProxyGroup.Default;
                    group.Add(proxy);
                    group.Update();
                }

                this.WriteObject(proxy);
            }
        }

        /// <summary>
        /// Validate the arguments.
        /// </summary>
        protected override void InternalValidate()
        {
            // Validate a farm exists
            SPFarm farm = SPFarm.Local;
            if (farm == null)
            {
                this.ThrowTerminatingError(new InvalidOperationException("SharePoint server farm not found."), ErrorCategory.ResourceUnavailable, this);
            }

            SPServer server = SPServer.Local;
            if (server == null)
            {
                this.ThrowTerminatingError(new InvalidOperationException("SharePoint local server not found."), ErrorCategory.ResourceUnavailable, this);
            }
        }

        #endregion
    }
}
