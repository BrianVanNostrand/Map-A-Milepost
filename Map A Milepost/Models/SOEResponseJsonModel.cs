using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace Map_A_Milepost.Models
{
    public class SOEResponseJsonModel
    {
        public double Angle { get; set; }
        public double Arm { get; set; }
        public bool Back {  get; set; }
        public bool Decrease { get; set; }
        public double Distance { get; set; }

        public coordinatePair? EventPoint { get; set; }
        public string? Route { get; set; }
        public double Srmp { get; set; }
    }
    public class coordinatePair
    {
        public double x { get; set; }
        public double y { get; set; }
    }
    public class SOEResponseJsonModels
    {
        public List<SOEResponseJsonModel>? data { get; set; }
    }
}
