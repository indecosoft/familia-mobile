using System.Collections.Generic;

namespace Familia.JsonModels {
	class FirstSetupModel {
		public string Base64Image { get; set; }
		public string DateOfBirth { get; set; }
		public string Gender { get; set; }
		public List<int> Disease = new List<int>();
		public string ImageName { get; set; }
		public string ImageExtension { get; set; }
	}
}