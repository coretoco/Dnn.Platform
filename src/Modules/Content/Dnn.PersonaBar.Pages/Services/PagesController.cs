﻿#region Copyright

// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2016
// by DotNetNuke Corporation
// All Rights Reserved

#endregion

using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Dnn.PersonaBar.Library;
using Dnn.PersonaBar.Library.Attributes;
using Dnn.PersonaBar.Pages.Components;
using Dnn.PersonaBar.Pages.Components.Exceptions;
using Dnn.PersonaBar.Pages.Services.Dto;
using Dnn.PersonaBar.Themes.Components;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Tabs;
using DotNetNuke.Services.OutputCache;
using DotNetNuke.Web.Api;

namespace Dnn.PersonaBar.Pages.Services
{
    [ServiceScope(Identifier = "Pages")]
    [DnnExceptionFilter]
    public class PagesController : PersonaBarApiController
    {
        private readonly IPagesController _pagesController;
        private readonly IThemesController _themesController;

        public PagesController()
        {
            _pagesController = Components.PagesController.Instance;
            _themesController = ThemesController.Instance;
        }

        /// GET: api/Pages/GetPageDetails
        /// <summary>
        /// Get detail of a page
        /// </summary>
        /// <param name="pageId"></param>
        /// <returns></returns>
        [HttpGet]
        public HttpResponseMessage GetPageDetails(int pageId)
        {
            try
            {
                var page = Converters.ConvertToPageSettings(_pagesController.GetPageDetails(pageId));
                page.Modules = _pagesController.GetModules(page.TabId).Select(Converters.ConvertToModuleItem);
                page.PageUrls = _pagesController.GetPageUrls(page.TabId);
                page.Permissions = _pagesController.GetPermissionsData(pageId);
                return Request.CreateResponse(HttpStatusCode.OK, page);
            }
            catch (PageNotFoundException)
            {
                return Request.CreateResponse(HttpStatusCode.NotFound, new {Message = "Page doesn't exists."});
            }
        }

        /// GET: api/Pages/GetPageList
        /// <summary>
        /// 
        /// </summary>
        /// <param name="parentId"></param>
        /// <param name="searchKey"></param>
        /// <returns></returns>
        [HttpGet]
        public HttpResponseMessage GetPageList(int parentId = -1, string searchKey = "")
        {
            var adminTabId = PortalSettings.AdminTabId;
            var tabs = TabController.GetPortalTabs(PortalSettings.PortalId, adminTabId, false, true, false, true);
            var pages = from p in _pagesController.GetPageList(parentId, searchKey)
                select Converters.ConvertToPageItem<PageItem>(p, tabs);
            return Request.CreateResponse(HttpStatusCode.OK, pages);
        }

