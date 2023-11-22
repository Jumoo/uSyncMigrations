using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace uSync.Migrations.Migrators.Community.GibeLinkPicker.Models
{
	public class GibeLinkPickerData
	{
		public int Id { get; set; }
		public string Uid { get; set; }
		public string Name { get; set; }
		public string Url { get; set; }
		public string Target { get; set; }
	}
}
