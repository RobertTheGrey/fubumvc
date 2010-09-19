﻿using System;
using System.Web;
using FubuMVC.Core.Runtime;
using FubuMVC.Core.View;
using Microsoft.Practices.ServiceLocation;
using FubuMVC.Core.Urls;
using FubuCore.Util;
using Spark;

namespace Spark.Web.FubuMVC.ViewCreation
{
    public abstract class SparkView : SparkViewBase, ISparkView, IFubuPage
    {
        private readonly Cache<Type, object> _services = new Cache<Type, object>();
        private string _siteRoot;

        public SparkView()
        {
            _services.OnMissing = type => { return ServiceLocator.GetInstance(type); };
        }

        public IResourcePathManager ResourcePathManager { get; set; }
        string IFubuPage.ElementPrefix { get; set; }
        public IServiceLocator ServiceLocator { get; set; }

        public string SiteRoot
        {
            get
            {
                var context = Get<HttpContextBase>();
                if (_siteRoot == null)
                {
                    var appPath = context.Request.ApplicationPath;
                    if (string.IsNullOrEmpty(appPath) || string.Equals(appPath, "/"))
                    {
                        _siteRoot = string.Empty;
                    }
                    else
                    {
                        _siteRoot = "/" + appPath.Trim('/');
                    }
                }
                return _siteRoot;
            }
        }
        public string SiteResource(string path)
        {
            return ResourcePathManager.GetResourcePath(SiteRoot, path);
        }
        public string H(object value)
        {
            return HttpUtility.HtmlEncode(value.ToString());
        }
        public object HTML(object value)
        {
            return value;
        }
        public T Get<T>()
        {
            return (T)_services[typeof(T)];
        }
        public T GetNew<T>()
        {
            return ServiceLocator.GetInstance<T>();
        }
        public IUrlRegistry Urls
        {
            get { return Get<IUrlRegistry>(); }
        }
    }

    public abstract class SparkView<TModel> : SparkView, IFubuPage<TModel> where TModel : class
    {
        public TModel Model { get; private set; }

        public void SetModel(IFubuRequest request)
        {
            Model = request.Get<TModel>();
        }
        public void SetModel(object model)
        {
            throw new NotImplementedException();
        }
    }
}