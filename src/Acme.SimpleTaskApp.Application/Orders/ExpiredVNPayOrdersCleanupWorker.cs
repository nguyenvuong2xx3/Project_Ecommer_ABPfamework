using Abp.BackgroundJobs;
using Abp.Dependency;
using Abp.Threading.BackgroundWorkers;
using Abp.Threading.Timers;
using System;

namespace Acme.SimpleTaskApp.Orders
{
	/// <summary>
	/// Background Worker chạy định kỳ để kiểm tra và hủy các đơn hàng VNPay quá hạn thanh toán.
	/// Worker này chạy mỗi 5 phút để kiểm tra các đơn hàng VNPay đã tạo hơn 15 phút 
	/// mà chưa được thanh toán.
	/// </summary>
	public class ExpiredVNPayOrdersCleanupWorker : PeriodicBackgroundWorkerBase, ISingletonDependency
	{
		private readonly IBackgroundJobManager _backgroundJobManager;

		// Chạy mỗi 5 phút (5 * 60 * 1000 = 300000 ms)
		private const int CHECK_INTERVAL_MINUTES = 5;
		private const int CHECK_INTERVAL_MS = CHECK_INTERVAL_MINUTES * 60 * 1000;

		public ExpiredVNPayOrdersCleanupWorker(
			AbpTimer timer,
			IBackgroundJobManager backgroundJobManager)
			: base(timer)
		{
			_backgroundJobManager = backgroundJobManager;

			// Thiết lập thời gian chạy định kỳ
			Timer.Period = CHECK_INTERVAL_MS;
		}

		protected override void DoWork()
		{
			try
			{
				Logger.Info($"ExpiredVNPayOrdersCleanupWorker: Đang kiểm tra đơn hàng VNPay quá hạn... (Chạy mỗi {CHECK_INTERVAL_MINUTES} phút)");

				// Enqueue job để xử lý
				_backgroundJobManager.Enqueue<ExpiredVNPayOrdersCleanupJob, ExpiredVNPayOrdersCleanupJobArgs>(
					new ExpiredVNPayOrdersCleanupJobArgs()
				);
			}
			catch (Exception ex)
			{
				Logger.Error("ExpiredVNPayOrdersCleanupWorker: Lỗi khi enqueue job", ex);
			}
		}
	}
}
