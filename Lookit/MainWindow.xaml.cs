using Lookit.Context;
using Lookit.Helpers;
using Lookit.Logic;
using Lookit.Models;
using Lookit.ViewModels;
using Lookit.Views;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Input;
using Cursors = System.Windows.Input.Cursors;

namespace Lookit
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        // at class level
        [System.Runtime.InteropServices.DllImport("gdi32.dll")]
        public static extern bool DeleteObject(IntPtr hObject);

        public LookitMainViewModel Viewmodel => (DataContext as LookitMainViewModel);

        private readonly OpenFileDialog _loadFileDialog = new()
        {
            Filter = "JSON files|*.json"
        };

        private readonly SaveFileDialog _saveFileDialog = new()
        {
            Filter = "JSON files|*.json"
        };

        private readonly KeyMap _keyboardBindings = new();

        private Point _previousMousePosition = new();
        private Point _currentMousePosition = new();
        private Timer _panTimer = new();
        private bool _isPanning = false;
        private Panning _panningHandler = new();
        private (MeasurementViewModel Measurement, int Point) _lastClicked = (null, -1);

        private bool IsDraggingPoint() => _lastClicked.Measurement is not null;

        public MainWindow()
        {
            _panTimer.Tick += PanTimer_Tick;
            _panTimer.Interval = 1;
            InitializeComponent();
            this.DataContext = new LookitMainViewModel();
            zoomPicker.ZoomChanged += ZoomPicker_ZoomChanged;
            ContentRendered += MainWindow_ContentRendered;

            ListMeasurements.SelectionChanged += (s, e) =>
            {
                try
                {
                    Viewmodel.SelectedMeasurement = Viewmodel.Measurements[ListMeasurements.SelectedIndex]?.Measurement;
                } catch
                {
                    //TODO Fix
                }
            };
        }

        private void PanTimer_Tick(object sender, EventArgs e)
        {
            _panningHandler.Update(
                Viewmodel.ZoomLevel, 
                _previousMousePosition, 
                _currentMousePosition, 
                sv);
        }

        private void MainWindow_ContentRendered(object sender, EventArgs e)
        {
            RegisterKeyBindings();
        }

        private void RegisterKeyBindings()
        {
            _keyboardBindings.Bind(Key.NumPad0)
                .To(() => TrySwitch(Mode.None));

            _keyboardBindings.Bind(Key.P, Key.NumPad1)
                .To(() => TrySwitch(Mode.MeasurePolygon));

            _keyboardBindings.Bind(Key.R, Key.NumPad2)
                .To(() => TrySwitch(Mode.MeasureRectangle));

            _keyboardBindings.Bind(Key.L, Key.NumPad3)
                .To(() => TrySwitch(Mode.MeasureLine));

            _keyboardBindings.Bind(Key.S)
                .To(() => TrySwitch(Mode.Scale));

            _keyboardBindings.Bind(Key.Escape)
                .To(() => Viewmodel.OnCancelMeasurement.Execute(null));

            _keyboardBindings.Bind(Key.Enter, Key.Return)
                .To(() =>
            {
                if (Viewmodel.TempPoints.Any() && Viewmodel.Mode is Mode.MeasurePolygon or Mode.MeasureRectangle)
                {
                    Viewmodel.OnFinalizePreview.Execute(null);
                }
            });
        }

        private void TrySwitch(Mode mode)
        {
            if (Viewmodel.TempPoints.Any())
            {
                return;
            }

            Viewmodel.Mode = mode;
        }

        private void DdnPages_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            PageContext.SelectPage((int)ddnPages.SelectedValue);
            Viewmodel.SelectedPage = PageContext.SelectedPage;
            Viewmodel.OnSetImageSource.Execute(PageContext.Get(PageContext.SelectedPage));
            var pageNumber = PageContext.SelectedPage;
            if (PageContext.Has(pageNumber))
            {
                Viewmodel.OnSetImageSource.Execute(PageContext.Get(pageNumber));
                return;
            }

            Viewmodel.OnSetImageSource.Execute(PDF.GetImage(pageNumber));
        }

        public void ShowPdfPicker()
        {
            new PdfPicker().ShowDialog();
            Viewmodel?.OnSetImageSource.Execute(PageContext.Get(PageContext.SelectedPage));
            UpdatePagesDropdown();
        }

        private void ZoomPicker_ZoomChanged(object sender, UserControls.ZoomChangedEventArgs e)
        {
            (DataContext as LookitMainViewModel).ZoomLevel = e.Value;
        }

        private void ImgControl_OnMouseUp(object sender, MouseButtonEventArgs e)
        {
            if (Viewmodel.IsPanning)
                return;

            switch (e.ChangedButton)
            {
                case MouseButton.Left:
                    if (Viewmodel.Mode is Mode.None || this.IsDraggingPoint())
                    {
                        return;
                    }
                    if (Viewmodel.Mode is Mode.Scale)
                    {
                        var pos = e.GetPosition(ImgMain);
                        Viewmodel.OnAddPoint.Execute(pos.ToPoint());

                        if (Viewmodel.TempPoints.Count == 2)
                        {
                            var measurement = Viewmodel.LinePreview.Measurement as LineMeasurement;
                            OpenSetScale(measurement);
                        }
                        return;
                    }

                    Viewmodel.OnAddPoint.Execute(e.GetPosition(ImgMain).ToPoint());
                    break;

                case MouseButton.Right:
                    Viewmodel.OnRemovePoint.Execute(null);
                    break;
            }
        }


        private void BtnScale_Click(object sender, RoutedEventArgs e)
        {
            Viewmodel.Mode = Mode.Scale;
        }

        private void BtnMeasure_Click(object sender, RoutedEventArgs e)
        {
            Viewmodel.Mode = Mode.MeasurePolygon; //TODO!
        }

        private void BtnMeasureLine_Click(object sender, RoutedEventArgs e)
        {
            Viewmodel.Mode = Mode.MeasureLine;
        }


        private void BtnMeasureRect_Click(object sender, RoutedEventArgs e)
        {
            Viewmodel.Mode = Mode.MeasureRectangle;
        }

        private void BtnUpdateScale_Click(object sender, RoutedEventArgs e)
        {
            ScaleContext.Scale = ScaleContext.Scale.UpdateDistance(double.Parse(TxtScaleDistance.Text));
        }

        private void btnOpenFile_Click(object sender, RoutedEventArgs e)
        {
            ShowPdfPicker();
        }

        private void ScrollViewer_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (!LookitMainViewModel.IsControlDown())
            {
                return;
            }

            if (e.Delta > 0)
            {
                zoomPicker.ZoomIn();
            } 
            else
            {
                zoomPicker.ZoomOut();
            }
        }

        private async void btnLoadFile_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_loadFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    await LoadPersistedSession(_loadFileDialog.FileName);
                
                    UpdatePagesDropdown();
                }
            } catch (Exception ex)
            {
                System.Windows.MessageBox.Show(ex.ToString());
            }
        }

        private void btnSaveFile_Click(object sender, RoutedEventArgs e)
        {
            if (_saveFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                PersistToSession(_saveFileDialog.FileName);
            }
        }

        private void UpdatePagesDropdown()
        {
            if (PageContext.PDF != null)
            {
                var pages = Enumerable.Range(1, (int)PageContext.PDF.PageCount).ToList();
                ddnPages.ItemsSource = pages;
                ddnPages.SelectionChanged += DdnPages_SelectionChanged;
                ddnPages.SelectedValue = PageContext.SelectedPage;
            }
        }

        public async Task LoadPersistedSession(string filename)
        {
            var persistedSession = Persist.ReadSession(filename);
            this.DataContext = await LookitMainViewModel.FromPersistedSession(persistedSession);
            PageContext.SelectPage(persistedSession.SelectedPage);
            PageContext.Filename = persistedSession.PdfPath;
        }

        public void PersistToSession(string filename)
        {
            var persistableSession = Viewmodel.GetPersistableSession();
            Persist.PersistSession(persistableSession, filename);
        }

        private void UpdatePanCursor(Point newPosition, Point oldPosition)
        {
            var canScrollHorizontal = sv.ComputedHorizontalScrollBarVisibility is Visibility.Visible;

            if (newPosition.Y < oldPosition.Y)
            {
                sv.Cursor = newPosition switch
                {
                    { X: var x } when x < oldPosition.X && canScrollHorizontal => Cursors.ScrollNW,
                    { X: var x } when x > oldPosition.X && canScrollHorizontal => Cursors.ScrollNE,
                    _ => Cursors.ScrollN,
                };
            } else if (newPosition.Y > oldPosition.Y)
            {
                sv.Cursor = newPosition switch
                {
                    { X: var x } when x < oldPosition.X && canScrollHorizontal => Cursors.ScrollSW,
                    { X: var x } when x > oldPosition.X && canScrollHorizontal => Cursors.ScrollSE,
                    _ => Cursors.ScrollS
                };
            }
        }

        private void ScrollViewer_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            var pos = e.GetPosition(ImgMain);
            if (Viewmodel.IsPanning)
            {
                Point oldMousePosition = _previousMousePosition;
                _currentMousePosition = Mouse.GetPosition(sv);

                if (_isPanning || Mouse.LeftButton is MouseButtonState.Pressed)
                {
                    _panTimer.Start();
                    sv.CaptureMouse();
                    UpdatePanCursor(_currentMousePosition, _previousMousePosition);
                }
                else
                {
                    _panTimer.Stop();
                    sv.ReleaseMouseCapture();
                    _previousMousePosition = _currentMousePosition;
                }
            }

            else if (Viewmodel.Mode is not Mode.None && Viewmodel.TempPoints.Count > 0)
            {
                Viewmodel.OnUpdateTemporaryPoint.Execute(pos.ToPoint());
                var tolerance = 100;
                var shouldPan = pos.IsCloseToEdges(sv, tolerance);

                Panning.UpdateManual(
                    sv, 
                    shouldPan.Right
                        ? 0.25
                        : shouldPan.Left 
                            ? -0.25
                            : 0, 
                    shouldPan.Bottom
                        ? 0.25
                        : shouldPan.Top
                            ? -0.25
                            : 0
                    );
            }
        }

        private void BtnDeleteScale_Click(object sender, RoutedEventArgs e)
        {
            Viewmodel.OnRemoveScale.Execute(null);
        }

        private void BtnEditScale_Click(object sender, RoutedEventArgs e)
        {
            OpenSetScale(Viewmodel.Scale);
        }

        private void OpenSetScale(LineMeasurement measurement)
        {
            var view = new SetScaleView(measurement.Start, measurement.End);
            view.OnConfirm += (Scale scale) => Viewmodel.OnSetScale?.Execute(scale);
            view.ShowDialog();
        }

        private void OpenSetScale(Scale scale)
        {
            var view = new SetScaleView(scale);
            view.OnConfirm += (Scale scale) => Viewmodel.OnSetScale?.Execute(scale);
            view.ShowDialog();
        }

        private void Window_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            var action = _keyboardBindings.GetBinding(e.Key);
            action?.Invoke();
        }

        private void BtnRotate_Click(object sender, RoutedEventArgs e)
        {
            Viewmodel.OnRotate.Execute(sender == BtnRotateCCW ? Direction.CounterClockwise : Direction.Clockwise);
        }

        private void BtnPan_Click(object sender, RoutedEventArgs e)
        {
            Viewmodel.IsPanning = !Viewmodel.IsPanning;
        }

        private void CnvEditPoints_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            // forget last clicked point on button release
            if (e.LeftButton is MouseButtonState.Released)
            {
                if (_lastClicked.Measurement is not null)
                {
                    Viewmodel.OnReleaseMeasurementPoint.Execute(null);
                }
                _lastClicked = (null, -1);
                return;
            }

            var pos = e.GetPosition(ImgMain);

            if (!Viewmodel.AllowPointDragging)
                return;

            if (e.LeftButton is MouseButtonState.Pressed)
            {
                // Determine the clicked measurement & point
                _lastClicked = _lastClicked.Measurement is null ? GetClicked() : _lastClicked;

                if (_lastClicked.Point != -1)
                {
                    Viewmodel.OnDragMeasurementPoint.Execute((_lastClicked.Measurement, _lastClicked.Point, pos.ToPoint()));
                }
            }

            (MeasurementViewModel Measurement, int Point) GetClicked()
            {
                var clickedMeasurement = Viewmodel.Measurements
                    .FirstOrDefault(m => m.Measurement.Points.Any(pt => pos.IsClose(pt)));

                var clickedPoint = clickedMeasurement?.Measurement.Points
                    .FirstOrDefault(pt => pos.IsClose(pt)) ?? System.Drawing.Point.Empty;

                return (clickedMeasurement, clickedMeasurement?.Measurement.Points.IndexOf(clickedPoint) ?? -1);
            }
        }
    }
}
