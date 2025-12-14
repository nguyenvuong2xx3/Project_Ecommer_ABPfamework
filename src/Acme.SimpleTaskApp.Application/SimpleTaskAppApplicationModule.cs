using Abp.AutoMapper;
using Abp.Configuration;
using Abp.MailKit;
using Abp.Modules;
using Abp.Reflection.Extensions;
using Abp.Threading.BackgroundWorkers;
using Acme.SimpleTaskApp.Authorization;
using Acme.SimpleTaskApp.BackgroundWorkers;
using Acme.SimpleTaskApp.Configuration;

namespace Acme.SimpleTaskApp
{
	[DependsOn(
			typeof(SimpleTaskAppCoreModule),
			typeof(AbpAutoMapperModule), typeof(AbpMailKitModule))
	]
	public class SimpleTaskAppApplicationModule : AbpModule
	{
		public override void PreInitialize()
		{
			Configuration.Authorization.Providers.Add<SimpleTaskAppAuthorizationProvider>();

			Configuration.Settings.Providers.Add<AppSettingProvider>();
		}

		public override void Initialize()
		{
			var thisAssembly = typeof(SimpleTaskAppApplicationModule).GetAssembly();

			IocManager.RegisterAssemblyByConvention(thisAssembly);

			Configuration.Modules.AbpAutoMapper().Configurators.Add(
					cfg => cfg.AddMaps(thisAssembly)
			);
		}

		public override void PostInitialize()
		{
			// Đăng kí job workers
			var workManager = IocManager.Resolve<IBackgroundWorkerManager>();

			// Gửi báo cáo doanh thu hàng ngày
			workManager.Add(IocManager.Resolve<DailyRevenueReportWorker>());

			// Thêm cảnh báo tồn kho thấp
			workManager.Add(IocManager.Resolve<LowStockAlertWorker>());
		}
	}
}
