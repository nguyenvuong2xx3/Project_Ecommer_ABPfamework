using Abp;
using Abp.Dependency;
using Abp.Notifications;
using Abp.RealTime;
using Abp.Runtime.Session;
using Microsoft.AspNetCore.SignalR;
using System.Linq;
using System.Threading.Tasks;

namespace Acme.SimpleTaskApp.Web.Realtime
{
	/// <summary>
	/// SignalR Hub for sending real-time notifications to clients
	/// </summary>
	public class NotificationHub : Hub, ITransientDependency
	{
		public IAbpSession AbpSession { get; set; }
		private readonly IOnlineClientManager _onlineClientManager;

		public NotificationHub(IOnlineClientManager onlineClientManager)
		{
			_onlineClientManager = onlineClientManager;
			AbpSession = NullAbpSession.Instance;
		}

		/// <summary>
		/// Gửi khi client kết nối
		/// </summary>
		public override async Task OnConnectedAsync()
		{
			await base.OnConnectedAsync();

			// Register client as online
			if (AbpSession.UserId.HasValue)
			{
				// Log connection
				System.Diagnostics.Debug.WriteLine($"User {AbpSession.UserId} connected to NotificationHub");
			}
		}

		/// <summary>
		/// gửi khi client ngắt kết nối
		/// </summary>
		public override async Task OnDisconnectedAsync(System.Exception exception)
		{
			await base.OnDisconnectedAsync(exception);
			
			// Log disconnection
			System.Diagnostics.Debug.WriteLine($"User {AbpSession.UserId} disconnected from NotificationHub");
		}

		/// <summary>
		/// Client gọi hàm này để nhận thông báo test
		/// </summary>
		public async Task SendTestNotification(string message)
		{
			await Clients.Caller.SendAsync("ReceiveNotification", new
			{
				title = "Test Notification",
				message = message,
				type = "info"
			});
		}
	}
}
