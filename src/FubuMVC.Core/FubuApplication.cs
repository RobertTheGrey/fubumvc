using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Routing;
using FubuCore;
using FubuMVC.Core.Bootstrapping;
using FubuMVC.Core.Packaging;
using FubuMVC.Core.Registration;
using FubuMVC.Core.Routing;

namespace FubuMVC.Core
{
    // PLEASE NOTE:  This code is primarily tested with the StoryTeller suite for Packaging
    public class FubuApplication : IContainerFacilityExpression
    {
    	private readonly Func<FubuRegistry> _registryBuilder;
		private FubuRegistry _registryCache;
        private IContainerFacility _facility;
        private Func<IContainerFacility> _facilitySource;
        private BehaviorGraph _graph;
        private FubuMvcPackageFacility _fubuFacility;
		private readonly List<Action<FubuRegistry>> _registryModifications = new List<Action<FubuRegistry>>();
		private readonly List<Action<IPackageFacility>> _packagingDirectives = new List<Action<IPackageFacility>>();

        private FubuApplication(Func<FubuRegistry> registry)
        {
            _registryBuilder = registry;
        }


		private FubuRegistry registry()
		{
			return _registryCache ?? (_registryCache = _registryBuilder());
		}

        FubuApplication IContainerFacilityExpression.ContainerFacility(IContainerFacility facility)
        {
            return registerContainerFacilitySource(() => facility);
        }
        FubuApplication IContainerFacilityExpression.ContainerFacility(Func<IContainerFacility> facilitySource)
        {
            return registerContainerFacilitySource(facilitySource);
        }

        // TODO -- replace w/ Lazy<T> when we ditch 3.5 support
        private IContainerFacility facility
        {
            get
            {
                if (_facility == null)
                {
                    _facility = _facilitySource();
                }

                return _facility;
            }
        }
		
        private FubuApplication registerContainerFacilitySource(Func<IContainerFacility> facilitySource)
        {
            _facilitySource = facilitySource;
            return this;
        }

        public static IContainerFacilityExpression For(Func<FubuRegistry> registry)
        {
            return new FubuApplication(registry);
        }
        public static IContainerFacilityExpression For<T>() where T : FubuRegistry, new()
        {
            return For(() => new T());
        }

        [SkipOverForProvenance]
        public void Bootstrap(IList<RouteBase> routes)
        {
            Bootstrap().Each(routes.Add);
        }

        [SkipOverForProvenance]
        public IList<RouteBase> Bootstrap()
        {
            if (HttpContext.Current != null)
            {
                UrlContext.Live();
            }

            _fubuFacility = new FubuMvcPackageFacility();

            // TODO -- would be nice if this little monster also logged 
            PackageRegistry.LoadPackages(x =>
            {
                x.Facility(_fubuFacility);
                _packagingDirectives.Each(d => d(x));
                x.Bootstrap(log => startApplication());
            });

            return buildRoutes();
        }

        private IEnumerable<IActivator> startApplication()
        {
			// Building up the facility first forces the creation of the container
			// and executes any additional bootstrapping done in the respective lambdas
            facility.SpinUp();
            

			registry()
				.Services(_fubuFacility.RegisterServices);

        	_registryModifications.Each(m => m(registry()));

            FindAllExtensions().Each(x => x.Configure(registry()));

            // "Bake" the fubu configuration model into your
            // IoC container for the application
            _graph = registry().BuildGraph();
            _graph.EachService(facility.Register);
			facility.BuildFactory();

            return facility.GetAllActivators();
        }

        private IList<RouteBase> buildRoutes()
        {
            var routes = new List<RouteBase>();           
            
            // Build route objects from route definitions on graph + add packaging routes
            var factory = facility.BuildFactory();
            facility.Get<IRoutePolicy>().BuildRoutes(_graph, factory).Each(routes.Add);                      
            _fubuFacility.AddPackagingContentRoutes(routes);

            return routes;
        }

        public FubuApplication Packages(Action<IPackageFacility> configure)
        {
            _packagingDirectives.Add(configure);
            return this;
        }

        public FubuApplication ModifyRegistry(Action<FubuRegistry> modifications)
        {
        	_registryModifications.Add(modifications);
            return this;
        }

        public static IEnumerable<IFubuRegistryExtension> FindAllExtensions()
        {
            if (!PackageRegistry.PackageAssemblies.Any()) return new IFubuRegistryExtension[0];

            var pool = new TypePool(null)
            {
                ShouldScanAssemblies = true
            };
            pool.AddAssemblies(PackageRegistry.PackageAssemblies);

            // Yeah, it really does have to be this way
            return pool.TypesMatching(
                t =>
                hasDefaultCtor(t) && t.GetInterfaces().Any(i => i.FullName == typeof(IFubuRegistryExtension).FullName))
                .Select(buildExtension);
        }
        private static bool hasDefaultCtor(Type type)
        {
            return type.GetConstructor(new Type[0]) != null;
        }
        private static IFubuRegistryExtension buildExtension(Type type)
        {
            var contextType = Type.GetType(type.AssemblyQualifiedName);
            return (IFubuRegistryExtension)Activator.CreateInstance(contextType);
        }

        public IContainerFacility Facility { get { return _facility; } }
    }

    public interface IContainerFacilityExpression
    {
        FubuApplication ContainerFacility(IContainerFacility facility);
        FubuApplication ContainerFacility(Func<IContainerFacility> facilitySource);
    }
}
