using Abp.Application.Services.Dto;
using Abp.Notifications;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Acme.SimpleTaskApp.Notifications.Dtos
{
	public class GetUserNotificationsInput : PagedInputDto
	{
		public UserNotificationState? State { get; set; }

		public DateTime? StartDate { get; set; }

		public DateTime? EndDate { get; set; }
	}
	public class PagedInputDto : IPagedResultRequest, ILimitedResultRequest
	{
		[Range(1, 1000)]
		public int MaxResultCount { get; set; }

		[Range(0, int.MaxValue)]
		public int SkipCount { get; set; }

		public PagedInputDto()
		{
			MaxResultCount = 10;
		}
	}


}
