namespace Acme.SimpleTaskApp.Configuration
{
	public static class AppSettingNames
	{
		public const string UiTheme = "App.UiTheme";
	}
	public static class AppMailSettingNames
	{
		public const string Server = "App.Mail.Server";
		public const string SmtpPort = "App.Mail.SmtpPort";
		public const string SenderName = "App.Mail.SenderName";
		public const string EnableSsl = "App.Mail.EnableSsl";
		public const string SenderEmail = "App.Mail.SenderEmail";
		public const string UserName = "App.Mail.UserName";
		public const string Password = "App.Mail.Password";
	}
	public static class AppNameStore
	{
		public const string NameStore = "App.NameStore";
		public const string UrlLogo = "App.UrlLogo";
	}

	/// <summary>
	/// Setting names cho Background Workers
	/// </summary>
	public static class AppBackgroundWorkerSettings
	{
		/// <summary>
		/// Ngày chạy cuối cùng của DailyRevenueReportWorker (format: yyyy-MM-dd)
		/// </summary>
		public const string DailyRevenueReport_LastExecutionDate = "App.BackgroundWorker.DailyRevenueReport.LastExecutionDate";

		/// <summary>
		/// Ngày chạy cuối cùng của LowStockAlertWorker (format: yyyy-MM-dd)
		/// </summary>
		public const string LowStockAlert_LastExecutionDate = "App.BackgroundWorker.LowStockAlert.LastExecutionDate";
	}
}
