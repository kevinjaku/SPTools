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
	
namespace Navertica.SharePoint.RepoService.Service.Administration
{
    using System;
    using System.Globalization;
    using System.Web;
    using System.Web.UI;
    using System.Web.UI.WebControls;
    using Microsoft.SharePoint;
    using Microsoft.SharePoint.Administration;
    using Microsoft.SharePoint.Utilities;
    using Microsoft.SharePoint.WebControls;

    /// <summary>
    /// The Create Service Application page.
    /// </summary>
    public partial class CreateApplicationPage : BaseAdminPage
    {
        #region Fields

        #endregion

        #region Properties

        #endregion

        #region Page Events

        /// <summary>
        /// Page event.
        /// </summary>
        /// <param name="sender">The Sender.</param>
        /// <param name="e">The EventArgs.</param>
        protected void Page_Init(object sender, EventArgs e)
        {
            ((DialogMaster)this.Page.Master).OkButton.Click += new EventHandler(this.OkButton_Click);
        }

        /// <summary>
        /// Page_Load event.
        /// </summary>
        /// <param name="sender">The Sender.</param>
        /// <param name="e">The EventArgs.</param>
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!Page.IsPostBack)
            {
            }
        }

        /// <summary>
        /// Page_Error method.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The event args.</param>
        protected void Page_Error(object sender, EventArgs e)
        {
            Exception ex = Server.GetLastError();
            if (ex != null)
            {
            }
        }

        #endregion

        #region Control Events

        /// <summary>
        /// Click event.
        /// </summary>
        /// <param name="sender">The Sender.</param>
        /// <param name="e">The EventArgs.</param>
        protected void OkButton_Click(object sender, EventArgs e)
        {
            // Perform validation
            this.Validate();

            if (this.IsValid)
            {
                this.CreateApplication();
            }
            else
            {
                return;
            }
        }

        /// <summary>
        /// ServerValidate event.
        /// </summary>
        /// <param name="sender">The Sender.</param>
        /// <param name="e">The EventArgs.</param>
        protected void CustomValidatorServiceNameDuplicate_ServerValidate(object sender, ServerValidateEventArgs e)
        {
            if (e == null)
            {
                throw new ArgumentNullException("e");
            }

            string serviceName = e.Value;

            // Get the service
            RepoService service = RepoService.GetOrCreateService();

            // Try to get a duplicate service application
            RepoServiceApplication duplicate = SPFarm.Local.GetObject(serviceName, service.Id, typeof(RepoServiceApplication)) as RepoServiceApplication;

            if (duplicate != null)
            {
                e.IsValid = false;
            }
            else
            {
                e.IsValid = true;
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Creates the service application.
        /// </summary>
        private void CreateApplication()
        {
            using (SPLongOperation operation = new SPLongOperation(this))
            {
                operation.LeadingHTML = HttpContext.GetGlobalResourceObject("NVRRepoService.ServiceAdminResources", "CreateOperationLeadingHtml", CultureInfo.CurrentCulture).ToString();
                operation.TrailingHTML = HttpContext.GetGlobalResourceObject("NVRRepoService.ServiceAdminResources", "CreateOperationTrailingHtml", CultureInfo.CurrentCulture).ToString();
                operation.Begin();

                try
                {
                    RepoService service = RepoService.GetOrCreateService();
                    RepoServiceProxy serviceProxy = RepoServiceProxy.GetOrCreateServiceProxy();

                    // Create the application pool
                    IisWebServiceApplicationPoolSection applicationPoolSectionCasted = this.applicationPoolSection as IisWebServiceApplicationPoolSection;
                    SPIisWebServiceApplicationPool applicationPool = applicationPoolSectionCasted.GetOrCreateApplicationPool();

                    // Create the service application
                    RepoServiceApplication application = new RepoServiceApplication(
                        this.textBoxServiceName.Text.Trim(),
                        service,
                        applicationPool);
                    application.Update();
                    application.Provision();

                    // Create the service application proxy
                    RepoServiceApplicationProxy proxy = new RepoServiceApplicationProxy(
                        string.Format(
                            CultureInfo.CurrentCulture,
                            HttpContext.GetGlobalResourceObject("NVRRepoService.ServiceAdminResources", "ServiceApplicationProxyNameTemplate", CultureInfo.CurrentCulture).ToString(),
                            this.textBoxServiceName.Text.Trim()),
                        serviceProxy,
                        application.Uri);
                    proxy.Update();
                    proxy.Provision();

                    if (this.checkBoxIncludeInDefaultProxy.Checked)
                    {
                        SPServiceApplicationProxyGroup group = SPServiceApplicationProxyGroup.Default;
                        group.Add(proxy);
                        group.Update();
                    }

                    operation.EndScript("window.frameElement.commitPopup();");
                }
                catch (Exception ex)
                {
                    SPUtility.TransferToErrorPage(ex.ToString());
                }
            }
        }
        #endregion
    }
}