using Acme.SimpleTaskApp.Notifications.Dtos;

namespace Acme.SimpleTaskApp.Notifications.Dtos
{
public class NotificationSubscriptionWithDisplayNameDto : NotificationSubscriptionDto
{
	public string DisplayName { get; set; }

	public string Description { get; set; }
}
}