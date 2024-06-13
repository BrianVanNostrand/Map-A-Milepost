using ArcGIS.Core.CIM;
using ArcGIS.Core.Data;
using ArcGIS.Core.Geometry;
using ArcGIS.Desktop.Catalog;
using ArcGIS.Desktop.Core;
using ArcGIS.Desktop.Editing;
using ArcGIS.Desktop.Extensions;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Framework.Dialogs;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Layouts;
using ArcGIS.Desktop.Mapping;
using Map_A_Milepost.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Map_A_Milepost
{
    internal class MapAMilepostMaptool : MapTool
    {
        public static MapPointViewModel currentPointViewModel { get; set; }
        public static MapLineViewModel currentLineViewModel { get; set; }
        private string _previousTool = null;//ID of the active tool in use before the creation session initializes
        public MapAMilepostMaptool()
        {
            IsSketchTool = true;
            SketchType = SketchGeometryType.Point;
            SketchOutputMode = SketchOutputMode.Map;
        }

        protected override Task OnToolActivateAsync(bool active)
        {
            return base.OnToolActivateAsync(active);
        }
        protected override Task OnToolDeactivateAsync(bool hasMapViewChanged)
        {
            return base.OnToolDeactivateAsync(hasMapViewChanged);
        }
        protected override Task<bool> OnSketchCompleteAsync(Geometry geometry)
        {
            return base.OnSketchCompleteAsync(geometry);
        }
        protected override async void OnToolMouseDown(MapViewMouseButtonEventArgs e)
        {
            if (e.ChangedButton == System.Windows.Input.MouseButton.Left)
            {
                MapPoint mapPoint =  await QueuedTask.Run(() =>
                {
                    return MapView.Active.ClientToMap(e.ClientPoint);
                });


                if (currentPointViewModel !=null)
                {
                    currentPointViewModel.Submit(mapPoint);
                }
            }
                //e.Handled = true; //Handle the event args to get the call to the corresponding async method
        }
        public static Task<CIMPointSymbol> CreatePointSymbolAsync(string fillColor)
        {
            return QueuedTask.Run(() =>
            {
                CIMPointSymbol circlePtSymbol = SymbolFactory.Instance.ConstructPointSymbol(ColorFactory.Instance.GreenRGB, 8, SimpleMarkerStyle.Circle);
                var marker = circlePtSymbol.SymbolLayers[0] as CIMVectorMarker;
                var polySymbol = marker.MarkerGraphics[0].Symbol as CIMPolygonSymbol;
                switch (fillColor)
                {
                    case "blue":
                        polySymbol.SymbolLayers[1] = SymbolFactory.Instance.ConstructSolidFill(ColorFactory.Instance.CreateRGBColor(62, 108, 214)); //This is the fill
                        break;
                    case "yellow":
                        polySymbol.SymbolLayers[1] = SymbolFactory.Instance.ConstructSolidFill(ColorFactory.Instance.CreateRGBColor(224, 227, 66)); //This is the fill
                        break;
                    case "green":
                        polySymbol.SymbolLayers[1] = SymbolFactory.Instance.ConstructSolidFill(ColorFactory.Instance.CreateRGBColor(2, 222, 28));
                        break;
                    case "red":
                        polySymbol.SymbolLayers[1] = SymbolFactory.Instance.ConstructSolidFill(ColorFactory.Instance.CreateRGBColor(227, 13, 9));
                        break;
                    case "purple":
                        polySymbol.SymbolLayers[1] = SymbolFactory.Instance.ConstructSolidFill(ColorFactory.Instance.CreateRGBColor(185, 12, 247));
                        break;
                }
                
                polySymbol.SymbolLayers[0] = SymbolFactory.Instance.ConstructStroke(ColorFactory.Instance.WhiteRGB, 1, SimpleLineStyle.Solid); //This is the outline
                return circlePtSymbol;
            });

        }

        #region Methods for selecting and delesecting the point creation map tool
        public void SetSession(MapPointViewModel mapPointViewModel = null, MapLineViewModel mapLineViewModel = null)
        {
            currentLineViewModel = mapLineViewModel;
            currentPointViewModel = mapPointViewModel;
            Map map = MapView.Active.Map;
            var graphicsLayer = map.FindLayer("CIMPATH=map/milepostmappinglayer.json") as GraphicsLayer;//look for layer
            if (graphicsLayer != null)//if layer exists
            {
                map.TargetGraphicsLayer = graphicsLayer;
            }
            else // else create layer
            {
                GraphicsLayerCreationParams gl_param = new GraphicsLayerCreationParams { Name = "MilepostMappingLayer"};
                QueuedTask.Run(() =>
                {
                    GraphicsLayer graphicsLayer = LayerFactory.Instance.CreateLayer<GraphicsLayer>(gl_param, map);
                
                });
            };
            selectMapTool();

        }
        public void EndSession()
        {
            OnToolDeactivateAsync(true);
            FrameworkApplication.SetCurrentToolAsync(_previousTool);
        }

        private void selectMapTool()
        {
            _previousTool = FrameworkApplication.ActiveTool.ID;
            FrameworkApplication.SetCurrentToolAsync("Map_A_Milepost_MapAMilepostMaptool");
        }
        #endregion
    }
}
