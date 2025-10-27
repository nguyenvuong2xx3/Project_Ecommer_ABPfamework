using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Acme.SimpleTaskApp.Locations.Dtos
{
	public class TinhThanhDto
	{
		public string MatinhBNV { get; set; }
		public string MatinhTMS { get; set; }
		public string Tentinhmoi { get; set; }
		public List<PhuongXaDto> Phuongxa { get; set; } = new();
	}
	public class PhuongXaDto
	{
		public string Maphuongxa { get; set; }
		public string Tenphuongxa { get; set; }
	}
}
