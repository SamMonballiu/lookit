using Lookit.Context;
using Lookit.Logic;
using Lookit.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Clipboard = System.Windows.Clipboard;
using IDataObject = System.Windows.IDataObject;
using Point = System.Drawing.Point;
using Lookit.Extensions;
using Lookit.Helpers;
using Windows.Storage;
using Windows.Data.Pdf;
using System.Threading.Tasks;

namespace Lookit.ViewModels
{
    public enum Rotation
    {
        Zero = 0,
        Ninety = 90,
        OneEighty = 180,
        TwoSeventy = 270
    }

    public enum Direction
    {
        Clockwise,
        CounterClockwise
    }

    public partial class LookitMainViewModel : ObservableObject
    {
        public void Reset()
        {
            Measurements.Clear();
            _tempPoints.Clear();
            ScaleContext.Scale = Scale.Default;
            _mode = Mode.MeasurePolygon;
        }

        [ObservableProperty]
        private int _pageRotation = (int)Rotation.Zero;

        [ObservableProperty]
        private bool _isPanning = false;

        [ObservableProperty]
        private bool _allowPointDragging = true;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(Measurements))]
        [NotifyPropertyChangedFor(nameof(Scale))]
        protected int _selectedPage = 1;

        private readonly Dictionary<int, ObservableCollection<MeasurementViewModel>> _pagedMeasurements = new();
        private readonly Dictionary<int, Scale> _pagedScales = new();

        public Scale Scale
        {
            get
            {
                if (!_pagedScales.ContainsKey(_selectedPage))
                {
                    _pagedScales.Add(_selectedPage, Scale.Default);
                }
                return _pagedScales[_selectedPage];
            } set
            {
                _pagedScales[_selectedPage] = value;
            }
        }

        public ObservableCollection<MeasurementViewModel> Measurements
        {
            get
            {
                if (!_pagedMeasurements.ContainsKey(_selectedPage))
                {
                    _pagedMeasurements.Add(_selectedPage, new ObservableCollection<MeasurementViewModel>());
                }
                return _pagedMeasurements[_selectedPage];
            }
        }

        public LineMeasurementViewModel LinePreview { 
            get
            {
                LineMeasurement measurement;
                if (_mode is not Mode.Scale && _mode is not Mode.MeasureLine || !TempPoints.Any())
                {
                    
                    measurement = new LineMeasurement(Point.Empty, Point.Empty);
                }

                else if (TempPoints.Count == 1)
                {
                    measurement = new LineMeasurement(TempPoints.First(), TempPoints.First());
                }

                else
                {
                    measurement = new LineMeasurement(TempPoints.First(), TempPoints.Last());
                }

                return new LineMeasurementViewModel(measurement, Scale, string.Empty);
            }
        }

        public PolygonMeasurementViewModel PolygonPreview
        {
            get
            {
                PolygonalMeasurement measurement;
                if ((_mode is not Mode.MeasurePolygon && _mode is not Mode.MeasureRectangle) || TempPoints.Count <= 1)
                {
                    measurement = new PolygonalMeasurement(new List<Point>());
                }

                else
                {
                    measurement = new PolygonalMeasurement(TempPoints.ToList());
                }

                return new PolygonMeasurementViewModel(measurement, Scale, string.Empty);
            }
        }

        [ObservableProperty]
        private double _zoomLevel = 1;

        [ObservableProperty]
        private Mode _mode = Mode.None;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(LinePreview))]
        private ObservableCollection<Point> _tempPoints = new();
        
        [ObservableProperty]
        private BitmapSource _imageSource;

        [ObservableProperty]
        private Measurement _selectedMeasurement;

        [ObservableProperty]
        private bool _straighten = true;

        public ICommand OnPasteImage { get; private set; }
        public ICommand OnAddPoint { get; private set; }
        public ICommand OnRemovePoint { get; private set; }
        public ICommand OnRemoveMeasurement { get; private set; }
        public ICommand OnToggleMeasurementHidden { get; private set; }
        public ICommand OnSetImageSource { get; private set; }
        public ICommand OnSwitchMode { get; private set; }
        public ICommand OnSetScale { get; private set; }
        public ICommand OnRemoveScale { get; private set; }
        public ICommand OnUpdateTemporaryPoint { get; private set; }
        public ICommand OnCancelMeasurement { get; private set; }
        public ICommand OnFinalizePreview { get; private set; }
        public ICommand OnRotate { get; private set; }
        public ICommand OnMoveMeasurementPoint { get; private set; }

        public LookitMainViewModel()
        {
            OnSetImageSource = new RelayCommand<BitmapSource>((bitmap) =>
            {
                ImageSource = bitmap;
            });
            OnPasteImage = new RelayCommand(PasteImage);
            OnAddPoint = new RelayCommand<Point>(AddPoint);
            OnRemovePoint = new RelayCommand(RemoveLastPoint);
            OnRemoveMeasurement = new RelayCommand<MeasurementViewModel>(measurement =>
            {
                Measurements.Remove(measurement);
                OnPropertyChanged(nameof(Measurements));
            });
            OnToggleMeasurementHidden = new RelayCommand<MeasurementViewModel>(measurement =>
            {
                measurement.Hidden = !measurement.Hidden;
                OnPropertyChanged(nameof(Measurements));
            });
            OnSwitchMode = new RelayCommand<Mode>(mode => Mode = mode);
            OnSetScale = new RelayCommand<Scale>(SetScale);
            OnRemoveScale = new RelayCommand(() => SetScale(null));
            OnUpdateTemporaryPoint = new RelayCommand<Point>(point =>
            {
                if (_straighten && (Mode is not Mode.MeasureRectangle) && !IsControlDown() && _tempPoints.Count > 1)
                {
                    point = Align(_tempPoints.SkipLast(1).Last(), point);

                    if (point.SharesAxisWith(PolygonPreview.Origin))
                    {
                        point = Align(PolygonPreview.Origin, point);
                    }
                }

                if (TempPoints.Count == 1)
                {
                    TempPoints.Add(point);
                }
                else
                {
                    if (_mode is Mode.MeasureRectangle)
                    {
                        var firstPoint = TempPoints.First();
                        var points = new List<Point>
                        {
                            firstPoint,
                            new Point(firstPoint.X, point.Y),
                            point,
                            new Point(point.X, firstPoint.Y),
                        };
                        TempPoints = points.ToObservableCollection();
                    }
                    else
                    {
                        TempPoints[^1] = point;
                    }
                }

                OnPropertyChanged(nameof(TempPoints));
                OnPropertyChanged(nameof(LinePreview));
                if (_mode is Mode.MeasurePolygon or Mode.MeasureRectangle)
                {
                    OnPropertyChanged(nameof(PolygonPreview));
                }
            });
            OnCancelMeasurement = new RelayCommand(() =>
            {
                if (_mode is Mode.None || !TempPoints.Any())
                {
                    return;
                }

                TempPoints.Clear();
                OnPropertyChanged(nameof(LinePreview));
                OnPropertyChanged(nameof(PolygonPreview));
                Mode = Mode.None;
            });
            OnFinalizePreview = new RelayCommand(() =>
            {
                if (Mode is Mode.MeasurePolygon or Mode.MeasureRectangle)
                {
                    var points = Mode switch
                    {
                        Mode.MeasurePolygon => TempPoints.SkipLast(1).ToList(),
                        _ => TempPoints.ToList()
                    };

                    var measurement = new PolygonalMeasurement(points);
                    Measurements.Add(new PolygonMeasurementViewModel(measurement, Scale, $"Item {Measurements.Count + 1}"));
                    TempPoints.Clear();
                    OnPropertyChanged(nameof(Measurements));
                    OnPropertyChanged(nameof(PolygonPreview));
                }
            });
            OnRotate = new RelayCommand<Direction>(direction =>
            {
                Rotate(direction);
            });
            OnMoveMeasurementPoint = new RelayCommand<(MeasurementViewModel Measurement, int Index, Point Point)>(Data =>
            {
                if (!_allowPointDragging)
                {
                    return;
                }

                var measurements = Measurements.ToList();
                var index = measurements.IndexOf(Data.Measurement);
                var point = Data.Point;
                if (_straighten)
                {
                    var commonAxes = Data.Measurement.Measurement.Points.Except(Data.Index).Where(pt => pt.SharesAxisWith(point));
                    foreach(var commonAxis in commonAxes)
                    {
                        point = Align(commonAxis, point);
                    }
                }

                measurements[index].Measurement.Points[Data.Index] = point;
                _pagedMeasurements[_selectedPage] = measurements.ToObservableCollection();
                OnPropertyChanged(nameof(Measurements));
            });
        }

        public static bool IsControlDown()
        {
            return (Control.ModifierKeys & Keys.Control) == Keys.Control;
        }

        private void AddPoint(Point point)
        {
            switch (_mode)
            {
                case Mode.MeasurePolygon:
                    AddPolygonPoint(point);
                    break;
                case Mode.MeasureLine:
                    AddLinePoint(point);
                    break;
                case Mode.MeasureRectangle:
                    AddRectanglePoint(point);
                    break;
                case Mode.Scale:
                    AddScalePoint(point);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(_mode));
            }
        }

        private void AddScalePoint(Point point)
        {
            if (_tempPoints.Any())
            {
                if (_straighten && !IsControlDown())
                {
                    point = Align(_tempPoints.First(), point);
                }
            }

            if (_tempPoints.Count <= 1)
            {
                _tempPoints.Add(point);
            }
        }

        private void AddLinePoint(Point point)
        {
            if (_tempPoints.Any())
            {
                if (_straighten && !IsControlDown())
                {
                    point = Align(_tempPoints.First(), point);
                }

                var measurement = new LineMeasurement(_tempPoints.First(), point);
                Measurements.Add(new LineMeasurementViewModel(measurement, Scale, $"Item {Measurements.Count() + 1}"));
                TempPoints.Clear();
                OnPropertyChanged(nameof(Measurements));
                OnPropertyChanged(nameof(LinePreview));
                Mode = Mode.None;
                return;
            }

            _tempPoints.Add(point);
        }

        private void AddRectanglePoint(Point point)
        {
            if (!TempPoints.Any())
            {
                if (Straighten && !IsControlDown() && TempPoints.Any())
                {
                    point = Align(_tempPoints.Last(), point);
                }
                TempPoints.Add(point);
                
            } else
            {
                var firstPoint = TempPoints.First();
                var points = new List<Point>
                {
                    firstPoint,
                    new Point(firstPoint.X, point.Y),
                    point,
                    new Point(point.X, firstPoint.Y),
                };

                var measurement = new PolygonalMeasurement(points);
                Measurements.Add(new PolygonMeasurementViewModel(measurement, Scale, $"Item {Measurements.Count() + 1}"));
                TempPoints.Clear();
                OnPropertyChanged(nameof(Measurements));
            }
            OnPropertyChanged(nameof(PolygonPreview));
        }

        private void AddPolygonPoint(Point point)
        {
            if (!TempPoints.Any() || !point.IsClose(TempPoints.ElementAt(0)))
            {
                if (Straighten && !IsControlDown() && TempPoints.Any())
                {
                    point = Align(_tempPoints.Last(), point);
                }
                
                TempPoints.Add(point);

            } else if (point.IsClose(TempPoints.ElementAt(0)))
            {
                var measurement = new PolygonalMeasurement(TempPoints.SkipLast(1).ToList());
                Measurements.Add(new PolygonMeasurementViewModel(measurement, Scale, $"Item {Measurements.Count() + 1}"));
                TempPoints.Clear();
                OnPropertyChanged(nameof(Measurements));
            }
            OnPropertyChanged(nameof(PolygonPreview));
        }

        private Point Align(Point first, Point second)
        {
            var tolerance = Convert.ToInt32(10 * (1 / _zoomLevel));
            return second.Align(first, tolerance);
        }

        private void RemoveLastPoint()
        {
            if (!new[] { Mode.MeasurePolygon, Mode.MeasureRectangle, Mode.MeasureLine, Mode.Scale }.Contains(_mode))
            {
                return;
            }

            if (_mode is Mode.MeasureLine or Mode.Scale or Mode.MeasureRectangle || _mode is Mode.MeasurePolygon && _tempPoints.Count == 1)
            {
                _tempPoints = new ObservableCollection<Point>();
                OnPropertyChanged(nameof(LinePreview));
            }
            
            if (_tempPoints.Any())
            {
                TempPoints.Remove(TempPoints.Last());
            }

            OnPropertyChanged(nameof(PolygonPreview));
        }

        private void PasteImage()
        {
            if (Clipboard.ContainsImage())
            {
                IDataObject clipboardData = Clipboard.GetDataObject();
                if (clipboardData != null)
                {
                    if (clipboardData.GetDataPresent(DataFormats.Bitmap))
                    {
                        _imageSource = Clipboard.GetImage();
                    }
                }
            }
        }

        private void SetScale(Scale scale)
        {
            scale ??= Scale.Default;
            Scale = scale;
            OnPropertyChanged(nameof(Scale));
            foreach (var measurement in _pagedMeasurements[_selectedPage])
            {
                measurement.Scale = scale;
            }
            TempPoints = new ObservableCollection<Point>();
        }

        private void Rotate(Direction direction)
        {
            var newRotation = direction switch
            {
                Direction.Clockwise => PageRotation switch
                {
                    0 => 90,
                    90 => 180,
                    180 => 270,
                    270 => 0,
                    _ => throw new NotImplementedException(),
                },
                Direction.CounterClockwise => PageRotation switch
                {
                    0 => 270,
                    90 => 0,
                    180 => 90,
                    270 => 180,
                    _ => throw new NotImplementedException(),
                },
                _ => throw new NotImplementedException()
            };

            PageRotation = newRotation;
            OnPropertyChanged(nameof(PageRotation));
        }

        

        public LookitMainViewModel(Dictionary<int, List<PersistableMeasurement>> pagedMeasurements, Dictionary<int, PersistableScale> pagedScales): this()
        {
            foreach (var page in pagedScales)
            {
                _pagedScales.Add(page.Key, new Scale(page.Value));
            }

            foreach(var page in pagedMeasurements)
            {
                var scale = _pagedScales[page.Key];
                _pagedMeasurements.Add(page.Key, page.Value.Select(x => x.ToViewmodel(scale)).ToObservableCollection());
            }

        }

        public static async Task<LookitMainViewModel> FromPersistedSession(PersistableSession session)
        {
            var file = await StorageFile.GetFileFromPathAsync(session.PdfPath);
            PageContext.Filename = session.PdfPath;
            PageContext.PDF = await PdfDocument.LoadFromFileAsync(file);

            var pageNumber = session.SelectedPage;
            
            var viewmodel = new LookitMainViewModel(session.PagedMeasurements, session.PagedScales);

            viewmodel.OnSetImageSource.Execute(PDF.GetImage(pageNumber));
            return viewmodel;
        }

        public PersistableSession GetPersistableSession()
        {
            var session = new PersistableSession
            {
                PdfPath = PageContext.Filename,
                SelectedPage = _selectedPage,
                PagedMeasurements = _pagedMeasurements.OrderBy(x => x.Key).Aggregate(
                    new Dictionary<int, List<PersistableMeasurement>>(),
                    (acc, keyValuePair) =>
                    {
                        acc.Add(keyValuePair.Key, keyValuePair.Value.Select(x => x.ToPersistableMeasurement()).ToList());
                        return acc;
                    }),
                PagedScales = _pagedScales.OrderBy(x => x.Key).Aggregate(
                    new Dictionary<int, PersistableScale>(),
                    (acc, keyValuePair) =>
                    {
                        acc.Add(keyValuePair.Key, keyValuePair.Value.ToPersistableScale());
                        return acc;
                    })
            };

            return session;
        }
    }
}
