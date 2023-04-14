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
        [NotifyPropertyChangedFor(nameof(PolygonMeasurements))]
        [NotifyPropertyChangedFor(nameof(LineMeasurements))]
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

        public LineMeasurement LinePreview { 
            get
            {
                if (_mode != Mode.MeasureLine || !TempPoints.Any())
                {
                    return new LineMeasurement(new Point(-100, -100), new Point(-100, -100));
                }

                if (TempPoints.Count == 1)
                {
                    return new LineMeasurement(TempPoints.First(), TempPoints.First());
                }

                return new LineMeasurement(TempPoints.First(), TempPoints.Last());
            }
        }

        [ObservableProperty]
        private double _zoomLevel = 1;

        [ObservableProperty]
        private Mode _mode = Mode.MeasurePolygon;

        public IEnumerable<PolygonMeasurementViewModel> PolygonMeasurements => Measurements.OfType<PolygonMeasurementViewModel>();
        public IEnumerable<LineMeasurementViewModel> LineMeasurements => Measurements.OfType<LineMeasurementViewModel>();

        [ObservableProperty]
        private ObservableCollection<Point> _tempPoints = new();
        
        public string TempPointsString => string.Join(",", _tempPoints.Select(p => $"{p.X}, {p.Y}"));
        public Point FirstTempPoint => _tempPoints.Any() 
            ? new Point(TempPoints.First().X, TempPoints.First().Y) 
            : new Point(int.MaxValue, int.MaxValue);

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
        public ICommand OnUpdateTemporaryPoint { get; private set; }
        
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
                default:
                    throw new ArgumentOutOfRangeException(nameof(_mode));
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
                OnPropertyChanged(nameof(LineMeasurements));
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
                OnPropertyChanged(nameof(PolygonMeasurements));
            }
            OnPropertyChanged(nameof(TempPointsString));
            OnPropertyChanged(nameof(FirstTempPoint));
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
                var measurement = new PolygonalMeasurement(TempPoints.ToList());
                Measurements.Add(new PolygonMeasurementViewModel(measurement, Scale, $"Item {Measurements.Count() + 1}"));
                TempPoints.Clear();
                OnPropertyChanged(nameof(PolygonMeasurements));
            }
            OnPropertyChanged(nameof(TempPointsString));
            OnPropertyChanged(nameof(FirstTempPoint));
        }

        private Point Align(Point first, Point second)
        {
            var tolerance = Convert.ToInt32(10 * (1 / _zoomLevel));
            return second.Align(first, tolerance);
        }

        private void RemoveLastPoint()
        {
            if (Mode != Mode.MeasurePolygon && Mode != Mode.MeasureRectangle)
            {
                return;
            }

            if (_tempPoints.Any())
            {
                TempPoints.Remove(TempPoints.Last());
                OnPropertyChanged(nameof(TempPointsString));
                OnPropertyChanged(nameof(FirstTempPoint));
            }
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
                OnPropertyChanged(nameof(PolygonMeasurements));
                OnPropertyChanged(nameof(LineMeasurements));
            });
            OnToggleMeasurementHidden = new RelayCommand<MeasurementViewModel>(measurement =>
            {
                measurement.Hidden = !measurement.Hidden;
            });
            OnSwitchMode = new RelayCommand<Mode>(mode => Mode = mode);
            OnSetScale = new RelayCommand<Scale>(scale =>
            {
                Scale = scale;
                OnPropertyChanged(nameof(Scale));
                foreach (var measurement in _pagedMeasurements[_selectedPage])
                {
                    measurement.Scale = scale;
                }
            });
            OnUpdateTemporaryPoint = new RelayCommand<Point>(point =>
            {
                if (_straighten && !IsControlDown())
                {
                    point = Align(_tempPoints.First(), point);
                }

                if (TempPoints.Count == 1)
                {
                    TempPoints.Add(point);
                } else
                {
                    TempPoints[^1] = point;
                }
                OnPropertyChanged(nameof(TempPoints));
                OnPropertyChanged(nameof(LinePreview));
            });
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
