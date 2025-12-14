using System;
using System.Collections.Generic;

namespace Acme.SimpleTaskApp.Email.Dtos
{
	[Serializable]
	public class SendEmailToAdminsJobArgs
	{
		/// Danh sách email của admin
		public List<string> AdminEmails { get; set; } = new List<string>();

		/// Tiêu đề email
		public string Subject { get; set; }

		/// Nội dung email (HTML)
		public string Body { get; set; }
	}
}
