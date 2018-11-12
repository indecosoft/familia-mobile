using Org.Json;

namespace FamiliaXamarin.JsonModels
{
    class FirstSetupModel
    {
        public string Base64Image { get; set; }
        public string DateOfBirth { get; set; }
        public string Gender { get; set; }
        public int[] Disease { get; set; }
        public string ImageName { get; set; }
        public string ImageExtension { get; set; }
    }
}