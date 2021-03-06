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
	
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.SharePoint;
using Navertica.SharePoint.Extensions;
using Navertica.SharePoint.Interfaces;
using Navertica.SharePoint.Logging;

namespace Navertica.SharePoint.RepoService
{
    /// <summary>
    /// Class for loading and executing classes from DLLs
    /// </summary>
    public class DLLExecute : IExecuteScript
    {
        private SPSite site;
        private bool _initialized;

        public bool Initialized
        {
            get { return _initialized; }
        }

        // cannot load .dll into a DocLib, rename them to .dllplugin
        public string[] FileExtensions()
        {
            return new[] { "dllplugin" };
        }

        public void InitEngine(SPSite site, List<string> fullPathsToAllFilesAndFolders)
        {
            _initialized = true;
            this.site = site;
        }

        /// <summary>
        /// Load a dllplugin from a path in SiteScripts, execute with given context
        /// </summary>
        /// <param name="fullPath"></param>
        /// <param name="scriptEditedAndBody"></param>
        /// <param name="context">Contains (at least) SPSite site</param>
        /// <returns></returns>
        public ScriptContext Execute(string fullPath, FileAndDate scriptEditedAndBody, ScriptContext context)
        {
            ILogging log = NaverticaLog.GetInstance();

            Assembly dll = null;
            try
            {
                dll = Assembly.Load(scriptEditedAndBody.BinaryData);
            }
            catch (Exception exc)
            {
                log.LogError(fullPath, exc);
            }

            if (dll != null)
            {
                try
                {
                    object instance = dll.CreateInstance(dll.DefinedTypes.First().FullName);

                    if (instance != null)
                    {
                        MethodInfo getInstanceMethod = instance.GetType().GetMethod("GetInstance");
                        var pluginInstance = getInstanceMethod.Invoke(instance, null);

                        MethodInfo pluginScopeMethod = instance.GetType().GetMethod("PluginScope");
                        var pluginScopes = pluginScopeMethod.Invoke(instance, null);

                        if (pluginScopes != null)
                        {
                            foreach (PluginHost scope in ((IEnumerable) pluginScopes))
                            {
                                switch (scope.GetType().Name)
                                {
                                    case "ExecutePagePluginHost":
                                        ExecutePagePluginHost.Add(this.site, pluginInstance as IPlugin);
                                        break;
                                    case "ItemReceiverPluginHost":
                                        ItemReceiverPluginHost.Add(this.site, pluginInstance as IPlugin);
                                        break;
                                    case "ListReceiverPluginHost":
                                        ListReceiverPluginHost.Add(this.site, pluginInstance as IPlugin);
                                        break;
                                    case "TimerJobPluginHost":
                                        TimerJobPluginHost.Add(this.site, pluginInstance as IPlugin);
                                        break;
                                }
                            }
                        }
                    }
                }
                catch (ReflectionTypeLoadException rtle)
                {
                    log.LogError("DLLExecute.Execute", rtle, rtle.LoaderExceptions.Select(ee => ee.ToString()).JoinStrings("\n"));
                }
                catch (Exception e)
                {
                    log.LogError("DLLExecute.Execute", e);
                }
            }

            return context;
        }
    }
}