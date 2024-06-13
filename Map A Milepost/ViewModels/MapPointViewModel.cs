using Map_A_Milepost.Models;
using Map_A_Milepost.Utils;
using Map_A_Milepost.Views;
using Map_A_Milepost;
using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using ArcGIS.Core.Geometry;
using ArcGIS.Core.CIM;
using ArcGIS.Desktop.Mapping;
using ArcGIS.Desktop.Layouts;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Core.Internal.CIM;
using System.Windows.Media;

namespace Map_A_Milepost.ViewModels
{
    public class MapPointViewModel:ViewModelBase
    {
        //Map point view variables
        private SOEResponseModel _soeResponse;
        private SOEArgsModel _soeArgs;
        private ICommand _updateSOEResponse;
        private ICommand _updateSOEArgs;
        private ObservableCollection<SOEResponseModel> _soePointResponses;
        private ICommand _savePointResultCommand;
        private ICommand _initializeMapToolSession;
        private bool _isSaved = false;
        private bool _sessionActive = false;
        private MapAMilepostMaptool _mapTool = new();
        private string _mapButtonLabel = "Start Mapping"; 
        public MapPointViewModel()//constructor
        {
            _soeResponse = new SOEResponseModel();
            _soeArgs = new SOEArgsModel();
            _soePointResponses = new ObservableCollection<SOEResponseModel>();
        }

        public string MapButtonLabel
        {
            get { return _mapButtonLabel; } set
            {
                _mapButtonLabel = value;
                OnPropertyChanged("MapButtonLabel");
            }
        }
        public bool IsSaved
        {
            get { return _isSaved; }
            set
            {
                _isSaved = value;
                OnPropertyChanged("IsSaved");
            }
        }
        public SOEResponseModel SOEResponse
        {
            get{ return _soeResponse;}
            set{ _soeResponse = value; OnPropertyChanged("SOEResponse"); }
        }
        public SOEArgsModel SOEArgs
        {
            get { return _soeArgs;}
            set { _soeArgs = value; OnPropertyChanged("SOEArgs"); }
        }
        public ObservableCollection<SOEResponseModel> SoePointResponses
        {
            get { return _soePointResponses; }
            set { _soePointResponses = value; OnPropertyChanged("SOEPointResponses"); }
        }
        public List<SOEResponseModel> SelectedItems { get; set; } = new List<SOEResponseModel>();
        public ICommand SelectedItemsCommand
        {
            get
            {
                return new Commands.RelayCommand(list =>
                {
                    SelectedItems.Clear();
                    SelectedItems = Commands.DataGridCommands.UpdateSelection(SelectedItems, list);
                });
            }
        }
        public ICommand DeleteItemsCommand
        {
            get
            {
                return new Commands.RelayCommand(list =>
                {
                    if (SoePointResponses.Count > 0 && SelectedItems.Count > 0)
                    {
                        if (MessageBox.Show(
                            $"Are you sure you wish to delete these {SelectedItems.Count} records?",
                            "Delete Rows",
                            MessageBoxButton.YesNo,
                            MessageBoxImage.Question) == MessageBoxResult.Yes
                        ){
                            for (int i=SoePointResponses.Count-1; i>=0; i--)
                            {
                                if (SelectedItems.Contains(SoePointResponses[i]))
                                {
                                    SoePointResponses.Remove(SoePointResponses[i]);
                                }
                            }
                        }
                    }
                    
                });
            }
        }
        public ICommand ClearItemsCommand
        {
            get
            {
                return new Commands.RelayCommand(list =>
                {
                    if (SoePointResponses.Count > 0)
                    {
                        if (MessageBox.Show(
                            $"Are you sure you wish to clear all {SoePointResponses.Count} point records?", 
                            "Clear Results",
                            MessageBoxButton.YesNo,
                            MessageBoxImage.Question) == MessageBoxResult.Yes
                        ){
                            SoePointResponses.Clear();
                        }
                    }
                });
            }
        }
        public ICommand UpdateSOEResponse
        {
            get
            {
                if (_updateSOEResponse == null)
                    _updateSOEResponse = new Commands.RelayCommand(Submit,
                        null);
                return _updateSOEResponse;
            }
            set
            {
                _updateSOEResponse = value;
            }
        }
        public ICommand SavePointResultCommand
        {
            get
            {
                if (_savePointResultCommand == null)
                    _savePointResultCommand = new Commands.RelayCommand(SavePointResult,
                        null);
                return _savePointResultCommand;
            }
            set
            {
                _savePointResultCommand = value;
            }
        }
        public ICommand InitializeMapToolSession
        {
            get
            {
                if (_initializeMapToolSession == null)
                    _initializeMapToolSession = new Commands.RelayCommand(InitializeSession, null);
                return _initializeMapToolSession;
            }
            set
            {
                _initializeMapToolSession = value;
            }
        }

