using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace RickAndMortyPersonInfoWebAPI.Models
{
    public class Character
    {
        [BindNever]
        public int ID { get; set; }
        public string Name { get; set; }
        public string Status { get; set; }
        public string Species { get; set; } 
        public string Type { get; set; }
        public string Gender { get; set; }
        public Origin Origin { get; set; }
    }
}