        [HttpGet]
        public HttpResponseMessage GetPageHierarchy(int pageId)
        {
            try
            {
                var paths = _pagesController.GetPageHierarchy(pageId);
                return Request.CreateResponse(HttpStatusCode.OK, paths);
            }
            catch (PageNotFoundException)
            {
                return Request.CreateResponse(HttpStatusCode.NotFound);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public HttpResponseMessage MovePage(PageMoveRequest request)
        {
            try
            {
                var tab = _pagesController.MovePage(request);
                var tabs = TabController.GetPortalTabs(PortalSettings.PortalId, Null.NullInteger, false, true, false,
                    true);
                var pageItem = Converters.ConvertToPageItem<PageItem>(tab, tabs);
                return Request.CreateResponse(HttpStatusCode.OK, new {Status = 0, Page = pageItem});
            }
            catch (PageNotFoundException)
            {
                return Request.CreateResponse(HttpStatusCode.NotFound);
            }
            catch (PageException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new {Status = 1, ex.Message});
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public HttpResponseMessage DeletePage(PageItem page)
        {
            try
            {
                _pagesController.DeletePage(page);
                return Request.CreateResponse(HttpStatusCode.OK, new {Status = 0});
            }
            catch (PageNotFoundException)
            {

                return Request.CreateResponse(HttpStatusCode.NotFound);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public HttpResponseMessage DeletePageModule(PageModuleItem module)
        {
            try
            {
                _pagesController.DeleteTabModule(module.PageId, module.ModuleId);
                return Request.CreateResponse(HttpStatusCode.OK, new {Status = 0});
            }
            catch (PageModuleNotFoundException)
            {
                return Request.CreateResponse(HttpStatusCode.NotFound);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public HttpResponseMessage CopyThemeToDescendantPages(CopyThemeRequest copyTheme)
        {
            _pagesController.CopyThemeToDescendantPages(copyTheme.PageId, copyTheme.Theme);
            return Request.CreateResponse(HttpStatusCode.OK, new {Status = 0});
        }

        // TODO: This should be a POST
        [HttpGet]
        public HttpResponseMessage EditModeForPage(int id)
        {
            _pagesController.EditModeForPage(id, UserInfo.UserID);
            return Request.CreateResponse(HttpStatusCode.OK);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public HttpResponseMessage SavePageDetails(PageSettings pageSettings)
        {
            try
            {
                var tab = _pagesController.SavePageDetails(pageSettings);
                var tabs = TabController.GetPortalTabs(PortalSettings.PortalId, Null.NullInteger, false, true, false,
                    true);

                return Request.CreateResponse(HttpStatusCode.OK, new
                {
                    Status = 0,
                    Page = Converters.ConvertToPageItem<PageItem>(tab, tabs)
                });
            }
            catch (PageNotFoundException)
            {
                return Request.CreateResponse(HttpStatusCode.NotFound, new {Message = "Page doesn't exists."});
            }
            catch (PageValidationException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new {Status = 1, ex.Field, ex.Message});
            }
        }

        [HttpGet]
        public HttpResponseMessage GetDefaultPermissions()
        {
            var permissions = _pagesController.GetPermissionsData(0);
            return Request.CreateResponse(HttpStatusCode.OK, permissions);
        }

        [HttpGet]
        public HttpResponseMessage GetCacheProviderList()
        {
            var providers = from p in OutputCachingProvider.GetProviderList() select p.Key;
            return Request.CreateResponse(HttpStatusCode.OK, providers);
        }

        [HttpGet]
        public HttpResponseMessage GetPageUrlPreview(string url)
        {
            if (string.IsNullOrEmpty(url))
            {
                return Request.CreateResponse(HttpStatusCode.OK, new {Url = string.Empty});
            }

            var cleanedUrl = _pagesController.CleanTabUrl(url);
            return Request.CreateResponse(HttpStatusCode.OK, new {Url = cleanedUrl});
        }

        [HttpGet]
        public HttpResponseMessage GetThemes()
        {
            var themes = _themesController.GetLayouts(PortalSettings, ThemeLevel.Global | ThemeLevel.Site);
            return Request.CreateResponse(HttpStatusCode.OK, new { themes });
        }

        [HttpGet]
        public HttpResponseMessage GetThemeFiles(string themeName)
        {
            const ThemeLevel level = ThemeLevel.Global | ThemeLevel.Site;
            var themeLayout = _themesController.GetLayouts(PortalSettings, level).FirstOrDefault(t => t.PackageName.Equals(themeName, StringComparison.InvariantCultureIgnoreCase));
            var themeContainer = _themesController.GetContainers(PortalSettings, level).FirstOrDefault(t => t.PackageName.Equals(themeName, StringComparison.InvariantCultureIgnoreCase));

            if (themeLayout == null || themeContainer == null)
            {
                return Request.CreateErrorResponse(HttpStatusCode.NotFound, "ThemeNotFound");
            }

            return Request.CreateResponse(HttpStatusCode.OK, new {
                layouts = _themesController.GetThemeFiles(PortalSettings, themeLayout),
                containers = _themesController.GetThemeFiles(PortalSettings, themeContainer)
            });
        }
    }
}