        public async void Submit(object mapPoint)
        {
            GraphicsLayer graphicsLayer = MapView.Active.Map.FindLayer("CIMPATH=map/milepostmappinglayer.json") as GraphicsLayer;//look for layer
            var graphics = graphicsLayer.GetElementsAsFlattenedList().Where(elem => elem.GetGraphic() is CIMPointGraphic);
            if ((MapPoint)mapPoint != null)
            {
                SOEArgs.X = ((MapPoint)mapPoint).X;
                SOEArgs.Y = ((MapPoint)mapPoint).Y;
                SOEArgs.SR = ((MapPoint)mapPoint).SpatialReference.Wkid;
            }
            Dictionary<string,object> response =  await Utils.HTTPRequest.QuerySOE(SOEArgs);
            if((string)response["message"]=="success")
            {
                IsSaved = false;
                CopyProps.CopyProperties(response["soeResponse"], SOEResponse);
                if (SoePointResponses.Contains(SOEResponse))
                {
                    IsSaved = true;
                }
                else
                {
                    IsSaved = false;
                }
                await QueuedTask.Run(async() =>
                {

                    # region remove previous element if it isn't saved
                    if (graphics != null)
                    {
                        foreach (GraphicElement item in graphicsLayer.GetElementsAsFlattenedList())
                        {
                            //if this graphic item was generated in a point mapping session and is unsaved (if it is a click point or unsaved route point)
                            if (item.GetCustomProperty("sessionType") == "point" && item.GetCustomProperty("saved") == "false")
                            {
                                graphicsLayer.RemoveElement(item);
                            }
                        }
                    }
                    #endregion

                    #region create and add point graphics
                    var clickedPtGraphic = new CIMPointGraphic();
                    clickedPtGraphic.Attributes = new Dictionary<string, object>();
                    var clickedPtSymbol = await MapAMilepostMaptool.CreatePointSymbolAsync("yellow");
                    clickedPtGraphic.Symbol = clickedPtSymbol.MakeSymbolReference();
                    clickedPtGraphic.Location = MapPointBuilderEx.CreateMapPoint(SOEArgs.X, SOEArgs.Y, SOEArgs.SR);
                    //create custom click point props
                    var clickPtElemInfo = new ElementInfo()
                    {
                        CustomProperties = new List<CIMStringMap>()
                        {
                            new CIMStringMap(){ Key="saved", Value="false"},
                            new CIMStringMap(){ Key="sessionType", Value="point"},
                            new CIMStringMap(){ Key="eventType", Value="click"}
                        }
                    };
                    graphicsLayer.AddElement(cimGraphic: clickedPtGraphic, elementInfo: clickPtElemInfo);
                    
                    var soePtGraphic = new CIMPointGraphic();
                    soePtGraphic.Attributes = new Dictionary<string, object>();
                    var soePtSymbol = await MapAMilepostMaptool.CreatePointSymbolAsync("purple");
                    soePtGraphic.Symbol = soePtSymbol.MakeSymbolReference();
                    soePtGraphic.Location = MapPointBuilderEx.CreateMapPoint(SOEResponse.RouteGeometry.x, SOEResponse.RouteGeometry.y, SOEArgs.SR);
                    //create custom route point props
                    var routePtElemInfo = new ElementInfo()
                    {
                        CustomProperties = new List<CIMStringMap>()
                        {
                            new CIMStringMap(){ Key="saved", Value="false"},
                            new CIMStringMap(){ Key="sessionType", Value="point"},
                            new CIMStringMap(){ Key="eventType", Value="route"}
                        }
                    };
                    graphicsLayer.AddElement(cimGraphic: soePtGraphic, elementInfo: routePtElemInfo);
                    graphicsLayer.ClearSelection();
                    #endregion
                });
                
            }
        }
        public void SavePointResult(object state)
        {
            //if a point has been mapped
            if (Utils.CheckObject.HasBeenUpdated(SOEResponse))
            {
                if (SoePointResponses.Contains(SOEResponse))
                {
                    if(MessageBox.Show(
                        $"The selected point already exists in the results tab. Save a duplicate?",
                        "Clear Results",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Question) == MessageBoxResult.Yes
                    ){
                        SoePointResponses.Add(SOEResponse);
                        UpdateSaveGraphicInfos();
                    }
                }
                else
                {
                    SoePointResponses.Add(new SOEResponseModel() { 
                        Angle = SOEResponse.Angle,
                        Arm = SOEResponse.Arm,
                        Back = SOEResponse.Back,
                        Decrease = SOEResponse.Decrease,
                        Distance = SOEResponse.Distance,
                        Route = SOEResponse.Route,
                        RouteGeometry = SOEResponse.RouteGeometry,
                        Srmp = SOEResponse.Srmp,
                    });
                    UpdateSaveGraphicInfos();
                    CreateLabel(SOEResponse);
                    IsSaved = true;
                }
            }
            else
            {
                MessageBox.Show("Create a point to save it to the results tab.", "Save error", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
        private async void UpdateSaveGraphicInfos()
        {
            GraphicsLayer graphicsLayer = MapView.Active.Map.FindLayer("CIMPATH=map/milepostmappinglayer.json") as GraphicsLayer;//look for layer
            CIMPointSymbol greenPointSymbol = await MapAMilepostMaptool.CreatePointSymbolAsync("green");
            //var graphicsLayer = MapView.Active.Map.FindLayer("CIMPATH=map/milepostmappinglayer.json") as GraphicsLayer;//look for layer
            IEnumerable<GraphicElement> graphicItems = graphicsLayer.GetElementsAsFlattenedList();
            await QueuedTask.Run(() =>
            {
                foreach (GraphicElement item in graphicItems)
                {
                    var cimPointGraphic = item.GetGraphic() as CIMPointGraphic;
                    //set graphic saved property to true
                    item.SetCustomProperty("saved", "true");
                    //if point was generated in a point mapping session and it is a click point, remove it.
                    if (item.GetCustomProperty("sessionType") == "point" && item.GetCustomProperty("eventType") == "click")
                    {
                        graphicsLayer.RemoveElement(item);
                    }
                    //if point was generated in a point mapping session and it is a route point, turn it green because it is now saved.
                    if(item.GetCustomProperty("eventType")== "route" && item.GetCustomProperty("sessionType") == "point")
                    {
                        cimPointGraphic.Symbol = greenPointSymbol.MakeSymbolReference();
                        item.SetGraphic(cimPointGraphic);
                    }
                }
            });
        }
        private async void CreateLabel(SOEResponseModel soeResponse)
        {
            var textSymbol = new CIMTextSymbol();
            //define the text graphic
            var textGraphic = new CIMTextGraphic();
            await QueuedTask.Run(() =>
            {
                var labelGeometry = (MapPointBuilderEx.CreateMapPoint(SOEResponse.RouteGeometry.x, SOEResponse.RouteGeometry.y, SOEArgs.SR));
                //Create a simple text symbol
                textSymbol = SymbolFactory.Instance.ConstructTextSymbol(ColorFactory.Instance.BlackRGB, 10, "Arial", "Bold");
                //Sets the geometry of the text graphic
                textGraphic.Shape = labelGeometry;
                //Sets the text string to use in the text graphic
                textGraphic.Text = $"    {soeResponse.Srmp}";
                //Sets symbol to use to draw the text graphic
                textGraphic.Symbol = textSymbol.MakeSymbolReference();
                //Draw the overlay text graphic
                MapView.Active.AddOverlay(textGraphic);
            });
        }
        public void InitializeSession(object state)
        {
            if(!_sessionActive)
            {
                _sessionActive = true;
                MapButtonLabel = "Stop Mapping";
                _mapTool.SetSession(mapPointViewModel: this);
            }
            else
            {
                _sessionActive = false;
                MapButtonLabel = "Start Mapping";
                _mapTool.EndSession();
            }
        }
    }
}
