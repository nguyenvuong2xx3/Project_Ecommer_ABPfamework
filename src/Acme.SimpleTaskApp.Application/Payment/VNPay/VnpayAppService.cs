using Abp.Application.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;

namespace Acme.SimpleTaskApp.Payment.VNPay
{
	public interface IVnpayAppService : IApplicationService
	{
		VnpayPaymentResponse CreatePaymentUrl(VnpayPaymentRequest request);
		VnpayPaymentResponse ProcessCallback(IQueryCollection queryCollection);
	}

	public class VnpayAppService : ApplicationService, IVnpayAppService
	{
		private readonly VnpayConfig _config;
		private readonly IHttpContextAccessor _httpContextAccessor;

		public VnpayAppService(IOptions<VnpayConfig> config, IHttpContextAccessor httpContextAccessor)
		{
			_config = config.Value;
			_httpContextAccessor = httpContextAccessor;
		}

		public VnpayPaymentResponse CreatePaymentUrl(VnpayPaymentRequest request)
		{
			var context = _httpContextAccessor.HttpContext;
			var timeZoneById = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
			var timeNow = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, timeZoneById);
			var tick = DateTime.Now.Ticks.ToString();

			var vnpay = new VnPayLibrary();
			vnpay.AddRequestData("vnp_Version", "2.1.0");
			vnpay.AddRequestData("vnp_Command", "pay");
			vnpay.AddRequestData("vnp_TmnCode", _config.TmnCode);
			vnpay.AddRequestData("vnp_Amount", ((long)(request.Amount * 100)).ToString());
			vnpay.AddRequestData("vnp_CreateDate", timeNow.ToString("yyyyMMddHHmmss"));
			vnpay.AddRequestData("vnp_CurrCode", "VND");
			vnpay.AddRequestData("vnp_IpAddr", GetIpAddress(context));
			vnpay.AddRequestData("vnp_Locale", "vn");
			// Store orderId directly in OrderInfo for easy retrieval
			vnpay.AddRequestData("vnp_OrderInfo", $"DH{request.OrderId}");
			vnpay.AddRequestData("vnp_OrderType", "other");
			vnpay.AddRequestData("vnp_ReturnUrl", _config.CallbackUrl);
			vnpay.AddRequestData("vnp_TxnRef", tick);

			var paymentUrl = vnpay.CreateRequestUrl(_config.BaseUrl, _config.HashSecret);

			return new VnpayPaymentResponse
			{
				Success = true,
				PaymentUrl = paymentUrl
			};
		}

		public VnpayPaymentResponse ProcessCallback(IQueryCollection queryCollection)
		{
			var vnpay = new VnPayLibrary();

			foreach (var (key, value) in queryCollection)
			{
				if (!string.IsNullOrEmpty(key) && key.StartsWith("vnp_"))
				{
					vnpay.AddResponseData(key, value.ToString());
				}
			}

			var vnpayTxnRef = Convert.ToInt64(vnpay.GetResponseData("vnp_TxnRef"));
			var vnpaySecureHash = queryCollection["vnp_SecureHash"];
			var vnpayResponseCode = vnpay.GetResponseData("vnp_ResponseCode");
			var vnpayOrderInfo = vnpay.GetResponseData("vnp_OrderInfo");

			bool checkSignature = vnpay.ValidateSignature(vnpaySecureHash, _config.HashSecret);

			if (!checkSignature)
			{
				return new VnpayPaymentResponse
				{
					Success = false,
					Message = "Chữ ký không hợp lệ",
					OrderId = ExtractOrderId(vnpayOrderInfo)
				};
			}

			// Extract orderId from OrderInfo (format: "DH{orderId}")
			var orderId = ExtractOrderId(vnpayOrderInfo);

			return new VnpayPaymentResponse
			{
				Success = vnpayResponseCode == "00",
				Message = GetResponseMessage(vnpayResponseCode),
				OrderId = orderId,
				TransactionId = vnpayTxnRef.ToString(),
				ResponseCode = vnpayResponseCode
			};
		}

		private string ExtractOrderId(string orderInfo)
		{
			if (string.IsNullOrEmpty(orderInfo))
				return string.Empty;

			// OrderInfo format: "DH{orderId}"
			// Extract orderId
			var match = Regex.Match(orderInfo, @"DH(\d+)");
			if (match.Success && match.Groups.Count > 1)
			{
				return match.Groups[1].Value;
			}

			// Fallback: if format is different, return as is
			return orderInfo.Replace("DH", "").Trim();
		}

		private string GetResponseMessage(string responseCode)
		{
			switch (responseCode)
			{
				case "00":
					return "Thanh toán thành công";
				case "07":
					return "Trừ tiền thành công. Giao dịch bị nghi ngờ (liên quan tới lừa đảo, giao dịch bất thường)";
				case "09":
					return "Giao dịch không thành công do thẻ chưa đăng ký dịch vụ InternetBanking tại ngân hàng";
				case "10":
					return "Giao dịch không thành công do khách hàng xác thực thông tin thẻ/tài khoản không đúng quá 3 lần";
				case "11":
					return "Giao dịch không thành công do đã hết hạn chờ thanh toán";
				case "12":
					return "Giao dịch không thành công do thẻ/tài khoản bị khóa";
				case "13":
					return "Giao dịch không thành công do nhập sai mật khẩu xác thực giao dịch (OTP)";
				case "24":
					return "Giao dịch không thành công do khách hàng hủy giao dịch";
				case "51":
					return "Giao dịch không thành công do tài khoản không đủ số dư";
				case "65":
					return "Giao dịch không thành công do tài khoản đã vượt quá hạn mức giao dịch trong ngày";
				case "75":
					return "Ngân hàng thanh toán đang bảo trì";
				case "79":
					return "Giao dịch không thành công do nhập sai mật khẩu thanh toán quá số lần quy định";
				default:
					return "Giao dịch thất bại";
			}
		}

