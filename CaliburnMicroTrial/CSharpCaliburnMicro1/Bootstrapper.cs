using Caliburn.Micro;
using CSharpCaliburnMicro1.ViewModels;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Windows;
using CSharpCaliburnMicro1.Views;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Hosting;
using Ninject;
using System.Reflection;
using System.Globalization;
using System.Linq;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using CSharpCaliburnMicro1.Behaviors;
using Ninject.Parameters;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Interop;
using Microsoft.Extensions.Configuration;
using Serilog;
using CSharpCaliburnMicro1.Configuration;
using IResult = Caliburn.Micro.IResult;
using CSharpCaliburnMicro1.Helpers;

namespace CSharpCaliburnMicro1
{
	
	public class Bootstrapper : BootstrapperBase
    {
        private IServiceCollection _services;
		private IConfiguration _configuration;

        public static IHost Host { get; private set; }

        public Bootstrapper()
        {
			_configuration = new ConfigurationBuilder()
				.AddJsonFile("appsettings.json")
				.AddJsonFile($"appsettings.Development.json", true)
				//.AddTemplateJsonFile()
				.Build();
			Serilog.Log.Logger = new LoggerConfiguration().ReadFrom.Configuration(_configuration).CreateLogger();

			var appSettings = _configuration.GetSection("AppSettings").Get<AppSettings>();
			// Reading app settings section to set app environment
			AppEnvironment.Instance.AuthHeaderName = appSettings.authHeaderName;
			AppEnvironment.Instance.AuthType = appSettings.authType;
			AppEnvironment.Instance.DataUrl = appSettings.dataUrl;
			AppEnvironment.Instance.MockDataJson = appSettings.mockJson;
			AppEnvironment.Instance.Xps2Pdf = appSettings.xps2pdf;

			Log = new LogViewModel();

			Host = Microsoft.Extensions.Hosting.Host.CreateDefaultBuilder()
            .ConfigureServices((context, services) =>
            {
                _services = services;
                ConfigureServices(services);
            })
            .Build();

            Initialize();

			FrameworkElement.LanguageProperty.OverrideMetadata(typeof(FrameworkElement),
				new FrameworkPropertyMetadata(XmlLanguage.GetLanguage("it-IT")));
			RenderOptions.ProcessRenderMode = RenderMode.SoftwareOnly;
		}

		#region Riportato da Boot.cs in Ors.Custom.Report.Viewer

		static StandardKernel kernel;

		static Bootstrapper()
		{
			LogManager.GetLog = (t) => new TraceLogger(t);
		}

		public static LogViewModel Log { get; set; }
		protected override void Configure()
        {
            kernel = new StandardKernel();
            BindExplicits();
			IEventAggregator aggregator = kernel.Get<IEventAggregator>();
			SetBindings(Assembly.GetExecutingAssembly());

			base.Configure();
        }

        private void BindExplicits()
        {
            kernel.Bind<IEventAggregator>().To<EventAggregator>().InSingletonScope();
            kernel.Bind<IWindowManager>().To<WindowManager>().InSingletonScope();
        }

		protected override void BuildUp(object instance)
		{
			kernel.Inject(instance);
		}

		public static void SetBindings(Assembly assembly)
		{
			ConventionallyBindViewModels(assembly);
			ConventionallyBindFactories(assembly);
			ConventionallyBindActions(assembly);
			ConventionallyBindAdapters(assembly);
			AutoSubscribeEventHandlers(assembly);
		}

		private static void ConventionallyBindAdapters(Assembly assembly)
		{
			foreach (var t in assembly.GetTypes())
			{
				var svc = t.GetInterfaces().Where(i => i.Name.EndsWith("Adapter")).FirstOrDefault();
				if (null != svc)
				{
					kernel.Bind(svc).To(t).Named(t.Name);
					Serilog.Log.Logger.Information(string.Format(CultureInfo.InvariantCulture, "Type {0} has been registered for {1}", svc, t));
				}
			}
		}

		static List<Type> subscribers = new List<Type>();
		private static void AutoSubscribeEventHandlers(Assembly assembly)
		{
			foreach (var t in assembly.GetTypes())
			{
				var svc = t.GetInterfaces().Where(i => i.Name.StartsWith("IHandle`1")).FirstOrDefault();
				if (null != svc)
				{
					subscribers.Add(t);
				}
			}
		}

