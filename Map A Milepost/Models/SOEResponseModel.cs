using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;
using Map_A_Milepost.Utils;

namespace Map_A_Milepost.Models
{

    public class SOEResponseModel : ObservableObject
    {

        private double? _angle;
        public double? Angle
        {
            get { return _angle; }
            set
            {
                _angle = value;
                OnPropertyChanged("Angle");
            }
        }

        private double? _arm;
        public double? Arm
        {
            get { return _arm; }
            set
            {
                _arm = value;
                OnPropertyChanged("ARM");
            }
        }

        private bool? _back;
        public bool? Back
        {
            get { return _back; }
            set
            {
                _back = value;
                OnPropertyChanged("Back");
            }
        }

        private bool? _decrease;

        public bool? Decrease
        {
            get { return _decrease; }
            set
            {
                _decrease = value;
                OnPropertyChanged("Decrease");
            }
        }

        private double? _distance;

        public double? Distance
        {
            get { return _distance; }
            set
            {
                _distance = value;
                OnPropertyChanged("Distance");
            }
        }

        private string? _route;

        public string? Route
        {
            get { return _route; }
            set
            {
                _route = value;
                OnPropertyChanged("Route");
            }
        }

        private double? _srmp;

        public double? Srmp
        {
            get { return _srmp; }
            set
            {
                _srmp = value;
                OnPropertyChanged("SRMP");
            }
        }
        private coordinatePair? _routeGeometry;
        public coordinatePair? RouteGeometry {
            get { return _routeGeometry; }
            set
            {
                _routeGeometry = value;
                OnPropertyChanged("RouteGeometry");
            }
        }
        public class coordinatePair
        {
            public double x { get; set; }
            public double y { get; set; }
        }
    }
}
