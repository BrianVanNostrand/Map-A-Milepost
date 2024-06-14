using Map_A_Milepost.Models;
using Map_A_Milepost.Utils;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using ArcGIS.Core.Geometry;
using ArcGIS.Core.CIM;
using ArcGIS.Desktop.Mapping;
using ArcGIS.Desktop.Layouts;
using ArcGIS.Desktop.Framework.Threading.Tasks;

namespace Map_A_Milepost.ViewModels
{
    public class MapPointViewModel:ViewModelBase
    {
        /// <summary>
        /// Private variables with associated public variables, granting access to the INotifyPropertyChanged command via ViewModelBase.
        /// </summary>
        private SOEResponseModel _soeResponse;
        private SOEArgsModel _soeArgs;
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

        /// <summary>
        /// -   The label of the MapPointExecuteButton element in MapPointView.xaml. Used as the content of that element via data binding.
        /// </summary>
        public string MapButtonLabel
        {
            get { return _mapButtonLabel; } set
            {
                _mapButtonLabel = value;
                OnPropertyChanged("MapButtonLabel");
            }
        }

        /// <summary>
        /// -   Whether or not the currently maped route point feature already exists in the array of saved SOE responses (SOEPointResponses)
        /// </summary>
        public bool IsSaved
        {
            get { return _isSaved; }
            set
            {
                _isSaved = value;
                OnPropertyChanged("IsSaved");
            }
        }

        /// <summary>
        /// -   The SOE response of the currently mapped route point feature.
        /// </summary>
        public SOEResponseModel SOEResponse
        {
            get{ return _soeResponse;}
            set{ _soeResponse = value; OnPropertyChanged("SOEResponse"); }
        }

        /// <summary>
        /// -   Arguments passed to the SOE HTTP query.
        /// </summary>
        public SOEArgsModel SOEArgs
        {
            get { return _soeArgs;}
            set { _soeArgs = value; OnPropertyChanged("SOEArgs"); }
        }

        /// <summary>
        /// -   Array of saved SOEResponse data objects.
        /// </summary>
        public ObservableCollection<SOEResponseModel> SoePointResponses
        {
            get { return _soePointResponses; }
            set { _soePointResponses = value; OnPropertyChanged("SOEPointResponses"); }
        }

        /// <summary>
        /// -   Array of selected saved SOE response data objects in the DataGrid in ResultsView.xaml. Updated when a row is clicked in he DataGrid
        ///     via data binding.
        /// </summary>
        public List<SOEResponseModel> SelectedItems { get; set; } = new List<SOEResponseModel>();

        /// <summary>
        /// -   Update the selected items array based on the rows selected in the DataGrid in ResultsView.xaml via data binding.
        /// </summary>
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

        /// <summary>
        /// -   Delete the selected saved SOEPointResponses array. Accessed by the DeleteItemsButton in ResultsView.xaml via data binding.
        /// </summary>
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

        /// <summary>
        /// -   Clear the saved SOEPointResponses array. Accessed by the ClearItemsButton in ResultsView.xaml via data binding.
        /// </summary>
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

        /// <summary>
        /// -   Icommand granting UI access to the SavePointResult method via data binding to the MapPointSaveButton element in the MapPointView.
        /// </summary>
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

        /// <summary>
        /// -   Icommand granting UI access to the InitializeSession method via data binding to the MapPointExecuteButton element in the MapPointView.
        /// </summary>
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

        /// <summary>
        /// -   Create a reference to the graphics in the graphics layer and update the SOEArgs that will be passed
        ///     to the HTTP query based on the clicked point on the map.
        /// -   Query the SOE and if the query executes successfully, update the IsSaved public property to determine whether
        ///     or not a dialog box will be displayed, confirming that the user wants to save a duplicate point.
        /// -   Delete any previous unsaved graphics from the graphics layer that were created in a point editing session
        ///     (any existing click points or unsaved route points)
        /// -   Generate new click point and route points to reflect the new clicked map point, using custom properties to set the 
        ///     session type, saved status, and event type. These values are used to determine behavior in the UpdateSaveGraphicInfos method.
        /// -   Add the new route and click point to the map, then clear the selection on the graphics layer, since points are added
        ///     to the graphics layer in a "selected" state, displaying editing guide marks.
        ///     
        /// ##TODO## 
        /// look at using overlays to display these graphics rather than a graphics layer.
        /// </summary>
        /// <param name="mapPoint"></param>
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
        /// <summary>
        /// -   Check if the point has already been saved to the saved responses array, and if so,
        ///     present a dialog box to confirm the decision to save a duplicate
        /// -   If it has not already been saved, create new instance of the SOEResponseModel data object,
        ///     duplicating the properties of the target response model, and add the new instance to the 
        ///     saved response model array.
        /// </summary>
        /// <param name="state"></param>
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
                //create a duplicate responsemodel object and add it to the array of response models that will persist
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

        /// <summary>
        /// -   Update the graphics on the map, converting a newly mapped route graphic instance to a persisting
        ///     saved graphic (a green point).
        /// -   Delete the click point.
        /// </summary>
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

        /// <summary>
        /// -   Use the geometry of the route point from the SOE response to generate a label that is displayed
        ///     in an overlay on the map.
        /// </summary>
        /// <param name="soeResponse"></param>
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

        /// <summary>
        /// -   Initialize a mapping session (using the setsession method in MapAMilepostMaptool viewmodel)
        /// -   Update the public MapButtonLabel property to reflect the behavior of the MapPointExecuteButton.
        ///     This value is bound to the content of the button as a label.
        /// -   Update the private _setSession property to change the behavior of the method.
        /// </summary>
        /// <param name="state"></param>
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
                //  Calls the EndSession method from the MapAMilepostMapTool viewmodel, setting the active tool
                //  to whatever was selected before the mapping session was initialized.
                _mapTool.EndSession();
            }
        }
    }
}