		private static void ConventionallyBindViewModels(Assembly assembly)
		{
			foreach (var t in assembly.GetTypes())
			{
				if (t.Name.EndsWith("Model"))
				{
					var binding = kernel.Bind<object>().To(t);
					if (t.Name.StartsWith("Main"))
					{
						binding.InSingletonScope();
					}
					binding.Named(t.Name).OnActivation(k => BindSubscriptions(k));
					Serilog.Log.Logger.Information(string.Format(CultureInfo.InvariantCulture, "Type {0} has been registered for {1}", typeof(object), t));

					var toself = kernel.Bind(t).ToSelf();
					if (t.Name.StartsWith("Main"))
					{
						toself.InSingletonScope();
					}
					toself.OnActivation(k => BindSubscriptions(k));
					Serilog.Log.Logger.Information(string.Format(CultureInfo.InvariantCulture, "Type {0} has been bound to self", t));
				}
			}
		}

		private static void BindSubscriptions(object k)
		{
			if (subscribers.Contains(k.GetType()))
			{
				IoC.Get<IEventAggregator>().Subscribe(k);
			}
		}

		private static void ConventionallyBindFactories(Assembly assembly)
		{
			Type thisType = MethodBase.GetCurrentMethod().DeclaringType;
			foreach (var t in assembly.GetTypes())
			{
				if (t != thisType) // avoid mapping boot
				{
					var svc = t.GetInterfaces().Where(i => i.Name.EndsWith("Factory")).FirstOrDefault();
					if (null != svc)
					{
						kernel.Bind(svc).To(t).InSingletonScope();
						Serilog.Log.Logger.Information(string.Format(CultureInfo.InvariantCulture, "Type {0} has been registered as singleton for {1}", svc, t));
					}
				}
			}
		}

		private static void ConventionallyBindActions(Assembly assembly)
		{
			foreach (var t in assembly.GetTypes())
			{
				if (!t.IsAbstract)
				{
					var svc = t.GetInterfaces().Where(i => typeof(Caliburn.Micro.IResult).IsAssignableFrom(i)).FirstOrDefault();
					if (null != svc)
					{
						kernel.Bind(t).ToSelf();
						Serilog.Log.Logger.Information(string.Format(CultureInfo.InvariantCulture, "Type {0} has been bound to self", t));
					}
				}
			}
		}

        #endregion
       
		private void ConfigureServices(IServiceCollection serviceCollection)
        {
			serviceCollection.AddControllers();
			serviceCollection.AddEndpointsApiExplorer();
			serviceCollection.AddSwaggerGen();

			// Add functionality to inject IOptions<T>
			serviceCollection.AddOptions();
			// Add our Config object so it can be injected
			var reportsConfig = _configuration.GetSection("ReportsSection");
			serviceCollection.Configure<Configuration.ReportsSection>(reportsConfig);

			serviceCollection.AddTransient<BootView>();
			serviceCollection.AddTransient<BootViewModel>();
		}

        protected override async void OnStartup(object sender, StartupEventArgs e)
        {
			//await DisplayRootViewFor<BootViewModel>();		

			if (!AppEnvironment.Instance.Forked)
			{
                //TODO: expose asp.NET web service
                var builder = WebApplication.CreateBuilder();
                ConfigureServices(builder.Services);
                var app = builder.Build();
                // Configure the HTTP request pipeline.
                if (app.Environment.IsDevelopment())
                {
                    app.UseSwagger();
                    app.UseSwaggerUI();
                }
                app.UseHttpsRedirection();

                app.UseAuthorization();

                app.MapControllers();

                app.Run();
            }
			else
			{
				Coroutine.BeginExecute(Forked().GetEnumerator());
			}
		}

		private IEnumerable<IResult> Forked()
		{
			var engine = new Engine.ReportEngine(Serilog.Log.Logger);
			var reportsSection = _configuration.GetSection("ReportsSection").Get<ReportsSection>();         
			//forked instance uses parameters from file
			yield return new CoMethod(() => engine.MakeReport(null, null, null, null, null, null, null, reportsSection));
		}

		protected override object GetInstance(Type service, string key)
        {
			//return Host.Services.GetService(service);

			if (service == null)
				service = typeof(object);
			//pass the current context to the overflowable model
			var context = ReportRunner.GetThreadLocale() ?? new object();
			var svc = kernel.Get(service, key, ToParameters(context));

			return svc;
		}

        private IParameter[] ToParameters(object context)
        {
			List<Ninject.Parameters.IParameter> parms = new List<Ninject.Parameters.IParameter>();
			foreach (PropertyInfo pi in context.GetType().GetProperties())
			{
				parms.Add(new ConstructorArgument(pi.Name, pi.GetValue(context, null)));
			}
			return parms.ToArray();
		}

        protected override IEnumerable<object> GetAllInstances(Type service)
        {
			//return new List<object>() { Host.Services.GetService(service) };
			return new List<object>() { GetInstance(service, null) };
		}


        protected override void OnExit(object sender, EventArgs e)
        {
            base.OnExit(sender, e);
        }
    }
}
