﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Acme.SimpleTaskApp.Categories.Dtos
{
	public class EditCategoryDto
	{
		public int Id { get; set; }

		public string Name { get; set; }

		public string Description { get; set; }
	}
}