		private string GetIpAddress(HttpContext context)
		{
			var ipAddress = string.Empty;
			try
			{
				var remoteIpAddress = context.Connection.RemoteIpAddress;

				if (remoteIpAddress != null)
				{
					if (remoteIpAddress.AddressFamily == System.Net.Sockets.AddressFamily.InterNetworkV6)
					{
						remoteIpAddress = Dns.GetHostEntry(remoteIpAddress).AddressList
								.FirstOrDefault(x => x.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork);
					}

					if (remoteIpAddress != null) ipAddress = remoteIpAddress.ToString();

					return ipAddress;
				}
			}
			catch (Exception ex)
			{
				return "Invalid IP: " + ex.Message;
			}

			return "127.0.0.1";
		}
	}

	// Helper Library for VNPay
	public class VnPayLibrary
	{
		private readonly SortedList<string, string> _requestData = new SortedList<string, string>(new VnPayCompare());
		private readonly SortedList<string, string> _responseData = new SortedList<string, string>(new VnPayCompare());

		public void AddRequestData(string key, string value)
		{
			if (!string.IsNullOrEmpty(value))
			{
				_requestData.Add(key, value);
			}
		}

		public void AddResponseData(string key, string value)
		{
			if (!string.IsNullOrEmpty(value))
			{
				_responseData.Add(key, value);
			}
		}

		public string GetResponseData(string key)
		{
			return _responseData.TryGetValue(key, out var retValue) ? retValue : string.Empty;
		}

		public string CreateRequestUrl(string baseUrl, string vnpHashSecret)
		{
			var data = new StringBuilder();

			foreach (var (key, value) in _requestData.Where(kv => !string.IsNullOrEmpty(kv.Value)))
			{
				data.Append(WebUtility.UrlEncode(key) + "=" + WebUtility.UrlEncode(value) + "&");
			}

			var querystring = data.ToString();

			baseUrl += "?" + querystring;
			var signData = querystring;
			if (signData.Length > 0)
			{
				signData = signData.Remove(data.Length - 1, 1);
			}

			var vnpSecureHash = HmacSHA512(vnpHashSecret, signData);
			baseUrl += "vnp_SecureHash=" + vnpSecureHash;

			return baseUrl;
		}

		public bool ValidateSignature(string inputHash, string secretKey)
		{
			var rspRaw = GetResponseData();
			var myChecksum = HmacSHA512(secretKey, rspRaw);
			return myChecksum.Equals(inputHash, StringComparison.InvariantCultureIgnoreCase);
		}

		private string HmacSHA512(string key, string inputData)
		{
			var hash = new StringBuilder();
			var keyBytes = Encoding.UTF8.GetBytes(key);
			var inputBytes = Encoding.UTF8.GetBytes(inputData);
			using (var hmac = new HMACSHA512(keyBytes))
			{
				var hashValue = hmac.ComputeHash(inputBytes);
				foreach (var theByte in hashValue)
				{
					hash.Append(theByte.ToString("x2"));
				}
			}

			return hash.ToString();
		}

		private string GetResponseData()
		{
			var data = new StringBuilder();
			if (_responseData.ContainsKey("vnp_SecureHashType"))
			{
				_responseData.Remove("vnp_SecureHashType");
			}

			if (_responseData.ContainsKey("vnp_SecureHash"))
			{
				_responseData.Remove("vnp_SecureHash");
			}

			foreach (var (key, value) in _responseData.Where(kv => !string.IsNullOrEmpty(kv.Value)))
			{
				data.Append(WebUtility.UrlEncode(key) + "=" + WebUtility.UrlEncode(value) + "&");
			}

			//remove last '&'
			if (data.Length > 0)
			{
				data.Remove(data.Length - 1, 1);
			}

			return data.ToString();
		}
	}

	public class VnPayCompare : IComparer<string>
	{
		public int Compare(string x, string y)
		{
			if (x == y) return 0;
			if (x == null) return -1;
			if (y == null) return 1;
			var vnpCompare = CompareInfo.GetCompareInfo("en-US");
			return vnpCompare.Compare(x, y, CompareOptions.Ordinal);
		}
	}

	// Request/Response Models
	public class VnpayPaymentRequest
	{
		public string OrderId { get; set; }
		public decimal Amount { get; set; }
		public string OrderDescription { get; set; }
	}

	public class VnpayPaymentResponse
	{
		public bool Success { get; set; }
		public string Message { get; set; }
		public string OrderId { get; set; }
		public string TransactionId { get; set; }
		public string ResponseCode { get; set; }
		public string? PaymentUrl { get; set; }
	}
}
